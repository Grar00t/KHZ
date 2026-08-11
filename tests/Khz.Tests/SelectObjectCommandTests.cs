using System.Runtime.CompilerServices;
using Khz.Language;
using Khz.Runtime;

namespace Khz.Tests;

public sealed class SelectObjectCommandTests
{
    [Fact]
    public async Task Execute_FirstTwo_EmitsOriginalObjects()
    {
        var first = new KhzProcess(101, "alpha", 1.5, 1024);
        var second = new KhzProcess(202, "beta", 3.0, 2048);
        var third = new KhzProcess(303, "gamma", 4.5, 4096);

        var output = await ExecuteAsync(
            "Select-Object -First 2",
            ToAsync([first, second, third]));

        Assert.Equal(2, output.Count);
        Assert.Same(first, output[0]);
        Assert.Same(second, output[1]);
    }

    [Fact]
    public async Task Execute_FirstZero_DoesNotEnumerateSource()
    {
        var enumerated = 0;

        var output = await ExecuteAsync(
            "Select-Object -First 0",
            CountingSource(() => enumerated++));

        Assert.Empty(output);
        Assert.Equal(0, enumerated);
    }

    [Theory]
    [InlineData("Select-Object")]
    [InlineData("Select-Object value")]
    [InlineData("Select-Object -First")]
    [InlineData("Select-Object -Other 1")]
    [InlineData("Select-Object -First text")]
    public async Task Execute_InvalidArguments_Throws(string text)
    {
        var parseResult = new Parser(text).Parse();

        Assert.True(parseResult.IsSuccess);
        Assert.NotNull(parseResult.Script);

        var commandAst = Assert.Single(
            parseResult.Script.Pipeline.Commands);

        var command = new SelectObjectCommand();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                await CollectAsync(
                    command.ExecuteAsync(
                        commandAst,
                        ToAsync([]),
                        CancellationToken.None));
            });
    }

    private static async Task<List<object?>> ExecuteAsync(
        string text,
        IAsyncEnumerable<object?> input)
    {
        var parseResult = new Parser(text).Parse();

        Assert.True(parseResult.IsSuccess);
        Assert.NotNull(parseResult.Script);

        var commandAst = Assert.Single(
            parseResult.Script.Pipeline.Commands);

        var command = new SelectObjectCommand();

        return await CollectAsync(
            command.ExecuteAsync(
                commandAst,
                input,
                CancellationToken.None));
    }

    private static async Task<List<object?>> CollectAsync(
        IAsyncEnumerable<object?> source)
    {
        var output = new List<object?>();

        await foreach (var item in source)
        {
            output.Add(item);
        }

        return output;
    }

    private static async IAsyncEnumerable<object?> ToAsync(
        IEnumerable<object?> items,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<object?> CountingSource(
        Action onEnumerated,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        onEnumerated();
        yield return new object();
        await Task.Yield();
    }
}
