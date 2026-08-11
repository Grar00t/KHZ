using Khz.Language;
using Khz.Runtime;

if (args.Length < 2 ||
    !string.Equals(args[0], "-Command", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine(
        "Usage: Khz.Cli.exe -Command \"<command>\"");

    return 1;
}

var commandText = string.Join(' ', args.Skip(1));

var parseResult = new Parser(commandText).Parse();

if (!parseResult.IsSuccess || parseResult.Script is null)
{
    foreach (var diagnostic in parseResult.Diagnostics)
    {
        Console.Error.WriteLine(
            $"{diagnostic.Code}: {diagnostic.Message} " +
            $"at position {diagnostic.Position}.");
    }

    return 2;
}

var registry = new CommandRegistry()
    .Register(new WriteOutputCommand())
    .Register(new GetProcessCommand(
        new SystemProcessSource()))
    .Register(new SelectObjectCommand());

var executor = new PipelineExecutor(registry);

using var cancellation = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

try
{
    var wroteProcessHeader = false;

    await foreach (var item in executor.ExecuteAsync(
        parseResult.Script,
        cancellation.Token))
    {
        if (item is KhzProcess process)
        {
            if (!wroteProcessHeader)
            {
                Console.WriteLine(
                    $"{"Id",8}  {"Name",-30}  {"CPU(s)",12}  {"Memory",14}");

                Console.WriteLine(
                    $"{new string('-', 8)}  " +
                    $"{new string('-', 30)}  " +
                    $"{new string('-', 12)}  " +
                    $"{new string('-', 14)}");

                wroteProcessHeader = true;
            }

            Console.WriteLine(
                $"{process.Id,8}  " +
                $"{Truncate(process.Name, 30),-30}  " +
                $"{process.CpuSeconds,12:F2}  " +
                $"{process.WorkingSetBytes,14:N0}");

            continue;
        }

        Console.WriteLine(item?.ToString() ?? string.Empty);
    }

    return 0;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("KHZ execution cancelled.");
    return 130;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"KHZ runtime error: {exception.Message}");
    return 3;
}

static string Truncate(string value, int maximumLength)
{
    if (value.Length <= maximumLength)
    {
        return value;
    }

    return value[..(maximumLength - 1)] + "…";
}

