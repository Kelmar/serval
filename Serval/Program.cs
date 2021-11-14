using System;
using System.IO;

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

            var parser = new Parser(lex, rep);

            var tree = parser.BuildTree();

            Console.WriteLine("Errors: {0}, Warnings: {1}", rep.ErrorCount, rep.WarnCount);

            // By this point we should have a correct program

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
