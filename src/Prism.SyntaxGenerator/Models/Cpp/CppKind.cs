namespace Prism.SyntaxGenerator.Models.Cpp;

public interface ICppSyntaxVariant;

public class CppKind
{
    public string Name { get; }

    public string CppName { get; }

    public string DisplayName
    {
        get
        {
            return Data switch
            {
                CppTrivia trivia => trivia.DisplayName ?? trivia.Name,
                CppToken token => token.DisplayName ?? token.Text ?? token.Name,
                _ => Name,
            };
        }
    }

    public int Value { get; }

    public ICppSyntaxVariant Data { get; }

    internal CppKind(string name, string cppName, int value, ICppSyntaxVariant data)
    {
        Name = name;
        CppName = cppName;
        Value = value;
        Data = data;
    }
}
