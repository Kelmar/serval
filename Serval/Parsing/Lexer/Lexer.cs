using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LangTest
{
    public delegate void ErrorHandler(string format, params object[] args);
    
    public partial class Lexer : IDisposable, IEnumerator<Token>
    {
        private readonly StreamReader m_input;
        private readonly IReporter m_reporter;

        private string m_line;
        private int m_lineNumber;
        private int m_linePos;

        public Lexer(Stream input, IReporter reporter)
        {
            Debug.Assert(input != null);
            Debug.Assert(reporter != null);
            Debug.Assert(input.CanRead);

            m_input = new StreamReader(input);
            m_reporter = reporter;

            m_line = "";
            m_lineNumber = 0;
            m_linePos = 0;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                m_input.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Token Current { get; private set; }

        public Token LookAhead { get; private set; }

        private Char CurrentChar => m_line != null && m_linePos < m_line.Length ? m_line[m_linePos] : '\0';

        object IEnumerator.Current => throw new NotImplementedException();

        private void Error(string fmt, params object[] args)
        {
            m_reporter.Error(m_lineNumber, fmt, args);
        }

        private void Warn(string fmt, params object[] args)
        {
            m_reporter.Warn(m_lineNumber, fmt, args);
        }

        private bool ReadLine()
        {
            m_line = m_input.ReadLine();

            if (m_line == null)
                return false; 

            ++m_lineNumber;
            m_linePos = 0;

            return true;
        }

        private string ReadWhile(Func<char, bool> predicate, bool spanLines = false)
        {
            // If we start at the end of a line, go ahead and read a new line regardless of spanLines.
            if (m_line != null && m_linePos >= m_line.Length)
            {
                if (!ReadLine())
                    return null;
            }

            StringBuilder sb = new StringBuilder();

            int start = m_linePos;

            while (m_line != null)
            {
                if (m_linePos < m_line.Length)
                {
                    if (predicate(CurrentChar))
                        ++m_linePos;
                    else
                        break;
                }
                else
                {
                    if (spanLines)
                    {
                        sb.Append(m_line.Substring(start, m_linePos - start));
                        sb.Append('\n');

                        start = 0;

                        ReadLine();
                    }
                    else
                        break;
                }
            }

            if (start != m_linePos)
                sb.Append(m_line.Substring(start, m_linePos - start));

            return sb.ToString();
        }

        private void EatWhiteSpace() => ReadWhile(Char.IsWhiteSpace, true);

        private Token ReadSingleToken()
        {
            if (m_input.EndOfStream && m_linePos >= m_line.Length)
                return new Token("", TokenType.EndOfFile, m_lineNumber);

            EatWhiteSpace();

            if (m_line != null)
            {
                if (m_linePos >= m_line.Length)
                {
                    if (!ReadLine())
                        return null;
                }

                switch (CurrentChar)
                {
                case '\0':
                    break;

                // Hex starts with 0, numbers cannot start with _
                case '0': 
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ReadNumber();

                //case '\'':
                //    break;

                //case '$':
                //case '`':
                //case '"':
                //    break;

                default:
                    if (Char.IsLetter(CurrentChar))
                        return ReadKeywordOrIdentifier();

                    return ReadSymbol();
                }
            }

            return null;
        }

        public bool MoveNext()
        {
            if (Current?.Type == TokenType.EndOfFile)
                return false;

            if (LookAhead == null)
                LookAhead = ReadSingleToken();

            Current = LookAhead;
            LookAhead = ReadSingleToken();

            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();

            //m_input.BaseStream.Seek(0, SeekOrigin.Begin);
            //Current = null;
        }
    }
}
