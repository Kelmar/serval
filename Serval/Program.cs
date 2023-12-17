using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Serval.CodeGen;
using Serval.Lexing;

namespace Serval
{
    class Program
    {
        private static string s_filename;

        static void Compile(string filename)
        {
            var reporter = new Reporter();
            var symbolTable = new SymbolTable();

            symbolTable.InitGlobal();

            using var input = File.OpenRead(filename);
            using var lex = new Lexer(input, reporter);

            using var parser = new Parser(symbolTable, lex, reporter);

            var module = parser.ParseModule();

            // By this point we should have a correct program

            if (reporter.ErrorCount == 0)
            {
                using var gen = new TestExec();
                gen.Execute(module);
            }

            reporter.Summary();
        }

        static bool ParseArguments(string[] args)
        {
            var argStack = new Stack<string>(args.Reverse());

            while (argStack.Any())
            {
                string arg = argStack.Pop();

                s_filename = arg;
            }

            if (String.IsNullOrWhiteSpace(s_filename))
            {
                Console.Error.WriteLine("No input files.");
                return false;
            }

            return true;
        }

        static void Main(string[] args)
        {
            try
            {
                if (!ParseArguments(args))
                    return;

                Compile(s_filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine("FATAL: {0}", ex);
            }
        }
    }
}
