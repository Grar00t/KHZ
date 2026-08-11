namespace Khz.Language;

public abstract record SyntaxNode;

public sealed record LiteralAst(
    object? Value,
    Token Token) : SyntaxNode;

public sealed record PositionalArgumentAst(
    LiteralAst Value) : SyntaxNode;

public sealed record NamedArgumentAst(
    string Name,
    Token NameToken,
    LiteralAst? Value) : SyntaxNode;

public sealed record CommandAst(
    string Name,
    Token NameToken,
    IReadOnlyList<PositionalArgumentAst> PositionalArguments,
    IReadOnlyList<NamedArgumentAst> NamedArguments) : SyntaxNode;

public sealed record PipelineAst(
    IReadOnlyList<CommandAst> Commands) : SyntaxNode;

public sealed record ScriptAst(
    PipelineAst Pipeline) : SyntaxNode;
