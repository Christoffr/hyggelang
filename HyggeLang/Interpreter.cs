using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyggeLang
{
    internal class Interpreter : Expr.IVisitor<object?>
    {
        public void Interpret(Expr expression)
        {
            try
            {
                object? value = Evaluate(expression);
                Console.WriteLine(value);
            }
            catch (RuntimeError error)
            {
                Program.RuntimeError(error);
            }
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            throw new NotImplementedException();
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

        public object VisitCallExpr(Expr.Call expr)
        {
            throw new NotImplementedException();
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

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            throw new NotImplementedException();
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

        public object VisitVariableExpr(Expr.Variable expr)
        {
            throw new NotImplementedException();
        }

        private object? Evaluate(Expr expr)
        {
            return expr.Accept(this);
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
}
