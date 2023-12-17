using Serval.CodeGen;
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
            }
        }

        private void ParseClass()
        {
            Expect(TokenType.Class);

            var ident = m_lex.Current;

            if (!Expect(TokenType.Identifier))
                return;

            var type = new TypeDecl(ident.Literal, m_symbolTable);

            if (!Expect(TokenType.LeftCurl))
                return;

            while (m_lex.Current.Type != TokenType.RightCurl)
            {
                ParseClassItem(type);
            }

            if (!Expect(TokenType.RightCurl))
                return;

            m_symbolTable.Add(new Symbol
            {
                Name = ident.Literal,
                Usage = SymbolUsage.Type,
                LineNumber = ident.LineNumber
            });
        }
    }
}
