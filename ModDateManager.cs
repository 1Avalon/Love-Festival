using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class ModDateManager
    {
        public NPC datePartner = null;

        public bool hasDate = false;

        public DateLetter dateLetter = null;
        public static void readContentPacks()
        {
            DateLetter.updateDates();

            foreach (var kvp in DateLetter.ModDates)
            {
                var b = kvp.Value;

                Logger.Log_Warning(b.ToString());
            }
        }
    }
}
