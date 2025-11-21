# FlowFusion
<p style="display:flex;text-align:center;">
<img src="./resources/logo-transparent.png" width="120" height="80">
</p>
A high-performance expression evaluation engine for .NET applications, with support for workflow execution.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com)

## Features

- **High-Performance Expression Evaluation**: Uses compiled LINQ expressions for fast execution
- **C# Syntax Support**: Full C# expression syntax with operators, method calls, and property access
- **Async Execution**: Fully asynchronous with cancellation support
- **Context-Aware**: Rich evaluation with access to variables and complex objects
- **Workflow Support**: Conditional branching and execution based on runtime expressions
- **Clean Architecture**: Modular design with clear separation of concerns

## Quick Start - Expression Evaluation

```csharp
using FlowFusion.Core.Interfaces;
using FlowFusion.Core.Models;
using FlowFusion.Expression;
using Microsoft.Extensions.DependencyInjection;

// Setup DI
var services = new ServiceCollection();
services.AddSingleton<IExpressionEvaluator, ExpressionEvaluator>();
var provider = services.BuildServiceProvider();

var evaluator = provider.GetRequiredService<IExpressionEvaluator>();

// Create execution context with variables
var context = new FlowExecutionContext(new Dictionary<string, object?> {
    ["order"] = new { Total = 150.0, Items = new List<string> { "item1" } },
    ["customer"] = new { CreditLimit = 200.0, Status = "Gold" }
});

// Evaluate expressions
string expression = "Variables[\"order\"].Total > 100 && Variables[\"customer\"].Status == \"Gold\"";
bool result = await evaluator.EvaluateAsync(expression, context);
Console.WriteLine($"Result: {result}"); // Output: Result: True
```

## Architecture

FlowFusion follows clean architecture principles:

- **FlowFusion.Core**: Domain models and interfaces (IExpressionEvaluator, IWorkflow, etc.)
- **FlowFusion.Expression**: High-performance expression evaluation engine
- **FlowFusion.RunTime**: Workflow execution engine with conditional branching
- **FlowFusion.Builder**: Fluent APIs for workflow construction
- **Example**: Usage demonstrations

## Expression Syntax

Supports C# syntax for dynamic evaluations:

```csharp
// Direct variable access
Variables["order"].Total > 100

// Complex expressions with operators
Variables["user"].Age >= 18 && Variables["user"].Balance > 100 || Variables["user"].IsVip

// Method calls and properties
DateTime.Now.Hour > 9
Math.Max(Variables["a"], Variables["b"]) > Variables["threshold"]
Variables["list"].Count() > 0
```

## Workflow Execution

Build and execute workflows with conditional transitions:

```csharp
using FlowFusion.Core.Interfaces;
using FlowFusion.Core.Models;
using FlowFusion.Builder;
using FlowFusion.RunTime;

// Implement custom blocks
public class LogBlock : IBlock
{
    public string Id { get; }
    public string Message { get; }

    public LogBlock(string id, string message)
    {
        Id = id;
        Message = message;
    }

    public Task<ExecutionResult> ExecuteAsync(FlowExecutionContext context)
    {
        Console.WriteLine(Message);
        return Task.FromResult(ExecutionResult.Success());
    }
}

// Build workflow
var workflow = new WorkflowBuilder("sample-workflow")
    .StartWith(new LogBlock("start", "Starting workflow"))
    .AddConditionalTransition("start", "approved", "Variables[\"amount\"] <= 1000")
    .AddConditionalTransition("start", "review", "Variables[\"amount\"] > 1000")
    .Add(new LogBlock("approved", "Approved automatically"))
    .Add(new LogBlock("review", "Requires manual review"))
    .Build();

// Execute
var context = new FlowExecutionContext(new Dictionary<string, object?> {
    ["amount"] = 500.0
});

var engine = new WorkflowEngine(new ExpressionEvaluator());
await engine.RunAsync(workflow, context);
```

## Building from Source

```bash
# Clone the repository
git clone https://github.com/your-repo/FlowFusion.git
cd FlowFusion

# Build all projects
dotnet build

# Run tests
dotnet test

# Run the example application
cd example/FlowFusion.Example
dotnet run
```

## Project Structure

```
src/
├── FlowFusion.Core/          # Domain models and interfaces
├── FlowFusion.Expression/    # Expression evaluation
├── FlowFusion.RunTime/       # Workflow execution engine
└── FlowFusion.Builder/       # Workflow construction APIs

test/
└── FlowFusion.Tests/         # Unit tests

example/
└── FlowFusion.Example/       # Usage examples
```

## Contributing

1. Follow clean architecture principles
2. Add tests for new features
3. Update documentation
4. Ensure all tests pass

## License

MIT License - see LICENSE file for details.
