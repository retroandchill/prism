using System.Diagnostics.CodeAnalysis;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.Cpp;

public class CppProperty
{
    public CppNode Owner { get; }
    public string Name { get; }
    public string GetterName { get; }
    public string FieldName { get; }
    public string ParameterName { get; }
    public CppTypeReference Type { get; }
    public PropertyShape Shape { get; internal set; }

    [MemberNotNullWhen(true, nameof(OverrideOf))]
    public bool IsOverride => OverrideOf is not null;
    public CppProperty? OverrideOf { get; internal set; }

    internal CppProperty(
        CppNode owner,
        string name,
        string getterName,
        string fieldName,
        string parameterName,
        CppTypeReference type,
        PropertyShape shape
    )
    {
        Owner = owner;
        Name = name;
        GetterName = getterName;
        FieldName = fieldName;
        ParameterName = parameterName;
        Type = type;
        Shape = shape;
    }
}
