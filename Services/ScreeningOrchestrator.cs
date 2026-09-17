using AiScreeningMiddleware.Demo.Models;

namespace AiScreeningMiddleware.Demo.Services;

public class ScreeningOrchestrator
{
    private readonly IDocumentLookupService _lookup;
    private readonly IStorageRetrievalService _storage;
    private readonly IDocumentValidationService _validation;
    private readonly IRuleEngineService _rules;
    private readonly IAiServicesClient _ai;
    private readonly IResponseTransformer _transformer;
    private readonly ICommentsPersistenceService _comments;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ScreeningOrchestrator> _logger;

    public ScreeningOrchestrator(
        IDocumentLookupService lookup,
        IStorageRetrievalService storage,
        IDocumentValidationService validation,
        IRuleEngineService rules,
        IAiServicesClient ai,
        IResponseTransformer transformer,
        ICommentsPersistenceService comments,
        IConfiguration configuration,
        ILogger<ScreeningOrchestrator> logger)
    {
        _lookup = lookup;
        _storage = storage;
        _validation = validation;
        _rules = rules;
        _ai = ai;
        _transformer = transformer;
        _comments = comments;
        _configuration = configuration;
        _logger = logger;
    }

    public ScreeningResult Process(ScreeningRequest request)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..12];
        var result = new ScreeningResult
        {
            CorrelationId = correlationId,
            ApplicationId = request.ApplicationId,
            Mode = request.Mode
        };

        void Step(string message)
        {
            result.Trace.Add(message);
            _logger.LogInformation("[{CorrelationId}] {Step}", correlationId, message);
        }

        var enabled = request.Mode.Equals("Screening", StringComparison.OrdinalIgnoreCase)
            ? _configuration.GetValue<bool>("AiScreening:EnableAiScreening")
            : _configuration.GetValue<bool>("AiScreening:EnableAiDocumentVerification");

        if (!enabled)
        {
            Step($"Kill switch OFF for mode '{request.Mode}'; AI skipped and enrollment proceeds.");
            result.Status = "Skipped";
            return result;
        }

        var references = _lookup.GetDocuments(request.ApplicationId);
        Step($"1. Looked up {references.Count} document(s) from Provider DB [STUB].");

        var retrieved = new List<RetrievedDocument>();
        foreach (var reference in references)
        {
            var document = _storage.Retrieve(reference);
            _validation.Validate(document);
            retrieved.Add(document);
            Step($"2-3. {reference.DocumentId} ({reference.DocumentType}) from {reference.StoragePlatform}; valid={document.IsValid}" +
                 (document.IsValid ? string.Empty : $" ({document.ValidationNote})"));
        }

        var eligible = _rules.ApplyRules(retrieved, request.Mode);
        Step($"4. Rules applied; {eligible.Count} eligible document(s).");

        if (eligible.Count == 0)
        {
            Step("No eligible documents remain; AI call skipped.");
            result.Status = "Skipped";
            return result;
        }

        var aiRequest = new AiRequest
        {
            CorrelationId = correlationId,
            ApplicationId = request.ApplicationId,
            Mode = request.Mode,
            Documents = eligible.Select(document => new AiDocument
            {
                DocumentId = document.Ref.DocumentId,
                DocumentType = document.Ref.DocumentType,
                SizeBytes = document.Content.Length
            }).ToList()
        };
        Step("5. Built AI request payload.");

        AiResponse aiResponse;
        try
        {
            aiResponse = _ai.Screen(aiRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "[{CorrelationId}] AI service call failed.", correlationId);
            Step("6. AI service call failed; enrollment NOT blocked and manual review required.");
            result.Status = "ServiceUnavailable";
            return result;
        }

        if (!aiResponse.ServiceAvailable)
        {
            Step("6. AI service UNAVAILABLE; enrollment NOT blocked and manual review required.");
            result.Status = "ServiceUnavailable";
            return result;
        }

        Step($"6. AI returned {aiResponse.Findings.Count} finding(s).");

        var comments = _transformer.Transform(aiResponse, request.ApplicationId);
        _comments.Save(comments);
        result.Comments = comments;
        Step($"7-8. Transformed and persisted {comments.Count} AI-tagged comment(s) [STUB persistence].");

        result.Status = "Completed";
        Step("9. Done. Clerk or workflow reads results through the existing comments mechanism.");
        return result;
    }
}
