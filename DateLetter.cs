using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class DateLetter : LoveLetter
    {

        private static Dictionary<string, ModDate> modDates;

        [Obsolete]
        public static readonly Dictionary<string, ModDate> Dates = new(); //buffDict = SHelper.GameContent.Load<Dictionary<string, Dictionary<string, object>>>(dictKey);

        [Obsolete]
        public static readonly Dictionary<string, string> forks = new();

        public new string Context { get; set; }
        public string CachePath { get; set; }

        public string LocationName { get; set; }

        public NPC DatePartner;

        public string DateEventScript;

        public int day;

        public DateLetter(string context, string locationName) : base(context)
        {
            this.Context = context;
            this.CachePath = $"Events/{locationName}";
            this.LocationName = locationName;
        }

        public static DateLetter getRandomDateLetter()
        {
            modDates ??= ModEntry.modHelper.GameContent.Load<Dictionary<string, ModDate>>(ModEntry.modDateEntryKey);

            Logger.Log_Info(modDates.Keys.Count.ToString(), true);

            return new DateLetter("abc", "aav"); //TODO use values from modDates as soon as theyre loaded
        }

        public Dictionary<string, string> GenerateDateEventScript()
        {
            ModDate date = Dates[LocationName];
            string[] scriptForks = date.forks;
            string eventScript = date.eventScript.Replace("NPCNAME", DatePartner.Name);
            Dictionary<string, string> eventArguments = new Dictionary<string, string>();

            foreach (var item in forks)
            {
                if (scriptForks.Contains(item.Key))
                    eventArguments.Add(item.Key, item.Value.Replace("NPCNAME", DatePartner.Name));
            }
            eventArguments.Add(date.condition, eventScript);
            Debug.WriteLine(eventArguments.Count);
            return eventArguments;

  
        }
    }
}
