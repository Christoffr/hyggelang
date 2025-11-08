namespace HyggeLang
{
    public abstract class Stmt
    {
        internal interface IVisitor<T>
        {
            T VisitExpressionStmt(Expression stmt);
            T VisitSkrivStmt(Skriv stmt);
            T VisitSætStmt(Sæt stmt);
            T VisitBlockStmt(Block stmt);
        }

        internal class Expression : Stmt
        {
            internal Expression(Expr expression)
            {
                this.expression = expression;
            }
            internal override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
            readonly public Expr expression;
        }

        internal class Skriv : Stmt
        {
            internal Skriv(Expr expression)
            {
                this.expression = expression;
            }

            internal override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitSkrivStmt(this);
            }

            readonly public Expr expression;
        }

        internal class Sæt : Stmt
        {
            internal Sæt(Token name, Expr? initializer)
            {
                this.name = name;
                this.initializer = initializer;
            }

            internal override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitSætStmt(this);
            }

            readonly public Token name;
            readonly public Expr? initializer;
        }

        internal class Block : Stmt
        {
            internal Block(List<Stmt> statements)
            {
                this.statements = statements;
            }

            internal override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            readonly public List<Stmt> statements;
        }

        internal abstract T Accept<T>(IVisitor<T> visitor);
    }
}


