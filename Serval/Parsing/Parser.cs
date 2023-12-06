using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serval.AST;
using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    public partial class Parser : IDisposable
    {
        // TODO: These should be moved into the global symbol table later.

        private static readonly HashSet<TokenType> s_typeTokens =
        [
            TokenType.Int,
            TokenType.Char,
            TokenType.Float,
            TokenType.String
        ];

        private readonly Lexer m_lex;
        private readonly IReporter m_reporter;

        public Parser(Lexer lex, IReporter reporter)
        {
            Debug.Assert(lex != null);
            Debug.Assert(reporter != null);

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

            Resync(args);
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
        /// assignment: [ident] '=' expression
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseAssignment()
        {
            if (m_lex.Current.Type != TokenType.Identifier)
            {
                Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, TokenType.Identifier);
                Resync(TokenType.Semicolon);
                return null;
            }

            Token ident = m_lex.Current;

            //Symbol symbol = m_symbolTab.FindEntry(ident.Literal);

            //if (symbol == null)
            //{
            //    Error(m_lex.Current, "Undefined identifier {0}", ident.Literal);
            //    ErrorRecover();
            //    return null;
            //}

            m_lex.MoveNext();

            if (!Expect(TokenType.Assign))
                return null;

            m_lex.MoveNext(); // Eat '='

            ExpressionNode rval = new AssignmentStatement(ident, ParseExpression());

            return rval;
        }

        /// <summary>
        /// declaration: "var" [ident] ":" [type]
        ///            | "const" [ident] ":" [type]
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseDeclaration()
        {
            TokenType mod = m_lex.Current.Type;
            m_lex.MoveNext();

            if (m_lex.Current.Type != TokenType.Identifier)
            {
                Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, TokenType.Identifier);
                return null;
            }

            Token ident = m_lex.Current;
            m_lex.MoveNext();

            Expect(TokenType.Colon);

            TokenType type = m_lex.Current.Type;

            if (!s_typeTokens.Contains(type))
            {
                Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, SemanticType.TypeDeclaration);
                return null;
            }

            m_lex.MoveNext();

            // TODO: Look for optional assignment for variable initializer.

            return new VariableDecl(mod, type, ident);
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
                var expr = ParseCallArgument();

                if (expr != null)
                    rval.Add(expr);
                else if (!first)
                {
                    Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, SemanticType.CallArgument);
                    Resync(TokenType.Semicolon);
                    return null;
                }

                first = false;

                if (m_lex.Current.Type == TokenType.RightParen)
                    break;

                Expect(TokenType.Comma);
            }

            return rval;
        }

        /// <summary>
        /// function_call: identifier '(' parameter_list ')'
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseFunctionCall()
        {
            Token ident = m_lex.Current;
            m_lex.MoveNext();

            // Eat '('
            Expect(TokenType.LeftParen, TokenType.RightParen);

            var args = ParseParameterList();

            if (args == null)
                return null;

            // Eat ')'
            Expect(TokenType.RightParen);

            return new FunctionCallExpr(ident.Literal, args);
        }

        private ExpressionNode ParseFunctionOrAssignment()
        {
            return m_lex.LookAhead.Type switch
            {
                TokenType.Assign => ParseAssignment(),
                TokenType.LeftParen => ParseFunctionCall(),
                _ => null,
            };
        }

        /// <summary>
        /// statement: assignment ';'
        ///          | function_call ';'
        ///          | declaration ';'
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseStatement()
        {
            ExpressionNode rval;

            switch (m_lex.Current.Type)
            {
            case TokenType.Identifier:
                rval = ParseFunctionOrAssignment();
                break;

            case TokenType.Var:
            case TokenType.Const:
                rval = ParseDeclaration();
                break;

            default:
                Error(ErrorCodes.ParseUnexpectedSymbol, m_lex.Current);
                Resync(TokenType.Semicolon);
                return null;
            }

            if (!Expect(TokenType.Semicolon))
                return null;

            return rval;
        }

        public Module ParseModule()
        {
            var rval = new Module();

            while (m_lex.Current.Type != TokenType.EndOfFile)
            {
                ExpressionNode expr = ParseStatement();

                if (expr != null)
                    rval.Expressions.Add(expr);
            }

            return rval;
        }
    }
}
