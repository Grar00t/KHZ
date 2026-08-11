namespace Khz.Language;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private readonly List<SyntaxDiagnostic> _diagnostics = [];
    private int _position;

    public Parser(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _tokens = new Lexer(text).Lex();
    }

    public ParseResult Parse()
    {
        var commands = new List<CommandAst>();

        if (Current.Kind == TokenKind.EndOfInput)
        {
            Report(
                "KHZ1001",
                "Expected a command.",
                Current);

            return new ParseResult(null, _diagnostics);
        }

        while (Current.Kind != TokenKind.EndOfInput)
        {
            var command = ParseCommand();

            if (command is not null)
            {
                commands.Add(command);
            }

            if (Current.Kind == TokenKind.Pipeline)
            {
                var pipe = NextToken();

                if (Current.Kind == TokenKind.EndOfInput)
                {
                    Report(
                        "KHZ1002",
                        "Expected a command after the pipeline operator.",
                        pipe);

                    break;
                }

                continue;
            }

            if (Current.Kind != TokenKind.EndOfInput)
            {
                Report(
                    "KHZ1003",
                    $"Unexpected token '{Current.Text}'.",
                    Current);

                NextToken();
            }
        }

        if (commands.Count == 0)
        {
            return new ParseResult(null, _diagnostics);
        }

        var script = new ScriptAst(
            new PipelineAst(commands));

        return new ParseResult(script, _diagnostics);
    }

    private CommandAst? ParseCommand()
    {
        if (Current.Kind == TokenKind.BadToken)
        {
            Report(
                "KHZ1004",
                $"Invalid token '{Current.Text}'.",
                Current);

            NextToken();
            return null;
        }

        if (Current.Kind != TokenKind.Identifier)
        {
            Report(
                "KHZ1005",
                "Expected a command name.",
                Current);

            NextToken();
            return null;
        }

        var nameToken = NextToken();
        var positionalArguments = new List<PositionalArgumentAst>();
        var namedArguments = new List<NamedArgumentAst>();

        while (Current.Kind is not TokenKind.Pipeline
               and not TokenKind.EndOfInput)
        {
            if (Current.Kind == TokenKind.NamedParameter)
            {
                namedArguments.Add(ParseNamedArgument());
                continue;
            }

            var literal = ParseLiteral();

            if (literal is not null)
            {
                positionalArguments.Add(
                    new PositionalArgumentAst(literal));

                continue;
            }

            Report(
                "KHZ1006",
                $"Unexpected argument '{Current.Text}'.",
                Current);

            NextToken();
        }

        return new CommandAst(
            nameToken.Text,
            nameToken,
            positionalArguments,
            namedArguments);
    }

    private NamedArgumentAst ParseNamedArgument()
    {
        var nameToken = NextToken();
        LiteralAst? value = null;

        if (Current.Kind is TokenKind.Identifier
            or TokenKind.String
            or TokenKind.Integer)
        {
            value = ParseLiteral();
        }

        return new NamedArgumentAst(
            nameToken.Value?.ToString() ?? nameToken.Text.TrimStart('-'),
            nameToken,
            value);
    }

    private LiteralAst? ParseLiteral()
    {
        if (Current.Kind is not TokenKind.Identifier
            and not TokenKind.String
            and not TokenKind.Integer)
        {
            return null;
        }

        var token = NextToken();

        return new LiteralAst(
            token.Value,
            token);
    }

    private Token Current =>
        Peek(0);

    private Token Peek(int offset)
    {
        var index = _position + offset;

        if (index >= _tokens.Count)
        {
            return _tokens[^1];
        }

        return _tokens[index];
    }

    private Token NextToken()
    {
        var current = Current;

        if (_position < _tokens.Count - 1)
        {
            _position++;
        }

        return current;
    }

    private void Report(
        string code,
        string message,
        Token token)
    {
        _diagnostics.Add(
            new SyntaxDiagnostic(
                code,
                message,
                token.Position,
                token.Text));
    }
}
