using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class LoveLetter : ModLetter
    {
        private static readonly Random random = ModEntry.ModRandom;

        private static readonly Dictionary<string, int[]> letterTypes = new Dictionary<string, int[]>
        {
            { "Food", new int[] { 204, 194, 195, 196, 206 } }, // Dish items
            { "Vegetable", new int[] { 24, 270, 264, 272 } },   //Parsnip Corn Radish Eggplant
            { "Crops", new int[] { 475, 745, 484, 480, 488 } }, //Potato Strawberry Radish Tomato Eggplant
            { "Gems", new int[] { 60, 62, 64, 66, 68 } },       // /Emerald Aquamarine Amethyst Topaz
        };

        private static readonly Dictionary<string, Func<string>> letterGenerators = new Dictionary<string, Func<string>>
        {
            { "Food", () => I18n.LoveLetter_FoodMail() },
            { "Vegetable", () => I18n.LoveLetter_VegetableMail() },
            { "Crops", () => I18n.LoveLetter_CropMail() },
            { "Gems", () => I18n.LoveLetter_GemMail() },
            { "Money", () => I18n.LoveLetter_MoneyMail() }
        };

        private static readonly Dictionary<string, Func<string>> attachmentGenerators = new Dictionary<string, Func<string>>
        {
            { "Food", () => Attachments.Item(letterTypes["Food"][random.Next(letterTypes["Food"].Length)]) },
            { "Vegetable", () => Attachments.Item(letterTypes["Vegetable"][random.Next(letterTypes["Vegetable"].Length)], random.Next(2, 11))},
            { "Crops", () => Attachments.Item(letterTypes["Crops"][random.Next(letterTypes["Crops"].Length)], random.Next(5, 21)) },
            { "Gems", () => Attachments.Item(letterTypes["Gems"][random.Next(letterTypes["Gems"].Length)]) },
            { "Money", () => Attachments.Money() }
        };

        public static LoveLetter GetRandomLetter()
        {
            List<string> keys = attachmentGenerators.Keys.ToList();
            int index = random.Next(keys.Count);
            return new LoveLetter(keys[index]);
        }
        public LoveLetter(string type)
        {
            this.Content = letterGenerators[type]();
            this.Attachment = attachmentGenerators[type]();
        }
    }
}
