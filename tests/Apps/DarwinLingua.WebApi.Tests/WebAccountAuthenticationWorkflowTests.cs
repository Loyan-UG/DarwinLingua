using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebAccountAuthenticationWorkflowTests
{
    [Fact]
    public void LoginWorkflow_ShouldNormalizeReturnUrlAndHandleConfirmedLockoutAndNotAllowedStates()
    {
        string repositoryRoot = FindRepositoryRoot();
        string loginModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Areas/Identity/Pages/Account/Login.cshtml.cs"));
        string loginPage = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Areas/Identity/Pages/Account/Login.cshtml"));

        Assert.Contains("HttpContext.SignOutAsync(IdentityConstants.ExternalScheme)", loginModel, StringComparison.Ordinal);
        Assert.Contains("NormalizeReturnUrl(ReturnUrl)", loginModel, StringComparison.Ordinal);
        Assert.Contains("Url.IsLocalUrl(ReturnUrl)", loginModel, StringComparison.Ordinal);
        Assert.Contains("userManager.FindByEmailAsync(email)", loginModel, StringComparison.Ordinal);
        Assert.Contains("userManager.CheckPasswordAsync(user, Input.Password)", loginModel, StringComparison.Ordinal);
        Assert.Contains("userManager.IsEmailConfirmedAsync(user)", loginModel, StringComparison.Ordinal);
        Assert.Contains("RedirectToPage(\"/Account/UnconfirmedAccount\"", loginModel, StringComparison.Ordinal);
        Assert.Contains("signInManager.PasswordSignInAsync(", loginModel, StringComparison.Ordinal);
        Assert.Contains("lockoutOnFailure: true", loginModel, StringComparison.Ordinal);
        Assert.Contains("result.Succeeded", loginModel, StringComparison.Ordinal);
        Assert.Contains("LocalRedirect(", loginModel, StringComparison.Ordinal);
        Assert.Contains("result.IsLockedOut", loginModel, StringComparison.Ordinal);
        Assert.Contains("SendAccountLockedAsync(", loginModel, StringComparison.Ordinal);
        Assert.Contains("result.IsNotAllowed", loginModel, StringComparison.Ordinal);
        Assert.Contains("RedirectToPage(\"/Account/CheckEmail\"", loginModel, StringComparison.Ordinal);
        Assert.Contains("autocomplete=\"email\"", loginPage, StringComparison.Ordinal);
        Assert.Contains("autocomplete=\"current-password\"", loginPage, StringComparison.Ordinal);
        Assert.Contains("Input.RememberMe", loginPage, StringComparison.Ordinal);
    }

    [Fact]
    public void LoginPartial_ShouldPostLogoutThroughIdentityDefaultUi()
    {
        string repositoryRoot = FindRepositoryRoot();
        string loginPartial = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Shared/_LoginPartial.cshtml"));
        string program = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Program.cs"));

        Assert.Contains("SignInManager.IsSignedIn(User)", loginPartial, StringComparison.Ordinal);
        Assert.Contains("asp-area=\"Identity\"", loginPartial, StringComparison.Ordinal);
        Assert.Contains("asp-page=\"/Account/Logout\"", loginPartial, StringComparison.Ordinal);
        Assert.Contains("method=\"post\"", loginPartial, StringComparison.Ordinal);
        Assert.Contains("asp-route-returnUrl=\"@Url.Action(\"Index\", \"Home\", new { area = \"\" })\"", loginPartial, StringComparison.Ordinal);
        Assert.Contains("Sign out", loginPartial, StringComparison.Ordinal);
        Assert.Contains(".AddDefaultUI()", program, StringComparison.Ordinal);
        Assert.Contains("app.MapRazorPages()", program, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfirmationAndRecoveryWorkflow_ShouldUseSafeTokensRateLimitsAndEnumerationResistantRedirects()
    {
        string repositoryRoot = FindRepositoryRoot();
        string confirmEmailModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Areas/Identity/Pages/Account/ConfirmEmail.cshtml.cs"));
        string resendConfirmationModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Areas/Identity/Pages/Account/ResendEmailConfirmation.cshtml.cs"));
        string forgotPasswordModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Areas/Identity/Pages/Account/ForgotPassword.cshtml.cs"));
        string resetPasswordModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Areas/Identity/Pages/Account/ResetPassword.cshtml.cs"));

        Assert.Contains("WebEncoders.Base64UrlDecode(code)", confirmEmailModel, StringComparison.Ordinal);
        Assert.Contains("catch (FormatException)", confirmEmailModel, StringComparison.Ordinal);
        Assert.Contains("ConfirmEmailAsync(user, token)", confirmEmailModel, StringComparison.Ordinal);

        Assert.Contains("rateLimiter.TryConsume(", resendConfirmationModel, StringComparison.Ordinal);
        Assert.Contains("\"resend-confirmation-ip\"", resendConfirmationModel, StringComparison.Ordinal);
        Assert.Contains("\"resend-confirmation\"", resendConfirmationModel, StringComparison.Ordinal);
        Assert.Contains("GenerateEmailConfirmationTokenAsync(user)", resendConfirmationModel, StringComparison.Ordinal);
        Assert.Contains("SendEmailConfirmationAsync(", resendConfirmationModel, StringComparison.Ordinal);
        Assert.Contains("RedirectToPage(\"/Account/CheckEmail\"", resendConfirmationModel, StringComparison.Ordinal);

        Assert.Contains("\"forgot-password-ip\"", forgotPasswordModel, StringComparison.Ordinal);
        Assert.Contains("\"forgot-password\"", forgotPasswordModel, StringComparison.Ordinal);
        Assert.Contains("RedirectToPage(\"/Account/ForgotPasswordConfirmation\"", forgotPasswordModel, StringComparison.Ordinal);
        Assert.Contains("FindByEmailAsync(email)", forgotPasswordModel, StringComparison.Ordinal);
        Assert.Contains("IsEmailConfirmedAsync(user)", forgotPasswordModel, StringComparison.Ordinal);
        Assert.Contains("GeneratePasswordResetTokenAsync(user)", forgotPasswordModel, StringComparison.Ordinal);
        Assert.Contains("SendPasswordResetAsync(", forgotPasswordModel, StringComparison.Ordinal);

        Assert.Contains("WebEncoders.Base64UrlDecode(Input.Code)", resetPasswordModel, StringComparison.Ordinal);
        Assert.Contains("catch (FormatException)", resetPasswordModel, StringComparison.Ordinal);
        Assert.Contains("ResetPasswordAsync(user, token, Input.Password)", resetPasswordModel, StringComparison.Ordinal);
        Assert.Contains("UpdateSecurityStampAsync(user)", resetPasswordModel, StringComparison.Ordinal);
        Assert.Contains("SendPasswordResetCompletedAsync(", resetPasswordModel, StringComparison.Ordinal);
        Assert.Contains("IsTokenErrorCode(error.Code)", resetPasswordModel, StringComparison.Ordinal);
        Assert.Contains("InvalidToken", resetPasswordModel, StringComparison.Ordinal);
        Assert.Contains("ExpiredToken", resetPasswordModel, StringComparison.Ordinal);
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
}
