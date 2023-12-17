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

        private StatementNode ParseExpressionStatement()
        {
            var rval = new ExpressionStatement(ParseExpression());

            Expect(TokenType.Semicolon);

            return rval;
        }

        private StatementNode ParseLabeledStatement()
        {
            Token ident = m_lex.Current;
            m_lex.MoveNext();

            // Eat ':'
            Expect(TokenType.Colon);

            Symbol sym = m_symbolTable.Find(ident.Literal);

            if (sym != null)
                Error(ErrorCodes.ParseAlreadyDefined, sym);
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
                break; // Empty statement okay

            case TokenType.Identifier:
                if (m_lex.LookAhead.Type == TokenType.Colon)
                    rval = ParseLabeledStatement();
                else
                    rval = ParseExpressionStatement();
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
