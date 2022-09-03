namespace TheOmenDen.Shared.EfCore.Translators;


internal sealed class BetweenToSql : ConditionalExpression
{
    public override bool CanReduce => base.CanReduce;

    public override Type Type => base.Type;

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override Expression Reduce()
    {
        return base.Reduce();
    }

    public override string ToString()
    {
        return base.ToString();
    }

    protected override Expression Accept(ExpressionVisitor visitor)
    {
        return base.Accept(visitor);
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        return base.VisitChildren(visitor);
    }
}