using Khz.Language;
using Khz.Runtime;

namespace Khz.Tests;

public sealed class PipelineExecutorTests
{
    [Fact]
    public async Task Execute_TwoStagePipeline_PreservesObject()
    {
        var parseResult = new Parser(
            "Write-Output \"hello\" | Write-Output").Parse();

        Assert.True(parseResult.IsSuccess);
        Assert.NotNull(parseResult.Script);

        var registry = new CommandRegistry()
            .Register(new WriteOutputCommand());

        var executor = new PipelineExecutor(registry);

        var output = await CollectAsync(
            executor.ExecuteAsync(parseResult.Script));

        var item = Assert.Single(output);
        Assert.Equal("hello", item);
    }

    [Fact]
    public async Task Execute_UnregisteredCommand_Throws()
    {
        var parseResult = new Parser(
            "Missing-Command").Parse();

        Assert.True(parseResult.IsSuccess);
        Assert.NotNull(parseResult.Script);

        var executor = new PipelineExecutor(
            new CommandRegistry());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                await CollectAsync(
                    executor.ExecuteAsync(parseResult.Script));
            });

        Assert.Contains(
            "Missing-Command",
            exception.Message,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Register_DuplicateCommand_Throws()
    {
        var registry = new CommandRegistry()
            .Register(new WriteOutputCommand());

        var exception = Assert.Throws<InvalidOperationException>(
            () => registry.Register(new WriteOutputCommand()));

        Assert.Contains(
            "already registered",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Resolve_IsCaseInsensitive()
    {
        var expected = new WriteOutputCommand();

        var registry = new CommandRegistry()
            .Register(expected);

        var actual = registry.Resolve("write-output");

        Assert.Same(expected, actual);
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
}
