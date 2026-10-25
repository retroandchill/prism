// @file MetadataEmitter.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Prism.Core.Compiling;
using Prism.Core.Metadata;
using Prism.Core.Semantic;
using Prism.Core.Symbols;
using ConstantKind = Prism.Core.Metadata.ConstantKind;

namespace Prism.Core.Codegen;

internal sealed class MetadataEmitter : IDisposable
{
    private readonly Compilation _compilation;
    private readonly MetadataBuilder _builder;
    private readonly Dictionary<TypeSymbol, TypeHandle> _types = new();

    private readonly Dictionary<(TypeHandle, ConstantValue), ConstantHandle> _constants = new();

    public MetadataEmitter(Compilation compilation)
    {
        _compilation = compilation;
        var assembly = compilation.Assembly;
        var assemblyIdentity = assembly.Identity;
        var targetInfo = new TargetInfo(
            compilation.Settings.Architecture,
            compilation.Settings.OperatingSystem,
            compilation.Settings.Toolchain
        );

        _builder = new MetadataBuilder(assemblyIdentity, targetInfo);
    }

    public async ValueTask EmitAsync(string fileName, CancellationToken cancellationToken)
    {
        AddSymbols(cancellationToken);

        await using var file = File.Create(fileName);
        await _builder.WriteAsync(file, cancellationToken);
    }

    private void AddSymbols(CancellationToken cancellationToken)
    {
        foreach (var symbol in _compilation.Assembly.GlobalNamespace.GetMembers())
        {
            AddSymbol(symbol, MemberContainerDefHandle.None, cancellationToken);
        }
    }

    private void AddSymbol(
        Symbol symbol,
        MemberContainerDefHandle parent,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        switch (symbol)
        {
            case NamespaceSymbol ns:
            {
                var name = _builder.GetOrAddString(symbol.Name);
                var row = new NamespaceDefRow(name, (NamespaceDefHandle)parent);
                var handle = _builder.AddNamespaceDef(row);
                foreach (var member in ns.GetMembers())
                {
                    AddSymbol(member, handle, cancellationToken);
                }
                break;
            }
            case NamedTypeSymbol type:
            {
                var name = _builder.GetOrAddString(symbol.Name);
                var attributes = NamedTypeAttributes.Create(type.Kind, type.DeclaredVisibility);
                var row = new TypeDefinitionRow(attributes, name, parent);
                var handle = _builder.AddTypeDef(row);
                _types.Add(type, handle);
                foreach (var member in type.GetMembers())
                {
                    AddSymbol(member, handle, cancellationToken);
                }

                break;
            }
            case VariableSymbol variable:
            {
                break;
            }
        }
    }

    private TypeHandle GetOrAddType(TypeSymbol typeSymbol)
    {
        if (_types.TryGetValue(typeSymbol, out var handle))
            return handle;

        throw new NotImplementedException();
    }

    private ConstantHandle GetOrAddConstant(in ConstantValue value, TypeSymbol type)
    {
        var typeHandle = GetOrAddType(type);
        if (_constants.TryGetValue((typeHandle, value), out var handle))
        {
            return handle;
        }

        handle = CreateAndStoreConstant(in value, typeHandle);
        _constants.Add((typeHandle, value), handle);
        return handle;
    }

    private ConstantHandle CreateAndStoreConstant(in ConstantValue value, TypeHandle type)
    {
        return value switch
        {
            NullConstant => _builder.AddConstant(new ConstantRow(type, ConstantKind.Null)),
            BoolConstant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            Char8Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            Char16Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            RuneConstant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            I8Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            I16Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            I32Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            I64Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            I128Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            ISizeConstant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            U8Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            U16Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            U32Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            U64Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            U128Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            USizeConstant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            F32Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            F64Constant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.Scalar, _builder.GetOrAddBlob(v))
            ),
            StringConstant(var v) => _builder.AddConstant(
                new ConstantRow(type, ConstantKind.String, _builder.GetOrAddBlob(v))
            ),
            ArrayConstant(var v) => throw new NotImplementedException(),
        };
    }

    public void Dispose()
    {
        _builder.Dispose();
    }
}
