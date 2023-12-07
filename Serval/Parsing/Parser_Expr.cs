using System;
using System.Collections.Generic;

using Serval.AST;
using Serval.CodeGen;
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
                Symbol sym = m_symbolTable.FindEntry(m_lex.Current.Literal);

                if (sym == null)
                {
                    Error(ErrorCodes.ParseUndeclaredVar, m_lex.Current);

                    // So we only warn a bout undeclared once.
                    m_symbolTable.AddEntry(m_lex.Current, SymbolType.Undefined);
                }

                rval = new VariableExpr(sym);
                m_lex.MoveNext();
                break;

            case TokenType.FloatConst:
            case TokenType.IntConst:
            case TokenType.StringConst:
                rval = new ConstExpr(m_lex.Current);
                m_lex.MoveNext();
                break;

            case (TokenType)'(':
                m_lex.MoveNext(); // Eat '('
                rval = ParseExpression();

                // Eat ')'
                Expect(TokenType.RightParen, TokenType.Semicolon);
                break;

            default:
                rval = new DummyExpr();
                Error(ErrorCodes.ParseUnexpectedSymbol, m_lex.Current);
                break;
            }

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
            if (m_lex.Current.Type == (TokenType)'(')
            {
                Symbol type = m_symbolTable.FindEntry(m_lex.LookAhead.Literal);

                // TODO: If type is undefined, then it's probably the start of a lambda.

                if (type?.Type == SymbolType.Type)
                {
                    // Eat '('
                    Expect(TokenType.LeftParen, TokenType.RightParen);

                    // Eat type
                    m_lex.MoveNext();

                    // Eat ')'
                    if (!Expect(TokenType.RightParen, TokenType.Semicolon))
                        return new DummyExpr();

                    return new CastExpr(type, ParseCast());
                }
            }
            
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
