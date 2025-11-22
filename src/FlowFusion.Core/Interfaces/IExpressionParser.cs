namespace FlowFusion.Core.Interfaces;

internal interface IExpressionParser
{
    Expression Parse(IReadOnlyList<Token> tokens, ParameterExpression contextParam, string variablesPropertyName = "Variables", string contextIdentifier = "context", bool autoVariablesMode = true);
}