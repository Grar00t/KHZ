using Khz.Language;
using Khz.Runtime;

namespace Khz.Tests;

public sealed class GetProcessCommandTests
{
    [Fact]
    public async Task Execute_EmitsTypedProcessObjects()
    {
        var expected = new[]
        {
            new KhzProcess(101, "alpha", 1.5, 1024),
            new KhzProcess(202, "beta", 3.0, 2048)
        };

        var parseResult = new Parser("Get-Process").Parse();

        Assert.True(parseResult.IsSuccess);
        Assert.NotNull(parseResult.Script);

        var commandAst = Assert.Single(
            parseResult.Script.Pipeline.Commands);

        var command = new GetProcessCommand(
            new FakeProcessSource(expected));

        var output = await CollectAsync(
            command.ExecuteAsync(
                commandAst,
                EmptyAsync(),
                CancellationToken.None));

        Assert.Equal(2, output.Count);
        Assert.Same(expected[0], output[0]);
        Assert.Same(expected[1], output[1]);
        Assert.All(output, item => Assert.IsType<KhzProcess>(item));
    }

    [Fact]
    public async Task Execute_WithArguments_Throws()
    {
        var parseResult = new Parser(
            "Get-Process unexpected").Parse();

        Assert.True(parseResult.IsSuccess);
        Assert.NotNull(parseResult.Script);

        var commandAst = Assert.Single(
            parseResult.Script.Pipeline.Commands);

        var command = new GetProcessCommand(
            new FakeProcessSource([]));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                await CollectAsync(
                    command.ExecuteAsync(
                        commandAst,
                        EmptyAsync(),
                        CancellationToken.None));
            });

        Assert.Contains(
            "does not accept arguments",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeProcessSource(
        IEnumerable<KhzProcess> processes) : IProcessSource
    {
        public IEnumerable<KhzProcess> GetProcesses() =>
            processes;
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

    private static async IAsyncEnumerable<object?> EmptyAsync()
    {
        await Task.CompletedTask;
        yield break;
    }
}
