using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HyggeLang
{
    internal class Parser
    {
        private class ParseError : Exception { }

        private readonly List<Token> _tokens;
        private int _current;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Stmt> Parse()
        {
            List<Stmt> statements = new();

            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.SÆT)) return SætDeclaration();
                return Statement();
            }
            catch (ParseError error)
            {
                Synchronize();
                return null;
            }
        }
        private Stmt Statement()
        {
            if (Match(TokenType.HIVS)) return HvisStatement();
            if (Match(TokenType.SKRIV)) return SkrivStatement();
            if (Match(TokenType.IMENS)) return ImensStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());
            return ExpressionStatement();
        }

        private Stmt HvisStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'hvis'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after hvis betingelse.");

            Stmt thenBranch = Statement();
            Stmt? elseBranch = null;

            if (Match(TokenType.ELLERS))
                elseBranch = Statement();

            return new Stmt.Hvis(condition, thenBranch, elseBranch);
        }

        private Stmt SkrivStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value");
            return new Stmt.Skriv(value);
        }

        private Stmt SætDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Sæt(name, initializer);
        }

        private Stmt ImensStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'imens'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.Imens(condition, body);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Stmt.Expression(expr);
        }

        private List<Stmt> Block()
        {
            List<Stmt> statements = new();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expected '}' after block");
            return statements;
        }

        private Expr Assignment()
        {
            Expr expr = Eller();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Variable)
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Eller()
        {
            Expr expr = Og();

            while (Match(TokenType.ELLER))
            {
                Token @operator = Previous();
                Expr right = Og();
                expr = new Expr.Logical(expr, @operator, right);
            }
            return expr;
        }

        private Expr Og()
        {
            Expr expr = Equality();

            while (Match(TokenType.OG))
            {
                Token @operator = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, @operator, right);
            }
            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comperison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token @operator = Previous();
                Expr right = Comperison();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr Comperison()
        {
            Expr expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token @operator = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, @operator, right);
            }
            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token @operator = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, @operator, right);
            }
            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token @operator = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, @operator, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token @operator = Previous();
                Expr right = Unary();
                return new Expr.Unary(@operator, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(TokenType.FALSK)) return new Expr.Literal(false);
            if (Match(TokenType.SANDT)) return new Expr.Literal(true);
            if (Match(TokenType.INGENTING)) return new Expr.Literal(null);
            if (Match(TokenType.IDENTIFIER)) return new Expr.Variable(Previous());

            if (Match(TokenType.NUMBER, TokenType.STRING)) return new Expr.Literal(Previous().Literal);

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Exepct expression");
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private ParseError Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.KLASSE:
                    case TokenType.GØREMÅL:
                    case TokenType.SÆT:
                    case TokenType.FOR:
                    case TokenType.IMENS:
                    case TokenType.SKRIV:
                    case TokenType.RETURNER:
                        return;
                }

                Advance();
            }
        }
    }
}
