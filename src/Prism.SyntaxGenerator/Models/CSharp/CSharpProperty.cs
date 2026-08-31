using System.Diagnostics.CodeAnalysis;
using Prism.SyntaxGenerator.Models.Resolved;
using Prism.SyntaxGenerator.Models.Spec;

namespace Prism.SyntaxGenerator.Models.CSharp;

public class CSharpProperty
{
    public CSharpNode Owner { get; }
    public string Name { get; }
    public string PropertyName { get; }
    public string ParameterName { get; }
    public string FieldName { get; }
    public CSharpTypeReference Type { get; }
    public PropertyShape Shape { get; internal set; }

    [MemberNotNullWhen(true, nameof(OverrideOf))]
    public bool IsOverride => OverrideOf is not null;
    public CSharpProperty? OverrideOf { get; internal set; }

    internal CSharpProperty(
        CSharpNode owner,
        string name,
        string propertyName,
        string parameterName,
        string fieldName,
        CSharpTypeReference type,
        PropertyShape shape
    )
    {
        Owner = owner;
        Name = name;
        PropertyName = propertyName;
        ParameterName = parameterName;
        FieldName = fieldName;
        Type = type;
        Shape = shape;
    }
}
