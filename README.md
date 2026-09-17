# AI Screening Middleware Demo

A synchronous .NET 8 Web API demo for both pre-submission document verification and post-submission screening.

## Open and run in Visual Studio

1. Extract the ZIP.
2. Open `AiScreeningMiddleware.Demo.sln` in Visual Studio 2022.
3. Allow NuGet restore to complete.
4. Select the `https` launch profile.
5. Press F5. Swagger opens automatically.

## Demo request

```json
{
  "applicationId": "ATN-12345",
  "mode": "DocVerification"
}
```

## Expected result

- DOC-001 passes validation and returns mocked result code 1000.
- DOC-002 is empty and is excluded before the AI call.
- DOC-003 returns mocked result code 3002 for an expired document.
- The response includes the full trace and a correlation ID.
- `GET /api/screening/comments` shows the in-memory AI-tagged comments.

## Operational switches

Edit `appsettings.json`, restart, and rerun:

- `SimulateAiFailure: true` demonstrates graceful degradation.
- `EnableAiDocumentVerification: false` demonstrates the pre-submission kill switch.
- `EnableAiScreening: false` demonstrates the post-submission kill switch.

## Stub boundaries

The Provider DB lookup, Oracle/EDMS retrieval, Enso AI call, and comments persistence are stubs. Validation, rule filtering, orchestration, response transformation, mode validation, correlation logging, kill switches, and fallback behavior are executable demo logic.
