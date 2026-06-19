using System.Net;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public interface IEmailTemplateRenderer
{
    RenderedEmailTemplate Render(
        string templateKey,
        string? culture,
        IReadOnlyDictionary<string, string> values);
}

public sealed class TransactionalEmailTemplateRenderer(IOptions<TransactionalEmailOptions> options)
    : IEmailTemplateRenderer
{
    private static readonly Dictionary<string, Dictionary<string, EmailTemplateDefinition>> Templates =
        new(StringComparer.Ordinal)
        {
            [TransactionalEmailScenarios.AccountEmailConfirmation] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Confirm your Darwin Lingua email",
                    "Confirm your email for {ProductName} by opening this link: {ActionUrl}\n\nThis link expires in {ExpirationText}. If you did not create this account, you can ignore this email.\n\nNeed help? Contact {SupportEmail}.",
                    "<p>Confirm your email for <strong>{ProductName}</strong>.</p><p><a href=\"{ActionUrl}\">Confirm email</a></p><p>This link expires in {ExpirationText}.</p><p>If the button does not work, copy this URL: {ActionUrl}</p><p>Need help? Contact {SupportEmail}.</p>"),
                ["de"] = new(
                    "Bestatige deine Darwin Lingua E-Mail",
                    "Bestatige deine E-Mail fur {ProductName}, indem du diesen Link offnest: {ActionUrl}\n\nDieser Link lauft in {ExpirationText} ab. Wenn du dieses Konto nicht erstellt hast, ignoriere diese E-Mail.\n\nBrauchst du Hilfe? Kontaktiere {SupportEmail}.",
                    "<p>Bestatige deine E-Mail fur <strong>{ProductName}</strong>.</p><p><a href=\"{ActionUrl}\">E-Mail bestatigen</a></p><p>Dieser Link lauft in {ExpirationText} ab.</p><p>Falls der Button nicht funktioniert, kopiere diese URL: {ActionUrl}</p><p>Brauchst du Hilfe? Kontaktiere {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.AccountPasswordReset] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Reset your Darwin Lingua password",
                    "Reset your {ProductName} password by opening this link: {ActionUrl}\n\nThis link expires in {ExpirationText}. If you did not request a reset, you can ignore this email.\n\nNeed help? Contact {SupportEmail}.",
                    "<p>Reset your <strong>{ProductName}</strong> password.</p><p><a href=\"{ActionUrl}\">Reset password</a></p><p>This link expires in {ExpirationText}.</p><p>If the button does not work, copy this URL: {ActionUrl}</p><p>Need help? Contact {SupportEmail}.</p>"),
                ["de"] = new(
                    "Setze dein Darwin Lingua Passwort zuruck",
                    "Setze dein {ProductName} Passwort zuruck, indem du diesen Link offnest: {ActionUrl}\n\nDieser Link lauft in {ExpirationText} ab. Wenn du das nicht angefordert hast, ignoriere diese E-Mail.\n\nBrauchst du Hilfe? Kontaktiere {SupportEmail}.",
                    "<p>Setze dein <strong>{ProductName}</strong> Passwort zuruck.</p><p><a href=\"{ActionUrl}\">Passwort zurucksetzen</a></p><p>Dieser Link lauft in {ExpirationText} ab.</p><p>Falls der Button nicht funktioniert, kopiere diese URL: {ActionUrl}</p><p>Brauchst du Hilfe? Kontaktiere {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.AccountPasswordResetCompleted] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your Darwin Lingua password was changed",
                    "Your {ProductName} password was changed. If this was not you, contact {SupportEmail} immediately.",
                    "<p>Your <strong>{ProductName}</strong> password was changed.</p><p>If this was not you, contact {SupportEmail} immediately.</p>"),
                ["de"] = new(
                    "Dein Darwin Lingua Passwort wurde geandert",
                    "Dein {ProductName} Passwort wurde geandert. Wenn du das nicht warst, kontaktiere sofort {SupportEmail}.",
                    "<p>Dein <strong>{ProductName}</strong> Passwort wurde geandert.</p><p>Wenn du das nicht warst, kontaktiere sofort {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.AccountPasswordChanged] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your Darwin Lingua password was changed",
                    "Your {ProductName} password was changed from account settings. If this was not you, contact {SupportEmail} immediately.",
                    "<p>Your <strong>{ProductName}</strong> password was changed from account settings.</p><p>If this was not you, contact {SupportEmail} immediately.</p>"),
                ["de"] = new(
                    "Dein Darwin Lingua Passwort wurde geandert",
                    "Dein {ProductName} Passwort wurde in den Kontoeinstellungen geandert. Wenn du das nicht warst, kontaktiere sofort {SupportEmail}.",
                    "<p>Dein <strong>{ProductName}</strong> Passwort wurde in den Kontoeinstellungen geandert.</p><p>Wenn du das nicht warst, kontaktiere sofort {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.AccountLocked] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your Darwin Lingua account was temporarily locked",
                    "Your {ProductName} account was temporarily locked after repeated failed sign-in attempts. If this was not you, reset your password or contact {SupportEmail}.",
                    "<p>Your <strong>{ProductName}</strong> account was temporarily locked after repeated failed sign-in attempts.</p><p>If this was not you, reset your password or contact {SupportEmail}.</p>"),
                ["de"] = new(
                    "Dein Darwin Lingua Konto wurde vorubergehend gesperrt",
                    "Dein {ProductName} Konto wurde nach mehreren fehlgeschlagenen Anmeldeversuchen vorubergehend gesperrt. Wenn du das nicht warst, setze dein Passwort zuruck oder kontaktiere {SupportEmail}.",
                    "<p>Dein <strong>{ProductName}</strong> Konto wurde nach mehreren fehlgeschlagenen Anmeldeversuchen vorubergehend gesperrt.</p><p>Wenn du das nicht warst, setze dein Passwort zuruck oder kontaktiere {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.AccountEmailChangeConfirmation] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Confirm your new Darwin Lingua email",
                    "Confirm this new email address for {ProductName}: {ActionUrl}\n\nThis link expires in {ExpirationText}.",
                    "<p>Confirm this new email address for <strong>{ProductName}</strong>.</p><p><a href=\"{ActionUrl}\">Confirm new email</a></p><p>This link expires in {ExpirationText}.</p>"),
                ["de"] = new(
                    "Bestatige deine neue Darwin Lingua E-Mail",
                    "Bestatige diese neue E-Mail-Adresse fur {ProductName}: {ActionUrl}\n\nDieser Link lauft in {ExpirationText} ab.",
                    "<p>Bestatige diese neue E-Mail-Adresse fur <strong>{ProductName}</strong>.</p><p><a href=\"{ActionUrl}\">Neue E-Mail bestatigen</a></p><p>Dieser Link lauft in {ExpirationText} ab.</p>"),
            },
            [TransactionalEmailScenarios.AccountEmailChangedNotification] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your Darwin Lingua email was changed",
                    "Your {ProductName} account email was changed. If this was not you, contact {SupportEmail} immediately.",
                    "<p>Your <strong>{ProductName}</strong> account email was changed.</p><p>If this was not you, contact {SupportEmail} immediately.</p>"),
                ["de"] = new(
                    "Deine Darwin Lingua E-Mail wurde geandert",
                    "Die E-Mail-Adresse deines {ProductName} Kontos wurde geandert. Wenn du das nicht warst, kontaktiere sofort {SupportEmail}.",
                    "<p>Die E-Mail-Adresse deines <strong>{ProductName}</strong> Kontos wurde geandert.</p><p>Wenn du das nicht warst, kontaktiere sofort {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.OrganizerClaimSubmitted] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Organizer claim received",
                    "{ProductName} received your claim for {OrganizerName}. An operator will review it and contact you if more information is needed.\n\nNeed help? Contact {SupportEmail}.",
                    "<p><strong>{ProductName}</strong> received your claim for <strong>{OrganizerName}</strong>.</p><p>An operator will review it and contact you if more information is needed.</p><p>Need help? Contact {SupportEmail}.</p>"),
                ["de"] = new(
                    "Organizer-Anfrage erhalten",
                    "{ProductName} hat deine Anfrage fur {OrganizerName} erhalten. Ein Operator pruft sie und meldet sich, falls weitere Informationen notig sind.\n\nBrauchst du Hilfe? Kontaktiere {SupportEmail}.",
                    "<p><strong>{ProductName}</strong> hat deine Anfrage fur <strong>{OrganizerName}</strong> erhalten.</p><p>Ein Operator pruft sie und meldet sich, falls weitere Informationen notig sind.</p><p>Brauchst du Hilfe? Kontaktiere {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.AdminNewOrganizerClaim] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "New organizer claim needs review",
                    "A new organizer claim was submitted for {OrganizerName} by {RequesterName}. Open the admin organizer queue to review it.",
                    "<p>A new organizer claim was submitted for <strong>{OrganizerName}</strong> by <strong>{RequesterName}</strong>.</p><p>Open the admin organizer queue to review it.</p>"),
                ["de"] = new(
                    "Neue Organizer-Anfrage zur Prufung",
                    "Eine neue Organizer-Anfrage fur {OrganizerName} wurde von {RequesterName} eingereicht. Offne die Admin-Organizer-Warteschlange zur Prufung.",
                    "<p>Eine neue Organizer-Anfrage fur <strong>{OrganizerName}</strong> wurde von <strong>{RequesterName}</strong> eingereicht.</p><p>Offne die Admin-Organizer-Warteschlange zur Prufung.</p>"),
            },
            [TransactionalEmailScenarios.AdminEmailDeliveryFailureAlert] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Email delivery failures need attention",
                    "{FailureCount} transactional email deliveries failed in the last {WindowMinutes} minutes. Latest failure: {LastFailureScenarioKey} / {LastFailureCode}. Open admin email diagnostics.",
                    "<p><strong>{FailureCount}</strong> transactional email deliveries failed in the last <strong>{WindowMinutes}</strong> minutes.</p><p>Latest failure: <strong>{LastFailureScenarioKey}</strong> / <strong>{LastFailureCode}</strong>.</p><p>Open admin email diagnostics.</p>"),
                ["de"] = new(
                    "E-Mail-Zustellfehler benotigen Aufmerksamkeit",
                    "{FailureCount} transaktionale E-Mails sind in den letzten {WindowMinutes} Minuten fehlgeschlagen. Letzter Fehler: {LastFailureScenarioKey} / {LastFailureCode}. Offne die Admin-E-Mail-Diagnose.",
                    "<p><strong>{FailureCount}</strong> transaktionale E-Mails sind in den letzten <strong>{WindowMinutes}</strong> Minuten fehlgeschlagen.</p><p>Letzter Fehler: <strong>{LastFailureScenarioKey}</strong> / <strong>{LastFailureCode}</strong>.</p><p>Offne die Admin-E-Mail-Diagnose.</p>"),
            },
            [TransactionalEmailScenarios.BillingPremiumActivated] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your Darwin Lingua Premium access is active",
                    "Your {ProductName} Premium access is active. Billing status: {BillingStatus}. Current period end: {CurrentPeriodEnd}.\n\nYou can manage your subscription from Billing in your account.",
                    "<p>Your <strong>{ProductName}</strong> Premium access is active.</p><p>Billing status: <strong>{BillingStatus}</strong><br />Current period end: <strong>{CurrentPeriodEnd}</strong></p><p>You can manage your subscription from Billing in your account.</p>"),
                ["de"] = new(
                    "Dein Darwin Lingua Premium-Zugang ist aktiv",
                    "Dein {ProductName} Premium-Zugang ist aktiv. Abrechnungsstatus: {BillingStatus}. Aktueller Zeitraum endet: {CurrentPeriodEnd}.\n\nDu kannst dein Abo im Konto unter Billing verwalten.",
                    "<p>Dein <strong>{ProductName}</strong> Premium-Zugang ist aktiv.</p><p>Abrechnungsstatus: <strong>{BillingStatus}</strong><br />Aktueller Zeitraum endet: <strong>{CurrentPeriodEnd}</strong></p><p>Du kannst dein Abo im Konto unter Billing verwalten.</p>"),
            },
            [TransactionalEmailScenarios.BillingPaymentActionNeeded] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your Darwin Lingua payment needs attention",
                    "Your {ProductName} billing status is {BillingStatus}. Current period end: {CurrentPeriodEnd}. Please open Billing in your account to update payment details if needed.",
                    "<p>Your <strong>{ProductName}</strong> billing status is <strong>{BillingStatus}</strong>.</p><p>Current period end: <strong>{CurrentPeriodEnd}</strong></p><p>Please open Billing in your account to update payment details if needed.</p>"),
                ["de"] = new(
                    "Deine Darwin Lingua Zahlung braucht Aufmerksamkeit",
                    "Dein {ProductName} Abrechnungsstatus ist {BillingStatus}. Aktueller Zeitraum endet: {CurrentPeriodEnd}. Offne Billing in deinem Konto, um Zahlungsdaten bei Bedarf zu aktualisieren.",
                    "<p>Dein <strong>{ProductName}</strong> Abrechnungsstatus ist <strong>{BillingStatus}</strong>.</p><p>Aktueller Zeitraum endet: <strong>{CurrentPeriodEnd}</strong></p><p>Offne Billing in deinem Konto, um Zahlungsdaten bei Bedarf zu aktualisieren.</p>"),
            },
            [TransactionalEmailScenarios.BillingPremiumEnded] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your Darwin Lingua Premium access changed",
                    "Your {ProductName} Premium access is no longer active. Billing status: {BillingStatus}. You can continue using free learning features and restart Premium from Billing.",
                    "<p>Your <strong>{ProductName}</strong> Premium access is no longer active.</p><p>Billing status: <strong>{BillingStatus}</strong></p><p>You can continue using free learning features and restart Premium from Billing.</p>"),
                ["de"] = new(
                    "Dein Darwin Lingua Premium-Zugang hat sich geandert",
                    "Dein {ProductName} Premium-Zugang ist nicht mehr aktiv. Abrechnungsstatus: {BillingStatus}. Du kannst die kostenlosen Lernfunktionen weiter nutzen und Premium unter Billing neu starten.",
                    "<p>Dein <strong>{ProductName}</strong> Premium-Zugang ist nicht mehr aktiv.</p><p>Abrechnungsstatus: <strong>{BillingStatus}</strong></p><p>Du kannst die kostenlosen Lernfunktionen weiter nutzen und Premium unter Billing neu starten.</p>"),
            },
            [TransactionalEmailScenarios.AdminBillingReconciliationCompleted] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Stripe subscription reconciliation completed",
                    "{AdminActor} reconciled Stripe subscription {SubscriptionId}. User: {UserId}. Stripe status: {BillingStatus}. Entitlement: {EntitlementTier}.",
                    "<p><strong>{AdminActor}</strong> reconciled Stripe subscription <strong>{SubscriptionId}</strong>.</p><p>User: <strong>{UserId}</strong><br />Stripe status: <strong>{BillingStatus}</strong><br />Entitlement: <strong>{EntitlementTier}</strong></p>"),
                ["de"] = new(
                    "Stripe-Abo-Abgleich abgeschlossen",
                    "{AdminActor} hat Stripe-Abo {SubscriptionId} abgeglichen. Nutzer: {UserId}. Stripe-Status: {BillingStatus}. Entitlement: {EntitlementTier}.",
                    "<p><strong>{AdminActor}</strong> hat Stripe-Abo <strong>{SubscriptionId}</strong> abgeglichen.</p><p>Nutzer: <strong>{UserId}</strong><br />Stripe-Status: <strong>{BillingStatus}</strong><br />Entitlement: <strong>{EntitlementTier}</strong></p>"),
            },
            [TransactionalEmailScenarios.OrganizerClaimApproved] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your organizer claim was approved",
                    "Your claim for {OrganizerName} was approved. You can now continue with organizer workflows in {ProductName}.\n\nNeed help? Contact {SupportEmail}.",
                    "<p>Your claim for <strong>{OrganizerName}</strong> was approved.</p><p>You can now continue with organizer workflows in <strong>{ProductName}</strong>.</p><p>Need help? Contact {SupportEmail}.</p>"),
                ["de"] = new(
                    "Deine Organizer-Anfrage wurde genehmigt",
                    "Deine Anfrage fur {OrganizerName} wurde genehmigt. Du kannst jetzt mit den Organizer-Workflows in {ProductName} fortfahren.\n\nBrauchst du Hilfe? Kontaktiere {SupportEmail}.",
                    "<p>Deine Anfrage fur <strong>{OrganizerName}</strong> wurde genehmigt.</p><p>Du kannst jetzt mit den Organizer-Workflows in <strong>{ProductName}</strong> fortfahren.</p><p>Brauchst du Hilfe? Kontaktiere {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.OrganizerClaimRejected] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your organizer claim was reviewed",
                    "Your claim for {OrganizerName} was reviewed and could not be approved at this time. You can contact {SupportEmail} if you believe this needs another review.",
                    "<p>Your claim for <strong>{OrganizerName}</strong> was reviewed and could not be approved at this time.</p><p>Contact {SupportEmail} if you believe this needs another review.</p>"),
                ["de"] = new(
                    "Deine Organizer-Anfrage wurde gepruft",
                    "Deine Anfrage fur {OrganizerName} wurde gepruft und kann derzeit nicht genehmigt werden. Kontaktiere {SupportEmail}, wenn du eine erneute Prufung benotigst.",
                    "<p>Deine Anfrage fur <strong>{OrganizerName}</strong> wurde gepruft und kann derzeit nicht genehmigt werden.</p><p>Kontaktiere {SupportEmail}, wenn du eine erneute Prufung benotigst.</p>"),
            },
            [TransactionalEmailScenarios.OrganizerProfileOwnershipChanged] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Organizer profile access updated",
                    "You were assigned as an owner for organizer profile {OrganizerProfileSlug} in {ProductName}. If this looks wrong, contact {SupportEmail}.",
                    "<p>You were assigned as an owner for organizer profile <strong>{OrganizerProfileSlug}</strong> in <strong>{ProductName}</strong>.</p><p>If this looks wrong, contact {SupportEmail}.</p>"),
                ["de"] = new(
                    "Organizer-Profilzugriff aktualisiert",
                    "Du wurdest als Owner fur das Organizer-Profil {OrganizerProfileSlug} in {ProductName} eingetragen. Wenn das nicht stimmt, kontaktiere {SupportEmail}.",
                    "<p>Du wurdest als Owner fur das Organizer-Profil <strong>{OrganizerProfileSlug}</strong> in <strong>{ProductName}</strong> eingetragen.</p><p>Wenn das nicht stimmt, kontaktiere {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.EventRsvpConfirmation] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your event RSVP was saved",
                    "Your RSVP for {EventTitle} is saved as {RsvpStatus}.\n\nNeed help? Contact {SupportEmail}.",
                    "<p>Your RSVP for <strong>{EventTitle}</strong> is saved as <strong>{RsvpStatus}</strong>.</p><p>Need help? Contact {SupportEmail}.</p>"),
                ["de"] = new(
                    "Deine Event-RSVP wurde gespeichert",
                    "Deine RSVP fur {EventTitle} wurde als {RsvpStatus} gespeichert.\n\nBrauchst du Hilfe? Kontaktiere {SupportEmail}.",
                    "<p>Deine RSVP fur <strong>{EventTitle}</strong> wurde als <strong>{RsvpStatus}</strong> gespeichert.</p><p>Brauchst du Hilfe? Kontaktiere {SupportEmail}.</p>"),
            },
            [TransactionalEmailScenarios.PartnerRequestAccepted] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your partner request was accepted",
                    "{DisplayName} accepted your partner request in {ProductName}. You can now see the contact email in your partner requests.",
                    "<p><strong>{DisplayName}</strong> accepted your partner request in <strong>{ProductName}</strong>.</p><p>You can now see the contact email in your partner requests.</p>"),
                ["de"] = new(
                    "Deine Partneranfrage wurde angenommen",
                    "{DisplayName} hat deine Partneranfrage in {ProductName} angenommen. Du siehst die Kontakt-E-Mail jetzt in deinen Partneranfragen.",
                    "<p><strong>{DisplayName}</strong> hat deine Partneranfrage in <strong>{ProductName}</strong> angenommen.</p><p>Du siehst die Kontakt-E-Mail jetzt in deinen Partneranfragen.</p>"),
            },
            [TransactionalEmailScenarios.ModerationHighSeverityReport] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "High-severity report needs review",
                    "A high-severity report was submitted. Reason: {Reason}. Target: {TargetType}/{TargetKey}. Open moderation in the admin area.",
                    "<p>A high-severity report was submitted.</p><p>Reason: <strong>{Reason}</strong><br />Target: <strong>{TargetType}/{TargetKey}</strong></p><p>Open moderation in the admin area.</p>"),
                ["de"] = new(
                    "Wichtige Meldung zur Prufung",
                    "Eine wichtige Meldung wurde eingereicht. Grund: {Reason}. Ziel: {TargetType}/{TargetKey}. Offne Moderation im Admin-Bereich.",
                    "<p>Eine wichtige Meldung wurde eingereicht.</p><p>Grund: <strong>{Reason}</strong><br />Ziel: <strong>{TargetType}/{TargetKey}</strong></p><p>Offne Moderation im Admin-Bereich.</p>"),
            },
            [TransactionalEmailScenarios.ModerationReportOutcome] = new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = new(
                    "Your report was reviewed",
                    "Your {ProductName} report about {TargetType} was reviewed and is now marked {Status}. Thank you for helping keep the community safe.",
                    "<p>Your <strong>{ProductName}</strong> report about <strong>{TargetType}</strong> was reviewed and is now marked <strong>{Status}</strong>.</p><p>Thank you for helping keep the community safe.</p>"),
                ["de"] = new(
                    "Deine Meldung wurde gepruft",
                    "Deine {ProductName} Meldung zu {TargetType} wurde gepruft und ist jetzt als {Status} markiert. Danke, dass du die Community sicherer machst.",
                    "<p>Deine <strong>{ProductName}</strong> Meldung zu <strong>{TargetType}</strong> wurde gepruft und ist jetzt als <strong>{Status}</strong> markiert.</p><p>Danke, dass du die Community sicherer machst.</p>"),
            },
        };

    public RenderedEmailTemplate Render(
        string templateKey,
        string? culture,
        IReadOnlyDictionary<string, string> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentNullException.ThrowIfNull(values);

        if (!Templates.TryGetValue(templateKey, out Dictionary<string, EmailTemplateDefinition>? localizedTemplates))
        {
            throw new InvalidOperationException($"Email template '{templateKey}' is not registered.");
        }

        string normalizedCulture = NormalizeCulture(culture);
        if (!localizedTemplates.TryGetValue(normalizedCulture, out EmailTemplateDefinition? template))
        {
            normalizedCulture = "en";
            template = localizedTemplates[normalizedCulture];
        }

        Dictionary<string, string> mergedValues = new(values, StringComparer.Ordinal)
        {
            ["ProductName"] = options.Value.ProductName,
            ["SupportEmail"] = options.Value.SupportEmail,
        };

        return new RenderedEmailTemplate(
            templateKey,
            normalizedCulture,
            ApplyValues(template.Subject, mergedValues, encodeHtml: false),
            ApplyValues(template.PlainTextBody, mergedValues, encodeHtml: false),
            BuildHtmlDocument(
                ApplyValues(template.Subject, mergedValues, encodeHtml: false),
                ApplyValues(template.HtmlBody, mergedValues, encodeHtml: true),
                options.Value));
    }

    private static string NormalizeCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return "en";
        }

        string normalized = culture.Trim();
        int separatorIndex = normalized.IndexOfAny(['-', '_']);
        if (separatorIndex > 0)
        {
            normalized = normalized[..separatorIndex];
        }

        return string.Equals(normalized, "de", StringComparison.OrdinalIgnoreCase) ? "de" : "en";
    }

    private static string ApplyValues(
        string template,
        IReadOnlyDictionary<string, string> values,
        bool encodeHtml)
    {
        string rendered = template;
        foreach (KeyValuePair<string, string> value in values)
        {
            string replacement = encodeHtml
                ? WebUtility.HtmlEncode(value.Value)
                : value.Value;
            rendered = rendered.Replace("{" + value.Key + "}", replacement, StringComparison.Ordinal);
        }

        return rendered;
    }

    private static string BuildHtmlDocument(
        string subject,
        string htmlBody,
        TransactionalEmailOptions emailOptions)
    {
        string safeSubject = WebUtility.HtmlEncode(subject);
        string safeProductName = WebUtility.HtmlEncode(emailOptions.ProductName);
        string safeSupportEmail = WebUtility.HtmlEncode(emailOptions.SupportEmail);

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <meta name="color-scheme" content="light dark">
              <meta name="supported-color-schemes" content="light dark">
              <title>{{safeSubject}}</title>
              <style>
                @media (prefers-color-scheme: dark) {
                  body, .email-shell { background: #111827 !important; }
                  .email-card { background: #1f2937 !important; border-color: #374151 !important; }
                  .email-title, .email-content, .email-footer { color: #f9fafb !important; }
                  .email-muted { color: #d1d5db !important; }
                }
              </style>
            </head>
            <body style="margin:0;padding:0;background:#f3f4f6;color:#111827;font-family:Arial,'Helvetica Neue',Helvetica,sans-serif;">
              <div class="email-shell" style="width:100%;background:#f3f4f6;padding:32px 12px;">
                <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">{{safeSubject}}</div>
                <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;">
                  <tr>
                    <td align="center">
                      <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;max-width:640px;">
                        <tr>
                          <td style="padding:0 0 16px 0;text-align:left;">
                            <div style="font-size:13px;line-height:20px;color:#4b5563;font-weight:700;letter-spacing:.08em;text-transform:uppercase;">{{safeProductName}}</div>
                          </td>
                        </tr>
                        <tr>
                          <td class="email-card" style="background:#ffffff;border:1px solid #e5e7eb;border-radius:16px;padding:32px;box-shadow:0 14px 40px rgba(17,24,39,.08);">
                            <h1 class="email-title" style="margin:0 0 20px 0;color:#111827;font-size:24px;line-height:32px;font-weight:800;">{{safeSubject}}</h1>
                            <div class="email-content" style="color:#1f2937;font-size:16px;line-height:26px;">
                              {{htmlBody}}
                            </div>
                          </td>
                        </tr>
                        <tr>
                          <td class="email-footer" style="padding:20px 4px 0 4px;color:#6b7280;font-size:13px;line-height:20px;">
                            <p class="email-muted" style="margin:0;">This is a transactional service email from {{safeProductName}}. Need help? Contact {{safeSupportEmail}}.</p>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </div>
            </body>
            </html>
            """;
    }

    private sealed record EmailTemplateDefinition(string Subject, string PlainTextBody, string HtmlBody);
}
