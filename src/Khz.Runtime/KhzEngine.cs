using Khz.Language;

namespace Khz.Runtime;

public sealed class KhzEngine
{
    private readonly PipelineExecutor _executor;

    public KhzEngine(IProcessSource? processSource = null)
    {
        var registry = new CommandRegistry()
            .Register(new WriteOutputCommand())
            .Register(new GetProcessCommand(
                processSource ?? new SystemProcessSource()))
            .Register(new SelectObjectCommand());

        _executor = new PipelineExecutor(registry);
    }

    public ParseResult Parse(string text)
    {
        return new Parser(text).Parse();
    }

    public IAsyncEnumerable<object?> ExecuteAsync(
        ScriptAst script,
        CancellationToken cancellationToken = default)
    {
        return _executor.ExecuteAsync(
            script,
            cancellationToken);
    }
}
