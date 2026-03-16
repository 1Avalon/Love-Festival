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
using StardewValley.Delegates;
using ContentPatcher;
using LoveFestival.UI;
using System.Linq;
using System.Reflection;
using StardewValley.Locations;
using StardewValley.BellsAndWhistles;

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

        public static List<NPC> npcs;

        public static ModConfig Config;

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

        public static readonly Season festivalSeason = Season.Winter; //TODO instead of checking for winter, check for festivalSeason

        public static FriendshipMultiplier multiplier;

        public static NetworkDataManager networkDataManager;

        private static bool hasDateContentPacks = false;

        private bool hasSeenDate = false;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            instance = this;
            modHelper = helper;
            bgLoveLetter = Helper.ModContent.Load<Texture2D>("assets/love_letter_bg");
            redRoseDebris = Helper.ModContent.Load<Texture2D>("assets/red_rose_debris");
            greenRoseDebris = Helper.ModContent.Load<Texture2D>("assets/green_rose_debris");

            helper.Events.Content.AssetRequested += this.OnAssetRequest;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            helper.Events.Player.Warped += this.OnWarped;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.Saved += this.OnGameSaved;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Multiplayer.ModMessageReceived += this.OnMessageReceived;
            helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;

            //helper.ConsoleCommands.Add("test_date", "Tests a Love Festival date.\n\nUsage: test_date <date_id>\n- date_id: the unique id of the date.\nThe NPC entered in the config will function as temporary actor. Feel free to change it. It may not work immediately after changing it in the config. Wait a few seconds until CP reloaded the token.", this.TestDate);
            helper.ConsoleCommands.Add("try_continue_event", "Goes to the next Event command. Try in case you get stuck during the event", this.TryContinueEvent);
            helper.ConsoleCommands.Add("test_multiplier", "Initialises a friendship multiplier for the corresponding NPC.\nThis was implemented for testing", this.test_multiplier);

            spouseDialouges = GetSpouseDialogues();

            ModRandom = new Random();

            I18n.Init(helper.Translation);

            Harmony harmony = new(this.ModManifest.UniqueID);
            //harmony.PatchAll();
            harmony.Patch(
                original: AccessTools.Method(typeof(LetterViewerMenu), nameof(LetterViewerMenu.draw), new Type[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Prefix_CustomLetterBackgroundPatch))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawString)),
                prefix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Prefix_drawString))
                );

            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.setNewDialogue), new Type[] { typeof(string), typeof(bool), typeof(bool) }),
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
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.IsDebrisWeatherHere)),
                postfix: new HarmonyMethod(typeof(LoveFestivalPatches), nameof(LoveFestivalPatches.Postfix_IsDebrisWeatherHere))
                );
            // Patching manually provides mobile compatibility ?


        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (Game1.CurrentEvent is null)
                return;

            if (!Game1.CurrentEvent.isSpecificFestival($"winter{festivalDate}"))
                return;
            
            if (e.OldMenu is DialogueBox)
            {
                foreach (NPC npc in Game1.CurrentEvent.actors)
                {
                    if (!letterSent)
                    {
                        if (!npc.isMarriedOrEngaged() || Game1.player.spouse == npc.Name)
                        {
                            if (npc.CurrentDialogue.Count > 0 && npc.CurrentDialogue.Peek().getCurrentDialogue().Equals(ModEntry.dialogueToBeReplaced))
                            {
                                npc.CurrentDialogue.Clear();
                            }
                            if (npc.CurrentDialogue.Count == 0)
                            {
                                if (npc.Name == Game1.player.spouse)
                                    seenSpouseDialogue = true;

                                npc.CurrentDialogue.Push(new Dialogue(npc, "0", ModEntry.dialogueToBeReplaced));
                            }
                        }
                    }
                }
            }
            
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Event.RegisterCommand("showLoveLetter", (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), typeof(ModEntry).GetMethod(nameof(showLoveLetter_command))));
            Event.RegisterCommand("askForDate", (EventCommandDelegate)Delegate.CreateDelegate(typeof(EventCommandDelegate), typeof(ModEntry).GetMethod(nameof(LoveFestival_AskForDate_command))));
            var api = this.Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            api.RegisterToken(this.ModManifest, "DatePartnerName", () =>
            {
                if (datePartner != null)
                    return new[] { datePartner.Name };

                if (dateLetter?.NpcName != null)
                    return new[] { dateLetter.NpcName };

                if (Config.TestNpcDateName != null || Config.TestNpcDateName != "")
                    return new[] { Config.TestNpcDateName };

                return null;
            });

            if (configMenu is null)
                return;
            
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(Config)
            );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: () => I18n.Config_TestDateActorName(),
                tooltip: () => I18n.Config_TestDateActorNameDescription(),
                getValue: () => Config.TestNpcDateName,
                setValue: value => Config.TestNpcDateName = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_SpouseAlwaysGivingLetter(),
                getValue: () => Config.SpouseAlwaysGivingLetter,
                setValue: value => Config.SpouseAlwaysGivingLetter = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18n.Config_MoneyLetterPulsating(),
                tooltip: () => I18n.Config_MoneyLetterPulsatingDescription(),
                getValue: () => Config.ChangeMoneyTextColorInLetter,
                setValue: value => Config.ChangeMoneyTextColorInLetter = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => I18n.Config_MinHeartsRequired(),
                getValue: () => Config.MinRequiredHearts,
                setValue: value => Config.MinRequiredHearts = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => I18n.Config_ChancePerHeart(),
                tooltip: () => I18n.Config_ChancePerHeartDescription(),
                getValue: () => Config.ChancePerHeart,
                setValue: value => Config.ChancePerHeart = value
            );
        }
        
        private void OnMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != this.ModManifest.UniqueID)
                return;

            if (!Context.IsMainPlayer)
            {
                if (e.Type == "MultiplierData")
                {
                    Logger.Log_Trace("Received Multiplier Data");
                    multiplier = e.ReadAs<FriendshipMultiplier>();
                }
                else if (e.Type == "DateLetterData")
                {
                    Logger.Log_Trace("Received Date Letter Data");
                    dateLetter = e.ReadAs<DateLetter>();
                }

                return;

            }
            networkDataManager?.ReceiveMessage(e);
            if (networkDataManager == null)
            {
                Monitor.Log("Love Festival tried to manage network data but the handler was not initialised", LogLevel.Warn);
            }//Additionally, add or update the date from the other players if host
        }

        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            networkDataManager.SendDataToFarmhand(e.Peer.PlayerID);
        }
        public static void showLoveLetter_command(Event instance, string[] split, EventContext context)
        {
            IClickableMenu acm = Game1.activeClickableMenu;
            if (acm is not LetterViewerMenu && acm is not ItemGrabMenu || acm is DialogueBox)
            {
                ModLetter letter;
                string authorName = split[1];
                if (datePartner != null && datePartner.Name == authorName && date == null)
                {
                    dateLetter = DateLetter.getRandomDateLetter();
                    letter = dateLetter;
                }
                else
                {
                    letter = LoveLetter.GetRandomLetter();
                }
                string translatedAuthorName = Game1.getCharacterFromName(authorName).displayName;
                string msg = letter.ContentWithAttachment(translatedAuthorName);

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
                Game1.currentLocation.createQuestionDialogue(I18n.LoveFestivalDates_Question(npc: npc.displayName), choices.ToArray(),
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
        private void TryContinueEvent(string command, string[] args)
        {
            Game1.CurrentEvent.currentCommand++;
        }

        private void test_multiplier(string command, string[] args)
        {
            multiplier = new FriendshipMultiplier(args[0]);
            Logger.Log_Info($"Initialised Friendship multiplier for {args[0]}");
        }
        private void TestDate(string command, string[] args)
        {
            string dateId = args[0];

            NPC oldDatePartner = datePartner;
            ModDate oldDate = date;
            datePartner = Game1.getCharacterFromName(Config.TestNpcDateName);
            if (datePartner == null)
            {
                datePartner = oldDatePartner;
                Monitor.Log("NPC NOT FOUND", LogLevel.Error);
                return;
            }
            else if (datePartner.Name == "Vincent" || datePartner.Name == "Jas") //TODO figure out how to check if NPC is child or not
            {
                datePartner = oldDatePartner;
                Monitor.Log("NPC not supported");
                return;
            }
            //Game1.warpFarmer("FarmHouse", 1, 1, false);//warp to random location so CP token will load
            Dictionary<string, ModDate> modDates = Helper.GameContent.Load<Dictionary<string, ModDate>>(modDateEntryKey);

            if (!modDates.ContainsKey(dateId))
            {
                Monitor.Log("Date not found", LogLevel.Error);
                return;
            }

            date = modDates[dateId];
            Helper.GameContent.InvalidateCache($"Data\\Events\\{date.Location}");

            Helper.GameContent.Load<Dictionary<string, string>>($"Data\\Events\\{date.Location}");

            bool x = Game1.PlayEvent(dateId, false, false);
            Logger.Log_Info(x == true ? "Sucessfully loaded date" : "Date was found by Love Festival but not by Stardew Valley");
            datePartner = oldDatePartner;
            date = oldDate;


        }
        private void OnGameSaved(object sender, SavedEventArgs e)
        {
            if (hasSeenDate && Context.IsMainPlayer)
            {
                Helper.Data.WriteSaveData<DateLetter>("DateLetter", null);
                Logger.Log_Trace("Cleared data Data...");
                hasSeenDate = false;
            }
            else if (dateLetter != null && Context.IsMainPlayer)
            {
                Helper.Data.WriteSaveData("DateLetter", dateLetter);
                Helper.Data.ReadSaveData<DateLetter>("DateLetter");
                Logger.Log_Trace("Saved Date Data");
            }
            if (multiplier != null && Game1.Date.DayOfMonth < multiplier.expiresAt && !Context.IsMainPlayer) //More consistent
            {
                Helper.Data.WriteSaveData("Multiplier", multiplier);
                Logger.Log_Trace("Saved Multiplier Data");
            }
            if (Context.IsMultiplayer) 
            {
                networkDataManager.SendDataToHost();
                networkDataManager?.Save();
            }
        }
        private void OnWarped(object? sender, WarpedEventArgs e)
        {

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
            else if (e.NewLocation.Name == "Temp" && Game1.Date.Season == Season.Winter && Game1.Date.DayOfMonth == festivalDate && Game1.timeOfDay <= 1400)
            {
                Logger.Log_Info("In case the event does not progress, try running the command 'try_continue_event'");
            }
        }

        //CREDITS: Stole that method from KhloeLeclair because I had no idea how to convert IModInfo into IContentPack
        internal IContentPack? GetContentPackFor(IModInfo mod)
        {
            IContentPack cp;

            if (mod.IsContentPack && mod.GetType().GetProperty("ContentPack", BindingFlags.Instance | BindingFlags.Public)?.GetValue(mod) is IContentPack pack)
                return pack;

            else if (mod.GetType().GetProperty("DirectoryPath", BindingFlags.Instance | BindingFlags.Public)?.GetValue(mod) is string str)
            {
                cp = Helper.ContentPacks.CreateTemporary(
                    directoryPath: str,
                    id: $"leclair.theme-loader.${mod.Manifest.UniqueID}",
                    name: mod.Manifest.Name,
                    description: mod.Manifest.Description,
                    author: mod.Manifest.Author,
                    version: mod.Manifest.Version
                );

            }
            else
                return null;

            return cp;
        }


        private void OnSaveLoaded(object? sender, EventArgs e)
        {
            /*
            var hasDateNight = Helper.ModRegistry.GetAll()
                .Where(x => x.IsContentPack && x.Manifest.UniqueID.Equals("agentlyoko.datenightredux"))
                .Select(x => GetContentPackFor(x));
            IContentPack dateNightRedux = hasDateNight.First();

            if (dateNightRedux.HasFile("data/TestDates.json"))
            {
                Monitor.Log("Found File");

                object data = dateNightRedux.ReadJsonFile<object>("data/TestDates.json");
                Debug.WriteLine(data.ToString());
            }
            */

            npcs = getAllNPCs();
            isValentinesFestival = false;
            date = null;
            Game1.player.eventsSeen.Remove("17819");

            if (Context.IsMainPlayer)
            {
                multiplier = Helper.Data.ReadSaveData<FriendshipMultiplier>("Multiplier");
                dateLetter = Helper.Data.ReadSaveData<DateLetter>("DateLetter");

                if (Context.IsMultiplayer)
                {
                    networkDataManager = Helper.Data.ReadSaveData<NetworkDataManager>("NetworkDataManager");
                    if (networkDataManager == null)
                    {
                        networkDataManager = new NetworkDataManager();
                    }
                    networkDataManager.TryLoadingDataForHost();
                }

            }
            else
            {
                networkDataManager = new(); //For the farmhands
            }

            if (multiplier != null)
            {
                Logger.Log_Trace($"Successfully loaded friendship multiplier for {multiplier.targetName}");
            }
            if (dateLetter != null)
            {
                Logger.Log_Trace($"Successfully loaded Date {dateLetter.DateId}. Taking place on day {dateLetter.day}");
            }
            Dictionary<string, DateLetter> modLetters = ModEntry.modHelper.GameContent.Load<Dictionary<string, DateLetter>>(ModEntry.modDateLetterEntryKey);
            if (modLetters.Count == 0 && !hasDateContentPacks)
            {
                Logger.Log_Info("No Date Packs for Love Festival found. Ignoring date mechanic...");
            }
            else if (!hasDateContentPacks)
            {
                Logger.Log_Info($"Found {modLetters.Count} dates for Love Festival");
                hasDateContentPacks = true;
            }

            if (hasDateContentPacks)
            {
                foreach (var item in modLetters)
                {
                    Game1.player.eventsSeen.Remove(item.Value.DateId);
                }
                Logger.Log_Info("Removed all dates from player.eventsSeen");
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

            if (date.DayOfMonth == 27 && date.Season == Season.Fall)
            {
                Game1.player.mailbox.Add("VEInvitationLetterWeek");
            }

            else if (date.DayOfMonth == festivalDate - 1 && date.Season == Season.Winter)
            {
                Game1.player.mailbox.Add("VEInvitationLetterTomorrow");
                //Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = 2; //remove and check if it changes anything
            }
            else if (multiplier != null && date.DayOfMonth == multiplier.expiresAt && date.Season == Season.Winter)
            {
                Logger.Log_Trace("Friendship multiplier expired");
                if (Context.IsMainPlayer)
                    Helper.Data.WriteSaveData<FriendshipMultiplier>("Multiplier", null);
                multiplier = null;
            }
        }
        public static string getRandomLetterDialogue()
        {

            if (!hasDateContentPacks && reactions.Contains(I18n.NpcGiftingLetter_AskForDate()))
            {
                reactions.Remove(I18n.NpcGiftingLetter_AskForDate());
            }

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
            var shuffledNpcs = npcs.OrderBy(item => ModRandom.Next());
            foreach (NPC npc in shuffledNpcs)
            {
                if (!Game1.player.friendshipData.ContainsKey(npc.Name) || npc.isMarriedOrEngaged() || !npc.CanSocialize || npc.Name == "Lewis" || npc.Name == "Marnie")
                    continue;
                Friendship fs = Game1.player.friendshipData[npc.Name];
                int hearts = fs.Points / 250;
                if (hearts < Config.MinRequiredHearts)
                    continue;
                if (hearts > 10)
                    hearts = 10;

                bool getsLetter = false;
                int chance = ModRandom.Next(1, 101);
                getsLetter = chance < hearts * Config.ChancePerHeart;

                if (getsLetter || (Game1.player.spouse == npc.Name && Config.SpouseAlwaysGivingLetter))
                {
                    chosenLoveLetterGifters.Add(npc);
                    string letterDialogue = getRandomLetterDialogue();

                    commands += $"/warp {npc.Name} 39 34/move {npc.Name} 0 -11 0 false/pause 500/speak {npc.Name} \"{letterDialogue}\"/showLoveLetter {npc.Name}/move {npc.Name} -1 0 3/move {npc.Name} 0 13 0 true";
                }

            }

            if (commands == "")
            {
                commands = $"/pause 3000/faceDirection Marnie 1/pause 500/speak Marnie \"{I18n.MarnieReaction_NoLoveLetters()}\"/pause 500/emote farmer 28/pause 500";
            }
            else if (commands.EndsWith("true"))
            {
                string substring = commands.Substring(0, commands.Length - 5);
                commands = substring + " false";
            }
            return commands;
        }
        private void OnUpdateTicking(object? sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is DialogueBox box && box.getCurrentString() == dialogueToBeReplaced && Game1.isFestival() && isValentinesFestival
                || Game1.activeClickableMenu is DialogueBox box2 && GetSpouseDialogues().Contains(box2.getCurrentString()) && seenSpouseDialogue && isValentinesFestival) //TODO check if box.getCurrentString is equal to spouse dialogue and if player has seen spouse dialogue / code function for getting spouse dialogue using i18n
            {
                box = Game1.activeClickableMenu as DialogueBox;
                Game1.activeClickableMenu = null;
                if (letterSent)
                    return;

                NPC npc = box.characterDialogue.speaker;
                List<Response> responses = new()
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
            List<string> npcNames = new(); 
            foreach (NPC npc in Utility.getAllVillagers())  //For whatever reason, this function sometimes returns 4 different instances of an npc so we have to filter by name
            {
                if (!npcNames.Contains(npc.Name))
                {
                    npcList.Add(npc);
                    npcNames.Add(npc.Name);
                }
            }
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
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/LoveFestivalMap"))
            {
                e.LoadFromModFile<Map>("assets/FestivalMap.tbin", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/BeachDateOceanSky"))
            {
                e.LoadFromModFile<Map>("assets/BeachDateOceanSky.tbin", AssetLoadPriority.Exclusive);
            }
            else if (date != null && e.NameWithoutLocale.IsEquivalentTo($"Data/Events/{date.Location}"))
            {
                e.Edit((IAssetData asset) =>
                {
                    Debug.WriteLine("Adding Script..." + date.EventScript.Values.ToString());
                    var data = asset.AsDictionary<string, string>().Data;
                    foreach (var item in date.EventScript)
                    {
                        if (data.Keys.Contains(item.Key))
                        {
                            data[item.Key] = item.Value;
                            continue;
                        }
                        data.Add(item);
                    }
                });
            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/FestivalDates"))
            {

                e.Edit(static (asset) =>
                {
                    asset.AsDictionary<string, string>().Data.Add($"winter{festivalDate}", I18n.Festival_Name());
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
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
                ["set-up"] = "musicboxsong/-1000 -1000/farmer 1 54 2/changeToTemporaryMap LoveFestivalMap/loadActors Set-Up/animate Robin false true 500 20 21 20 22/animate Demetrius false true 500 24 25 24 26/playerControl LoveFestival17819",
                ["mainEvent"] = $"globalFade/viewport -1000 -1000/warp farmer 39 22/faceDirection farmer 2/warp Marnie 38 22/faceDirection Marnie 2/warp Lewis 40 22/faceDirection Lewis 2/viewport 39 22/pause 1500/speak Marnie \"{I18n.MarnieReaction_Start()}\"LoveFestival17819command/waitForOtherPlayers festivalEnd/end",
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