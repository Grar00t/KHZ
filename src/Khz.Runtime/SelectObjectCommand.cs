using System.Runtime.CompilerServices;
using Khz.Language;

namespace Khz.Runtime;

public sealed class SelectObjectCommand : IKhzCommand
{
    public string Name => "Select-Object";

    public async IAsyncEnumerable<object?> ExecuteAsync(
        CommandAst command,
        IAsyncEnumerable<object?> input,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(input);

        if (command.PositionalArguments.Count != 0 ||
            command.NamedArguments.Count != 1)
        {
            throw new InvalidOperationException(
                "Select-Object currently requires exactly: -First <non-negative integer>.");
        }

        var parameter = command.NamedArguments[0];

        if (!string.Equals(
                parameter.Name,
                "First",
                StringComparison.OrdinalIgnoreCase) ||
            parameter.Value?.Token.Kind != TokenKind.Integer ||
            parameter.Value.Value is not long count ||
            count < 0)
        {
            throw new InvalidOperationException(
                "Select-Object currently requires: -First <non-negative integer>.");
        }

        if (count == 0)
        {
            yield break;
        }

        long emitted = 0;

        await foreach (var item in input.WithCancellation(cancellationToken))
        {
            yield return item;
            emitted++;

            if (emitted >= count)
            {
                yield break;
            }
        }
    }
}
