namespace FlowFusion.Core.Interfaces;

internal interface ITokenizer
{
    List<Token> Tokenize(string expression);
}