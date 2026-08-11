using Khz.Language;

namespace Khz.Tests;

public sealed class LexerTests
{
    [Fact]
    public void Lex_ProcessPipeline_ProducesTypedTokens()
    {
        var lexer = new Lexer(
            "Get-Process | Select-Object -First 5");

        var tokens = lexer.Lex();

        Assert.Collection(
            tokens,
            token =>
            {
                Assert.Equal(TokenKind.Identifier, token.Kind);
                Assert.Equal("Get-Process", token.Text);
                Assert.Equal(0, token.Position);
            },
            token =>
            {
                Assert.Equal(TokenKind.Pipeline, token.Kind);
                Assert.Equal("|", token.Text);
            },
            token =>
            {
                Assert.Equal(TokenKind.Identifier, token.Kind);
                Assert.Equal("Select-Object", token.Text);
            },
            token =>
            {
                Assert.Equal(TokenKind.NamedParameter, token.Kind);
                Assert.Equal("-First", token.Text);
                Assert.Equal("First", token.Value);
            },
            token =>
            {
                Assert.Equal(TokenKind.Integer, token.Kind);
                Assert.Equal(5L, token.Value);
            },
            token =>
            {
                Assert.Equal(TokenKind.EndOfInput, token.Kind);
                Assert.Equal("Get-Process | Select-Object -First 5".Length, token.Position);
            });
    }

    [Theory]
    [InlineData("\"hello world\"", "hello world")]
    [InlineData("'hello world'", "hello world")]
    public void Lex_QuotedString_ProducesStringToken(
        string input,
        string expectedValue)
    {
        var tokens = new Lexer(input).Lex();

        Assert.Equal(TokenKind.String, tokens[0].Kind);
        Assert.Equal(expectedValue, tokens[0].Value);
        Assert.Equal(TokenKind.EndOfInput, tokens[1].Kind);
    }

    [Fact]
    public void Lex_UnterminatedString_ProducesBadToken()
    {
        var tokens = new Lexer("\"unfinished").Lex();

        Assert.Equal(TokenKind.BadToken, tokens[0].Kind);
        Assert.Equal("\"unfinished", tokens[0].Text);
        Assert.Equal(TokenKind.EndOfInput, tokens[1].Kind);
    }

    [Fact]
    public void Lex_UnknownCharacter_ProducesBadToken()
    {
        var tokens = new Lexer("@").Lex();

        Assert.Equal(TokenKind.BadToken, tokens[0].Kind);
        Assert.Equal("@", tokens[0].Text);
        Assert.Equal(TokenKind.EndOfInput, tokens[1].Kind);
    }
}

