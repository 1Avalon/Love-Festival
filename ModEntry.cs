using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using HarmonyLib;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Collections.Generic;
using xTile;
using SpaceShared.APIs;
using Microsoft.VisualBasic;
using System.Xml.Linq;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace LoveFestival
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public static Mod instance;

        public static IModHelper modHelper;

        public static Texture2D bgLoveLetter;
        public static Texture2D redRoseDebris;
        public static Texture2D greenRoseDebris;
        public static Texture2D beachNightSky;


        public static List<NPC> npcs;
        public static bool letterSent = false;

        public static bool loveLetterNotGivenToSpouse = false;

        public static bool isGoingOnDate = false;

        private List<string> receiveLetterReactions = new List<string>();
        public static List<string> reactions = new List<string>();

        public static bool seenSpouseDialogue = false;
        public List<string> spouseDialouges;

        public static bool debrisEnabled = false;

        public static bool isValentinesFestival = false;

        public static readonly string dialogueToBeReplaced = "AvalonMFX.LoveFestival17819";

        public static List<NPC> chosenLoveLetterGifters = new();

        public static NPC datePartner;

        public static Random ModRandom;

        public static DateLetter dateLetter;

        public static bool ExecuteDateQuestion = false;
        public static void PushNPCDialogues(List<NPC> npcs, Farmer who)
        {
            foreach (NPC npc in npcs)
            {
                if ((bool)npc.datable || who.spouse == npc.Name)
                {
                    //if (npc.CurrentDialogue.Count > 0 && npc.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
                    // npc.CurrentDialogue.Clear();
                    if (npc.CurrentDialogue.Count == 0 && ModEntry.letterSent == false && who.spouse == npc.Name)
                    {
                        npc.CurrentDialogue.Push(new Dialogue(dialogueToBeReplaced, npc));
                        npc.setNewDialogue(dialogueToBeReplaced);
                        //npc.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1736", npc.displayName), add: true); //Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1736", npc.displayName)
                    }
                }
            }
        }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            instance = this;
            modHelper = helper;
            bgLoveLetter = Helper.ModContent.Load<Texture2D>("assets/love_letter_bg");
            redRoseDebris = Helper.ModContent.Load<Texture2D>("assets/red_rose_debris");
            greenRoseDebris = Helper.ModContent.Load<Texture2D>("assets/green_rose_debris");
            beachNightSky = Helper.ModContent.Load<Texture2D>("assets/OceanSkySheet");

            helper.Events.Content.AssetRequested += this.OnAssetRequest;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            spouseDialouges = GetSpouseDialogues();

            ModRandom = new Random();

            I18n.Init(helper.Translation);

            Harmony harmony = new("AvalonMFX.LoveFestival");
            //harmony.PatchAll();
            harmony.Patch(
                original: AccessTools.Method(typeof(LetterViewerMenu), nameof(LetterViewerMenu.draw), new Type[] {typeof(SpriteBatch)}),
                prefix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Prefix_CustomLetterBackgroundPatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(LetterViewerMenu), nameof(LetterViewerMenu.getTextColor)),
                postfix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Postfix_FontColorPatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.setUpPlayerControlSequence)),
                postfix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Postfix_LewisFestivalPatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.checkAction)),
                prefix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Prefix_ForceFestivalContinuePatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.IsDebrisWeatherHere)),
                postfix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Postfix_DebrisDuringFestivalPatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.populateDebrisWeatherArray)),
                postfix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Postfix_PopulateDebrisPatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(WeatherDebris), nameof(WeatherDebris.draw)),
                prefix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Prefix_CustomWeatherDebrisPatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(WeatherDebris), nameof(WeatherDebris.update), new Type[] { typeof(bool) }),
                postfix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Postfix_CustomWeatherDebrisUpdatePatch))
                );
            // Patching manually provides mobile compatibility ? TODO: simplify */


        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var spacecore = Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            spacecore.AddEventCommand("showLoveLetter", typeof(ModEntry).GetMethod(nameof(showLoveLetter_command)));
            spacecore.AddEventCommand("askForDate", typeof(ModEntry).GetMethod(nameof(LoveFestival_AskForDate_command)));
            spacecore.AddEventCommand("showShootingStar", typeof(ModEntry).GetMethod(nameof(showShootingStar_command)));
            Monitor.Log("Game crashed due to Love Festival? Make sure to update to the newest version if you haven't done it yet. There are new bug fixes every update!", LogLevel.Warn);
        }

        public static void showLoveLetter_command(Event instance, GameLocation location, GameTime time, string[] split)
        {
            if (Game1.activeClickableMenu is not LetterViewerMenu)
            {
                ValentineMails loveLetters = new();
                LoveLetter loveLetter;
                string authorName = split[1].Split("_")[0];
                if (datePartner != null && datePartner.Name == authorName && dateLetter == null)
                {
                    List<DateLetter> dates = loveLetters.dates;
                    int index = ModRandom.Next(0, dates.Count);
                    loveLetter = dates[index];
                    dateLetter = loveLetter as DateLetter;
                    dateLetter.DatePartner = datePartner;
                }
                else
                {
                    List<LoveLetter> mails = loveLetters.mails;
                    int index = ModRandom.Next(0, mails.Count);
                    if (Game1.player.isInventoryFull())
                        index = 2; //money mail
                    loveLetter = mails[index];
                }
                string msg = loveLetter.Context.Replace("NPCNAME", authorName);

                LetterViewerMenu menu = new LetterViewerMenu(msg, split[1]);
                //LetterViewerMenu menu = new LetterViewerMenu(msg, split[1]);
                menu.exitFunction = () =>
                {
                    instance.CurrentCommand++;
                };
                Game1.activeClickableMenu = menu;
            }
        }
        public static void LoveFestival_AskForDate_command(Event instance, GameLocation location, GameTime time, string[] split)
        {

            if (!ExecuteDateQuestion)
            {
                ExecuteDateQuestion = true;
                List<Response> choices = new List<Response>()
                {
                    new Response("dateAccepted", I18n.StartMessage_AnswerYes()),
                    new Response("dateRejected", I18n.StartMessage_AnswerNo())
                };
                string character = split[1];
                NPC npc = Game1.getCharacterFromName(character);
                Game1.currentLocation.createQuestionDialogue(I18n.LoveFestivalDates_Question(npc: character), choices.ToArray(),
                    new GameLocation.afterQuestionBehavior((Farmer who, string dialogue_id) =>
                {
                    if (dialogue_id == "dateAccepted")
                    {
                        Game1.activeClickableMenu = null;
                        BillboardWrapper wrapper = new BillboardWrapper();

                        wrapper.exitFunction = () =>
                        {
                            dateLetter.day = wrapper.day;
                            Debug.WriteLine(wrapper.day);
                            Debug.WriteLine($"Having date on day {dateLetter.day}");
                            Game1.drawDialogue(npc, "YES OMG HAHA XDROFL33");
                        };
                        Game1.activeClickableMenu = wrapper;

                        //modHelper.GameContent.InvalidateCache(dateLetter.CachePath);
                    }

                    else
                        Game1.drawDialogue(npc, I18n.LoveFestivalDates_DateRejected());
                }));
            }
        }

        public static void showShootingStar_command(Event instance, GameLocation location, GameTime time, string[] split)
        {
            Game1.fadeScreenToBlack();
            Game1.spriteBatch.Begin();
            Game1.spriteBatch.Draw(beachNightSky, Game1.viewportCenter.ToVector2(), new Rectangle(0, 0, 672, 5125), Color.White);
        }
        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (e.OldLocation.Name == "Temp" && Game1.Date.Season == "winter" && Game1.Date.DayOfMonth == 13 || e.OldLocation.Name == "Temp" && isValentinesFestival)
            {
                ExecuteDateQuestion = false;
                chosenLoveLetterGifters.Clear();
                Game1.debrisWeather.Clear();
                isValentinesFestival = false;
                datePartner = null;
                isGoingOnDate = false;
                if (loveLetterNotGivenToSpouse)
                {
                    NPC spouse = Game1.getCharacterFromName(Game1.player.spouse);
                    spouse.CurrentDialogue.Push(new Dialogue(I18n.JealousyDialogue_Spouse(), spouse));
                    seenSpouseDialogue = false;
                    //spouse.setNewDialogue("I truly appreciate how you gave your didn't give me your love letter...$s");
                }
            }
            else if (dateLetter != null && e.NewLocation.Name == dateLetter.LocationName && Game1.Date.DayOfMonth == dateLetter.day)
            {
                modHelper.GameContent.InvalidateCache(dateLetter.CachePath);
                Debug.WriteLine("Invalidating Cache...");
            }
        }

        private void OnSaveLoaded(object? sender, EventArgs e)
        {
            npcs = getAllNPCs();
            foreach (var translation in Helper.Translation.GetTranslations())
            {
                const string receiveLetterPrefix = "ReceiveLetterReaction.";
                const string npcGiftingPrefix = "npcGiftingLetter.";

                if (translation.Key.StartsWith(receiveLetterPrefix))
                    receiveLetterReactions.Add(translation.ToString());
                else if (translation.Key.StartsWith(npcGiftingPrefix))
                    reactions.Add(translation.ToString());

                isValentinesFestival = false;
            }
        }
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        { 
            WorldDate date = Game1.Date;
            if (dateLetter != null && dateLetter.day == date.DayOfMonth)
            {

                Debug.WriteLine("realoding maaan");
                //modHelper.GameContent.InvalidateCache(dateLetter.CachePath);
            }

            else if (date.DayOfMonth == 6 && date.Season == "winter")
            {
                Game1.player.mailbox.Add("VEInvitationLetterWeek");
            }

            else if (date.DayOfMonth == 12 && date.Season == "winter")
            {
                Game1.player.mailbox.Add("VEInvitationLetterTomorrow");
                //Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = 2; //remove and check if it changes anything
            }
        }
        public static string getRandomLetterDialogue()
        {
            int index = ModRandom.Next(0, reactions.Count);
            return reactions[index];
        }

        private List<string> GetSpouseDialogues()
        {
            List<string> dialogues = new List<string>();
            foreach (var translation in Helper.Translation.GetTranslations())
                if (translation.Key.Contains("_spouse"))
                {
                    string msg = translation.ToString();
                    string[] dialogue = msg.Split("$");
                    dialogues.Add(dialogue[0]);
                }

            return dialogues;

        }
        public static string getMainEvent()
        {
            string commands = "";

            foreach (NPC npc in npcs)
            {
                if (!Game1.player.friendshipData.ContainsKey(npc.Name) || (bool)!npc.datable)
                    continue;

                Friendship fs = Game1.player.friendshipData[npc.Name];
                int hearts = fs.Points / 250;
                if (hearts < 2)
                    continue;
                if (hearts > 9)
                    hearts = 9;
                int chance = ModRandom.Next(hearts, 11);

                if (chance == 9 || Game1.player.spouse == npc.Name)
                {
                    chosenLoveLetterGifters.Add(npc);
                    Debug.WriteLine(npc.Name);
                    string letterDialogue = getRandomLetterDialogue(); //39 3
                    if (letterDialogue == I18n.NpcGiftingLetter_AskForDate() && !isGoingOnDate)
                    {
                        isGoingOnDate = true;
                        datePartner = npc;
                        Debug.WriteLine($"Going on Date with {npc.Name}");
                        commands += $"/warp {npc.Name} 39 38/move {npc.Name} 0 -11 0/pause 500/speak {npc.Name} \"{letterDialogue}\"/showLoveLetter {npc.Name}_LoveFestival17819Letter/askForDate {npc.Name}/move {npc.Name} 0 11 0/warp {npc.Name} -1000 -1000";
                        continue;
                    }
                    commands += $"/warp {npc.Name} 39 38/move {npc.Name} 0 -11 0/pause 500/speak {npc.Name} \"{letterDialogue}\"/showLoveLetter {npc.Name}_LoveFestival17819Letter/move {npc.Name} 0 11 0/warp {npc.Name} -1000 -1000";
                }

            }
            if (commands == "")
            {
                commands = $"/pause 3000/faceDirection Marnie 1/pause 500/speak Marnie \"{I18n.MarnieReaction_NoLoveLetters()}\"/pause 500/emote farmer 28/pause 500";
            }
            return commands;
        }
        private void OnUpdateTicking(object?sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is DialogueBox box && box.getCurrentString() == dialogueToBeReplaced && Game1.isFestival() && isValentinesFestival 
                || Game1.activeClickableMenu is DialogueBox box2 && GetSpouseDialogues().Contains(box2.getCurrentString()) && seenSpouseDialogue && isValentinesFestival) //TODO check if box.getCurrentString is equal to spouse dialogue and if player has seen spouse dialogue / code function for getting spouse dialogue using i18n
            {
                box = Game1.activeClickableMenu as DialogueBox;
                Game1.activeClickableMenu = null;
                if (letterSent)
                    return;

                NPC npc = box.characterDialogue.speaker;
                List <Response> responses = new()
                {
                    new Response("loveLetter", I18n.SendLetter_Yes(name: npc.Name)),
                    new Response("no", I18n.SendLetter_No())
                };
                Game1.currentLocation.createQuestionDialogue(I18n.SendLetter_Message(), responses.ToArray(), new GameLocation.afterQuestionBehavior((Farmer who, string dialogueID) =>
                {
                    if (dialogueID != "loveLetter")
                        return;

                    letterSent = true;
                    int start2 = ModRandom.Next(0, receiveLetterReactions.Count);
                    string text = receiveLetterReactions[start2];
                    npc.CurrentDialogue.Push(new Dialogue(text, npc));
                    Game1.drawDialogue(npc);
                    Friendship fs = Game1.player.friendshipData[npc.Name];
                    fs.Points += 250;
                    //npc.receiveGift(new StardewValley.Object(Vector2.Zero, 272, 1), Game1.player, false, 5, false); // TODO PATCH
                    npc.doEmote(20);
                    npc.faceTowardFarmerForPeriod(5000, 200, false, Game1.player);
                    Game1.currentLocation.localSound("give_gift");
                    npc.CurrentDialogue.Clear();
                    if (Game1.player.isMarried() && Game1.player.spouse != npc.Name)
                    {
                        loveLetterNotGivenToSpouse = true;
                    }

                }));
            }
        }
        private List<NPC> getAllNPCs()
        {
            List<NPC> npcList = new();
            Utility.ForAllLocations((GameLocation location) =>
            {
                foreach (NPC npc in location.getCharacters())
                    npcList.Add(npc);
            });
            return npcList;
        }
        private void OnAssetRequest(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/winter13"))
            {
                e.LoadFrom(() =>
                {
                    var data = this.FestivalData();
                    return data;

                }, AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/Town-LoveFestival"))
            {
                e.LoadFromModFile<Map>("assets/Town-LoveFestival.tbin", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/LoveFestivalDateSky"))
                e.LoadFromModFile<Map>("assets/LoveFestivalDateSky.tbin", AssetLoadPriority.Exclusive);
            else if (dateLetter != null && e.NameWithoutLocale.IsEquivalentTo($"Data/{dateLetter.CachePath}"))
            {
                e.Edit((IAssetData asset) =>
                {
                    Debug.WriteLine("Adding Script...");
                    var data = asset.AsDictionary<string, string>().Data;
                    foreach (var item in dateLetter.GenerateDateEventScript())
                        data.Add(item);
                    dateLetter = null;
                });
            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/FestivalDates"))
            {

                e.Edit(static (asset) =>
                {
                    asset.AsDictionary<string, string>().Data.Add("winter13", I18n.Festival_Name());
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Beach"))
            {
                e.Edit((IAssetData asset) =>
                {
                    DateLetter testLetter = new DateLetter("a", "Beach");
                    testLetter.DatePartner = Game1.getCharacterFromName("Haley");
                    var data = asset.AsDictionary<string, string>().Data;
                    foreach (var item in testLetter.GenerateDateEventScript())
                        data.Add(item);
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
            {
                e.Edit((IAssetData asset) =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data["VEInvitationLetterWeek"] = I18n.Letter_WeekBefore();
                    data["VEInvitationLetterTomorrow"] = I18n.Letter_NextDay();
                });
            }
        }
        private IDictionary<string, string> FestivalData()
        {
            var data = new Dictionary<string, string>
            {
                ["name"] = I18n.Festival_Name(),
                ["conditions"] = "Town/900 1400",
                ["set-up"] = "musicboxsong/-1000 -1000/farmer 1 54 2/changeToTemporaryMap Town-LoveFestival/loadActors Set-Up/animate Robin false true 500 20 21 20 22/animate Demetrius false true 500 24 25 24 26/playerControl LoveFestival17819",
                ["mainEvent"] = $"globalFade/viewport -1000 -1000/warp farmer 39 26/faceDirection farmer 2/warp Marnie 38 26/faceDirection Marnie 2/warp Lewis 40 26/faceDirection Lewis 2/viewport 39 26/pause 1500/speak Marnie \"{I18n.MarnieReaction_Start()}\"LoveFestival17819command/waitForOtherPlayers festivalEnd/end",
            };
            foreach(var translation in Helper.Translation.GetTranslations())
            {
                const string prefix = "Dialogue.";
                if (translation.Key.StartsWith(prefix))
                {
                    string splittedKey = translation.Key.Substring(prefix.Length);
                    data[splittedKey] = translation.ToString();
                }
            }

            return data;
        }
    }
}