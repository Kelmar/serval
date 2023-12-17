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
                Symbol sym = m_symbolTable.Find(m_lex.Current.Literal);

                if (sym == null)
                {
                    Error(ErrorCodes.ParseUndeclaredVar, m_lex.Current);

                    // So we only warn about undeclared once.
                    sym = m_symbolTable.Add(new Symbol
                    {
                        Name = m_lex.Current.Literal,
                        Undefined = true,
                        Usage = SymbolUsage.Variable,
                        LineNumber = m_lex.Current.LineNumber
                    });
                }

                if (sym.Usage == SymbolUsage.Type)
                {
                    // We can't read from a "type" symbol
                    Error(ErrorCodes.ParseTypeNotValidHere, sym);
                }

                rval = new VariableExpr(sym);
                m_lex.MoveNext();
                break;

            case TokenType.CharConst:
                rval = new ConstExpr(m_lex.Current, m_symbolTable.Find("char"));
                m_lex.MoveNext();
                break;

            case TokenType.FloatConst:
                rval = new ConstExpr(m_lex.Current, m_symbolTable.Find("float"));
                m_lex.MoveNext();
                break;

            case TokenType.IntConst:
                rval = new ConstExpr(m_lex.Current, m_symbolTable.Find("int"));
                m_lex.MoveNext();
                break;

            case TokenType.StringConst:
                rval = new ConstExpr(m_lex.Current, m_symbolTable.Find("string"));
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
                {
                    char op = (char)m_lex.Current.Type;
                    m_lex.MoveNext();
                    var cast = ParseCast();
                    return new UnaryExpr(op, cast);
                }

            case "sizeof":
            case "typeof":
                {
                    TokenType op = m_lex.Current.Type;
                    m_lex.MoveNext();
                    Expect(TokenType.LeftParen);

                    if (m_lex.Current.Type != TokenType.Identifier)
                    {
                        Error(ErrorCodes.ParseTypeExpected, op);
                        return new DummyExpr();
                    }

                    var sym = m_symbolTable.Find(m_lex.Current.Literal);

                    if (sym == null)
                    {
                        sym = m_symbolTable.Add(new Symbol
                        {
                            Name = m_lex.Current.Literal,
                            Undefined = true,
                            Usage = SymbolUsage.Type,
                            LineNumber = m_lex.Current.LineNumber
                        });

                        Error(ErrorCodes.ParseTypeUndefined, sym);
                    }

                    m_lex.MoveNext();

                    Expect(TokenType.RightParen);
                    
                    if (sym.Usage != SymbolUsage.Type)
                    {
                        Error(ErrorCodes.ParseTypeExpected, op);
                        return new DummyExpr();
                    }

                    return new TypeOpExpr(op, sym);
                }

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
                Symbol type = m_symbolTable.Find(m_lex.LookAhead.Literal);

                // TODO: If type is undefined, then it's probably the start of a lambda.

                if (type?.Usage == SymbolUsage.Type)
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
