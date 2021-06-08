namespace LangTest
{
    public interface IReporter
    {
        void Error(int lineNumber, string fmt, params object[] args);

        void Error(Token t, string fmt, params object[] args);

        void Warn(int lineNumber, string fmt, params object[] args);

        void Warn(Token t, string fmt, params object[] args);
    }
}
