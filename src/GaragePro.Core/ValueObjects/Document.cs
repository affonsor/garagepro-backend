namespace GaragePro.Core.ValueObjects;

public sealed record Document
{
    private Document(string value, DocumentType type)
    {
        Value = value;
        Type = type;
    }

    public string Value { get; }
    public DocumentType Type { get; }

    public static Document Create(string? value)
    {
        if (!TryCreate(value, out var document))
            throw new ArgumentException("Document must be a valid CPF or CNPJ.", nameof(value));

        return document!;
    }

    public static bool TryCreate(string? value, out Document? document)
    {
        document = null;
        var normalized = Normalize(value);

        if (normalized.Length == 11 && IsValidCpf(normalized))
        {
            document = new Document(normalized, DocumentType.Cpf);
            return true;
        }

        if (normalized.Length == 14 && IsValidCnpj(normalized))
        {
            document = new Document(normalized, DocumentType.Cnpj);
            return true;
        }

        return false;
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static bool IsValidCpf(string value)
    {
        if (HasOnlyRepeatedDigits(value))
            return false;

        var firstDigit = CalculateCpfDigit(value, 9);
        var secondDigit = CalculateCpfDigit(value, 10);

        return value[9] - '0' == firstDigit && value[10] - '0' == secondDigit;
    }

    private static int CalculateCpfDigit(string value, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
            sum += (value[i] - '0') * (length + 1 - i);

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static bool IsValidCnpj(string value)
    {
        if (HasOnlyRepeatedDigits(value))
            return false;

        int[] firstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] secondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var firstDigit = CalculateCnpjDigit(value, firstWeights);
        var secondDigit = CalculateCnpjDigit(value, secondWeights);

        return value[12] - '0' == firstDigit && value[13] - '0' == secondDigit;
    }

    private static int CalculateCnpjDigit(string value, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
            sum += (value[i] - '0') * weights[i];

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static bool HasOnlyRepeatedDigits(string value) =>
        value.All(c => c == value[0]);
}

public enum DocumentType
{
    Cpf,
    Cnpj
}
