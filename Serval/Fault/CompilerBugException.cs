using System;

namespace Serval.Fault
{
    internal class CompilerBugException : Exception
    {
        public CompilerBugException(string reason, Exception innerEx = null)
            : base(reason, innerEx)
        {
        }
    }
}
