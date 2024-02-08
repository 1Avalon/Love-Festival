using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public static class Logger
    {
        public static void Log_Warning(string message, bool debug = false) 
        {
            ModEntry.instance.Monitor.Log(message, StardewModdingAPI.LogLevel.Warn);
            if (debug)
                Debug.WriteLine(message);
        }
    }
}
