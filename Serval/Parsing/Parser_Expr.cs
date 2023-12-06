using System;
using System.Collections.Generic;

using Serval.AST;
using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    public partial class Parser
    {
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
            if (m_lex.Current.Type == (TokenType)'(' && s_typeTokens.Contains(m_lex.LookAhead.Type))
            {
                // Eat '('
                Expect(TokenType.LeftParen, TokenType.RightParen);

                Token type = m_lex.Current;
                m_lex.MoveNext();

                // Eat ')'
                if (!Expect(TokenType.RightParen, TokenType.Semicolon))
                    return null;

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
    }
}
