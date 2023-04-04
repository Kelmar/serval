using System;

namespace Serval.Fault
{
    public class CompileException : Exception
    {
        public CompileException(CompileErrors error, string fileName, int lineNum = 0, Exception innerException = null)
            : base(GetExceptionText(error, fileName, lineNum), innerException)
        {
            ErrorCode = error;
            FileName = fileName;
            LineNumber = lineNum;
        }

        private static string GetExceptionText(CompileErrors error, string fileName, int lineNum)
        {
            return String.Empty;
        }

        public CompileErrors ErrorCode { get; }

        public string FileName { get; }

        public int LineNumber { get; }
    }
}
