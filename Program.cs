using AiScreeningMiddleware.Demo.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
});

// Each STUB can be replaced independently when the real contract is available.
builder.Services.AddSingleton<IDocumentLookupService, StubDocumentLookupService>();
builder.Services.AddSingleton<IStorageRetrievalService, StubStorageRetrievalService>();
builder.Services.AddSingleton<IDocumentValidationService, DocumentValidationService>();
builder.Services.AddSingleton<IRuleEngineService, RuleEngineService>();
builder.Services.AddSingleton<IAiServicesClient, StubAiServicesClient>();
builder.Services.AddSingleton<IResponseTransformer, ResponseTransformer>();
builder.Services.AddSingleton<ICommentsPersistenceService, InMemoryCommentsPersistenceService>();
builder.Services.AddSingleton<ScreeningOrchestrator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
