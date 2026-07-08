// @file BoundExpression.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Numerics;
using Prism.Core.Ast;
using Prism.Core.Semantic.Symbols;
using Prism.Core.Utils;
using Singulink.Numerics;

namespace Prism.Core.Semantic.Binding;

public abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get;}
    
    public virtual bool IsLValue => false;
    
    public virtual bool IsAssignable => false;
    
    private protected BoundExpression(SyntaxNode syntax, SemanticModel semanticModel) : base(syntax, semanticModel)
    {
    }
}

public sealed class BoundBoolLiteralExpression : BoundExpression
{
    public override TypeSymbol Type => SemanticModel.BuiltInTypes.Bool;
    public bool Value { get; }
    
    internal BoundBoolLiteralExpression(SyntaxNode syntax, SemanticModel semanticModel, bool value) : base(syntax, semanticModel)
    {
        Value = value;
    }
}

public sealed class BoundIntegerLiteralExpression : BoundExpression
{
    public override TypeSymbol Type => SemanticModel.BuiltInTypes.GetTypeSymbol(Suffix);
    public BigInteger Value { get; }
    public IntegerSuffix Suffix { get; }
    
    internal BoundIntegerLiteralExpression(SyntaxNode syntax, SemanticModel model, BigInteger value, IntegerSuffix suffix) : base(syntax, model)
    {
        Value = value;
        Suffix = suffix;
    }
}

public sealed class BoundFloatLiteralExpression : BoundExpression
{
    public override TypeSymbol Type => SemanticModel.BuiltInTypes.GetTypeSymbol(Suffix);
    public BigDecimal Value { get; }
    public FloatSuffix Suffix { get; }
    
    internal BoundFloatLiteralExpression(SyntaxNode syntax, SemanticModel model, BigDecimal value, FloatSuffix suffix) : base(syntax, model)
    {
        Value = value;
        Suffix = suffix;
    }
}

public sealed class BoundStringLiteralExpression : BoundExpression
{
    public string Value { get; }

    public override TypeSymbol Type => SemanticModel.BuiltInTypes.Str;
    
    internal BoundStringLiteralExpression(SyntaxNode syntax, SemanticModel model, string value) : base(syntax, model)
    {
        Value = value;
    }
}

public sealed class BoundVariableExpression : BoundExpression
{
    public ValueSymbol Symbol { get; }

    public override TypeSymbol Type { get; }
    public override bool IsLValue => true;
    
    public override bool IsAssignable => Symbol.IsAssignable;
    
    internal BoundVariableExpression(ValueSymbol symbol, TypeSymbol type) : base(symbol.Declaration.RequireNonNull(), symbol.Compilation.SemanticModel)
    {
        Symbol = symbol;
        Type = type;
    }
}

public sealed class BoundFunctionReferenceExpression : BoundExpression
{
    public CallableSymbol Function { get; }

    // TODO: Overload sets and function references will eventually be considered their own unique types
    public override TypeSymbol Type => SemanticModel.ErrorTypeSymbol;
    
    internal BoundFunctionReferenceExpression(SyntaxNode syntax, FunctionSymbol function) : base(syntax, function.Compilation.SemanticModel)
    {
        Function = function;
    }
}

public sealed class BoundOverloadSetExpression : BoundExpression
{
    public ImmutableArray<CallableSymbol> Functions { get; }

    // TODO: Overload sets and function references will eventually be considered their own unique types
    public override TypeSymbol Type => SemanticModel.ErrorTypeSymbol;
    
    internal BoundOverloadSetExpression(SyntaxNode syntax, SemanticModel semanticModel, ImmutableArray<CallableSymbol> functions) : base(syntax, semanticModel)
    {
        Functions = functions;
    }
}

public sealed class BoundConversionExpression : BoundExpression
{

    public override TypeSymbol Type { get; }
    public BoundExpression Input { get; }

    public bool IsImplicit { get; }
    
    internal BoundConversionExpression(SyntaxNode syntax, TypeSymbol type, BoundExpression input, bool isImplicit = false) : base(syntax, input.SemanticModel)
    {
        Input = input;
        IsImplicit = isImplicit;
        Type = type;
    }
}

public sealed class BoundUnaryExpression : BoundExpression
{
    public override TypeSymbol Type { get; }
    public BoundExpression Operand { get; }

    public UnaryOperator Operator { get; }
    
    internal BoundUnaryExpression(SyntaxNode syntax, BoundExpression operand, UnaryOperator @operator, TypeSymbol resultType) : base(syntax, operand.SemanticModel)
    {
        Type = resultType;
        Operand = operand;
        Operator = @operator;
    }
}

public sealed class BoundBinaryExpression : BoundExpression
{
    public override TypeSymbol Type { get; }
    public BoundExpression Left { get; }

    public BoundExpression Right { get; }

    public BoundBinaryOperator Operator { get; }
    
    internal BoundBinaryExpression(SyntaxNode syntax, BoundExpression left, BoundExpression right,
        BoundBinaryOperator @operator, TypeSymbol resultType) : base(syntax, left.SemanticModel)
    {
        Left = left;
        Right = right;
        Operator = @operator;
        Type = resultType;
    }
}

public sealed class BoundAssignmentExpression : BoundExpression
{
    public BoundExpression Assignee { get; }
    public BoundExpression Value { get; }

    public override TypeSymbol Type => SemanticModel.BuiltInTypes.Void;
    
    internal BoundAssignmentExpression(SyntaxNode syntax, SemanticModel semanticModel, BoundExpression assignee, BoundExpression value) : base(syntax, semanticModel)
    {
        Assignee = assignee;
        Value = value;
    }
}

public sealed class BoundConditionalExpression : BoundExpression
{

    public override TypeSymbol Type { get; }
    public BoundExpression Condition { get; }

    public BoundExpression TrueExpression { get; }

    public BoundExpression FalseExpression { get; }
    
    internal BoundConditionalExpression(SyntaxNode syntax, BoundExpression condition, BoundExpression trueExpression,
        BoundExpression falseExpression, TypeSymbol resultType) : base(syntax, condition.SemanticModel)
    {
        Condition = condition;
        TrueExpression = trueExpression;
        FalseExpression = falseExpression;
        Type = resultType;
    }
}

public sealed class BoundInvocationExpression : BoundExpression
{
    public CallableSymbol CalledFunction { get; }

    public override TypeSymbol Type { get; }

    public ImmutableArray<BoundExpression> Arguments { get; }
    
    internal BoundInvocationExpression(SyntaxNode syntax, CallableSymbol calledFunction, TypeSymbol returnType, ImmutableArray<BoundExpression> arguments) : base(syntax, calledFunction.Compilation.SemanticModel)
    {
        CalledFunction = calledFunction;
        Type = returnType;
        Arguments = arguments;
    }
}
