using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class DateLetter : ModLetter
    {
        public string DateId;

        public string AcceptDateResponse;

        private readonly string DAYSUNTILDATE_TOKEN = "{DAYS_UNTIL_DATE}";

        public int day;

        public string NpcName;
        public void LoadDaysUntillDateToken()
        {
            AcceptDateResponse = AcceptDateResponse.Replace(DAYSUNTILDATE_TOKEN, (day - Game1.dayOfMonth).ToString());
        }
        public static DateLetter getRandomDateLetter()
        {
            Dictionary<string, DateLetter> modLetters = ModEntry.modHelper.GameContent.Load<Dictionary<string, DateLetter>>(ModEntry.modDateLetterEntryKey);
            int index = ModEntry.ModRandom.Next(modLetters.Count);
            return modLetters.ElementAt(index).Value;
            
        }
    }
}

