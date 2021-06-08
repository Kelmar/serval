using System;
using System.IO;

namespace LangTest
{
    class Program
    {
        static void Test1()
        {
            var rep = new Reporter();

            var s = File.OpenRead("test.foo");
            var lex = new Lexer(s, rep);

            var parser = new SyntaxParser(lex, rep);

            parser.BuildTree();

            Console.WriteLine("Errors: {0}, Warnings: {1}", rep.ErrorCount, rep.WarnCount);
        }

        static void Main(string[] args)
        {
            try
            {
                Test1();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex);
            }
        }
    }
}
