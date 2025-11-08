namespace HyggeLang
{
    internal class Program
    {
        private static Interpreter interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadRuntimeError = false;

        static void Main(string[] args)
        {

            if (args.Length > 1)
            {
                Console.WriteLine("For mange argumenter, du...");
                System.Environment.Exit(64);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPromt();
            }

        }

        private static void RunFile(string path)
        {
            string source = File.ReadAllText(path);
            
            Run(source);
            
            if (hadError)
            {
                System.Environment.Exit(65);
            }
            if (hadRuntimeError)
            {
                System.Environment.Exit(70);
            }
        }

        private static void RunPromt()
        {
            string? input;

            ConsoleKeyInfo cki;

            while (true)
            {
                Console.Write("> ");

                cki = Console.ReadKey();

                if (cki.Key == ConsoleKey.Escape)
                {
                    break;
                }

                input = Console.ReadLine();

                if (input == null)
                {
                    break;
                }
                else
                {
                    Run(input);

                    hadError = false;
                }

            }
        }

        private static void Run(string source)
        {
            Scanner scanner = new(source);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);
            List<Stmt> statement = parser.Parse();

            // Stop if there was a syntax error
            if (hadError) return;

            //Console.WriteLine("Lexer (tokenize)");
            //foreach (var token in tokens)
            //{
            //    Console.WriteLine(token);
            //}
            //Console.WriteLine();

            //Console.WriteLine("Parser (Abstract Syntax Tree)");
            //Console.WriteLine(new AstPrinter().Print(expression));
            //Console.WriteLine();

            //Console.WriteLine("Interpreter");
            interpreter.Interpret(statement);

        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        private static void Report(int line, string where, string message)
        {
            Console.Beep();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            Console.ResetColor();
            hadError = true;
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
            hadRuntimeError = true;
        }
    }
}