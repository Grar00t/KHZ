# KHZ

KHZ is an experimental, independent, object-based automation shell built on .NET 10.

It implements its own lexer, parser, command registry, and asynchronous object pipeline. The verified runtime path does not delegate execution to `powershell.exe`, `pwsh.exe`, or `System.Management.Automation`.

## Current Status

Sprint `0.1` provides a minimum working runtime. It does not provide full PowerShell compatibility.

### Implemented

- Native `khz.exe` command-line host
- `-Command` execution mode
- KHZ lexer and parser
- Structured syntax diagnostics
- Asynchronous object pipeline
- Case-insensitive command registry
- `Write-Output`
- `Get-Process`
- `Select-Object -First`
- Typed `KhzProcess` objects
- Success exit code `0`
- Syntax-error exit code `2`

### Not Implemented

- Full PowerShell language compatibility
- PowerShell modules or cmdlets
- Interactive REPL
- Script-file execution
- Variables, functions, loops, or conditionals
- General property projection in `Select-Object`

## Example

```powershell
.\khz.exe -Command "Get-Process | Select-Object -First 5"
```

Example output:

```text
      Id  Name                                  CPU(s)          Memory
--------  ------------------------------  ------------  --------------
       4  System                               6080.56         143,360
     140  Secure System                           0.00      42,340,352
```

## Build and Test

Requirements:

- .NET SDK 10.0.302

```powershell
dotnet restore .\KHZ.slnx
dotnet build .\KHZ.slnx --configuration Release --no-restore
dotnet test .\KHZ.slnx --configuration Release --no-build
```

Locally verified result:

```text
Total:   26
Passed:  26
Failed:  0
Skipped: 0
```

## Runtime Verification

Verified command path:

```text
CLI
→ Lexer
→ Parser
→ Command Registry
→ Object Pipeline
→ Get-Process
→ Select-Object -First
→ Formatter
```

Observed during the tested runtime path:

- Five structured process objects returned
- Success exit code: `0`
- Syntax diagnostic: `KHZ1002`
- Syntax-error exit code: `2`
- Child processes observed: `0`

This evidence applies only to the tested path. It is not a claim that every current or future KHZ command will avoid child processes.

## Continuous Integration

GitHub Actions is currently blocked because the repository account is locked due to a billing issue.

The Windows runner did not start. No remote restore, build, or test step executed. This is recorded as an infrastructure block, not as a KHZ code or workflow failure.

## Repository Reference

- Integrated branch: `main`
- Verification tag: `sprint-0.1-runtime-proof`
- Pull request: `#1`

## Scope

KHZ is an independent automation runtime inspired by object-pipeline shell concepts. It does not claim complete PowerShell syntax, cmdlet, module, or behavioral compatibility.

## License

See `LICENSE`.

