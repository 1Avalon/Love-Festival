using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public static class LoveFestivalPatches
    {
        private static Color letterFontColor = new Color(0, 61, 40);

        private static string festivalKey = "StartLoveFestivalKey";
        public static bool Prefix_CustomLetterBackgroundPatch(LetterViewerMenu __instance)
        {
            if (__instance.mailTitle is null) // secret note
                return true;

            if (__instance.mailTitle.Contains("LoveFestival17819") && ModEntry.isValentinesFestival)
            {
                __instance.letterTexture = ModEntry.bgLoveLetter;
            }
            return true;
        }
        public static void Postfix_FontColorPatch(LetterViewerMenu __instance, ref Color? __result)
        {
            if (__instance.letterTexture == ModEntry.bgLoveLetter)
                __result = letterFontColor;
        }

        public static bool Prefix_setNewDialogue(NPC __instance, string translationKey)
        {
            if (translationKey == festivalKey)
            {
                __instance.setNewDialogue(new Dialogue(__instance, null, $"$q -1 null#{I18n.StartMessage_Question()}?#$r -1 0 yes#{I18n.StartMessage_AnswerYes()}#$r -1 0 no#{I18n.StartMessage_AnswerNo()}"));
                return false;
            }
            return true;
        }
        public static void Postfix_LewisFestivalPatch(Event __instance, string id)
        {
            if (id == "LoveFestival17819")
            {
                ModEntry.datePartner = null;
                ModEntry.dateLetter = null;
                ModEntry.ExecuteDateQuestion = false;
                if (ModEntry.chosenLoveLetterGifters.Count > 0)
                    ModEntry.chosenLoveLetterGifters.Clear();
                string command = ModEntry.getMainEvent();
                foreach (NPC npc in ModEntry.chosenLoveLetterGifters)
                {
                    __instance.actors.Add(npc);
                }

                ModEntry.debrisEnabled = true;
                ModEntry.loveLetterNotGivenToSpouse = false;
                ModEntry.letterSent = false;
                ModEntry.seenSpouseDialogue = false;
                ModEntry.isValentinesFestival = true;
                ModEntry.isGoingOnDate = false;
                Game1.populateDebrisWeatherArray();
                var festData = ModEntry.instance.Helper.Reflection.GetField<Dictionary<string, string>>(Game1.CurrentEvent, "festivalData").GetValue();
                //string agreedToDateInformation = festData["dialogueDateAgreed"].Replace("DATEINFORMATION", ModEntry.modHelper.Translation.Get($"LoveLetter.{ModEntry.date}Information"));
                string newCommands = festData["mainEvent"].Replace("LoveFestival17819command", command);
                festData["mainEvent"] = newCommands;
                Debug.WriteLine(newCommands);
                //festData["dialogueDateAgreed"] = agreedToDateInformation;
                //ModEntry.PushNPCDialogues(__instance.actors, __instance.farmer);
                ModEntry.instance.Helper.Reflection.GetField<Dictionary<string, string>>(Game1.CurrentEvent, "festivalData").SetValue(festData);
                ModEntry.instance.Helper.Reflection.GetField<NPC>(__instance, "festivalHost").SetValue(__instance.getActorByName("Lewis"));
                ModEntry.instance.Helper.Reflection.GetField<string>(__instance, "hostMessageKey").SetValue(festivalKey);
            }
        }
        public static void Prefix_ForceFestivalContinuePatch(Event __instance, Farmer who)
        {
            if (__instance.isSpecificFestival("winter13"))
            {
                foreach (NPC npc in __instance.actors)
                {
                    if (!ModEntry.letterSent)
                    {
                        if ((bool)npc.datable || who.spouse == npc.Name)
                        {
                            if (npc.CurrentDialogue.Count > 0 && npc.CurrentDialogue.Peek().getCurrentDialogue().Equals(ModEntry.dialogueToBeReplaced))
                            {
                                npc.CurrentDialogue.Clear();
                            }
                            if (npc.CurrentDialogue.Count == 0)
                            {
                                npc.CurrentDialogue.Push(new Dialogue(npc, null, ModEntry.dialogueToBeReplaced));
                            }
                        }
                    }
                }
            }
        }
        public static void Postfix_DebrisDuringFestivalPatch(ref bool __result)
        {
            if (Game1.Date.DayOfMonth == 13 && Game1.Date.Season == Season.Winter && Game1.isFestival() || ModEntry.isValentinesFestival)
                __result = true;
        }
        public static void Postfix_PopulateDebrisPatch(Game1 __instance)
        {
            if (!Context.IsWorldReady) return;

            if (Game1.Date.DayOfMonth == 13 && Game1.Date.Season == Season.Winter && Game1.isFestival() || ModEntry.isValentinesFestival) // perhaps use static field so the leaves will rain when getting to the festival although its not happening "natural"
            {
                int num = Game1.random.Next(16, 64);
                for (int i = 0; i < num; i++)
                {
                    Game1.debrisWeather.Add(new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.viewport.Width), (float)Game1.random.Next(0, Game1.viewport.Height)), 2, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
                    Game1.debrisWeather.Add(new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.viewport.Width), (float)Game1.random.Next(0, Game1.viewport.Height)), 1, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
                }
            }
        }
        public static bool Prefix_CustomWeatherDebrisPatch(WeatherDebris __instance, ref SpriteBatch b)
        {
            if (Game1.Date.DayOfMonth == 13 && Game1.Date.Season == Season.Winter && Game1.isFestival() || ModEntry.isValentinesFestival)
            {
                Texture2D texture = __instance.which == 2 ? ModEntry.redRoseDebris : ModEntry.greenRoseDebris;

                b.Draw(texture, __instance.position, __instance.sourceRect, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1E-06f);
                return false;
            }

            return true;
        }
        public static void Postfix_CustomWeatherDebrisUpdatePatch(WeatherDebris __instance)
        {
            if (Game1.Date.DayOfMonth == 13 && Game1.Date.Season == Season.Winter && Game1.isFestival() || ModEntry.isValentinesFestival)
            {
                __instance.sourceRect.X = 0 + __instance.animationIndex * 16;
                __instance.sourceRect.Y = 0;
            }
        }
    }
}
