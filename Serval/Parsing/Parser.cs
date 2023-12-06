using System;
using System.Collections.Generic;
using System.Diagnostics;

using Serval.AST;
using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    public class Parser : IDisposable
    {
        private static ISet<TokenType> Types = new HashSet<TokenType>
        {
            TokenType.Int,
            TokenType.Char,
            TokenType.Float,
            TokenType.String
        };

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
        }

        private void Error(ErrorCodes errorCode, params object[] args)
        {
            m_reporter.Error(m_lex.Current, errorCode, args);
        }

        [Obsolete("Use Resync() and Expect()")]
        private void ErrorRecover()
        {
            // Recover to semicolon

            while (m_lex.Current.Type != TokenType.EndOfFile && m_lex.Current.Type != (TokenType)';' && m_lex.MoveNext())
                ;

            if (m_lex.Current.Type == (TokenType)';')
                m_lex.MoveNext(); // Eat ';'
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
        /// primary: [identifier]
        ///        | [constant]
        ///        | '(' expression ')'
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParsePrimary()
        {
            ExpressionNode rval;

            switch (m_lex.Current.Type)
            {
            case TokenType.Identifier:
                rval = new VariableExpr(m_lex.Current);
                break;

            case TokenType.FloatConst:
            case TokenType.IntConst:
            case TokenType.StringConst:
                rval = new ConstExpr(m_lex.Current);
                break;

            case (TokenType)'(':
                m_lex.MoveNext(); // Eat '('
                rval = ParseExpression();

                //Expect(TokenType.RightParen, TokenType.Semicolon);

                if (m_lex.Current.Literal != ")")
                {
                    Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, TokenType.RightParen);
                    return null;
                }

                break;

            default:
                rval = null;
                Error(ErrorCodes.ParseUnexpectedSymbol, m_lex.Current);
                break;
            }

            m_lex.MoveNext(); // Also performs error recovery

            return rval;
        }

        /// <summary>
        /// unary: primary
        ///       | '+' primary
        ///       | '-' primary
        ///       | '*' primary
        ///       | '&' primary
        ///       | '~' primary
        ///       | '!' primary
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseUnary()
        {
            switch (m_lex.Current.Literal)
            {
            case "+":
            case "-":
            case "*":
            case "&":
            case "~":
            case "!":
                char op = (char)m_lex.Current.Type;
                m_lex.MoveNext();
                var primary = ParsePrimary();
                return new UnaryExpr(op, primary);

            default:
                return ParsePrimary();
            }
        }

        /// <summary>
        /// cast: unary
        ///     | '(' type ')' cast
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseCast()
        {
            if (m_lex.Current.Type == (TokenType)'(' && Types.Contains(m_lex.LookAhead.Type))
            {
                m_lex.MoveNext(); // Eat '('
                Token type = m_lex.Current;
                m_lex.MoveNext();

                if (m_lex.Current.Type != (TokenType)')')
                {
                    Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, TokenType.LeftParen);
                    ErrorRecover();
                    return null;
                }

                m_lex.MoveNext(); // Eat ')'

                return new CastExpr(type, ParseCast());
            }
            else
                return ParseUnary();
        }

        private ExpressionNode ParseBinary(Func<ExpressionNode> sub, params string[] ops)
        {
            ExpressionNode lhs = sub();

            if (lhs == null)
                return null;

            var opList = new HashSet<string>(ops);

            bool parsing = opList.Contains(m_lex.Current.Literal);

            while (parsing)
            {
                string op = m_lex.Current.Literal;
                m_lex.MoveNext();

                var rhs = sub();

                if (rhs == null)
                    return null;

                lhs = new BinaryExpr(op, lhs, rhs);

                parsing = opList.Contains(m_lex.Current.Literal);
            }

            return lhs;
        }

        /// <summary>
        /// factor: unary
        ///       | factor '*' unary
        ///       | factor '/' unary
        ///       | factor '%' unary
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseFactor()
        {
            return ParseBinary(ParseCast, "*", "/", "%");
        }

        /// <summary>
        /// additive: factor
        ///         | additive '+' factor
        ///         | additive '-' factor
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseAdditive()
        {
            return ParseBinary(ParseFactor, "+", "-");
        }

        /// <summary>
        /// shift: additive
        ///      | shift '<<' additive
        ///      | shift '>>' additive
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseShift()
        {
            return ParseBinary(ParseAdditive, "<<", ">>");
        }

        /// <summary>
        /// relational: shift
        ///           | relational '<' shift
        ///           | relational '>' shift
        ///           | relational '<=' shift
        ///           | relational '>=' shift
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseRelational()
        {
            return ParseBinary(ParseShift, "<", ">", "<=", ">=");
        }

        /// <summary>
        /// equality: relational
        ///         | equality '==' relational
        ///         | equality '!=' relational
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseEquality()
        {
            return ParseBinary(ParseRelational, "==", "!=");
        }
        
        /// <summary>
        /// expression: equality
        /// </summary>
        /// <returns></returns>
        private ExpressionNode ParseExpression()
        {
            return ParseEquality();
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
                ErrorRecover();
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

            if (m_lex.Current.Type != TokenType.Assign)
            {
                Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, TokenType.Assign);
                ErrorRecover();
                return null;
            }

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

            if (!Types.Contains(type))
            {
                Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, SemanticType.TypeDeclaration);
                return null;
            }

            m_lex.MoveNext();

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
                    ErrorRecover();
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

            m_lex.MoveNext(); // Eat '('

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
                ErrorRecover();
                return null;
            }

            if (rval == null)
            {
                Error(ErrorCodes.ParseUnknownError);
                ErrorRecover();
                return null;
            }

            if (rval != null && m_lex.Current.Type != TokenType.Semicolon)
            {
                Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, TokenType.Semicolon);
                ErrorRecover();
                return null;
            }

            m_lex.MoveNext(); // Eat ';'

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
