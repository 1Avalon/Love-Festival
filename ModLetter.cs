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

        public string ContentWithAttachment()
        {
            if (Attachment == null)
                return Content;

            return Content + "^^-NPCNAME < " + Attachment;
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
