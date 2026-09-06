namespace Prism.Core.Semantic;

public enum UnaryOperation : byte
{
    Identity,
    Negation,
    LogicalNot,
    BitwiseNot,
    PreIncrement,
    PreDecrement,
    PostIncrement,
    PostDecrement,
}

public enum BinaryOperation : byte
{
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Modulo,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LogicalAnd,
    LogicalOr,
    Equality,
    NotEquals,
    LessThan,
    LessThanOrEquals,
    GreaterThan,
    GreaterThanOrEquals,
    ThreeWayComparison,
    ShiftLeft,
    ShiftRight,
    UnsignedShiftRight,
};

public enum AssignmentOperation : byte
{
    Simple,
    Addition,
    Subtraction,
    Multiplication,
    Division,
    Modulo,
    BitwiseAnd,
    BitwiseOr,
    BitwiseXor,
    LogicalAnd,
    LogicalOr,
    ShiftLeft,
    ShiftRight,
    UnsignedShiftRight,
};

public static class OperationExtensions
{
    extension(UnaryOperation op)
    {
        public bool IsAssigning =>
            op
                is UnaryOperation.PreIncrement
                    or UnaryOperation.PreDecrement
                    or UnaryOperation.PostIncrement
                    or UnaryOperation.PostDecrement;
    }
}
