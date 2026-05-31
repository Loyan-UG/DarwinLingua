using System.Net;
using System.Text;
using System.Text.Json;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class TransactionalEmailBrevoTests
{
    [Fact]
    public async Task SendAsync_BrevoApi_PostsTransactionalPayloadWithApiKeyAndSandboxHeader()
    {
        CapturingHttpMessageHandler handler = new(
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("{\"messageId\":\"<brevo-message-id>\"}", Encoding.UTF8, "application/json"),
            });
        HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://api.brevo.com"),
        };
        TransactionalEmailSender sender = new(
            Options.Create(CreateBrevoOptions(sandboxMode: true)),
            new StaticHttpClientFactory(httpClient),
            new TestWebHostEnvironment(),
            NullLogger<TransactionalEmailSender>.Instance);

        TransactionalEmailSendResult result = await sender.SendAsync(CreateMessage(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("brevo-api", result.ProviderName);
        Assert.Equal("<brevo-message-id>", result.ProviderMessageId);
        Assert.Equal(HttpMethod.Post, handler.LastMethod);
        Assert.Equal(new Uri("https://api.brevo.com/v3/smtp/email"), handler.LastRequestUri);
        Assert.Equal("test-api-key", Assert.Single(handler.GetHeaderValues("api-key")));
        Assert.Equal("drop", Assert.Single(handler.GetHeaderValues("X-Sib-Sandbox")));

        using JsonDocument payload = JsonDocument.Parse(handler.LastRequestBody);
        JsonElement root = payload.RootElement;
        Assert.Equal("no-reply@example.com", root.GetProperty("sender").GetProperty("email").GetString());
        Assert.Equal("Darwin Lingua", root.GetProperty("sender").GetProperty("name").GetString());
        Assert.Equal("learner@example.com", root.GetProperty("to")[0].GetProperty("email").GetString());
        Assert.Equal("Confirm your email", root.GetProperty("subject").GetString());
        Assert.Equal("Plain body", root.GetProperty("textContent").GetString());
        Assert.Equal("<p>HTML body</p>", root.GetProperty("htmlContent").GetString());
        Assert.Contains("darwinlingua", root.GetProperty("tags").EnumerateArray().Select(static tag => tag.GetString()));
        Assert.Equal("drop", root.GetProperty("headers").GetProperty("X-Sib-Sandbox").GetString());
        Assert.Equal("Account.EmailConfirmation", root.GetProperty("headers").GetProperty("X-DarwinLingua-Scenario").GetString());
        Assert.Equal("account.email-confirmation", root.GetProperty("headers").GetProperty("X-DarwinLingua-Template").GetString());
        Assert.Equal("trace-123", root.GetProperty("headers").GetProperty("X-DarwinLingua-CorrelationId").GetString());
    }

    [Fact]
    public async Task SendAsync_BrevoApi_ReturnsUsefulFailureSummaryFromBrevoErrorPayload()
    {
        CapturingHttpMessageHandler handler = new(
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "Bad Request",
                Content = new StringContent("{\"code\":\"invalid_parameter\",\"message\":\"sender is invalid\"}", Encoding.UTF8, "application/json"),
            });
        TransactionalEmailSender sender = new(
            Options.Create(CreateBrevoOptions(sandboxMode: false)),
            new StaticHttpClientFactory(new HttpClient(handler)),
            new TestWebHostEnvironment(),
            NullLogger<TransactionalEmailSender>.Instance);

        TransactionalEmailSendResult result = await sender.SendAsync(CreateMessage(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("brevo-api", result.ProviderName);
        Assert.Equal("InvalidOperationException", result.FailureCode);
        Assert.Contains("Brevo error invalid_parameter: sender is invalid", result.FailureMessageSummary);
    }

    [Fact]
    public async Task MarkProviderEventAsync_ComplaintMarksDeliveryFailedAndSuppressesRecipient()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_web_email");
        DbContextOptions<WebIdentityDbContext> options = new DbContextOptionsBuilder<WebIdentityDbContext>()
            .UseNpgsql(database.ConnectionString)
            .Options;

        await using WebIdentityDbContext dbContext = new(options);
        await new WebUserStateDatabaseBootstrapper(dbContext).InitializeAsync(CancellationToken.None);
        EmailDeliveryLogRepository repository = new(dbContext);
        WebEmailDeliveryLog log = await repository.AddQueuedAsync(
            CreateMessage(),
            "brevo-api",
            CancellationToken.None);
        await repository.MarkSentAsync(log.Id, "<provider-message-id>", CancellationToken.None);

        bool updated = await repository.MarkProviderEventAsync(
            "<provider-message-id>",
            "complaint",
            DateTimeOffset.UtcNow,
            "recipient reported spam",
            CancellationToken.None);

        Assert.True(updated);
        WebEmailDeliveryLog storedLog = await dbContext.EmailDeliveryLogs.SingleAsync();
        Assert.Equal(WebEmailDeliveryStatus.Failed, storedLog.Status);
        Assert.Equal("brevo:complaint", storedLog.FailureCode);
        Assert.Equal("complaint", storedLog.ProviderLastEvent);
        WebEmailSuppression suppression = await dbContext.EmailSuppressions.SingleAsync();
        Assert.Equal("brevo:complaint", suppression.Reason);
        Assert.Equal("brevo-api", suppression.ProviderName);
        Assert.Equal("<provider-message-id>", suppression.ProviderMessageId);
    }

    [Fact]
    public void Validate_BrevoApiModeRequiresApiKeyAndWebhookSecret()
    {
        TransactionalEmailOptions options = CreateBrevoOptions(sandboxMode: false);
        options.BrevoApiKey = string.Empty;
        options.BrevoWebhookSecret = string.Empty;
        TransactionalEmailOptionsValidator validator = new(new TestWebHostEnvironment());

        ValidateOptionsResult result = validator.Validate(TransactionalEmailOptions.SectionName, options);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failures);
        Assert.Contains("TransactionalEmail:BrevoApiKey is required when Mode is BrevoApi.", result.Failures);
        Assert.Contains("TransactionalEmail:BrevoWebhookSecret is required when Mode is BrevoApi.", result.Failures);
    }

    [Fact]
    public void Validate_ProductionRejectsBrevoQuerySecretFallback()
    {
        TransactionalEmailOptions options = CreateBrevoOptions(sandboxMode: false);
        options.PublicBaseUrl = "https://lingua.example";
        options.BrevoAllowQuerySecretFallback = true;
        TransactionalEmailOptionsValidator validator = new(new TestWebHostEnvironment
        {
            EnvironmentName = Environments.Production,
        });

        ValidateOptionsResult result = validator.Validate(TransactionalEmailOptions.SectionName, options);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failures);
        Assert.Contains("TransactionalEmail:BrevoAllowQuerySecretFallback must be false in Production.", result.Failures);
    }

    [Fact]
    public void Validate_ProductionRequiresHttpsPublicBaseUrl()
    {
        TransactionalEmailOptions options = CreateBrevoOptions(sandboxMode: false);
        TransactionalEmailOptionsValidator validator = new(new TestWebHostEnvironment
        {
            EnvironmentName = Environments.Production,
        });

        ValidateOptionsResult missingResult = validator.Validate(TransactionalEmailOptions.SectionName, options);

        Assert.False(missingResult.Succeeded);
        Assert.NotNull(missingResult.Failures);
        Assert.Contains("TransactionalEmail:PublicBaseUrl is required in Production.", missingResult.Failures);

        options.PublicBaseUrl = "http://lingua.example";
        ValidateOptionsResult insecureResult = validator.Validate(TransactionalEmailOptions.SectionName, options);

        Assert.False(insecureResult.Succeeded);
        Assert.NotNull(insecureResult.Failures);
        Assert.Contains("TransactionalEmail:PublicBaseUrl must use HTTPS in Production.", insecureResult.Failures);
    }

    private static TransactionalEmailOptions CreateBrevoOptions(bool sandboxMode) => new()
    {
        Mode = "BrevoApi",
        BrevoApiBaseUrl = "https://api.brevo.com",
        BrevoApiKey = "test-api-key",
        BrevoWebhookSecret = "test-webhook-secret",
        BrevoSandboxMode = sandboxMode,
        FromEmail = "no-reply@example.com",
        FromName = "Darwin Lingua",
        ReplyToEmail = "support@example.com",
        SupportEmail = "support@example.com",
    };

    private static TransactionalEmailMessage CreateMessage() => new(
        TransactionalEmailScenarios.AccountEmailConfirmation,
        "account.email-confirmation",
        "learner@example.com",
        "user-123",
        "en",
        "Confirm your email",
        "Plain body",
        "<p>HTML body</p>",
        "trace-123");

    private sealed class StaticHttpClientFactory(HttpClient httpClient) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => httpClient;
    }

    private sealed class CapturingHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        private readonly Dictionary<string, string[]> headers = new(StringComparer.OrdinalIgnoreCase);

        public HttpMethod? LastMethod { get; private set; }

        public Uri? LastRequestUri { get; private set; }

        public string LastRequestBody { get; private set; } = string.Empty;

        public IReadOnlyList<string> GetHeaderValues(string name) =>
            headers.TryGetValue(name, out string[]? values) ? values : [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastMethod = request.Method;
            LastRequestUri = request.RequestUri;
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                headers[header.Key] = header.Value.ToArray();
            }

            if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return response;
        }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "DarwinLingua.WebApi.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public string EnvironmentName { get; set; } = Environments.Development;

        public string WebRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }
}
