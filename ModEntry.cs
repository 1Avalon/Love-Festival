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
using Microsoft.VisualBasic;
using System.Xml.Linq;
using System.Linq;
using StardewValley.Delegates;
using ContentPatcher;

namespace LoveFestival
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public static Mod instance;

        public static IModHelper modHelper;

        public static readonly string modDateEntryKey = "AvalonMFX.LoveFestival/Dates";

        public static readonly string modDateLetterEntryKey = "AvalonMFX.LoveFestival/DateLetters";

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

        public static ModDate date;

        public static bool ExecuteDateQuestion = false;

        public static readonly int festivalDate = 6;

        public static FriendshipMultiplier multiplier;

        private bool hasDateContentPacks = false;

        public static string mainEventScript;

        private bool hasSeenDate = false;

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
            helper.Events.GameLoop.Saved += this.OnGameSaved;
            spouseDialouges = GetSpouseDialogues();

            ModRandom = new Random();

            I18n.Init(helper.Translation);

            Harmony harmony = new(this.ModManifest.UniqueID);
            //harmony.PatchAll();
            harmony.Patch(
                original: AccessTools.Method(typeof(LetterViewerMenu), nameof(LetterViewerMenu.draw), new Type[] {typeof(SpriteBatch)}),
                prefix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Prefix_CustomLetterBackgroundPatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.setNewDialogue), new Type[] {typeof(string), typeof(bool), typeof(bool)}),
                prefix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Prefix_setNewDialogue))
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
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.changeFriendship)),
                prefix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Prefix_changeFriendship))
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
            // Patching manually provides mobile compatibility ?


        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Event.RegisterCommand("showLoveLetter", (EventCommandDelegate) Delegate.CreateDelegate(typeof(EventCommandDelegate), typeof(ModEntry).GetMethod(nameof(showLoveLetter_command))));
            Event.RegisterCommand("askForDate", (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), typeof(ModEntry).GetMethod(nameof(LoveFestival_AskForDate_command))));
            Monitor.Log("Game crashed due to Love Festival? Make sure to update to the newest version if you haven't done it yet. There are new bug fixes every update!", LogLevel.Warn);
            var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            api.RegisterToken(this.ModManifest, "DatePartnerName", () =>
            {
                Logger.Log_Info("Loading Date Partner Token");
                if (datePartner != null)
                    return new[] { datePartner.Name };

                if (dateLetter?.NpcName != null)
                    return new[] { dateLetter.NpcName };



                return null;
            });
        }
        public static void showLoveLetter_command(Event instance, string[] split, EventContext context)
        {
            IClickableMenu acm = Game1.activeClickableMenu;
            if (acm is not LetterViewerMenu && acm is not ItemGrabMenu|| acm is DialogueBox) 
            {
                ModLetter letter;
                string authorName = split[1].Split("_")[0];
                if (datePartner != null && datePartner.Name == authorName && date == null)
                {
                    dateLetter = DateLetter.getRandomDateLetter();
                    letter = dateLetter;
                    Logger.Log_Info(letter.Content);
                }
                else
                {
                    letter = LoveLetter.GetRandomLetter();
                }
                string msg = letter.ContentWithAttachment(authorName);

                LetterViewerMenu menu = new LetterViewerMenu(msg, split[1]);
                menu.exitFunction = () =>
                {
                        instance.currentCommand++;
                };
                Game1.activeClickableMenu = menu;
            }
        }
        public static void LoveFestival_AskForDate_command(Event instance, string[] split, EventContext context)
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
                            Debug.WriteLine($"Having date on day {wrapper.day}");
                            dateLetter.LoadDaysUntillDateToken();
                            dateLetter.NpcName = npc.Name;
                            Dialogue dialogue = new(npc, null, dateLetter.AcceptDateResponse);
                            npc.CurrentDialogue.Push(dialogue);
                            Game1.drawDialogue(npc);
                        };
                        Game1.activeClickableMenu = wrapper;

                        //modHelper.GameContent.InvalidateCache(dateLetter.CachePath);
                    }

                    else
                    {
                        Dialogue dialogue = new Dialogue(npc, null, I18n.LoveFestivalDates_DateRejected());
                        npc.CurrentDialogue.Push(dialogue);
                        Game1.drawDialogue(npc);
                    }
                        
                        //Game1.DrawDialogue(npc, null, I18n.LoveFestivalDates_DateRejected());
                }));
            }
        }

        private void OnGameSaved(object sender, SavedEventArgs e)
        {
            if (hasSeenDate)
            {
                Helper.Data.WriteSaveData<DateLetter>("DateLetter", null);
                Logger.Log_Trace("Cleared data Data...");
                hasSeenDate = false;
            }
            else if (dateLetter != null)
            {
                Helper.Data.WriteSaveData("DateLetter", dateLetter);
                DateLetter test = Helper.Data.ReadSaveData<DateLetter>("DateLetter");
                Logger.Log_Info(test?.Content);
                Logger.Log_Trace("Saved Date Data");
            }
            if (multiplier != null && Game1.Date.DayOfMonth < multiplier.expiresAt) //More consistent
            {
                Helper.Data.WriteSaveData("Multiplier", multiplier);
                Logger.Log_Trace("Saved Multiplier Data");
            }
        }
        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            Logger.Log_Info(e.NewLocation.Name);

            if (e.OldLocation.Name == "Temp" && Game1.Date.Season == Season.Winter && Game1.Date.DayOfMonth == festivalDate || e.OldLocation.Name == "Temp" && isValentinesFestival)
            {
                ExecuteDateQuestion = false;
                chosenLoveLetterGifters.Clear();
                Game1.debrisWeather.Clear();
                isValentinesFestival = false;
                isGoingOnDate = false;
                
                if (loveLetterNotGivenToSpouse)
                {
                    NPC spouse = Game1.getCharacterFromName(Game1.player.spouse);
                    spouse.CurrentDialogue.Push(new Dialogue(spouse, null, I18n.JealousyDialogue_Spouse()));
                    seenSpouseDialogue = false;
                    //spouse.setNewDialogue("I truly appreciate how you gave your didn't give me your love letter...$s");
                }
            }
            else if (dateLetter != null && Game1.Date.DayOfMonth == dateLetter.day)
            {
                date = ModDate.GetDateFromLetter(dateLetter);
                modHelper.GameContent.InvalidateCache($"Data\\Events\\{date.Location}");
                dateLetter = null;
                hasSeenDate = true;
                //Helper.Data.WriteSaveData<DateLetter>("DateLetter", null); //instead add a bool and run this when saving otherwise the player cant see the event again if he doesnt save
            }
            else if(e.NewLocation.Name == "Temp" && Game1.Date.Season == Season.Winter && Game1.Date.DayOfMonth == festivalDate && Game1.timeOfDay <= 1400)
            {
                Logger.Log_Trace("Attended Love Festival. Getting Main Event script...");
                mainEventScript = getMainEvent();
            }
        }

        private void OnSaveLoaded(object? sender, EventArgs e)
        {
            npcs = getAllNPCs();
            isValentinesFestival = false;
            date = null;
            Game1.player.eventsSeen.Remove("17819");

            multiplier = Helper.Data.ReadSaveData<FriendshipMultiplier>("Multiplier");
            dateLetter = Helper.Data.ReadSaveData<DateLetter>("DateLetter");
            if (multiplier != null)
            {
                Logger.Log_Trace($"Successfully loaded friendship multiplier for {multiplier.targetName}");
            }
            if (dateLetter != null)
            {
                Logger.Log_Trace($"Successfully loaded Date {dateLetter.DateId}. Taking place on day {dateLetter.day}");
            }

            foreach (var translation in Helper.Translation.GetTranslations())
            {
                const string receiveLetterPrefix = "ReceiveLetterReaction.";
                const string npcGiftingPrefix = "npcGiftingLetter.";

                if (translation.Key.StartsWith(receiveLetterPrefix))
                    receiveLetterReactions.Add(translation.ToString());
                else if (translation.Key.StartsWith(npcGiftingPrefix))
                    reactions.Add(translation.ToString());
            }
        }
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        { 
            WorldDate date = Game1.Date;

            if (date.DayOfMonth == 6 && date.Season == Season.Winter)
            {
                Game1.player.mailbox.Add("VEInvitationLetterWeek");
            }

            else if (date.DayOfMonth == 12 && date.Season == Season.Winter)
            {
                Game1.player.mailbox.Add("VEInvitationLetterTomorrow");
                //Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = 2; //remove and check if it changes anything
            }
            else if (multiplier != null && date.DayOfMonth == multiplier.expiresAt && date.Season == Season.Winter)
            {
                Logger.Log_Trace("Friendship multiplier expired");
                Helper.Data.WriteSaveData<FriendshipMultiplier>("Multiplier", null);
                multiplier = null;
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
                    string letterDialogue;
                    do
                    {
                        letterDialogue = getRandomLetterDialogue();    //39 3
                    } while (letterDialogue == I18n.NpcGiftingLetter_AskForDate() && isGoingOnDate);

                    if (letterDialogue == I18n.NpcGiftingLetter_AskForDate() && !isGoingOnDate)
                    {
                        isGoingOnDate = true;
                        datePartner = npc;
                        Logger.Log_Info(npc.Name);
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
                    npc.CurrentDialogue.Push(new Dialogue(npc, null, text));
                    Game1.drawDialogue(npc);
                    Friendship fs = Game1.player.friendshipData[npc.Name];
                    fs.Points += 250;
                    //npc.receiveGift(new StardewValley.Object(Vector2.Zero, 272, 1), Game1.player, false, 5, false); // TODO PATCH
                    npc.doEmote(20);
                    npc.faceTowardFarmerForPeriod(5000, 200, false, Game1.player);
                    Game1.currentLocation.localSound("give_gift");
                    npc.CurrentDialogue.Clear();
                    if (Game1.player.isMarriedOrRoommates() && Game1.player.spouse != npc.Name)
                    {
                        loveLetterNotGivenToSpouse = true;
                    }

                }));
            }
        }
        private List<NPC> getAllNPCs()
        {
            List<NPC> npcList = new();
            Utility.ForEachVillager((NPC npc) =>
            {
                npcList.Add(npc);
                return true; //TODO figure out what are the consequences of returning true
            });
            return npcList;
        }
        private void OnAssetRequest(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"Data/Festivals/winter{festivalDate}"))
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
            else if (date != null && e.NameWithoutLocale.IsEquivalentTo($"Data/Events/{date.Location}"))
            {
                e.Edit((IAssetData asset) =>
                {
                    Debug.WriteLine("Adding Script...");
                    var data = asset.AsDictionary<string, string>().Data;
                    foreach (var item in date.EventScript)
                        data.Add(item);
                    date = null;
                });
            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/FestivalDates"))
            {

                e.Edit(static (asset) =>
                {
                    asset.AsDictionary<string, string>().Data.Add($"winter{festivalDate}", I18n.Festival_Name());
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Events/Beach"))
            {
                e.Edit((IAssetData asset) =>
                {

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
            else if (e.NameWithoutLocale.IsEquivalentTo(modDateEntryKey))
            {
                e.LoadFrom(() => new Dictionary<string, ModDate>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(modDateLetterEntryKey))
            {
                e.LoadFrom(() => new Dictionary<string, DateLetter>(), AssetLoadPriority.Exclusive);
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
            foreach (var translation in Helper.Translation.GetTranslations())
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