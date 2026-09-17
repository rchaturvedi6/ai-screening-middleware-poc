using AiScreeningMiddleware.Demo.Models;

namespace AiScreeningMiddleware.Demo.Services;

public interface IDocumentLookupService
{
    List<DocumentRef> GetDocuments(string applicationId);
}

public interface IStorageRetrievalService
{
    RetrievedDocument Retrieve(DocumentRef doc);
}

public interface IDocumentValidationService
{
    void Validate(RetrievedDocument doc);
}

public interface IRuleEngineService
{
    List<RetrievedDocument> ApplyRules(List<RetrievedDocument> docs, string mode);
}

public interface IAiServicesClient
{
    AiResponse Screen(AiRequest request);
}

public interface IResponseTransformer
{
    List<ScreeningComment> Transform(AiResponse response, string applicationId);
}

public interface ICommentsPersistenceService
{
    void Save(IEnumerable<ScreeningComment> comments);
    IReadOnlyList<ScreeningComment> All();
}
