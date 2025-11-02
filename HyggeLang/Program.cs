namespace HyggeLang
{
    internal class Program
    {
        static bool hadError = false;
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
        }

        private static void RunPromt()
        {
            string? input;

            while (true)
            {
                Console.Write("> ");
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

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Beep();
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[line {line}] Error {where}: {message}");
            Console.ResetColor();
            hadError = true;
        }
    }
}