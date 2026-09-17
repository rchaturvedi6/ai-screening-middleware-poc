using AiScreeningMiddleware.Demo.Models;

namespace AiScreeningMiddleware.Demo.Services;

// STUB: Replace with the real Provider database query.
public class StubDocumentLookupService : IDocumentLookupService
{
    public List<DocumentRef> GetDocuments(string applicationId) => new()
    {
        new() { DocumentId = "DOC-001", DocumentType = "License", StoragePlatform = "Oracle", StorageKey = "K-1001", FileName = "license.pdf" },
        new() { DocumentId = "DOC-002", DocumentType = "W9", StoragePlatform = "EDMS", StorageKey = "K-1002", FileName = "w9.pdf" },
        new() { DocumentId = "DOC-003", DocumentType = "DEA", StoragePlatform = "Oracle", StorageKey = "K-1003", FileName = "dea.pdf" }
    };
}

// STUB: Replace with the real Oracle/EDMS connector.
public class StubStorageRetrievalService : IStorageRetrievalService
{
    public RetrievedDocument Retrieve(DocumentRef doc)
    {
        var bytes = doc.DocumentId == "DOC-002" ? Array.Empty<byte>() : new byte[2048];
        return new RetrievedDocument { Ref = doc, Content = bytes };
    }
}

// REAL demo logic: Basic document validation.
public class DocumentValidationService : IDocumentValidationService
{
    public void Validate(RetrievedDocument doc)
    {
        if (doc.Content.Length == 0)
        {
            doc.IsValid = false;
            doc.ValidationNote = "Empty or unretrievable file";
        }
        else if (doc.Content.Length > 10_000_000)
        {
            doc.IsValid = false;
            doc.ValidationNote = "Exceeds size limit";
        }
        else
        {
            doc.IsValid = true;
            doc.ValidationNote = null;
        }
    }
}

// REAL demo logic: Only valid documents are eligible for AI.
public class RuleEngineService : IRuleEngineService
{
    public List<RetrievedDocument> ApplyRules(List<RetrievedDocument> docs, string mode) =>
        docs.Where(document => document.IsValid).ToList();
}

// STUB/MOCK: Replace with a real HttpClient-based Enso integration.
public class StubAiServicesClient : IAiServicesClient
{
    private readonly IConfiguration _configuration;

    public StubAiServicesClient(IConfiguration configuration) => _configuration = configuration;

    public AiResponse Screen(AiRequest request)
    {
        if (_configuration.GetValue<bool>("AiScreening:SimulateAiFailure"))
        {
            return new AiResponse
            {
                CorrelationId = request.CorrelationId,
                ServiceAvailable = false
            };
        }

        var response = new AiResponse
        {
            CorrelationId = request.CorrelationId,
            ServiceAvailable = true
        };

        foreach (var document in request.Documents)
        {
            response.Findings.Add(document.DocumentType switch
            {
                "DEA" => new AiFinding
                {
                    DocumentId = document.DocumentId,
                    ResultCode = 3002,
                    Message = "Document expired",
                    Severity = "Warning",
                    Confidence = 0.88
                },
                _ => new AiFinding
                {
                    DocumentId = document.DocumentId,
                    ResultCode = 1000,
                    Message = "Document verified successfully",
                    Severity = "Info",
                    Confidence = 0.97
                }
            });
        }

        return response;
    }
}

// REAL demo logic: Convert AI findings to AI-tagged comments.
public class ResponseTransformer : IResponseTransformer
{
    public List<ScreeningComment> Transform(AiResponse response, string applicationId) =>
        response.Findings.Select(finding => new ScreeningComment
        {
            ApplicationId = applicationId,
            DocumentId = finding.DocumentId,
            Source = "AI",
            Severity = finding.Severity,
            CorrelationId = response.CorrelationId,
            Text = $"[AI] ({finding.ResultCode}) {finding.Message} (confidence {finding.Confidence:P0})"
        }).ToList();
}

// STUB: Replace with a real comments-table insert.
public class InMemoryCommentsPersistenceService : ICommentsPersistenceService
{
    private readonly List<ScreeningComment> _store = new();
    private readonly object _lock = new();

    public void Save(IEnumerable<ScreeningComment> comments)
    {
        lock (_lock)
        {
            _store.AddRange(comments);
        }
    }

    public IReadOnlyList<ScreeningComment> All()
    {
        lock (_lock)
        {
            return _store.ToList().AsReadOnly();
        }
    }
}
