﻿using System;

namespace Serval.Lexing
{
    public partial class Lexer
    {
        private string ReadWordLiteral() => ReadWhile(c => c == '_' || Char.IsLetterOrDigit(c));

        private Token ReadKeywordOrIdentifier()
        {
            int start = m_linePos;

            string word = ReadWordLiteral();

            if (!String.IsNullOrWhiteSpace(word))
            {
                return new Token(word, word switch
                {
                    "_" => TokenType.Discard,
                    "for" => TokenType.For,
                    "var" => TokenType.Var,
                    "class" => TokenType.Class,
                    "const" => TokenType.Const,
                    "sizeof" => TokenType.SizeOf,
                    "struct" => TokenType.Struct,
                    "typeof" => TokenType.TypeOf,
                    _ => TokenType.Identifier
                }, m_lineNumber, start, m_linePos);
            }

            return null;
        }
    }
}
