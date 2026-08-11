using System.Runtime.CompilerServices;
using Khz.Language;

namespace Khz.Runtime;

public sealed class GetProcessCommand : IKhzCommand
{
    private readonly IProcessSource _source;

    public GetProcessCommand(IProcessSource source)
    {
        _source = source ??
            throw new ArgumentNullException(nameof(source));
    }

    public string Name => "Get-Process";

    public async IAsyncEnumerable<object?> ExecuteAsync(
        CommandAst command,
        IAsyncEnumerable<object?> input,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(input);

        if (command.PositionalArguments.Count != 0 ||
            command.NamedArguments.Count != 0)
        {
            throw new InvalidOperationException(
                "Get-Process does not accept arguments in the current sprint.");
        }

        foreach (var process in _source.GetProcesses())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return process;
        }

        await Task.CompletedTask;
    }
}
