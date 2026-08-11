using System.Runtime.CompilerServices;
using Khz.Language;

namespace Khz.Runtime;

public sealed class WriteOutputCommand : IKhzCommand
{
    public string Name => "Write-Output";

    public async IAsyncEnumerable<object?> ExecuteAsync(
        CommandAst command,
        IAsyncEnumerable<object?> input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var emittedArgument = false;

        foreach (var argument in command.PositionalArguments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            emittedArgument = true;
            yield return argument.Value.Value;
        }

        if (emittedArgument)
        {
            yield break;
        }

        await foreach (var item in input.WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
}
