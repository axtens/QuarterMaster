namespace QuarterMaster.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public static class CommandLineArguments
    {
        public static Dictionary<string, string> Arguments(bool keepSlash = false, bool debug = false)
        {
            if (debug) Debugger.Launch();
            Dictionary<string, string> dict = new Dictionary<string, string>();
            const int c = 0;
            foreach (string arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("/", StringComparison.CurrentCulture))
                {
                    int index = arg.IndexOf(":", c, StringComparison.CurrentCulture);
                    string name;
                    string value;
                    if (index != -1)
                    {
                        int start = keepSlash ? 0 : 1;
                        int end = keepSlash ? index : index - 1;
                        name = arg.Substring(start, end);
                        value = arg.Substring(index + 1);
                    }
                    else
                    {
                        name = arg.Substring(keepSlash ? 0 : 1);
                        value = string.Empty;
                    }

                    dict.Add(name, value);
                }
            }

            return dict;
        }
    }
}
