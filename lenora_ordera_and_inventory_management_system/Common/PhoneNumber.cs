namespace lenora_ordera_and_inventory_management_system.Common;

internal static class PhoneNumber
{
    public static string Normalise(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var chars = input.Where(c => char.IsAsciiDigit(c) || c == '+').ToArray();
        return new string(chars);
    }

    public static string CanonicalMobile(string? input)
    {
        var local = ToLocalDigits(DigitsOnly(input));
        if (local.Length != Rules.SriLankanMobileLength)
        {
            return string.Empty;
        }

        if (!local.StartsWith(Rules.SriLankanMobilePrefix, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return local;
    }

    private static string DigitsOnly(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return new string(input.Where(char.IsAsciiDigit).ToArray());
    }

    private static string ToLocalDigits(string digits)
    {
        var hasCountryCode = digits.StartsWith(Rules.SriLankanCountryCode, StringComparison.Ordinal)
            && digits.Length == Rules.SriLankanMobileWithCountryLength;
        if (!hasCountryCode)
        {
            return digits;
        }

        return Rules.SriLankanTrunkPrefix + digits[Rules.SriLankanCountryCode.Length..];
    }
}
