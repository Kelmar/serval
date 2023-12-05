using Serval.Fault;
using Serval.Lexing;

namespace Serval
{
    public interface IReporter
    {
        int ErrorCount { get; }

        void Error(int lineNumber, ErrorCodes errorCode, params object[] args);

        void Error(Token t, ErrorCodes errorCode, params object[] args);
    }
}
