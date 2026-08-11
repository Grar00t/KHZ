using System.Runtime.CompilerServices;
using Khz.Language;

namespace Khz.Runtime;

public sealed class PipelineExecutor
{
    private readonly CommandRegistry _registry;

    public PipelineExecutor(CommandRegistry registry)
    {
        _registry = registry ??
            throw new ArgumentNullException(nameof(registry));
    }

    public IAsyncEnumerable<object?> ExecuteAsync(
        ScriptAst script,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(script);

        IAsyncEnumerable<object?> current =
            EmptyAsync(cancellationToken);

        foreach (var commandAst in script.Pipeline.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var command = _registry.Resolve(commandAst.Name);

            current = command.ExecuteAsync(
                commandAst,
                current,
                cancellationToken);
        }

        return current;
    }

    private static async IAsyncEnumerable<object?> EmptyAsync(
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask;
        yield break;
    }
}
