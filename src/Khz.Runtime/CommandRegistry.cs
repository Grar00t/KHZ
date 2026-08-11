namespace Khz.Runtime;

public sealed class CommandRegistry
{
    private readonly Dictionary<string, IKhzCommand> _commands =
        new(StringComparer.OrdinalIgnoreCase);

    public CommandRegistry Register(IKhzCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException(
                "Command name cannot be empty.",
                nameof(command));
        }

        if (!_commands.TryAdd(command.Name, command))
        {
            throw new InvalidOperationException(
                $"Command '{command.Name}' is already registered.");
        }

        return this;
    }

    public bool TryResolve(
        string name,
        out IKhzCommand? command)
    {
        return _commands.TryGetValue(name, out command);
    }

    public IKhzCommand Resolve(string name)
    {
        if (TryResolve(name, out var command))
        {
            return command!;
        }

        throw new InvalidOperationException(
            $"Command '{name}' was not found.");
    }
}
