using StardewValley;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class ValentineMails
    {
        public List<LoveLetter> mails;

        public List<DateLetter> dates;

        public Dictionary<string, string> dateInformation;

        public int[] foods = { 204, 194, 195, 196, 206 };
        public int[] vegetables = { 24, 270, 264, 272 }; //Parsnip Corn Radish Eggplant
        public int[] crops = { 475, 745, 484, 480, 488 }; //Potato Strawberry Radish Tomato Eggplant
        public int[] gems = { 60, 62, 64, 66, 68 }; //Emerald Aquamarine Amethyst Topaz
        private string GenerateFoodMail()
        {
            int index = Game1.random.Next(foods.Length);
            return $"{I18n.LoveLetter_FoodMail()} ^^-NPCNAME%item object {foods[index]} 1 %%";
        }

        private string GenerateVegetableMail()
        {
            int index = Game1.random.Next(vegetables.Length);
            int amount = Game1.random.Next(2, 11);
            return $"{I18n.LoveLetter_VegetableMail()}^^-NPCNAME < %item object {vegetables[index]} {amount} %%";
        }

        private string GenerateCropMail()
        {
            int index = Game1.random.Next(crops.Length);
            int amount = Game1.random.Next(5, 21);
            return $"{I18n.LoveLetter_CropMail()}^^-NPCNAME < %item object {crops[index]} {amount} %%";
        }
        private string GenerateGemMail()
        {
            int index = Game1.random.Next(gems.Length);
            return $"{I18n.LoveLetter_GemMail()}^^-NPCNAME < %item object {gems[index]} 1 %%";
        }

        public string foodMail
        {
            get { return GenerateFoodMail(); }
        }
        public string vegetableMail
        {
            get { return GenerateVegetableMail(); }
        }

        public string cropMail
        {
            get { return GenerateCropMail(); }
        }

        public string gemMail 
        { 
            get { return GenerateGemMail(); } 
        }

        public string moneyMail = $"{I18n.LoveLetter_MoneyMail()}^^-NPCNAME%item money 1000 %%";

        public ValentineMails()
        {
            mails = new List<LoveLetter>()
            {
                new LoveLetter(GenerateFoodMail),
                new LoveLetter(GenerateVegetableMail),
                new LoveLetter(moneyMail),
                new LoveLetter(GenerateCropMail),
                new LoveLetter(GenerateGemMail),
            };

            dates = new List<DateLetter>()
            {
                new DateLetter("Hi uwu", "Beach")
            };
        }

        public static Item GetMailItem(string mailContext)
        {
            if (mailContext.Contains("item object"))
            {
                string[] attachments = mailContext.Split("%");
                string[] itemData = attachments[1].Split(" ");
                int itemID = Int32.Parse(itemData[2]);
                int quantity = Int32.Parse(itemData[3]);
                return new StardewValley.Object(itemID, quantity);
            }
            return null;
        }
    }
}
