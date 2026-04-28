using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record OrganizerProfileIndexPageViewModel(
    IReadOnlyList<OrganizerProfileListItemModel> Profiles);

public sealed record OrganizerProfileDetailPageViewModel(
    OrganizerProfileDetailModel Profile,
    string? StatusMessage,
    string? ErrorMessage);

public sealed record OrganizerProfileClaimPageViewModel(
    OrganizerProfileDetailModel Profile,
    OrganizerClaimInputModel Input,
    string? StatusMessage,
    string? ErrorMessage);

public sealed class OrganizerClaimInputModel
{
    [System.ComponentModel.DataAnnotations.Required]
    public string RequesterName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string RequesterEmail { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string RelationshipToOrganizer { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string EvidenceText { get; set; } = string.Empty;
}
