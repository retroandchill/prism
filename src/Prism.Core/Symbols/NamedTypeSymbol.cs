namespace Prism.Core.Symbols;

public abstract class NamedTypeSymbol : TypeSymbol
{
    private protected NamedTypeSymbol(
        string name,
        Symbol? containingSymbol,
        SpecialType specialType = SpecialType.None
    )
        : base(name, containingSymbol, specialType) { }

    public sealed override void WriteDisplayString(TextWriter writer)
    {
        switch (SpecialType)
        {
            case SpecialType.Void:
                writer.Write("void");
                break;
            case SpecialType.Bool:
                writer.Write("bool");
                break;
            case SpecialType.I8:
                writer.Write("i8");
                break;
            case SpecialType.I16:
                writer.Write("i16");
                break;
            case SpecialType.I32:
                writer.Write("i32");
                break;
            case SpecialType.I64:
                writer.Write("i64");
                break;
            case SpecialType.I128:
                writer.Write("i128");
                break;
            case SpecialType.ISize:
                writer.Write("isize");
                break;
            case SpecialType.U8:
                writer.Write("u8");
                break;
            case SpecialType.U16:
                writer.Write("u16");
                break;
            case SpecialType.U32:
                writer.Write("u32");
                break;
            case SpecialType.U64:
                writer.Write("u64");
                break;
            case SpecialType.U128:
                writer.Write("u128");
                break;
            case SpecialType.USize:
                writer.Write("usize");
                break;
            case SpecialType.F32:
                writer.Write("f32");
                break;
            case SpecialType.F64:
                writer.Write("f64");
                break;
            case SpecialType.Char:
                writer.Write("char");
                break;
            case SpecialType.Char16:
                writer.Write("char16");
                break;
            case SpecialType.Rune:
                writer.Write("rune");
                break;
            case SpecialType.Str:
                writer.Write("str");
                break;
            case SpecialType.None:
            default:
            {
                if (ContainingNamespace is { IsGlobal: false } container)
                {
                    container.WriteDisplayString(writer);
                    writer.Write("::");
                }

                writer.Write(Name);
                break;
            }
        }
    }
}
