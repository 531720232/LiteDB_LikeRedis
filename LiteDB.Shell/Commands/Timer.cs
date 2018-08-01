using System;

namespace LiteDB.Shell.Commands
{
    [Help(
        Category = "Shell",
        Name = "timer",
        Syntax = "timer",
        Description = "Print millisecond counter before command. Used to test command performance."
    )]
    internal class Timer : IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return s.Match(@"timer$");
        }

        public void Execute(StringScanner s, Env env, out bool b, out object obj)
        {
            b = false;
            obj = null;
            env.Input.Timer.Start();
        }
    }
}