using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Serval.Lexing;
using Serval.Parsing.AST;

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

        private void Error(Token t, string fmt, params object[] args)
        {
            m_reporter.Error(t, fmt, args);
        }

        private void Warn(Token t, string fmt, params object[] args)
        {
            m_reporter.Warn(t, fmt, args);
        }

        private void ErrorRecover()
        {
            // Recover to simicolon

            while (m_lex.Current.Type != TokenType.EndOfFile && m_lex.Current.Type != (TokenType)';' && m_lex.MoveNext())
                ;

            if (m_lex.Current.Type == (TokenType)';')
                m_lex.MoveNext(); // Eat ';'
        }

        private Token Expect(TokenType type)
        {
            if (m_lex.Current.Type == type)
            {
                Token rval = m_lex.Current;
                m_lex.MoveNext();
                return rval;
            }

            Error(m_lex.Current, "Expected {0} token, got {1} instead.", type, m_lex.Current.Type);
            return null;
        }

        /// <summary>
        /// primary: [identifier]
        ///        | [constant]
        ///        | '(' expression ')'
        /// </summary>
        /// <returns></returns>
        private Expression ParsePrimary()
        {
            Expression rval;

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

                if (m_lex.Current.Literal != ")")
                {
                    Error(m_lex.Current, "Expecting ')' on line {0}", m_lex.Current.LineNumber);
                    return null;
                }

                break;

            default:
                rval = null;
                Error(m_lex.Current, "Unexpected {0} token, expecting primary expression.", m_lex.Current);
                break;
            }

            m_lex.MoveNext(); // Also performs error recovery

            return rval;
        }

        /// <summary>
        /// uniary: primary
        ///       | '+' primary
        ///       | '-' primary
        ///       | '*' primary
        ///       | '&' primary
        ///       | '~' primary
        ///       | '!' primary
        /// </summary>
        /// <returns></returns>
        private Expression ParseUniary()
        {
            char op = '\0';

            switch (m_lex.Current.Literal)
            {
            case "+":
            case "-":
            case "*":
            case "&":
            case "~":
            case "!":
                op = (char)m_lex.Current.Type;
                m_lex.MoveNext();
                var primary = ParsePrimary();
                return new UnaryExpr(op, primary);

            default:
                return ParsePrimary();
            }
        }

        /// <summary>
        /// cast: uniary
        ///     | '(' type ')' cast
        /// </summary>
        /// <returns></returns>
        private Expression ParseCast()
        {
            if (m_lex.Current.Type == (TokenType)'(' && Types.Contains(m_lex.LookAhead.Type))
            {
                m_lex.MoveNext(); // Eat '('
                Token type = m_lex.Current;
                m_lex.MoveNext();

                if (m_lex.Current.Type != (TokenType)')')
                {
                    Error(m_lex.Current, "Expected ')' for cast expression");
                    ErrorRecover();
                    return null;
                }

                m_lex.MoveNext(); // Eat ')'

                return new CastExpr(type, ParseCast());
            }
            else
                return ParseUniary();
        }

        private Expression ParseBinary(Func<Expression> sub, params string[] ops)
        {
            Expression lhs = sub();

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
        /// factor: uniary
        ///       | term '*' uniary
        ///       | term '/' uniary
        ///       | term '%' uniary
        /// </summary>
        /// <returns></returns>
        private Expression ParseFactor()
        {
            return ParseBinary(ParseCast, "*", "/", "%");
        }

        /// <summary>
        /// additive: factor
        ///         | additive '+' factor
        ///         | additive '-' factor
        /// </summary>
        /// <returns></returns>
        private Expression ParseAdditive()
        {
            return ParseBinary(ParseFactor, "+", "-");
        }

        /// <summary>
        /// shift: additive
        ///      | shift '<<' additive
        ///      | shift '>>' additive
        /// </summary>
        /// <returns></returns>
        private Expression ParseShift()
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
        private Expression ParseRelational()
        {
            return ParseBinary(ParseShift, "<", ">", "<=", ">=");
        }

        /// <summary>
        /// equality: relational
        ///         | equality '==' relational
        ///         | equality '!=' relational
        /// </summary>
        /// <returns></returns>
        private Expression ParseEquality()
        {
            return ParseBinary(ParseRelational, "==", "!=");
        }
        
        /// <summary>
        /// expression: equality
        /// </summary>
        /// <returns></returns>
        private Expression ParseExpression()
        {
            return ParseEquality();
        }

        /// <summary>
        /// assignment: [ident] '=' expression
        /// </summary>
        /// <returns></returns>
        private Expression ParseAssignment()
        {
            if (m_lex.Current.Type != TokenType.Identifier)
            {
                Error(m_lex.Current, "Unexpected {0} token", m_lex.Current);
                ErrorRecover();
                return null;
            }

            Token ident = m_lex.Current;

            if (ident == null)
            {
                throw new Exception("Not sure what's going on here, current token is null, end of file?");
            }

            //Symbol symbol = m_symbolTab.FindEntry(ident.Literal);

            //if (symbol == null)
            //{
            //    Error(m_lex.Current, "Undefined identifier {0}", ident.Literal);
            //    ErrorRecover();
            //    return null;
            //}

            m_lex.MoveNext();

            if (m_lex.Current.Type != (TokenType)'=')
            {
                Error(m_lex.Current, "Unexpected {0} token on line {1}", m_lex.Current);
                ErrorRecover();
                return null;
            }

            m_lex.MoveNext(); // Eat '='

            Expression rval = new AssignmentStatement(ident, ParseExpression());

            return rval;
        }

        /// <summary>
        /// declaration: "var" [ident] ":" [type]
        ///            | "const" [ident] ":" [type]
        /// </summary>
        /// <returns></returns>
        private Expression ParseDeclaration()
        {
            TokenType mod = m_lex.Current.Type;
            m_lex.MoveNext();

            if (m_lex.Current.Type != TokenType.Identifier)
            {
                Error(m_lex.Current, "Expected identifier");
                return null;
            }

            Token ident = m_lex.Current;
            m_lex.MoveNext();

            Expect(TokenType.Colon);

            TokenType type = m_lex.Current.Type;

            if (!Types.Contains(type))
            {
                Error(m_lex.Current, "Expected type declaration");
                return null;
            }

            m_lex.MoveNext();

            return new VariableDecl(mod, type, ident);
        }

        /// <summary>
        /// call_argument: expression
        /// </summary>
        /// <returns></returns>
        private Expression ParseCallArgument()
        {
            return ParseExpression();
        }

        /// <summary>
        /// parameter_list:
        ///               | call_argument
        ///               | parameter_list ',' call_argument
        /// </summary>
        /// <returns></returns>
        private List<Expression> ParseParameterList()
        {
            var rval = new List<Expression>();
            bool first = true;

            while (true)
            {
                var expr = ParseCallArgument();

                if (expr != null)
                    rval.Add(expr);
                else if (!first)
                {
                    Error(m_lex.Current, "Expected call argument");
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
        private Expression ParseFunctionCall()
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

        private Expression ParseFunctionOrAssignment()
        {
            switch (m_lex.LookAhead.Type)
            {
            case TokenType.Assign:
                return ParseAssignment();

            case TokenType.LeftParen:
                return ParseFunctionCall();
            }

            return null;
        }

        /// <summary>
        /// statement: assignment ';'
        ///          | function_call ';'
        ///          | declaration ';'
        /// </summary>
        /// <returns></returns>
        private Expression ParseStatement()
        {
            Expression rval = null;

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
                Error(m_lex.Current, "Unexpected {0} token", m_lex.Current.Type);
                ErrorRecover();
                return null;
            }

            if (rval == null)
            {
                Error(m_lex.Current, "Confused");
                ErrorRecover();
                return null;
            }

            if (rval != null && m_lex.Current.Type != TokenType.Simicolon)
            {
                Error(m_lex.Current, "Unexpected {0} token", m_lex.Current);
                ErrorRecover();
                return null;
            }

            m_lex.MoveNext(); // Eat ';'

            return rval;
        }

        public IEnumerable<Expression> BuildTree()
        {
            while (m_lex.Current.Type != TokenType.EndOfFile)
            {
                Expression expr = ParseStatement();

                if (expr != null)
                    yield return expr;
            }
        }
    }
}
