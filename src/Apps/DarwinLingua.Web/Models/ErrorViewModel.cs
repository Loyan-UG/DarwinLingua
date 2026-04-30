namespace DarwinLingua.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public string Title { get; set; } = "Something went wrong";

    public string Message { get; set; } = "We could not complete this request. Please try again, or return to the learning dashboard.";

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
