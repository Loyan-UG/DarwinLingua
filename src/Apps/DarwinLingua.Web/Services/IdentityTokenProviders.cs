using DarwinLingua.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public static class DarwinLinguaIdentityTokenProviders
{
    public const string EmailConfirmation = "DarwinLinguaEmailConfirmation";
    public const string PasswordReset = "DarwinLinguaPasswordReset";
    public const string EmailChange = "DarwinLinguaEmailChange";
}

public sealed class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions;

public sealed class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions;

public sealed class EmailChangeTokenProviderOptions : DataProtectionTokenProviderOptions;

public sealed class EmailConfirmationTokenProvider(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<EmailConfirmationTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<DarwinLinguaIdentityUser>> logger)
    : DataProtectorTokenProvider<DarwinLinguaIdentityUser>(dataProtectionProvider, options, logger);

public sealed class PasswordResetTokenProvider(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<PasswordResetTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<DarwinLinguaIdentityUser>> logger)
    : DataProtectorTokenProvider<DarwinLinguaIdentityUser>(dataProtectionProvider, options, logger);

public sealed class EmailChangeTokenProvider(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<EmailChangeTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<DarwinLinguaIdentityUser>> logger)
    : DataProtectorTokenProvider<DarwinLinguaIdentityUser>(dataProtectionProvider, options, logger);
