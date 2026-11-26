// @file Handles.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using JetBrains.Annotations;
using MagicArchive;
using Prism.Constants;
using CharacterConstant = Prism.Constants.CharacterConstant;
using ConstantHeader = Prism.Constants.ConstantHeader;
using ConstantValue = Prism.Constants.ConstantValue;
using IntegerConstant = Prism.Constants.IntegerConstant;
using IntegerKind = Prism.Constants.IntegerKind;

namespace Prism.Metadata;

public interface ITableRowHandle<out THandle>
    where THandle : struct, ITableRowHandle<THandle>
{
    internal static abstract THandle Create(int id);
}

[Archivable]
[Handle]
public readonly partial record struct StringHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial StringHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct BlobHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial BlobHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct InlineConstHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private const int KindMask = 0x00FF0000;
    private const int KindShift = 16;
    private const int PayloadMask = 0x0000FFFF;

    private partial InlineConstHandle([UsedImplicitly] int id);

    public static InlineConstHandle? TryCreate(in ConstantValue value)
    {
        return value switch
        {
            // ReSharper disable once ShiftExpressionZeroLeftOperand
            NullConstant => new InlineConstHandle(ConstantHeader.Null.AsRaw() << KindShift | 1),
            bool b => new InlineConstHandle(ConstantHeader.Bool.AsRaw() << KindShift | (b ? 1 : 0)),
            IntegerConstant i => TryCreateFromInteger(in i),
            CharacterConstant c => TryCreateFromCharacter(c),
            _ => null,
        };
    }

    private static InlineConstHandle? TryCreateFromInteger(in IntegerConstant value)
    {
        // We can save space by storing larger integer types that can be represented by a small type inline
        var header = ConstantHeader.Integer(value.Kind);
        return value switch
        {
            sbyte i => Create(header, i),
            byte i => Create(header, i),
            short i => Create(header, i),
            ushort i => Create(header, i),
            int i and <= short.MaxValue and >= short.MinValue => Create(header, (short)i),
            uint i and <= ushort.MaxValue => Create(header, (ushort)i),
            long i and <= short.MaxValue and >= short.MinValue => Create(header, (short)i),
            ulong i and <= ushort.MaxValue => Create(header, (ushort)i),
            Int128 i when i <= short.MaxValue && i >= short.MinValue => Create(header, (short)i),
            UInt128 i when i <= ushort.MaxValue => Create(header, (ushort)i),
            _ => null,
        };
    }

    private static InlineConstHandle? TryCreateFromCharacter(CharacterConstant value)
    {
        var header = ConstantHeader.Character(value.Kind);
        return value switch
        {
            byte utf8 => Create(header, utf8),
            char utf16 => Create(header, utf16),
            Rune utf32 when (uint)utf32.Value <= char.MaxValue => Create(
                header,
                unchecked((ushort)utf32.Value)
            ),
            _ => null,
        };
    }

    private static InlineConstHandle Create(ConstantHeader header, short value)
    {
        return Create(header, unchecked((ushort)value));
    }

    private static InlineConstHandle Create(ConstantHeader header, ushort value)
    {
        return new InlineConstHandle(header.AsRaw() << KindShift | value);
    }

    public ConstantValue GetValue()
    {
        if (IsNone)
            throw new InvalidOperationException("Constant value is not set");

        var rawKind = (byte)((_id & KindMask) >> KindShift);
        var header = ConstantHeader.FromRaw(rawKind);
        return header.Kind switch
        {
            ConstantKind.Null => new NullConstant(),
            ConstantKind.Bool => (_id & 1) != 0,
            ConstantKind.Integer => ReadIntegerConstant(header.IntegerKind),
            ConstantKind.Character => ReadCharacterConstant(header.CharacterKind),
            _ => throw new InvalidDataException("Not a constant type that can be inlined"),
        };
    }

    private ConstantValue ReadIntegerConstant(IntegerKind kind)
    {
        var payload = (ushort)(_id & PayloadMask);
        return kind.Sign switch
        {
            IntegerSign.Unsigned => kind.Width switch
            {
                IntegerWidth.W8 => new IntegerConstant((byte)payload),
                IntegerWidth.W16 => new IntegerConstant(payload),
                IntegerWidth.W32 => new IntegerConstant((uint)payload),
                IntegerWidth.W64 => new IntegerConstant((ulong)payload),
                IntegerWidth.W128 => new IntegerConstant((UInt128)payload),
                _ => throw new InvalidOperationException("Invalid width"),
            },
            IntegerSign.Signed => kind.Width switch
            {
                IntegerWidth.W8 => new IntegerConstant((sbyte)unchecked((short)payload)),
                IntegerWidth.W16 => new IntegerConstant(unchecked((short)payload)),
                IntegerWidth.W32 => new IntegerConstant((int)unchecked((short)payload)),
                IntegerWidth.W64 => new IntegerConstant((long)unchecked((short)payload)),
                IntegerWidth.W128 => new IntegerConstant((Int128)unchecked((short)payload)),
                _ => throw new InvalidOperationException("Invalid width"),
            },
            _ => throw new InvalidOperationException("Invalid sign"),
        };
    }

    private ConstantValue ReadCharacterConstant(CharacterKind kind)
    {
        var payload = (ushort)(_id & PayloadMask);
        return kind switch
        {
            CharacterKind.C8 => new CharacterConstant((byte)payload),
            CharacterKind.C16 => new CharacterConstant((char)payload),
            CharacterKind.C32 => new CharacterConstant(new Rune(payload)),
            _ => throw new InvalidOperationException("Invalid character kind"),
        };
    }

    public ConstantHandle ToConstantHandle() => this;
}

[Archivable]
[CompositeHandle(typeof(StringHandle), typeof(BlobHandle), typeof(InlineConstHandle))]
public readonly partial record struct ConstantHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial ConstantHandle([UsedImplicitly] MetadataToken token);
}

[Archivable]
[Handle]
public readonly partial record struct AssemblyRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial AssemblyRefHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct NamespaceDefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial NamespaceDefHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct NamespaceRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial NamespaceRefHandle([UsedImplicitly] int id);
}

[Archivable]
[CompositeHandle(typeof(AssemblyRefHandle), typeof(NamespaceRefHandle))]
public readonly partial record struct NamespaceOrAssemblyRefHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial NamespaceOrAssemblyRefHandle([UsedImplicitly] MetadataToken token);

    public static implicit operator MemberContainerRefHandle(NamespaceOrAssemblyRefHandle handle)
    {
        return handle switch
        {
            AssemblyRefHandle asm => asm,
            NamespaceRefHandle ns => ns,
            null => MemberContainerRefHandle.None,
        };
    }
}

[Archivable]
[Handle]
public readonly partial record struct TypeDefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeDefHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct TypeRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeRefHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct TypeSpecHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeSpecHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct TypeAliasHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial TypeAliasHandle([UsedImplicitly] int id);
}

[Archivable]
[CompositeHandle(
    typeof(TypeDefHandle),
    typeof(TypeRefHandle),
    typeof(TypeSpecHandle),
    typeof(TypeAliasHandle)
)]
public readonly partial record struct TypeHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial TypeHandle([UsedImplicitly] MetadataToken token);
}

[Archivable]
[CompositeHandle(typeof(TypeDefHandle), typeof(NamespaceDefHandle))]
public readonly partial record struct MemberContainerDefHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial MemberContainerDefHandle([UsedImplicitly] MetadataToken token);
}

[Archivable]
[CompositeHandle(typeof(TypeRefHandle), typeof(NamespaceRefHandle), typeof(AssemblyRefHandle))]
public readonly partial record struct MemberContainerRefHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial MemberContainerRefHandle([UsedImplicitly] MetadataToken token);
}

[Archivable]
[Handle]
public readonly partial record struct VariableHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial VariableHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct FunctionHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial FunctionHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct ParameterHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial ParameterHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct EnumOrdinalHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial EnumOrdinalHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct CustomAttributeHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial CustomAttributeHandle([UsedImplicitly] int id);
}

[Archivable]
[CompositeHandle(typeof(FunctionHandle), typeof(TypeDefHandle))]
public readonly partial record struct FunctionOrAttributeHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial FunctionOrAttributeHandle([UsedImplicitly] MetadataToken token);
}

[Archivable]
[CompositeHandle(typeof(StringHandle), typeof(BlobHandle), typeof(InlineConstHandle))]
public readonly partial record struct ParameterDefaultHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial ParameterDefaultHandle([UsedImplicitly] MetadataToken token);

    public static implicit operator ParameterDefaultHandle(ConstantHandle handle)
    {
        return handle switch
        {
            StringHandle h => h,
            BlobHandle h => h,
            InlineConstHandle h => h,
        };
    }

    public static explicit operator ConstantHandle(ParameterDefaultHandle handle)
    {
        return handle switch
        {
            StringHandle h => h,
            BlobHandle h => h,
            InlineConstHandle h => h,
        };
    }
}

[Archivable]
[CompositeHandle(
    typeof(TypeDefHandle),
    typeof(VariableHandle),
    typeof(FunctionHandle),
    typeof(ParameterHandle),
    typeof(EnumOrdinalHandle)
)]
public readonly partial record struct AttributeHolderHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial AttributeHolderHandle([UsedImplicitly] MetadataToken token);
}

[Archivable]
[Handle]
public readonly partial record struct VariableRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial VariableRefHandle([UsedImplicitly] int id);
}

[Archivable]
[Handle]
public readonly partial record struct FunctionRefHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial FunctionRefHandle([UsedImplicitly] int id);
}

[Archivable]
[CompositeHandle(typeof(VariableHandle), typeof(VariableRefHandle))]
public readonly partial record struct VariableDescriptorHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial VariableDescriptorHandle([UsedImplicitly] MetadataToken token);
}

[CompositeHandle(typeof(FunctionHandle), typeof(FunctionRefHandle))]
public readonly partial record struct FunctionDescriptorHandle
{
    [ArchiveInclude]
    private readonly MetadataToken _token;

    private partial FunctionDescriptorHandle([UsedImplicitly] MetadataToken token);
}

[Archivable]
[Handle]
public readonly partial record struct VariableInitializerHandle
{
    [ArchiveInclude]
    private readonly int _id;

    private partial VariableInitializerHandle([UsedImplicitly] int id);
}
