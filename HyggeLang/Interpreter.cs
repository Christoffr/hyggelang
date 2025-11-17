using HyggeLang.NativeClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyggeLang
{
    internal class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
    {
        internal readonly Environment _gloabal = new();
        private Environment _environment;

        public Interpreter()
        {
            _gloabal.Define("Clock", new Clock());
            _environment = _gloabal;
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Program.RuntimeError(error);
            }
        }

        #region Staments
        public object? VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }

        public object? VisitSkrivStmt(Stmt.Skriv stmt)
        {
            object? value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object? VisitSætStmt(Stmt.Sæt stmt)
        {
            object? value = null;

            if (stmt.initializer != null)
                value = Evaluate(stmt.initializer);

            _environment.Define(stmt.name.Lexeme, value);
            return null;
        }

        public object? VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Environment(_environment));
            return null;
        }

        public object? VisitHvisStmt(Stmt.Hvis stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if(stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object? VisitImensStmt(Stmt.Imens stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }

        public object? VisitGøremålStmt(Stmt.Gøremål stmt)
        {
            HLGøremål gøremål = new HLGøremål(stmt);
            _environment.Define(stmt.name.Lexeme, gøremål);
            return null;
        }

        #endregion

        #region Expressions
        public object VisitAssignExpr(Expr.Assign expr)
        {
            object? value = Evaluate(expr.value);
            _environment.Assign(expr.name, value);
            return value;
        }

        public object? VisitBinaryExpr(Expr.Binary expr)
        {
            object? left = Evaluate(expr.left);
            object? right = Evaluate(expr.right);

            switch (expr.@operator.Type)
            {
                case TokenType.GREATER:
                    CheckNumberOperand(expr.@operator, left, right);
                    return (double?)left > (double?)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperand(expr.@operator, left, right);
                    return (double?)left >= (double?)right;
                case TokenType.LESS:
                    CheckNumberOperand(expr.@operator, left, right);
                    return (double?)left < (double?)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperand(expr.@operator, left, right);
                    return (double?)left <= (double?)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    CheckNumberOperand(expr.@operator, left, right);
                    return IsEqual(left, right);
                case TokenType.PLUS:
                    if (left is double && right is double)
                        return (double?)left + (double?)right;
                    if (left is string && right is string)
                        return (string?)left + (string?)right;
                    throw new RuntimeError(expr.@operator, "Operands must match.");
                case TokenType.MINUS:
                    CheckNumberOperand(expr.@operator, left, right);
                    return (double?)left - (double?)right;
                case TokenType.SLASH:
                    CheckNumberOperand(expr.@operator, left, right);
                    return (double?)left / (double?)right;
                case TokenType.STAR:
                    CheckNumberOperand(expr.@operator, left, right);
                    return (double?)left * (double?)right;
                default:
                    break;
            }

            // Unreachable
            return null;
        }

        public object? VisitCallExpr(Expr.Call expr)
        {
            object? callee = Evaluate(expr.callee);

            List<object?> arguments = new();

            foreach (Expr argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (callee is not IHLCallable)
            {
                throw new RuntimeError(expr.paren,"Can only call functions and classes.");
            }

            IHLCallable function = (IHLCallable)callee;

            if (arguments.Count != function.Arity)
            {
                throw new RuntimeError(expr.paren, $"Expected {function.Arity} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            throw new NotImplementedException();
        }

        public object? VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object? VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object? VisitLogicalExpr(Expr.Logical expr)
        {
            object? left = Evaluate(expr.left);

            if (expr.@operator.Type == TokenType.ELLER)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
                else
                {
                    if(!IsTruthy(left))
                    {
                        return left;
                    }
                }
            }
            return Evaluate(expr.right);
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            throw new NotImplementedException();
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            throw new NotImplementedException();
        }

        public object VisitThisExpr(Expr.This expr)
        {
            throw new NotImplementedException();
        }

        public object? VisitUnaryExpr(Expr.Unary expr)
        {
            object? right = Evaluate(expr.right);

            switch (expr.@operator.Type)
            {

                case TokenType.MINUS:
                    CheckNumberOperand(expr.@operator, right);
                    return -(double?)right;
                case TokenType.BANG: 
                    return !IsTruthy(right);
                default:
                    return null;
            }

            // Unreachable
            return null; 
        }

        public object? VisitVariableExpr(Expr.Variable expr)
        {
            return _environment.Get(expr.name);
        }
        #endregion

        #region Helper functions
        private object? Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public void ExecuteBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = _environment;

            try
            {
                _environment = environment;

                foreach (Stmt stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {

                _environment = previous;
            }
        }

        private bool IsTruthy(object? obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;

            return true;
        }

        private bool IsEqual(object? a, object? b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b);
        }

        private string Stringify(object? obj)
        {
            if (obj == null) return "ingenting";

            if (obj is bool b) return b ? "sandt" : "falsk";

            if (obj is double)
            {
                string? text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.Substring(0, text.Length - 2);
                }
                return text;
            }

            return obj.ToString();
        }

        private void CheckNumberOperand(Token @operator, object? operand)
        {
            if (operand is double && operand is not null) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }

        private void CheckNumberOperand(Token @operator, object? left, object? right)
        {
            if (left is double && right is double) return;
            throw new RuntimeError(@operator, "Operand must be a number.");
        }
    }
    #endregion
}
