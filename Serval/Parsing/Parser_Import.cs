using Serval.Lexing;

namespace Serval
{
    public partial class Parser
    {
        private void ParseImport()
        {
            Expect(TokenType.Import);

            var ident = m_lex.Current;

            bool valid = Expect(TokenType.Identifier);

            valid &= Expect(TokenType.Semicolon);

            if (!valid)
                return;

            // TODO: Maybe compile module here.

            // TODO: Load the module here.
        }
    }
}
