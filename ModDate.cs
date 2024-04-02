using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace LoveFestival
{
    public sealed class ModDate
    {
        public string Condition;

        public string Location;

        public Dictionary<string, string> EventScript;

        public int day;

        public string DatePartnerName;

        public string Id;
        public static ModDate GetDateFromLetter(DateLetter letter)
        {
            Dictionary<string, ModDate> modDates = ModEntry.modHelper.GameContent.Load<Dictionary<string, ModDate>>(ModEntry.modDateEntryKey);
            ModDate date = modDates[letter.DateId];
            date.Id = letter.DateId;
            date.day = letter.day;
            return date;
        } 
    }
}
