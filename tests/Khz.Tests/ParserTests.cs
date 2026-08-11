using Khz.Language;

namespace Khz.Tests;

public sealed class ParserTests
{
    [Fact]
    public void Parse_ProcessPipeline_ProducesExpectedAst()
    {
        var result = new Parser(
            "Get-Process | Select-Object -First 5").Parse();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Script);
        Assert.Empty(result.Diagnostics);

        var commands = result.Script.Pipeline.Commands;
        Assert.Equal(2, commands.Count);

        var getProcess = commands[0];
        Assert.Equal("Get-Process", getProcess.Name);
        Assert.Empty(getProcess.PositionalArguments);
        Assert.Empty(getProcess.NamedArguments);

        var selectObject = commands[1];
        Assert.Equal("Select-Object", selectObject.Name);
        Assert.Empty(selectObject.PositionalArguments);

        var first = Assert.Single(selectObject.NamedArguments);
        Assert.Equal("First", first.Name);
        Assert.NotNull(first.Value);
        Assert.Equal(5L, first.Value.Value);
        Assert.Equal(TokenKind.Integer, first.Value.Token.Kind);
    }

    [Fact]
    public void Parse_WriteOutputString_ProducesPositionalArgument()
    {
        var result = new Parser(
            "Write-Output \"hello world\"").Parse();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Script);

        var command = Assert.Single(
            result.Script.Pipeline.Commands);

        Assert.Equal("Write-Output", command.Name);

        var argument = Assert.Single(
            command.PositionalArguments);

        Assert.Equal("hello world", argument.Value.Value);
        Assert.Equal(
            TokenKind.String,
            argument.Value.Token.Kind);
    }

    [Fact]
    public void Parse_EmptyInput_ReportsExpectedCommand()
    {
        var result = new Parser(string.Empty).Parse();

        Assert.False(result.IsSuccess);
        Assert.Null(result.Script);

        var diagnostic = Assert.Single(
            result.Diagnostics);

        Assert.Equal("KHZ1001", diagnostic.Code);
        Assert.Equal(0, diagnostic.Position);
    }

    [Fact]
    public void Parse_TrailingPipeline_ReportsExpectedCommand()
    {
        var result = new Parser(
            "Get-Process |").Parse();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Script);

        var diagnostic = Assert.Single(
            result.Diagnostics);

        Assert.Equal("KHZ1002", diagnostic.Code);
        Assert.Equal("|", diagnostic.ActualText);
    }

    [Fact]
    public void Parse_InvalidToken_ReportsDiagnostic()
    {
        var result = new Parser("@").Parse();

        Assert.False(result.IsSuccess);
        Assert.Null(result.Script);

        var diagnostic = Assert.Single(
            result.Diagnostics);

        Assert.Equal("KHZ1004", diagnostic.Code);
        Assert.Equal("@", diagnostic.ActualText);
        Assert.Equal(0, diagnostic.Position);
    }
}
