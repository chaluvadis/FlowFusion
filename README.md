# FlowFusion

A high-performance workflow execution engine with built-in expression evaluation capabilities, designed following clean architecture principles.

## Overview

FlowFusion enables the creation and execution of complex workflows with conditional branching based on dynamic expressions. The engine supports C# syntax expressions that can access workflow context variables, enabling powerful decision-making logic in automated processes.

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com)

## Architecture

FlowFusion follows clean architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────┐
│             Example                 │
│   (Application Entry Point)         │
└─────────────────┬───────────────────┘
                  │
┌─────────────────┴───────────────────┐
│             Builder                 │
│   (Workflow Construction)           │
└─────────────────┬───────────────────┘
                  │
┌─────────────────┴───────────────────┐
│             RunTime                 │
│   (Workflow Execution Engine)       │
│   ├── WorkflowEngine                │
│   └── Services                      │
└─────────────────┬───────────────────┘
                  │
┌─────────────────┴───────────────────┐
│             Expression              │
│   (Expression Evaluation)           │
│   ├── ExpressionEvaluator           │
│   └── ExpressionParser              │
└─────────────────┬───────────────────┘
                  │
┌─────────────────┴───────────────────┐
│              Core                   │
│   (Domain Models & Interfaces)      │
│   ├── IExpressionEvaluator          │
│   ├── FlowExecutionContext          │
│   ├── ExecutionResult               │
│   ├── ConditionalTransition         │
│   ├── IWorkflow                     │
│   └── IBlock                        │
└─────────────────────────────────────┘
```

### Layer Responsibilities

- **Core**: Contains domain models, interfaces, and business rules. Defines contracts that other layers implement.
- **Expression**: Implements expression evaluation using compiled C# expressions for high performance.
- **RunTime**: Provides the workflow execution engine that orchestrates block execution and conditional transitions.
- **Builder**: Offers fluent APIs for constructing workflows programmatically.
- **Example**: Demonstrates usage patterns and integration examples.

## Key Features

- **High-Performance Expression Evaluation**: Uses compiled LINQ expressions for fast execution
- **C# Syntax Support**: Full C# expression syntax including operators, method calls, and property access
- **Context-Aware**: Expressions can access workflow variables and context data
- **Conditional Transitions**: Dynamic branching based on expression results
- **Async Execution**: Fully asynchronous workflow execution with cancellation support
- **Extensible Architecture**: Plugin-based design for custom blocks and evaluators

## Expression Syntax

Expressions support standard C# syntax:

```csharp
// Variable access
x > 5
context.Variables["user"].Age >= 18

// Complex expressions
(x + y) * 2 == z && status == "active"

// Method calls
DateTime.Now.Hour > 9
Math.Max(a, b) > threshold

// Property access
user.Profile.IsActive && user.Balance > 100
```

## Usage Example

```csharp
// Create workflow context
var context = new FlowExecutionContext(new Dictionary<string, object?> {
    ["x"] = 10.0,
    ["status"] = "active"
});

// Build workflow
var workflow = new WorkflowBuilder()
    .AddBlock("start", new CustomBlock())
    .AddConditionalTransition("start", "approved", "x > 5 && status == \"active\"")
    .AddBlock("approved", new ApprovalBlock())
    .Build();

// Execute workflow
var engine = new WorkflowEngine(new ExpressionEvaluator());
await engine.RunAsync(workflow, context);
```

## Building and Testing

```bash
# Build all projects
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
├── FlowFusion.Expression/    # Expression evaluation implementation
├── FlowFusion.RunTime/       # Workflow execution engine
└── FlowFusion.Builder/       # Workflow construction APIs

test/
└── FlowFusion.Tests/         # Unit tests

example/
└── FlowFusion.Example/       # Usage examples
```

## Dependencies

- **Core**: No external dependencies
- **Expression**: Depends on Core
- **RunTime**: Depends on Core
- **Builder**: Depends on Core
- **Tests**: Depends on all projects

## Contributing

1. Follow clean architecture principles
2. Add tests for new features
3. Update documentation
4. Ensure all builds pass

## License

See LICENSE file for details.
