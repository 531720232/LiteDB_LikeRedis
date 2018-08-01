using System;

namespace LiteDB.Shell.Commands
{
    internal class Comment : IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return s.Match(@"--");
        }

        public void Execute(StringScanner s, Env env, out bool b, out object obj)
        {
            b = false;
            obj = null;
        }
    }
}