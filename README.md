# FlowFusion

A high-performance workflow execution engine with built-in expression evaluation for .NET applications.

## Features

- **High-Performance Expression Evaluation**: Uses compiled LINQ expressions for fast execution
- **C# Syntax Support**: Full C# expression syntax with operators, method calls, and property access
- **Async Execution**: Fully asynchronous with cancellation support
- **Clean Architecture**: Modular design with clear separation of concerns
- **Extensible**: Plugin-based architecture for custom blocks and evaluators
- **Context-Aware**: Rich expression evaluation with access to workflow variables
- **Conditional Branching**: Dynamic workflow branching based on runtime expressions

## Quick Start

```csharp
using FlowFusion.Core;
using FlowFusion.Expression;
using FlowFusion.RunTime;
using FlowFusion.Builder;

// Create execution context
var context = new FlowExecutionContext(new Dictionary<string, object?> {
    ["x"] = 10.0,
    ["status"] = "active"
});

// Build workflow
var workflow = new WorkflowBuilder("sample-workflow")
    .StartWith(new StartBlock("start"))
    .AddConditionalTransition("start", "process", "x > 5 && status == \"active\"")
    .Add(new ProcessBlock("process"))
    .Build();

// Execute
var engine = new WorkflowEngine(new ExpressionEvaluator());
await engine.RunAsync(workflow, context);
```

## Architecture

FlowFusion follows clean architecture principles:

- **Core**: Domain models and interfaces
- **Expression**: Expression evaluation engine
- **RunTime**: Workflow execution engine
- **Builder**: Fluent APIs for workflow construction
- **Example**: Usage demonstrations

## Expression Syntax

Supports C# syntax for dynamic evaluations:

```csharp
// Variables
x > 5
context.Variables["userId"]

// Complex expressions
(user.Age >= 18 && user.Balance > 100) || user.IsVip

// Method calls
DateTime.Now.Hour > 9
Math.Max(a, b) > threshold
```

## Installation

```bash
dotnet add package FlowFusion.Core
dotnet add package FlowFusion.Expression
dotnet add package FlowFusion.RunTime
dotnet add package FlowFusion.Builder
```

## Building and Testing

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run example
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
