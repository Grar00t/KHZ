using Khz.Language;

namespace Khz.Runtime;

public interface IKhzCommand
{
    string Name { get; }

    IAsyncEnumerable<object?> ExecuteAsync(
        CommandAst command,
        IAsyncEnumerable<object?> input,
        CancellationToken cancellationToken);
}
