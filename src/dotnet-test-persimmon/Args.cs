using System;
using System.Collections.Generic;

namespace Persimmon.Runner
{
    public class Args
    {
        // .Net Core runner options https://docs.microsoft.com/en-us/dotnet/articles/core/tools/test-protocol
        public bool DesignTime { get; private set; }

        public int Port { get; private set; } = -1;
        public bool PortSpecified => Port >= 0;

        public IList<string> Inputs { get; } = new List<string>();

        public bool WaitCommand { get; private set; }

        public bool List { get; private set; }

        public static Args Parse(string[] rawArgs)
        {
            var args = new Args();

            for(int i = 0; i < rawArgs.Length; i++)
            {
                switch (rawArgs[i])
                {
                    case "--port":
                        i++;
                        args.Port = int.Parse(rawArgs[i]);
                        break;
                    case "--designtime":
                        args.DesignTime = true;
                        break;
                    case "--wait-command":
                        args.WaitCommand = true;
                        break;
                    case "--list":
                        args.List = true;
                        break;
                    default:
                        args.Inputs.Add(rawArgs[i]);
                        break;
                }
            }

            return args;
        }
    }
}
