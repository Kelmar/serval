using System;

namespace LangTest
{
    public partial class Lexer
    {
        private string ReadWordLiteral() => ReadWhile(c => c == '_' || Char.IsLetterOrDigit(c));

        private Token ReadKeywordOrIdentifier()
        {
            string word = ReadWordLiteral();

            if (!String.IsNullOrWhiteSpace(word))
            {
                return new Token(word, word switch
                {
                    "for" => TokenType.For,
                    "int" => TokenType.Int,
                    "class" => TokenType.Class,
                    "const" => TokenType.Const,
                    "string" => TokenType.String,
                    "struct" => TokenType.Struct,
                    _ => TokenType.Identifier
                }, m_lineNumber);
            }

            return null;
        }
    }
}
