using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class ModLetter
    {
        public string Content;

        public string Attachment = null;

        public string FullLetter;

        private readonly string BG_NAME = $"[letterbg {ModEntry.instance.ModManifest.UniqueID}/LoveLetterBackground 0][textcolor white]";

        public string ContentWithAttachment(string substitutableNpcName)
        {

            Attachment ??= "";

            string content = Content + " " + Attachment + "^^-" + substitutableNpcName + " " + BG_NAME;

            string basic2 = content.Replace("NPCNAME", substitutableNpcName);

            FullLetter = basic2;

            return FullLetter;
        }
    }

    public static class Attachments
    {
        public static string Money(int money)
        {
            return $" %item money {money} %%";
        }

        public static string Money()
        {
            return $"%item money 1000 %%";
        }
        public static string Money(int moneyMin, int moneyMax)
        {
            return $"%item money {moneyMin} {moneyMax} %%";
        }
        public static string Item(int itemID, int amount = 1)
        {
            return $"%item object {itemID} {amount} %%";
        }
    }
}
