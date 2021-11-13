using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Serval.Parsing.AST;

namespace Serval
{
    public class Parser : IDisposable
    {
        private static ISet<TokenType> Types = new HashSet<TokenType>
        {
            TokenType.Int,
            TokenType.Float,
            TokenType.String
        };

        private static ISet<TokenType> TypeModifier = new HashSet<TokenType>
        {
            TokenType.Const
        };

        private readonly Lexer m_lex;
        private readonly IReporter m_reporter;

        private readonly ISet<string> m_symbolTab = new HashSet<string>();

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

        /// <summary>
        /// type: [base_type]
        ///     | [modifier] type
        /// </summary>
        /// <returns></returns>
        private TypeExpr ParseType()
        {
            var mods = new List<Token>();

            while (TypeModifier.Contains(m_lex.Current.Type))
            {
                mods.Add(m_lex.Current);

                if (!m_lex.MoveNext())
                    return null;
            }

            if (Types.Contains(m_lex.Current.Type))
            {
                var rval = new TypeExpr(m_lex.Current, mods);
                m_lex.MoveNext();
                return rval;
            }
            else if (mods.Any())
            {
                Error(m_lex.Current, "Expected type specifier");
            }

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
                if (!m_symbolTab.Contains(m_lex.Current.Literal))
                {
                    rval = null;
                    Error(m_lex.Current, "Undeclared identifier '{0}'", m_lex.Current.Literal);
                }
                else
                    rval = new PrimaryExpr(m_lex.Current);

                break;

            case TokenType.FloatConst:            
            case TokenType.IntConst:
                rval = new PrimaryExpr(m_lex.Current);
                break;

            case (TokenType)'(':
                m_lex.MoveNext(); // Eat '('
                rval = ParseExpression();

                if (m_lex.Current.Literal != ")")
                {
                    Error(m_lex.Current, "Expecting ')' on line {1}");
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
                return new UniaryExpr(op, primary);

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

            if (!m_symbolTab.Contains(ident?.Literal))
            {
                Error(m_lex.Current, "Undefined identifier {0}", ident.Literal);
                ErrorRecover();
                return null;
            }

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
        /// declaration: [type] [ident]
        ///            | [modifier] [type] [ident]
        /// </summary>
        /// <returns></returns>
        private Expression ParseDeclaration()
        {
            var type = ParseType();

            //for (;;)
            //{
                if (m_lex.Current.Type != TokenType.Identifier)
                {
                    Error(m_lex.Current, "Expected identifier");
                    return null;
                }

                m_symbolTab.Add(m_lex.Current.Literal);

                var rval = new DeclarationExpr(type, m_lex.Current);
                m_lex.MoveNext();

                //if (m_lex.Current.Type == TokenType.Simicolon)
                //    break;

                //if (m_lex.Current.Type != TokenType.Comma)
                //{
                //    Error("Expected comma or simicolon in declaration on line {0}", m_lex.Current.LineNumber);
                //    ErrorRecover();
                //    return null;
                //}
            //}

            return rval;
        }

        /// <summary>
        /// statement: assignment ';'
        ///          | declaration ';'
        /// </summary>
        /// <returns></returns>
        private Expression ParseStatement()
        {
            Expression rval = null;

            if (m_lex.Current.Type == TokenType.Identifier)
            {
                rval = ParseAssignment();
            }
            else if (Types.Contains(m_lex.Current.Type) || TypeModifier.Contains(m_lex.Current.Type))
            {
                rval = ParseDeclaration();
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

        public Expression BuildTree()
        {
            while (m_lex.Current.Type != TokenType.EndOfFile)
            {
                Expression expr = ParseStatement();

                if (expr != null)
                {
                    Console.WriteLine("{0}", expr);
                }
            }

            return null;
        }
    }
}
