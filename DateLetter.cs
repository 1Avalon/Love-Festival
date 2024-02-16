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

        private static Dictionary<string, ModDate> modDates;

        public int day;

        public NPC DatePartner;

        public ModDate Date;

        public DateLetter(ModDate date, NPC datePartner)
        {
            Date = date;
            DatePartner = datePartner;
            Content = date.DateLetterContent;
        }

        public static DateLetter getRandomDateLetter(NPC datePartner)
        {
            modDates ??= ModEntry.modHelper.GameContent.Load<Dictionary<string, ModDate>>(ModEntry.modDateEntryKey)

            int index = ModEntry.ModRandom.Next(modDates.Count);
           
            ModDate randomDate = modDates.ElementAt(index).Value;

            return new DateLetter(randomDate, datePartner); //TODO use values from modDates as soon as theyre loaded
        }
    }
}
