namespace AiScreeningMiddleware.Demo.Models;

public class ScreeningRequest
{
    /// <summary>Application or enrollment identifier, such as an ATN.</summary>
    public string ApplicationId { get; set; } = default!;

    /// <summary>DocVerification (pre-submission) or Screening (post-submission).</summary>
    public string Mode { get; set; } = "DocVerification";
}

public class DocumentRef
{
    public string DocumentId { get; set; } = default!;
    public string DocumentType { get; set; } = default!;
    public string StoragePlatform { get; set; } = default!;
    public string StorageKey { get; set; } = default!;
    public string FileName { get; set; } = default!;
}

public class RetrievedDocument
{
    public DocumentRef Ref { get; set; } = default!;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public bool IsValid { get; set; }
    public string? ValidationNote { get; set; }
}

public class AiRequest
{
    public string CorrelationId { get; set; } = default!;
    public string ApplicationId { get; set; } = default!;
    public string Mode { get; set; } = default!;
    public List<AiDocument> Documents { get; set; } = new();
}

public class AiDocument
{
    public string DocumentId { get; set; } = default!;
    public string DocumentType { get; set; } = default!;
    public int SizeBytes { get; set; }
}

public class AiResponse
{
    public string CorrelationId { get; set; } = default!;
    public bool ServiceAvailable { get; set; } = true;
    public List<AiFinding> Findings { get; set; } = new();
}

public class AiFinding
{
    public string DocumentId { get; set; } = default!;
    public int ResultCode { get; set; }
    public string Message { get; set; } = default!;
    public string Severity { get; set; } = "Info";
    public double Confidence { get; set; } = 1.0;
}

public class ScreeningComment
{
    public string ApplicationId { get; set; } = default!;
    public string DocumentId { get; set; } = default!;
    public string Source { get; set; } = "AI";
    public string Text { get; set; } = default!;
    public string Severity { get; set; } = "Info";
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = default!;
}

public class ScreeningResult
{
    public string CorrelationId { get; set; } = default!;
    public string ApplicationId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Mode { get; set; } = default!;
    public List<ScreeningComment> Comments { get; set; } = new();
    public List<string> Trace { get; set; } = new();
}
