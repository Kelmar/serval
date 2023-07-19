using System;
using System.IO;
using System.Linq;

using Serval.CodeGen;
using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    class Program
    {
        static void Test1()
        {
            var rep = new Reporter();

            using var s = File.OpenRead("test.svl");
            using var lex = new Lexer(s, rep);

            using var parser = new Parser(lex, rep);

            var module = parser.ParseModule();

            // By this point we should have a correct program

            if (rep.ErrorCount == 0)
            {
                using var gen = new Generator();
                gen.Generate(module);
            }

            Console.WriteLine("Errors: {0}, Warnings: {1}", rep.ErrorCount, rep.WarnCount);
        }

        static void Main(string[] args)
        {
            try
            {
                Test1();
            }
            catch (CompilerBugException ex)
            {
                Console.WriteLine("COMPILER BUG!\r\n{0}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
            }
        }
    }
}
