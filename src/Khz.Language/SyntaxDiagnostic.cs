namespace Khz.Language;

public sealed record SyntaxDiagnostic(
    string Code,
    string Message,
    int Position,
    string? ActualText = null);
