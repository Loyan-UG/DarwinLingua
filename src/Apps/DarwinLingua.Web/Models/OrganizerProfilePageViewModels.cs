using System.ComponentModel.DataAnnotations;
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
    [Required]
    [StringLength(128)]
    public string RequesterName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string RequesterEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string RelationshipToOrganizer { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string EvidenceText { get; set; } = string.Empty;
}
