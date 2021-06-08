using System;

namespace LangTest
{
    public class Reporter : IReporter
    {
        public int ErrorCount { get; private set; }

        public int WarnCount { get; private set; }

        private void LogCondition(int lineNumber, string type, string fmt, object[] args)
        {
            string msg = args == null ? fmt : String.Format(fmt, args);
            Console.WriteLine("{0}:{1} %{2}", type, lineNumber, msg);
        }

        public void Error(int lineNumber, string fmt, params object[] args)
        {
            ++ErrorCount;
            LogCondition(lineNumber, "ERROR", fmt, args);
        }

        public void Error(Token t, string fmt, params object[] args)
        {
            ++ErrorCount;
            LogCondition(t.LineNumber, "ERROR", fmt, args);
        }

        public void Warn(int lineNumber, string fmt, params object[] args)
        {
            ++WarnCount;
            LogCondition(lineNumber, "WARN", fmt, args);
        }

        public void Warn(Token t, string fmt, params object[] args)
        {
            ++WarnCount;
            LogCondition(t.LineNumber, "WARN", fmt, args);
        }
    }
}
