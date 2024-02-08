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

        public static Dictionary<string, Dictionary<string, object>> ModDates = new();

        public static readonly Dictionary<string, ModDate> Dates = new()
        {
            ["Beach"] = new ModDate("17819001/t 1900 2200",
                "ocean/25 18/farmer 26 10 2 NPCNAME 25 19 2/skippable/pause 500/move farmer 0 9 3/pause 500/faceDirection NPCNAME 1/pause 500/speak NPCNAME \"Hey @...#$b#Thank's for coming. I was waiting for you.$h#$b#I know this isn't the most romantic place for a date.#$b#However, there's a reason why I brought you here...\"/emote farmer 8/pause 1000/speak NPCNAME \"I always go to this place at night, whenever I need time for my thoughts.\"/speak NPCNAME \"The sound of the waves...\"/pause 500/speak NPCNAME \"It's so soothing...\"/pause 500/speak NPCNAME \"Try it, @.#$b#Try to be one with the sound of the waves.\"/pause 500/showFrame farmer 15/pause 1000/speak NPCNAME \"Now tell me, how do you feel\"/question fork1 \"How do you feel?#It's very relaxing.#Nothing, it's very silly honestly.\"/fork dislikingBeach/friendship 20/pause 500/speak NPCNAME \"I'm glad you like it.$h\"/pause 1000/speak NPCNAME \"Now, take a look in the distance, isn't it beautiful?$h\"/",
                "dislikingBeach")
        };
        public static readonly Dictionary<string, string> forks = new()
        {
            ["dislikingBeach"] = "friendship NPCNAME -20/speak NPCNAME \"Oh I see...$s#$b#It probably wasn't a good idea to take you to this place...$s\"/pause 1000/speak NPCNAME \"Well take a look in the distance\""
        };

        public static void updateDates()
        {

            Logger.Log_Warning("Updated Dates", true);
            ModEntry.modHelper.GameContent.InvalidateCache(ModEntry.ModDateEntryKey);
            ModDates = ModEntry.modHelper.GameContent.Load<Dictionary<string, Dictionary<string, object>>>(ModEntry.ModDateEntryKey);
            
        }
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

        public Dictionary<string, string> GenerateDateEventScript()
        {
            ModDate date = Dates[LocationName];
            string[] scriptForks = date.forks;
            string eventScript = date.EventScript.Replace("NPCNAME", DatePartner.Name);
            Dictionary<string, string> eventArguments = new Dictionary<string, string>();

            foreach (var item in forks)
            {
                if (scriptForks.Contains(item.Key))
                    eventArguments.Add(item.Key, item.Value.Replace("NPCNAME", DatePartner.Name));
            }
            eventArguments.Add(date.Condition, eventScript);
            Debug.WriteLine(eventArguments.Count);
            return eventArguments;

  
        }
    }
    public sealed class ModDate
    {

        public string Condition;

        public string EventScript;

        public string[] forks;

        public ModDate(string condition, string eventScript, params string[] forks)
        {
            Condition = condition;
            EventScript = eventScript;
            this.forks = forks;
        }
    }
}
