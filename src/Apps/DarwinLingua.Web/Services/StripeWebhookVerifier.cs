using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class StripeWebhookVerifier(IOptions<BillingOptions> options)
{
    public bool IsValid(string payload, string? signatureHeader, DateTimeOffset nowUtc)
    {
        if (string.IsNullOrWhiteSpace(payload) || string.IsNullOrWhiteSpace(signatureHeader))
        {
            return false;
        }

        BillingOptions billingOptions = options.Value;
        if (string.IsNullOrWhiteSpace(billingOptions.StripeWebhookSecret))
        {
            return false;
        }

        long? timestamp = null;
        List<string> signatures = [];
        foreach (string part in signatureHeader.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] pieces = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (pieces.Length != 2)
            {
                continue;
            }

            if (string.Equals(pieces[0], "t", StringComparison.Ordinal) &&
                long.TryParse(pieces[1], NumberStyles.None, CultureInfo.InvariantCulture, out long parsedTimestamp))
            {
                timestamp = parsedTimestamp;
            }
            else if (string.Equals(pieces[0], "v1", StringComparison.Ordinal))
            {
                signatures.Add(pieces[1]);
            }
        }

        if (timestamp is null || signatures.Count == 0)
        {
            return false;
        }

        DateTimeOffset signedAtUtc = DateTimeOffset.FromUnixTimeSeconds(timestamp.Value);
        if (Duration(nowUtc - signedAtUtc) > TimeSpan.FromMinutes(billingOptions.StripeWebhookToleranceMinutes))
        {
            return false;
        }

        string signedPayload = $"{timestamp.Value.ToString(CultureInfo.InvariantCulture)}.{payload}";
        byte[] secretBytes = Encoding.UTF8.GetBytes(billingOptions.StripeWebhookSecret);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(signedPayload);
        byte[] digest = HMACSHA256.HashData(secretBytes, payloadBytes);
        string expected = Convert.ToHexString(digest).ToLowerInvariant();

        foreach (string signature in signatures)
        {
            if (FixedTimeEquals(expected, signature))
            {
                return true;
            }
        }

        return false;
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        byte[] expectedBytes = Encoding.ASCII.GetBytes(expected);
        byte[] actualBytes = Encoding.ASCII.GetBytes(actual);
        return expectedBytes.Length == actualBytes.Length &&
            CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    private static TimeSpan Duration(TimeSpan value) => value < TimeSpan.Zero ? -value : value;
}
