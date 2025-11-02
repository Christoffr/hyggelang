using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyggeLang
{
    internal class Token
    {
        readonly public TokenType Type;
        readonly public string Lexeme;
        readonly public object? Literal;
        readonly public int Line;

        public Token(TokenType type, string lexeme, object? literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
        }

        public override string ToString()
        {
            return Type + " " + Lexeme + " " + Literal;
        }
    }
}
