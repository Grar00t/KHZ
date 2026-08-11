namespace Khz.Language;

public sealed record ParseResult(
    ScriptAst? Script,
    IReadOnlyList<SyntaxDiagnostic> Diagnostics)
{
    public bool IsSuccess =>
        Script is not null &&
        Diagnostics.Count == 0;
}
