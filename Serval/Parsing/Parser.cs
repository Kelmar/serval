﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serval.AST;
using Serval.CodeGen;
using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    public partial class Parser : IDisposable
    {
        private readonly SymbolTable m_symbolTable;
        private readonly Lexer m_lex;
        private readonly IReporter m_reporter;

        public Parser(SymbolTable symbolTable, Lexer lex, IReporter reporter)
        {
            Debug.Assert(symbolTable != null);
            Debug.Assert(lex != null);
            Debug.Assert(reporter != null);
            
            m_symbolTable = symbolTable;
            m_lex = lex;
            m_reporter = reporter;

            m_lex.MoveNext();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                m_lex.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Error(ErrorCodes errorCode, params object[] args)
        {
            m_reporter.Error(m_lex.Current, errorCode, args);
        }

        /// <summary>
        /// Resyncs the lexer to a (hopefully) recoverable token.
        /// </summary>
        /// <param name="tokens">List of tokens to try to resync to.</param>
        private void Resync(IList<TokenType> tokens)
        {
            Debug.Assert(tokens != null);
            Debug.Assert(tokens.Count > 0);

            do
            {
                if (tokens.Contains(m_lex.Current.Type))
                    break;
            } while (m_lex.MoveNext());
        }

        /// <summary>
        /// Resyncs the lexer to a (hopefully) recoverable token.
        /// </summary>
        /// <param name="first">The first token to try and resync to.</param>
        /// <param name="args">List of tokens to try to resync to.</param>
        private void Resync(TokenType first, params TokenType[] args)
        {
            var set = new List<TokenType>
            {
                first
            };

            if (args != null)
                set.AddRange(args);

            Resync(set);
        }

        /// <summary>
        /// Checks for the desired token and advances the lexer if it's the correct type.
        /// </summary>
        /// <remarks>
        /// Emits an error if the token is not what is expected and attempts to resync.
        /// </remarks>
        /// <param name="expected">The token we are expecting to find.</param>
        /// <param name="resyncTo">Additional tokens to try to resync to.</param>
        /// <returns>True if the expected item was found, false if not.</returns>
        private bool Expect(TokenType expected, params TokenType[] resyncTo)
        {
            bool rval = true;

            if (m_lex.Current.Type != expected)
            {
                Error(ErrorCodes.ParseUnexpectedSymbol, m_lex.Current.Type, expected);
                Resync(expected, resyncTo);
                rval = false;
            }

            if (m_lex.Current.Type == expected)
                m_lex.MoveNext();

            return rval;
        }

        

        /// <summary>
        /// call_argument: expression
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseCallArgument()
        {
            return ParseExpression();
        }

        /// <summary>
        /// parameter_list:
        ///               | call_argument
        ///               | parameter_list ',' call_argument
        /// </summary>
        /// <returns></returns>
        private List<ExpressionNode> ParseParameterList()
        {
            var rval = new List<ExpressionNode>();
            bool first = true;

            while (true)
            {
                if (!first && m_lex.Current.Type != TokenType.RightParen)
                    Expect(TokenType.Comma, TokenType.RightParen);

                if (m_lex.Current.Type == TokenType.RightParen)
                    break;

                first = false;
                var expr = ParseCallArgument();
                rval.Add(expr);
            }

            return rval;
        }

        /// <summary>
        /// function_call: identifier '(' parameter_list ')'
        /// </summary>
        /// <returns></returns>
        //private ExpressionNode ParseFunctionCall()
        //{
        //    Token ident = m_lex.Current;
        //    m_lex.MoveNext();

        //    // Eat '('
        //    Expect(TokenType.LeftParen, TokenType.RightParen);

        //    var args = ParseParameterList();

        //    // Eat ')'
        //    Expect(TokenType.RightParen);

        //    return new FunctionCallExpr(ident.Literal, args);
        //}

        //private StatementNode ParseFunctionOrAssignment()
        //{
        //    return m_lex.LookAhead.Type switch
        //    {
        //        TokenType.Assign => ParseAssignment(),
        //        //TokenType.LeftParen => ParseFunctionCall(),
        //        _ => null,
        //    };
        //}

        private void ParseEnum()
        {
            Expect(TokenType.Enum);

            var ident = m_lex.Current;
            bool valid = true;

            string name = ident.Literal;
            Symbol sym;

            if (!Expect(TokenType.Identifier))
            {
                valid = false;
                name = m_symbolTable.GenName();
            }
            else
            {
                sym = m_symbolTable.Find(ident.Literal);

                if (sym != null)
                {
                    Error(ErrorCodes.ParseAlreadyDefined, sym, sym.LineNumber);
                    return;
                }
            }

            var typeDef = new TypeDecl(TypeType.Enum, name, m_symbolTable);

            var itemValues = new HashSet<int>();
            int lastValue = 0;

            valid &= Expect(TokenType.LeftCurl);

            while (m_lex.Current.Type != TokenType.RightCurl)
            {
                var item = m_lex.Current;
                m_lex.MoveNext();

                if (item.Type != TokenType.Identifier)
                { 
                    valid = false;
                    Resync(TokenType.Comma);
                    continue;
                }

                sym = typeDef.Members.Find(item.Literal);

                if (sym != null)
                {
                    valid = false;
                    Error(ErrorCodes.ParseAlreadyDefined, sym, sym.LineNumber);
                    continue;
                }

                // Check for user assigned value
                if (m_lex.Current.Type == TokenType.Assign)
                {
                    // Assigned value
                    m_lex.MoveNext();

                    var value = m_lex.Current;
                    m_lex.MoveNext();

                    if (value.Type != TokenType.IntConst)
                    {
                        Error(ErrorCodes.ParseExpectedSymbol, value, TokenType.IntConst);
                        Resync(TokenType.Comma, TokenType.RightCurl);
                        valid = false;
                    }
                    else
                    {
                        lastValue = (int)value.Parsed;
                    }
                }
                else
                {
                    while (itemValues.Contains(lastValue))
                        ++lastValue;
                }

                itemValues.Add(lastValue);

                sym = new Symbol
                {
                    Name = item.Literal,
                    Usage = SymbolUsage.Constant,
                    LineNumber = item.LineNumber,
                    Value = lastValue
                };

                typeDef.Members.Add(sym);

                if (m_lex.Current.Type == TokenType.RightCurl)
                    break;

                valid &= Expect(TokenType.Comma);
            }

            valid &= Expect(TokenType.RightCurl);

            m_symbolTable.Add(new Symbol
            {
                Name = name,
                Usage = SymbolUsage.Type,
                LineNumber = ident.LineNumber,
                TypeDefinition = typeDef,
                Valid = valid
            });
        }

        public Module ParseModule()
        {
            var rval = new Module();

            while (m_lex.Current.Type != TokenType.EndOfFile)
            {
                switch (m_lex.Current.Type)
                {
                case TokenType.Class:
                    ParseClass();
                    break;

                case TokenType.Import:
                    ParseImport();
                    break;

                case TokenType.Enum:
                    ParseEnum();
                    break;

                default:
                    StatementNode statement = ParseStatement();

                    if (statement != null)
                        rval.Statements.Add(statement);
                    break;
                }
            }

            return rval;
        }
    }
}
