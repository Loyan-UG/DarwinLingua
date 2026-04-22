using System.Net.Http.Json;
using System.Text;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Configuration;
using DarwinLingua.Web.Models;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public interface IWebCatalogApiClient
{
    Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(
        string topicKey,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(
        string cefrLevel,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(
        string query,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(
        IReadOnlyList<Guid> wordIds,
        string meaningLanguageCode,
        CancellationToken cancellationToken);

    Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken);

    Task<AdminImportsPageViewModel> GetAdminImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminDraftWordsPageViewModel> GetAdminDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminHistoryPageViewModel> GetAdminHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminRollbackPageViewModel> GetAdminRollbackPreviewAsync(CancellationToken cancellationToken);
}

internal sealed class WebCatalogApiClient(HttpClient httpClient) : IWebCatalogApiClient
{
    public Task<IReadOnlyList<TopicListItemModel>> GetTopicsAsync(string uiLanguageCode, CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<TopicListItemModel>>(
            BuildPath("/api/catalog/topics", [new("uiLanguageCode", uiLanguageCode)]),
            cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> GetWordsByTopicPageAsync(
        string topicKey,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<WordListItemModel>>(
            BuildPath(
                $"/api/catalog/words/topic/{Uri.EscapeDataString(topicKey)}",
                [
                    new("meaningLanguageCode", meaningLanguageCode),
                    new("skip", skip.ToString()),
                    new("take", take.ToString())
                ]),
            cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> GetWordsByCefrPageAsync(
        string cefrLevel,
        string meaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<WordListItemModel>>(
            BuildPath(
                $"/api/catalog/words/cefr/{Uri.EscapeDataString(cefrLevel)}",
                [
                    new("meaningLanguageCode", meaningLanguageCode),
                    new("skip", skip.ToString()),
                    new("take", take.ToString())
                ]),
            cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> SearchWordsAsync(
        string query,
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetRequiredAsync<IReadOnlyList<WordListItemModel>>(
            BuildPath(
                "/api/catalog/words/search",
                [
                    new("q", query),
                    new("meaningLanguageCode", meaningLanguageCode)
                ]),
            cancellationToken);

    public Task<WordDetailModel?> GetWordDetailsAsync(
        Guid publicId,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        string uiLanguageCode,
        CancellationToken cancellationToken) =>
        GetAsync<WordDetailModel>(
            BuildPath(
                $"/api/catalog/words/{publicId:D}",
                [
                    new("primaryMeaningLanguageCode", primaryMeaningLanguageCode),
                    new("secondaryMeaningLanguageCode", secondaryMeaningLanguageCode),
                    new("uiLanguageCode", uiLanguageCode)
                ]),
            cancellationToken);

    public Task<IReadOnlyList<WordListItemModel>> GetWordsByIdsAsync(
        IReadOnlyList<Guid> wordIds,
        string meaningLanguageCode,
        CancellationToken cancellationToken) =>
        PostRequiredAsync<CatalogWordLookupRequest, IReadOnlyList<WordListItemModel>>(
            "/api/catalog/words/by-ids",
            new CatalogWordLookupRequest(wordIds, meaningLanguageCode),
            cancellationToken);

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync(CancellationToken cancellationToken)
    {
        AdminCatalogDashboardResponse response = await GetRequiredAsync<AdminCatalogDashboardResponse>(
            "/api/admin/catalog/dashboard",
            cancellationToken).ConfigureAwait(false);

        return new AdminDashboardViewModel(
            response.ActiveWordCount,
            response.DraftWordCount,
            response.TotalTopicCount,
            response.ImportedPackageCount,
            response.FailedPackageCount,
            response.LastImportAtUtc);
    }

    public async Task<AdminImportsPageViewModel> GetAdminImportsAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        AdminCatalogImportsResponse response = await GetRequiredAsync<AdminCatalogImportsResponse>(
            BuildPath("/api/admin/catalog/imports", [new("status", statusFilter)]),
            cancellationToken).ConfigureAwait(false);

        return new AdminImportsPageViewModel(
            response.StatusFilter,
            response.Packages
                .Select(package => new AdminContentPackageListItemViewModel(
                    package.PackageId,
                    package.PackageVersion,
                    package.PackageName,
                    package.SourceType,
                    package.Status,
                    package.TotalEntries,
                    package.InsertedEntries,
                    package.InvalidEntries,
                    package.WarningCount,
                    package.CreatedAtUtc))
                .ToArray());
    }

    public async Task<AdminDraftWordsPageViewModel> GetAdminDraftWordsAsync(string? query, CancellationToken cancellationToken)
    {
        AdminCatalogDraftWordsResponse response = await GetRequiredAsync<AdminCatalogDraftWordsResponse>(
            BuildPath("/api/admin/catalog/draft-words", [new("q", query)]),
            cancellationToken).ConfigureAwait(false);

        return new AdminDraftWordsPageViewModel(
            response.Query,
            response.Words
                .Select(word => new AdminDraftWordListItemViewModel(
                    word.PublicId,
                    word.Lemma,
                    word.PartOfSpeech,
                    word.CefrLevel,
                    word.PublicationStatus))
                .ToArray());
    }

    public async Task<AdminHistoryPageViewModel> GetAdminHistoryAsync(string? statusFilter, CancellationToken cancellationToken)
    {
        AdminCatalogHistoryViewResponse response = await GetRequiredAsync<AdminCatalogHistoryViewResponse>(
            BuildPath("/api/admin/catalog/history-view", [new("status", statusFilter)]),
            cancellationToken).ConfigureAwait(false);

        return new AdminHistoryPageViewModel(
            response.StatusFilter,
            response.Items
                .Select(item => new AdminHistoryItemViewModel(
                    item.PackageId,
                    item.PackageVersion,
                    item.Status,
                    item.TotalEntries,
                    item.InsertedEntries,
                    item.InvalidEntries,
                    item.CreatedAtUtc))
                .ToArray());
    }

    public async Task<AdminRollbackPageViewModel> GetAdminRollbackPreviewAsync(CancellationToken cancellationToken)
    {
        AdminCatalogRollbackPreviewResponse response = await GetRequiredAsync<AdminCatalogRollbackPreviewResponse>(
            "/api/admin/catalog/rollback-preview",
            cancellationToken).ConfigureAwait(false);

        return new AdminRollbackPageViewModel(
            response.DraftWordCount,
            response.ImportedPackageCount,
            response.WarningMessage);
    }

    private async Task<T?> GetAsync<T>(string relativeUri, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(relativeUri, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, relativeUri, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken).ConfigureAwait(false);
    }

    private async Task<T> GetRequiredAsync<T>(string relativeUri, CancellationToken cancellationToken)
    {
        T? response = await GetAsync<T>(relativeUri, cancellationToken).ConfigureAwait(false);
        return response ?? throw new InvalidOperationException($"The Web API returned an empty payload for '{relativeUri}'.");
    }

    private async Task<TResponse> PostRequiredAsync<TRequest, TResponse>(
        string relativeUri,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(relativeUri, request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, relativeUri, cancellationToken).ConfigureAwait(false);

        TResponse? payload = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken).ConfigureAwait(false);
        return payload ?? throw new InvalidOperationException($"The Web API returned an empty payload for '{relativeUri}'.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string relativeUri,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException(
            $"The Web API call to '{relativeUri}' failed with status {(int)response.StatusCode}. Response: {body}");
    }

    private static string BuildPath(string path, IEnumerable<KeyValuePair<string, string?>> queryParameters)
    {
        StringBuilder builder = new(path);
        bool isFirst = true;

        foreach ((string key, string? value) in queryParameters)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            builder.Append(isFirst ? '?' : '&');
            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
            isFirst = false;
        }

        return builder.ToString();
    }
}

internal static class WebCatalogApiClientRegistration
{
    public static IServiceCollection AddWebCatalogApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<WebApiOptions>()
            .Bind(configuration.GetSection(WebApiOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "WebApi:BaseUrl must be an absolute URL.")
            .ValidateOnStart();

        services.AddHttpClient<IWebCatalogApiClient, WebCatalogApiClient>((serviceProvider, client) =>
        {
            WebApiOptions options = serviceProvider.GetRequiredService<IOptions<WebApiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            WebApiOptions options = serviceProvider.GetRequiredService<IOptions<WebApiOptions>>().Value;
            HttpClientHandler handler = new();

            if (options.IgnoreSslErrors)
            {
                handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        });

        return services;
    }
}

internal sealed record CatalogWordLookupRequest(
    IReadOnlyList<Guid> WordIds,
    string MeaningLanguageCode);

internal sealed record AdminCatalogDashboardResponse(
    int ActiveWordCount,
    int DraftWordCount,
    int TotalTopicCount,
    int ImportedPackageCount,
    int FailedPackageCount,
    DateTime? LastImportAtUtc);

internal sealed record AdminCatalogImportsResponse(
    string? StatusFilter,
    IReadOnlyList<AdminCatalogImportItemResponse> Packages);

internal sealed record AdminCatalogImportItemResponse(
    string PackageId,
    string PackageVersion,
    string PackageName,
    string SourceType,
    string Status,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    int WarningCount,
    DateTime CreatedAtUtc);

internal sealed record AdminCatalogDraftWordsResponse(
    string? Query,
    IReadOnlyList<AdminCatalogDraftWordItemResponse> Words);

internal sealed record AdminCatalogDraftWordItemResponse(
    Guid PublicId,
    string Lemma,
    string PartOfSpeech,
    string CefrLevel,
    string PublicationStatus);

internal sealed record AdminCatalogHistoryViewResponse(
    string? StatusFilter,
    IReadOnlyList<AdminCatalogHistoryItemResponse> Items);

internal sealed record AdminCatalogHistoryItemResponse(
    string PackageId,
    string PackageVersion,
    string Status,
    int TotalEntries,
    int InsertedEntries,
    int InvalidEntries,
    DateTime CreatedAtUtc);

internal sealed record AdminCatalogRollbackPreviewResponse(
    int DraftWordCount,
    int ImportedPackageCount,
    string WarningMessage);
