using System.Globalization;

namespace Khz.Language;

public sealed class Lexer
{
    private readonly string _text;
    private int _position;

    public Lexer(string text)
    {
        _text = text ?? throw new ArgumentNullException(nameof(text));
    }

    public IReadOnlyList<Token> Lex()
    {
        var tokens = new List<Token>();

        while (true)
        {
            var token = NextToken();
            tokens.Add(token);

            if (token.Kind == TokenKind.EndOfInput)
            {
                return tokens;
            }
        }
    }

    public Token NextToken()
    {
        SkipWhitespace();

        if (_position >= _text.Length)
        {
            return new Token(
                TokenKind.EndOfInput,
                string.Empty,
                null,
                _position);
        }

        var start = _position;
        var current = _text[_position];

        if (current == '|')
        {
            _position++;

            return new Token(
                TokenKind.Pipeline,
                "|",
                null,
                start);
        }

        if (current is '"' or '\'')
        {
            return ReadString();
        }

        if (char.IsDigit(current))
        {
            return ReadInteger();
        }

        if (current == '-' &&
            _position + 1 < _text.Length &&
            IsIdentifierStart(_text[_position + 1]))
        {
            return ReadNamedParameter();
        }

        if (IsIdentifierStart(current))
        {
            return ReadIdentifier();
        }

        _position++;

        return new Token(
            TokenKind.BadToken,
            _text[start].ToString(),
            null,
            start);
    }

    private Token ReadString()
    {
        var start = _position;
        var quote = _text[_position++];

        while (_position < _text.Length && _text[_position] != quote)
        {
            _position++;
        }

        if (_position >= _text.Length)
        {
            return new Token(
                TokenKind.BadToken,
                _text[start..],
                null,
                start);
        }

        _position++;

        var text = _text[start.._position];
        var value = text[1..^1];

        return new Token(
            TokenKind.String,
            text,
            value,
            start);
    }

    private Token ReadInteger()
    {
        var start = _position;

        while (_position < _text.Length &&
               char.IsDigit(_text[_position]))
        {
            _position++;
        }

        var text = _text[start.._position];

        if (!long.TryParse(
                text,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var value))
        {
            return new Token(
                TokenKind.BadToken,
                text,
                null,
                start);
        }

        return new Token(
            TokenKind.Integer,
            text,
            value,
            start);
    }

    private Token ReadNamedParameter()
    {
        var start = _position++;

        while (_position < _text.Length &&
               IsIdentifierPart(_text[_position]))
        {
            _position++;
        }

        var text = _text[start.._position];

        return new Token(
            TokenKind.NamedParameter,
            text,
            text[1..],
            start);
    }

    private Token ReadIdentifier()
    {
        var start = _position;

        while (_position < _text.Length &&
               IsIdentifierPart(_text[_position]))
        {
            _position++;
        }

        var text = _text[start.._position];

        return new Token(
            TokenKind.Identifier,
            text,
            text,
            start);
    }

    private void SkipWhitespace()
    {
        while (_position < _text.Length &&
               char.IsWhiteSpace(_text[_position]))
        {
            _position++;
        }
    }

    private static bool IsIdentifierStart(char value) =>
        char.IsLetter(value) || value == '_';

    private static bool IsIdentifierPart(char value) =>
        char.IsLetterOrDigit(value) ||
        value is '_' or '-' or '.';
}
