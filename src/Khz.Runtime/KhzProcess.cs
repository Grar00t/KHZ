namespace Khz.Runtime;

public sealed record KhzProcess(
    int Id,
    string Name,
    double CpuSeconds,
    long WorkingSetBytes);
