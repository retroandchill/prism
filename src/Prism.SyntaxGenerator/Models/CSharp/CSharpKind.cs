namespace Prism.SyntaxGenerator.Models.CSharp;

public interface ICSharpSyntaxVariant;

public class CSharpKind
{
    public string Name { get; }

    public string CSharpName { get; }

    public string DisplayName
    {
        get
        {
            return Data switch
            {
                CSharpTrivia trivia => trivia.DisplayName ?? trivia.Name,
                CSharpToken token => token.DisplayName ?? token.Text ?? token.Name,
                _ => Name,
            };
        }
    }

    public int Value { get; }

    public ICSharpSyntaxVariant Data { get; }

    internal CSharpKind(string name, string cSharpName, int value, ICSharpSyntaxVariant data)
    {
        Name = name;
        CSharpName = cSharpName;
        Value = value;
        Data = data;
    }
}
