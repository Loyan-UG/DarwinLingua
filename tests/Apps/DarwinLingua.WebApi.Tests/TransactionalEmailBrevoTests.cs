using System.Net;
using System.Text;
using System.Text.Json;
using DarwinLingua.Web.Controllers;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    [Theory]
    [InlineData("hardBounce", "hard_bounce")]
    [InlineData("softBounce", "soft_bounce")]
    [InlineData("uniqueOpened", "unique_opened")]
    [InlineData("clicked", "click")]
    [InlineData("invalidEmail", "invalid_email")]
    [InlineData("spam", "spam")]
    [InlineData("Complaint", "complaint")]
    public async Task BrevoWebhook_ShouldNormalizeOfficialEventNamesBeforePersisting(
        string incomingEvent,
        string expectedStoredEvent)
    {
        CapturingEmailDeliveryLogRepository repository = new();
        BrevoTransactionalWebhookController controller = new(
            repository,
            Options.Create(CreateBrevoOptions(sandboxMode: false)),
            NullLogger<BrevoTransactionalWebhookController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext(),
            },
        };
        controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer test-webhook-secret";

        IActionResult result = await controller.Receive(
            secret: null,
            new BrevoTransactionalEmailWebhookEvent(
                incomingEvent,
                "<provider-message-id>",
                null,
                null,
                null,
                1_700_000_000,
                null,
                "provider reason",
                null,
                null),
            CancellationToken.None);

        Assert.IsType<OkResult>(result);
        Assert.Equal("<provider-message-id>", repository.ProviderMessageId);
        Assert.Equal(expectedStoredEvent, repository.ProviderEvent);
        Assert.Equal("provider reason", repository.Reason);
    }

    [Fact]
    public async Task DeliveryLogRepository_ShouldStoreDiagnosticsWithoutEmailBodyOrRecoveryUrl()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_web_email_contract");
        DbContextOptions<WebIdentityDbContext> options = new DbContextOptionsBuilder<WebIdentityDbContext>()
            .UseNpgsql(database.ConnectionString)
            .Options;

        await using WebIdentityDbContext dbContext = new(options);
        await new WebUserStateDatabaseBootstrapper(dbContext).InitializeAsync(CancellationToken.None);
        EmailDeliveryLogRepository repository = new(dbContext);
        const string recoveryUrl = "https://lingua.example/Identity/Account/ResetPassword?code=secret-reset-token";
        TransactionalEmailMessage message = new(
            TransactionalEmailScenarios.AccountPasswordReset,
            "account.password-reset",
            "learner@example.com",
            "user-123",
            "en",
            "Reset your password",
            $"Use this recovery link: {recoveryUrl}",
            $"<a href=\"{recoveryUrl}\">Reset</a>",
            "trace-123");

        WebEmailDeliveryLog log = await repository.AddQueuedAsync(message, "brevo-api", CancellationToken.None);
        await repository.MarkFailedAsync(log.Id, "provider-error", $"Provider rejected payload containing {recoveryUrl}", CancellationToken.None);

        WebEmailDeliveryLog storedLog = await dbContext.EmailDeliveryLogs.SingleAsync();
        string[] persistedTextValues =
        [
            storedLog.ScenarioKey,
            storedLog.RecipientEmailHash,
            storedLog.RecipientUserId ?? string.Empty,
            storedLog.TemplateKey,
            storedLog.Culture,
            storedLog.Subject,
            storedLog.ProviderName,
            storedLog.ProviderMessageId ?? string.Empty,
            storedLog.ProviderLastEvent ?? string.Empty,
            storedLog.ProviderLastEventReason ?? string.Empty,
            storedLog.FailureCode ?? string.Empty,
            storedLog.FailureMessageSummary ?? string.Empty,
            storedLog.CorrelationId ?? string.Empty,
        ];

        Assert.DoesNotContain(persistedTextValues, value => value.Contains("secret-reset-token", StringComparison.Ordinal));
        Assert.DoesNotContain(persistedTextValues, value => value.Contains("/ResetPassword", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(persistedTextValues, value => value.Contains(recoveryUrl, StringComparison.Ordinal));
        Assert.Equal("Provider rejected payload containing [redacted-url]", storedLog.FailureMessageSummary);
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

    [Fact]
    public void TransactionalEmailTemplateRenderer_ShouldRenderEnglishGermanAndFallbackSafely()
    {
        TransactionalEmailOptions options = CreateBrevoOptions(sandboxMode: true);
        options.ProductName = "Darwin <Lingua>";
        options.SupportEmail = "support@example.com";
        TransactionalEmailTemplateRenderer renderer = new(Options.Create(options));
        Dictionary<string, string> values = new(StringComparer.Ordinal)
        {
            ["ActionUrl"] = "https://lingua.example/confirm?code=<unsafe>",
            ["ExpirationText"] = "24 hours",
        };

        RenderedEmailTemplate english = renderer.Render(
            TransactionalEmailScenarios.AccountEmailConfirmation,
            "en-US",
            values);
        RenderedEmailTemplate german = renderer.Render(
            TransactionalEmailScenarios.AccountEmailConfirmation,
            "de-DE",
            values);
        RenderedEmailTemplate fallback = renderer.Render(
            TransactionalEmailScenarios.AccountEmailConfirmation,
            "fa",
            values);
        RenderedEmailTemplate accountDeletedEnglish = renderer.Render(
            TransactionalEmailScenarios.AccountDeleted,
            "en-US",
            new Dictionary<string, string>(StringComparer.Ordinal));
        RenderedEmailTemplate accountDeletedGerman = renderer.Render(
            TransactionalEmailScenarios.AccountDeleted,
            "de-DE",
            new Dictionary<string, string>(StringComparer.Ordinal));

        Assert.Equal("en", english.Culture);
        Assert.Contains("Confirm your Darwin Lingua email", english.Subject, StringComparison.Ordinal);
        Assert.Contains("https://lingua.example/confirm?code=<unsafe>", english.PlainTextBody, StringComparison.Ordinal);
        Assert.Contains("Darwin &lt;Lingua&gt;", english.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("code=&lt;unsafe&gt;", english.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("<!doctype html>", english.HtmlBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("email-card", english.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("box-shadow", english.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("overflow-wrap:anywhere", english.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("border-radius:999px", english.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("background:#2563eb", english.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("prefers-color-scheme: dark", english.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("transactional service email", english.HtmlBody, StringComparison.Ordinal);

        Assert.Equal("de", german.Culture);
        Assert.Contains("Bestatige deine Darwin Lingua E-Mail", german.Subject, StringComparison.Ordinal);
        Assert.Contains("Dieser Link lauft", german.PlainTextBody, StringComparison.Ordinal);
        Assert.Contains("E-Mail bestatigen", german.HtmlBody, StringComparison.Ordinal);

        Assert.Equal("en", fallback.Culture);
        Assert.Equal(english.Subject, fallback.Subject);

        Assert.Equal("en", accountDeletedEnglish.Culture);
        Assert.Contains("account was deleted", accountDeletedEnglish.Subject, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("self-service request", accountDeletedEnglish.PlainTextBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("operational records", accountDeletedEnglish.HtmlBody, StringComparison.OrdinalIgnoreCase);

        Assert.Equal("de", accountDeletedGerman.Culture);
        Assert.Contains("Konto wurde geloscht", accountDeletedGerman.Subject, StringComparison.Ordinal);
        Assert.Contains("Self-Service-Anfrage", accountDeletedGerman.PlainTextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void TransactionalEmailTemplateRenderer_ShouldRenderEveryScenarioInEnglishAndGerman()
    {
        TransactionalEmailOptions options = CreateBrevoOptions(sandboxMode: true);
        options.ProductName = "Darwin Lingua";
        options.SupportEmail = "support@example.com";
        TransactionalEmailTemplateRenderer renderer = new(Options.Create(options));
        Dictionary<string, string> values = CreateTemplateValues();

        string[] scenarioKeys = typeof(TransactionalEmailScenarios)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(static field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            .Select(static field => (string)field.GetRawConstantValue()!)
            .OrderBy(static scenario => scenario, StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(scenarioKeys);

        foreach (string scenarioKey in scenarioKeys)
        {
            RenderedEmailTemplate english = renderer.Render(scenarioKey, "en-US", values);
            RenderedEmailTemplate german = renderer.Render(scenarioKey, "de-DE", values);

            Assert.Equal("en", english.Culture);
            Assert.Equal("de", german.Culture);
            Assert.Equal(scenarioKey, english.TemplateKey);
            Assert.Equal(scenarioKey, german.TemplateKey);
            Assert.False(string.IsNullOrWhiteSpace(english.Subject));
            Assert.False(string.IsNullOrWhiteSpace(english.PlainTextBody));
            Assert.False(string.IsNullOrWhiteSpace(english.HtmlBody));
            Assert.False(string.IsNullOrWhiteSpace(german.Subject));
            Assert.False(string.IsNullOrWhiteSpace(german.PlainTextBody));
            Assert.False(string.IsNullOrWhiteSpace(german.HtmlBody));
            AssertNoUnresolvedTemplateTokens(english, values.Keys);
            AssertNoUnresolvedTemplateTokens(german, values.Keys);
        }
    }

    [Fact]
    public void BrevoProductionReadinessTool_ShouldCoverConfigurationAndOperatorGates()
    {
        string repositoryRoot = FindRepositoryRoot();
        string script = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools/Web/Invoke-BrevoProductionReadinessCheck.ps1"));
        string emailBacklog = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/73-Transactional-Email-And-Account-Communication-Backlog.md"));
        string releaseChecklist = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/61-Web-Release-Checklist.md"));

        Assert.Contains("BrevoApi", script, StringComparison.Ordinal);
        Assert.Contains("BrevoApiKey", script, StringComparison.Ordinal);
        Assert.Contains("BrevoWebhookSecret", script, StringComparison.Ordinal);
        Assert.Contains("Secret value was not printed", script, StringComparison.Ordinal);
        Assert.Contains("SenderVerified", script, StringComparison.Ordinal);
        Assert.Contains("DnsAuthenticated", script, StringComparison.Ordinal);
        Assert.Contains("WebhookConfigured", script, StringComparison.Ordinal);
        Assert.Contains("DpaAccepted", script, StringComparison.Ordinal);
        Assert.Contains("RequireRealDelivery", script, StringComparison.Ordinal);
        Assert.Contains("artifacts/validation/brevo-readiness", script, StringComparison.Ordinal);

        Assert.Contains("Invoke-BrevoProductionReadinessCheck.ps1", emailBacklog, StringComparison.Ordinal);
        Assert.Contains("Invoke-BrevoProductionReadinessCheck.ps1", releaseChecklist, StringComparison.Ordinal);
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

    private static Dictionary<string, string> CreateTemplateValues() => new(StringComparer.Ordinal)
    {
        ["ActionUrl"] = "https://lingua.example/action",
        ["AdminActor"] = "Admin User",
        ["BillingStatus"] = "active",
        ["CurrentPeriodEnd"] = "2026-07-20",
        ["DisplayName"] = "Mina",
        ["EntitlementTier"] = "Premium",
        ["EventTitle"] = "Sprachcafe",
        ["ExpirationText"] = "24 hours",
        ["FailureCount"] = "3",
        ["LastFailureCode"] = "provider-error",
        ["LastFailureScenarioKey"] = "Account.EmailConfirmation",
        ["OrganizerName"] = "Berlin Sprachschule",
        ["OrganizerProfileSlug"] = "berlin-sprachschule",
        ["Reason"] = "safety",
        ["RequesterName"] = "Mina",
        ["RsvpStatus"] = "confirmed",
        ["Status"] = "reviewed",
        ["SubscriptionId"] = "sub_123",
        ["TargetKey"] = "target-123",
        ["TargetType"] = "profile",
        ["UserId"] = "user-123",
        ["WindowMinutes"] = "15",
    };

    private static void AssertNoUnresolvedTemplateTokens(
        RenderedEmailTemplate template,
        IEnumerable<string> tokenKeys)
    {
        foreach (string tokenKey in tokenKeys)
        {
            string token = "{" + tokenKey + "}";
            Assert.DoesNotContain(token, template.Subject, StringComparison.Ordinal);
            Assert.DoesNotContain(token, template.PlainTextBody, StringComparison.Ordinal);
            Assert.DoesNotContain(token, template.HtmlBody, StringComparison.Ordinal);
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DarwinLingua.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }

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

    private sealed class CapturingEmailDeliveryLogRepository : IEmailDeliveryLogRepository
    {
        public string? ProviderMessageId { get; private set; }

        public string? ProviderEvent { get; private set; }

        public string? Reason { get; private set; }

        public Task<WebEmailDeliveryLog> AddQueuedAsync(
            TransactionalEmailMessage message,
            string providerName,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<WebEmailDeliveryLog> AddSuppressedAsync(
            TransactionalEmailMessage message,
            string providerName,
            string reason,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> IsSuppressedAsync(string recipientEmail, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkSentAsync(Guid id, string? providerMessageId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task MarkFailedAsync(
            Guid id,
            string? failureCode,
            string? failureMessageSummary,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> MarkProviderEventAsync(
            string providerMessageId,
            string providerEvent,
            DateTimeOffset providerEventAtUtc,
            string? reason,
            CancellationToken cancellationToken)
        {
            ProviderMessageId = providerMessageId;
            ProviderEvent = providerEvent;
            Reason = reason;
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<WebEmailDeliveryLog>> GetRecentAsync(
            int take,
            string? status,
            string? dialogue,
            DateTimeOffset? fromUtc,
            DateTimeOffset? toUtc,
            string? recipientHashPrefix,
            string? providerMessageId,
            string? providerEvent,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<int> DeleteOlderThanAsync(DateTimeOffset cutoffUtc, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<EmailDeliverySummary> GetSummarySinceAsync(DateTimeOffset sinceUtc, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<EmailDeliveryFailureAlertSnapshot> GetFailureAlertSnapshotAsync(
            DateTimeOffset sinceUtc,
            string excludedScenarioKey,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<EmailSuppressionSummary> GetSuppressionSummaryAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<WebEmailSuppression>> GetSuppressionsAsync(
            string? recipientHashPrefix,
            string? reason,
            int take,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> DeleteSuppressionByHashAsync(string recipientEmailHash, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
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
