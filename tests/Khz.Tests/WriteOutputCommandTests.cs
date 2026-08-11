using System.Runtime.CompilerServices;
using Khz.Language;
using Khz.Runtime;

namespace Khz.Tests;

public sealed class WriteOutputCommandTests
{
    [Fact]
    public async Task Execute_WithArgument_EmitsArgumentAsObject()
    {
        var parseResult = new Parser(
            "Write-Output \"hello world\"").Parse();

        Assert.True(parseResult.IsSuccess);

        var commandAst = Assert.Single(
            parseResult.Script!.Pipeline.Commands);

        var command = new WriteOutputCommand();

        var output = await CollectAsync(
            command.ExecuteAsync(
                commandAst,
                EmptyAsync(),
                CancellationToken.None));

        var item = Assert.Single(output);
        Assert.Equal("hello world", item);
    }

    [Fact]
    public async Task Execute_WithoutArguments_PassesPipelineObjectsThrough()
    {
        var parseResult = new Parser(
            "Write-Output").Parse();

        Assert.True(parseResult.IsSuccess);

        var commandAst = Assert.Single(
            parseResult.Script!.Pipeline.Commands);

        var expected = new object?[]
        {
            10L,
            "second",
            new { Name = "object" }
        };

        var command = new WriteOutputCommand();

        var output = await CollectAsync(
            command.ExecuteAsync(
                commandAst,
                ToAsync(expected),
                CancellationToken.None));

        Assert.Equal(expected, output);
        Assert.Same(expected[2], output[2]);
    }

    private static async Task<List<object?>> CollectAsync(
        IAsyncEnumerable<object?> source)
    {
        var items = new List<object?>();

        await foreach (var item in source)
        {
            items.Add(item);
        }

        return items;
    }

    private static async IAsyncEnumerable<object?> EmptyAsync()
    {
        await Task.CompletedTask;
        yield break;
    }

    private static async IAsyncEnumerable<object?> ToAsync(
        IEnumerable<object?> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }

        await Task.CompletedTask;
    }
}
