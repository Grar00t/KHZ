namespace Khz.Language;

public sealed record Token(
    TokenKind Kind,
    string Text,
    object? Value,
    int Position);
