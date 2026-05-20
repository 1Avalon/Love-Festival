using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using LoveFestival.UI;
using Location = xTile.Dimensions.Location;
using StardewValley.BellsAndWhistles;

namespace LoveFestival
{
    public static class LoveFestivalPatches
    {
        private static double fontColorGradientOffset = 0;

        private static Color letterFontColor = Color.Black;

        private static string festivalKey = "StartLoveFestivalKey";

        public static bool boughtCupidArrow = false;

        public static NPC friendshipMultiplierTarget; //TODO move ít somewhere else to avoid CA2211

        public static void Prefix_drawString(SpriteText __instance, string s, ref Color? color)
        {
            double abs = Math.Abs(fontColorGradientOffset);
            if (s.StartsWith("$ ") && ModEntry.isValentinesFestival && ModEntry.Config.ChangeMoneyTextColorInLetter)
                color = ModEntry.Config.ChangeMoneyTextColorInLetter ? Color.Lerp(Color.Lavender, Color.Red, (float)Math.Sin(abs)) : Color.Black;
        }

        public static bool Prefix_setNewDialogue(NPC __instance, string translationKey)
        {
            if (translationKey == festivalKey)
            {
                __instance.setNewDialogue(new Dialogue(__instance, null, $"$q -1 null#{I18n.StartMessage_Question()}#$r -1 0 yes#{I18n.StartMessage_AnswerYes()}#$r -1 0 no#{I18n.StartMessage_AnswerNo()}"));
                return false;
            }
            return true;
        }
        public static void Postfix_LewisFestivalPatch(Event __instance, string id)
        {
            if (id == "LoveFestival17819")
            {
                if (ModEntry.chosenLoveLetterGifters.Count > 0)
                    ModEntry.chosenLoveLetterGifters.Clear();
                string command = ModEntry.getMainEvent();
                ModEntry.modHelper.GameContent.InvalidateCache(ModEntry.modDateEntryKey);
                foreach (NPC npc in ModEntry.chosenLoveLetterGifters)
                {
                    __instance.actors.Add(npc);
                }

                ModEntry.debrisEnabled = true; //TODO move these to a method and call it ResetValues
                ModEntry.loveLetterNotGivenToSpouse = false;
                ModEntry.letterSent = false;
                ModEntry.seenSpouseDialogue = false;
                ModEntry.isValentinesFestival = true;
                ModEntry.isGoingOnDate = false;
                boughtCupidArrow = false;
                Game1.populateDebrisWeatherArray();
                var festData = ModEntry.instance.Helper.Reflection.GetField<Dictionary<string, string>>(Game1.CurrentEvent, "festivalData").GetValue();
                //string agreedToDateInformation = festData["dialogueDateAgreed"].Replace("DATEINFORMATION", ModEntry.modHelper.Translation.Get($"LoveLetter.{ModEntry.date}Information"));
                if (!festData["mainEvent"].Contains("LoveFestival17819command"))
                    festData["mainEvent"] = $"pause 500/globalFade/viewport -1000 -1000/warp farmer 39 22/stopAnimation Robin/stopAnimation Demetrius/faceDirection farmer 2/warp Marnie 38 22/faceDirection Marnie 2/warp Lewis 40 22/faceDirection Lewis 2/viewport 39 22 true/pause 1500/speak Marnie \"{I18n.MarnieReaction_Start()}\"LoveFestival17819command/waitForOtherPlayers festivalEnd/end";
                string newCommands = festData["mainEvent"].Replace("LoveFestival17819command", command);
                festData["mainEvent"] = newCommands;
                //festData["dialogueDateAgreed"] = agreedToDateInformation;
                //ModEntry.PushNPCDialogues(__instance.actors, __instance.farmer);
                ModEntry.instance.Helper.Reflection.GetField<Dictionary<string, string>>(Game1.CurrentEvent, "festivalData").SetValue(festData);
                ModEntry.instance.Helper.Reflection.GetField<NPC>(__instance, "festivalHost").SetValue(__instance.getActorByName("Lewis"));
                ModEntry.instance.Helper.Reflection.GetField<string>(__instance, "hostMessageKey").SetValue(festivalKey);
            }
        }
        public static void Prefix_ForceFestivalContinuePatch(Event __instance, Farmer who, Location tileLocation)
        {
            if (__instance.isSpecificFestival($"winter{ModEntry.festivalDate}"))
            {
                GameLocation location = Game1.currentLocation;
                string tileAction = location.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");

                if (tileAction == "CupidShop") //TODO: Move this to the new hook
                {
                    Response[] responses = new Response[2]
                    {
                        new Response("Yes", I18n.CupidStore_Yes()),
                        new Response("No", I18n.StartMessage_AnswerNo())
                    };
                    location.createQuestionDialogue(I18n.CupidStore_BuyArrow(), responses, new GameLocation.afterQuestionBehavior((Farmer who, string dialogue_id) =>
                    {
                        if (dialogue_id == "Yes")
                        {
                            if (boughtCupidArrow)
                            {
                                Game1.drawObjectDialogue(I18n.CupidStore_AlreadyBought());
                                return;
                            }
                            if (Game1.player.Money < 2500)
                            {
                                Game1.drawObjectDialogue(I18n.Misc_NotEnoughMoney());
                                return;
                            }

                            Game1.activeClickableMenu = new GameMenuWrapper();
                            //Game1.drawObjectDialogue("Increased friendship gain for Haley by 20% for 7 days.");
                        }
                    }));
                }
            }
        }

        public static bool Prefix_changeFriendship(Farmer __instance, ref int amount, NPC n)
        {
            if (ModEntry.multiplier != null && n == Game1.getCharacterFromName(ModEntry.multiplier.targetName) && !ModEntry.isValentinesFestival) //dont activate during festival otherwise the letter will add too many points
            {
                float amount2 = amount * 1.2f;
                amount = (int)amount2;
            }
            return true;
        }
        public static void Postfix_DebrisDuringFestivalPatch(ref bool __result)
        {
            if (Game1.Date.DayOfMonth == ModEntry.festivalDate && Game1.Date.Season == Season.Winter || ModEntry.isValentinesFestival)
            {
                __result = true;
            }
        }
        public static void Postfix_PopulateDebrisPatch(Game1 __instance)
        {
            if (!Context.IsWorldReady) return;

            if (Game1.Date.DayOfMonth == ModEntry.festivalDate && Game1.Date.Season == Season.Winter || ModEntry.isValentinesFestival) // perhaps use static field so the leaves will rain when getting to the festival although its not happening "natural"
            {
                int num = Game1.random.Next(32, 96);
                for (int i = 0; i < num; i++)
                {
                    Game1.debrisWeather.Add(new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.viewport.Width), (float)Game1.random.Next(0, Game1.viewport.Height)), 2, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
                    Game1.debrisWeather.Add(new WeatherDebris(new Vector2((float)Game1.random.Next(0, Game1.viewport.Width), (float)Game1.random.Next(0, Game1.viewport.Height)), 1, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
                }
            }
        }
        public static bool Prefix_CustomWeatherDebrisPatch(WeatherDebris __instance, ref SpriteBatch b)
        {
            if (Game1.Date.DayOfMonth == ModEntry.festivalDate && Game1.Date.Season == Season.Winter || ModEntry.isValentinesFestival)
            {
                Texture2D texture = __instance.which == 2 ? ModEntry.redRoseDebris : ModEntry.greenRoseDebris;

                b.Draw(texture, __instance.position, __instance.sourceRect, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1E-06f);
                return false;
            }

            return true;
        }
        public static void Postfix_CustomWeatherDebrisUpdatePatch(WeatherDebris __instance)
        {
            if (Game1.Date.DayOfMonth == ModEntry.festivalDate && Game1.Date.Season == Season.Winter || ModEntry.isValentinesFestival)
            {
                __instance.sourceRect.X = 0 + __instance.animationIndex * 16;
                __instance.sourceRect.Y = 0;
            }
        }
        public static void Postfix_IsDebrisWeatherHere(GameLocation __instance, ref bool __result)
        {
            if (Game1.Date.DayOfMonth == ModEntry.festivalDate && Game1.Date.Season == Season.Winter || ModEntry.isValentinesFestival)
            {
                __result = true;
            }
        }
    }
}
