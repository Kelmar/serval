using System;
using System.Linq;

using Serval.CodeGen;
using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    public class Reporter : IReporter
    {
        public int ErrorCount { get; private set; }

        public int WarnCount { get; private set; }

        private string TranslateError(ErrorCodes errorCode)
        {
            // TODO: Look this up in a table.

            return errorCode switch
            {
                ErrorCodes.LexHangingEscape => "String escape at end of line",
                ErrorCodes.LexUnterminatedString => "Unterminated string",
                ErrorCodes.LexInvalidConstant => "Invalid constant value: {0}",
                ErrorCodes.LexIdentifierTooLong => "Identifier too long",
                ErrorCodes.LexInvalidHexChar => "Invalid hexadecimal value for character",
                ErrorCodes.LexUnknownEscape => "Unknown character escape code",
                ErrorCodes.LexExpectedEndOfString => "Expected terminating quote",
                ErrorCodes.LexExpectedEndOfChar => "Expected terminating single quote",

                ErrorCodes.LexUnknownError => "BUG: Unknown lexing error",

                ErrorCodes.ParseUnexpectedSymbol => "Unexpected {0} symbol",
                ErrorCodes.ParseExpectedSymbol => "Unexpected {0} symbol, expecting {1}",
                ErrorCodes.ParseUnexpectedEOF => "Unexpected end of file",
                ErrorCodes.ParseAlreadyDefined => "Identifier '{0}' already defined on line {1}",
                ErrorCodes.ParseUndeclaredVar => "Undeclared variable '{0}'",
                ErrorCodes.ParseAssignToLabel => "Cannot assign to label '{0}'",
                ErrorCodes.ParseTypeNotValidHere => "'{0}' is a type, which is not valid in this context",

                ErrorCodes.ParseUnknownError => "BUG: Unknown parsing error",

                _ => $"Unknown error code: {errorCode}"
            };
        }

        private string TranslateTokenType(TokenType type)
        {
            int i = (int)type;

            if (i >= 32 && i <= 255)
                return $"'{(char)i}'"; // Translate back to literal character.

            // TODO: Look this up in a table.
            return type.ToString();
        }

        private string TranslateSemanticType(SemanticType type)
        {
            // TODO: Look this up in a table.
            return type.ToString();
        }

        private string TranslateObject(object o)
        {
            switch (o)
            {
            case Token token:
                switch (token.Type)
                {
                case TokenType.Identifier:
                case TokenType.IntConst:
                case TokenType.FloatConst:
                case TokenType.StringConst:
                    return token.Literal;

                default:
                    return TranslateTokenType(token.Type);
                }

            case Symbol symbol:
                return symbol.Name;

            case TokenType type:
                return TranslateTokenType(type);

            case SemanticType type:
                return TranslateSemanticType(type);

            case string s:
                return s;

            default:
                return o.ToString();
            }
        }

        public void Error(int lineNumber, ErrorCodes errorCode, params object[] args)
        {
            ++ErrorCount;

            string message = TranslateError(errorCode);

            if (args != null)
                message = String.Format(message, args.Select(TranslateObject).ToArray());

            string file = String.Empty;
            Console.WriteLine("{0}{1}: ERROR S{2}: {3}", file, lineNumber, (int)errorCode, message);
        }

        public void Error(Token t, ErrorCodes errorCode, params object[] args)
        {
            Error(t.LineNumber, errorCode, args);
        }
    }
}
