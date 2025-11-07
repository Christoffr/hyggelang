using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyggeLang
{
    internal class Scanner
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new();
        private int start = 0;
        private int current = 0;
        private int line = 1;

        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            { "og", TokenType.OG},
            { "klass", TokenType.KLASSE },
            { "ellers", TokenType.ELLERS },
            { "falsk", TokenType.FALSK },
            { "for", TokenType.FOR },
            { "gøremål", TokenType.GØREMÅL },
            { "hvis", TokenType.HIVS },
            { "ingenting", TokenType.INGENTING },
            { "eller", TokenType.ELLER },
            { "skriv", TokenType.SKRIV },
            { "returner", TokenType.RETURNER },
            { "super", TokenType.SUPER },
            { "dette", TokenType.DETTE },
            { "sandt", TokenType.SANDT },
            { "sæt", TokenType.SÆT },
            { "imens", TokenType.IMENS },
            { "måske", TokenType.MÅSKE }
        };

        public Scanner(string source)
        {
            _source = source;
        }

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                // We are at the beginning of the next lexeme.
                start = current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, line));
            return _tokens;
        }

        private void ScanToken()
        {
            char c = Advance();
            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace.
                    break;

                case '\n':
                    line++;
                    break;
                case '"': literalString(); break;
                default:
                    if (IsDigit(c))
                    {
                        literalNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Program.Error(line, "Unexpected character.");
                    }
                    break;
            }
        }

        private char Advance()
        {
            return _source[current++];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object? literal)
        {
            string text = _source[start..current];
            _tokens.Add(new Token(type, text, literal, line));
        }

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[current] != expected) return false;

            current++;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return _source[current];
        }

        private char PeekNext()
        {
            if (current + 1 >= _source.Length) return '\0';
            return _source[current + 1];
        }

        private void literalString()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if (IsAtEnd())
            {
                Program.Error(line, "Unterminated string.");
                return;
            }

            // The closing ".
            Advance();

            // Trim the surrounding quotes.
            string value = _source[(start + 1)..(current - 1)];
            AddToken(TokenType.STRING, value);
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private void literalNumber()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                // Consume the "."
                Advance();

                while (IsDigit(Peek())) Advance();
            }

            AddToken(TokenType.NUMBER, Double.Parse(_source[start..current], System.Globalization.CultureInfo.InvariantCulture));
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();
            string text = _source[start..current];

            // Try to get the type from the keywords dictionary
            if (!keywords.TryGetValue(text, out TokenType type))
                // If the keyword doesn't exist in the dictionary, it's an identifier
                type = TokenType.IDENTIFIER;

            AddToken(type);
        }

        private bool IsAlpha(char c)
        {
            return char.IsLetter(c) || c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsAtEnd()
        {
            return current >= _source.Length;
        }
    }
}
