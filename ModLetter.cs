using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public abstract class ModLetter
    {
        public string Content;

        public string Attachment = null;

        public string FullLetter;

        public string ContentWithAttachment(string substitutableNpcName = null)
        {

            Attachment ??= "";

            string basic = Content.Contains(substitutableNpcName) ? Content + " " + Attachment : Content + "^^-NPCNAME < " + Attachment;

            if (substitutableNpcName != null)
            {
                string basic2 = basic.Replace("NPCNAME", substitutableNpcName);
                FullLetter = basic2.Replace("{DATEPARTNER_NAME}", substitutableNpcName); // just for safety optimise later
            }

            return FullLetter;
        }
        public bool HasItem()
        {
            return Attachment.Contains("%item object");
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
