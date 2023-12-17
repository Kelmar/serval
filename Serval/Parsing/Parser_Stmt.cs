using Serval.AST;
using Serval.CodeGen;
using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    public partial class Parser
    {
        /// <summary>
        /// declaration: "var" [ident] ":" [ident] ';'
        ///            | "const" [ident] ":" [ident] ';'
        /// </summary>
        /// <returns></returns>
        private StatementNode ParseDeclaration()
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

            var type = m_symbolTable.Find(m_lex.Current.Literal);

            if (type == null || type.Usage != SymbolUsage.Type)
            {
                Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, SemanticType.TypeDeclaration);
                return null;
            }

            m_lex.MoveNext();

            Symbol sym = m_symbolTable.Find(ident.Literal);

            if (sym != null)
            {
                Error(ErrorCodes.ParseAlreadyDefined, sym);
                return null;
            }

            sym = m_symbolTable.Add(new Symbol
            {
                Name = ident.Literal,
                Usage = mod == TokenType.Const ? SymbolUsage.Constant : SymbolUsage.Variable,
                LineNumber = ident.LineNumber
            });

            // TODO: Look for optional assignment for variable initializer.

            Expect(TokenType.Semicolon);

            return new VariableDecl(mod, type, sym);
        }

        //private StatementNode ParseExpressionStatement()
        //{
        //    var rval = new ExpressionStatement(ParseExpression());

        //    Expect(TokenType.Semicolon);

        //    return rval;
        //}

        private StatementNode ParseLabeledStatement()
        {
            Token ident = m_lex.Current;
            m_lex.MoveNext();

            // Eat ':'
            Expect(TokenType.Colon);

            Symbol sym = m_symbolTable.Find(ident.Literal);

            if (sym != null)
            {
                if (sym.Usage != SymbolUsage.Label)
                    Error(ErrorCodes.ParseNotLabel, sym);
                else
                {
                    // We could get a goto for a label that is not yet defined, that's okay.

                    if (!sym.Undefined)
                        Error(ErrorCodes.ParseAlreadyDefined, sym);

                    sym.Undefined = false;
                    sym.LineNumber = ident.LineNumber;
                }
            }
            else
            {
                sym = m_symbolTable.Add(new Symbol
                {
                    Name = ident.Literal,
                    Usage = SymbolUsage.Label,
                    LineNumber = ident.LineNumber
                });
            }

            return new LabeledStatement(sym, ParseStatement());
        }

        /// <summary>
        /// assignment: [ident] '=' expression
        /// </summary>
        /// <returns></returns>
        private StatementNode ParseAssignmentStatement()
        {
            if (m_lex.Current.Type != TokenType.Identifier)
            {
                Error(ErrorCodes.ParseExpectedSymbol, m_lex.Current, TokenType.Identifier);
                Resync(TokenType.Semicolon);
                return null;
            }

            //var target = ParseUnary();

            var ident = m_lex.Current;
            m_lex.MoveNext();

            Symbol symbol = m_symbolTable.Find(ident.Literal);

            if (symbol == null)
            {
                Error(ErrorCodes.ParseUndeclaredVar, ident);

                symbol = m_symbolTable.Add(new Symbol()
                {
                    Name = ident.Literal,
                    Usage = SymbolUsage.Variable,
                    Undefined = true,
                    LineNumber = ident.LineNumber
                });
            }

            if (symbol.Usage != SymbolUsage.Variable)
                Error(ErrorCodes.ParseAssignToNonVar, symbol);

            Expect(TokenType.Assign);

            return new AssignmentStatement(symbol, ParseExpression());
        }

        /// <summary>
        /// compound_statement: '{' '}'
        ///                   | '{' statement_list '}'
        /// </summary>
        /// <returns></returns>
        private StatementNode ParseCompoundStatement()
        {
            Expect(TokenType.LeftCurl);
            var rval = new CompoundStatement();

            while (true)
            {
                if (m_lex.Current.Type == TokenType.RightCurl)
                    break;

                StatementNode item = ParseStatement();

                if (item != null)
                    rval.Statements.Add(item);
            }

            Expect(TokenType.RightCurl);

            return rval;
        }

        /// <summary>
        /// statement: [ident] ':' statement
        ///          | expression_statement
        ///          | compound_statement
        ///          | declaration
        /// </summary>
        /// <returns></returns>
        private StatementNode ParseStatement()
        {
            StatementNode rval = null;

            switch (m_lex.Current.Type)
            {
            case TokenType.Semicolon:
                m_lex.MoveNext();
                break; // Empty statement okay

            case TokenType.Identifier:
                switch (m_lex.LookAhead.Type)
                {
                case TokenType.Colon:
                    rval = ParseLabeledStatement();
                    Expect(TokenType.Semicolon);
                    break;

                case TokenType.Assign:
                    rval = ParseAssignmentStatement();
                    Expect(TokenType.Semicolon);
                    break;
                }
                break;

            case TokenType.LeftCurl:
                rval = ParseCompoundStatement();
                break;

            case TokenType.Var:
            case TokenType.Const:
                rval = ParseDeclaration();
                break;

            default:
                Error(ErrorCodes.ParseUnexpectedSymbol, m_lex.Current);
                Resync(TokenType.Semicolon, TokenType.RightCurl);
                break;
            }

            return rval;
        }
    }
}
