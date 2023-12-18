using Serval.CodeGen;
using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    public partial class Parser
    {
        private void ParseClassItem(TypeDecl type)
        {
            switch (m_lex.Current.Type)
            {
            case TokenType.Const:
            case TokenType.Var:
                ParseDeclaration(type.Members);
                break;

            //case TokenType.Func:
                //ParseFunction();
                //break;

            default:
                Error(ErrorCodes.ParseUnexpectedSymbol, m_lex.Current);
                Resync(TokenType.Semicolon, TokenType.RightCurl);
                break;
            }
        }

        private void ParseClass()
        {
            Expect(TokenType.Class);

            var ident = m_lex.Current;
            bool valid = true;

            string name = ident.Literal;

            if (!Expect(TokenType.Identifier))
            {
                valid = false;
                name = m_symbolTable.GenName();
            }
            else
            {
                var sym = m_symbolTable.Find(name);

                if (sym != null)
                {
                    Error(ErrorCodes.ParseAlreadyDefined, sym, sym.LineNumber);
                    valid = false;
                }
            }

            var typeDef = new TypeDecl(TypeType.Class, ident.Literal, m_symbolTable);

            valid &= Expect(TokenType.LeftCurl);

            while (m_lex.Current.Type != TokenType.RightCurl)
            {
                ParseClassItem(typeDef);
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
    }
}
