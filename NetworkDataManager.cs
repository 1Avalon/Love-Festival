using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class NetworkDataManager
    {

        public Dictionary<long, FriendshipMultiplier> multiplierData;

        public Dictionary<long, DateLetter> dateLetterData;
        public NetworkDataManager()
        {
            if (!Context.IsMultiplayer)
            {
                return;
            }
            multiplierData = new Dictionary<long, FriendshipMultiplier>();
            dateLetterData = new Dictionary<long, DateLetter>();
            Logger.Log_Info("Multiplayer active. Initialising NetworkDataManager...");
        }

        public void SendDataToHost()
        {
            if (Context.IsMainPlayer) //you can send data to yourself lol
            {
                long localhost = Game1.player.UniqueMultiplayerID;
                if (ModEntry.multiplier !=  null)
                    multiplierData[localhost] = ModEntry.multiplier;
                if (ModEntry.dateLetter != null)
                    dateLetterData[localhost] = ModEntry.dateLetter;

                return;
            }

            long hostId = 0;
            foreach(IMultiplayerPeer peer in ModEntry.modHelper.Multiplayer.GetConnectedPlayers())
            {
                if (peer.IsHost)
                {
                    hostId = peer.PlayerID;
                    break;
                }
            }

            DateLetter dateLetter = ModEntry.dateLetter;
            FriendshipMultiplier multiplier = ModEntry.multiplier;


            if (dateLetter != null)
            {
                ModEntry.modHelper.Multiplayer.SendMessage<DateLetter>(dateLetter, "DateLetterData", modIDs: new[] { ModEntry.instance.ModManifest.UniqueID }, new long[] { hostId });
                Logger.Log_Trace("Sent Date Letter Data to the host. They will store the data");
            }
            else
            {
                dateLetter = new (); //Create a new pointless letter because sending null will raise an error (alternatively let host catch it)
                ModEntry.modHelper.Multiplayer.SendMessage<DateLetter>(dateLetter, "ClearDateLetterData", modIDs: new[] { ModEntry.instance.ModManifest.UniqueID }, new long[] { hostId });
            }

            if (multiplier != null)
            {
                ModEntry.modHelper.Multiplayer.SendMessage<FriendshipMultiplier>(multiplier, "MultiplierData", modIDs: new[] { ModEntry.instance.ModManifest.UniqueID }, new long[] { hostId });
                Logger.Log_Trace("Sent Multiplier Data to the host. They will store the data");
            }
            else
            {
                multiplier = new("None"); //Create a new pointless letter because sending null will raise an error (alternatively let host catch it)
                ModEntry.modHelper.Multiplayer.SendMessage<FriendshipMultiplier>(multiplier, "ClearMultiplierData", modIDs: new[] { ModEntry.instance.ModManifest.UniqueID }, new long[] { hostId });
            }

        }

        public void SendDataToFarmhand(long playerId)
        {
            Logger.Log_Trace($"Sending data to farmhand {playerId}");
            if (dateLetterData.ContainsKey(playerId))
            {
                Logger.Log_Trace($"Found Date Letter Data for {playerId}. Sending...");
                DateLetter targetLetter = dateLetterData[playerId];
                ModEntry.modHelper.Multiplayer.SendMessage(targetLetter, "DateLetterData", modIDs: new[] { ModEntry.instance.ModManifest.UniqueID }, new long[] { playerId });
            }

            if (multiplierData.ContainsKey(playerId))
            {
                Logger.Log_Trace($"Found Multiplier Data for {playerId}. Sending...");
                FriendshipMultiplier targetMultiplier = multiplierData[playerId];
                ModEntry.modHelper.Multiplayer.SendMessage(targetMultiplier, "MultiplierData", modIDs: new[] { ModEntry.instance.ModManifest.UniqueID }, new long[] { playerId });

            }
        }

        private void AddToData<T>(ref Dictionary<long, T> data, string type, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModEntry.instance.ModManifest.UniqueID && e.Type == type)
            {
                T obj = e.ReadAs<T>();

                if (!data.ContainsKey(e.FromPlayerID))
                {
                    data.Add(e.FromPlayerID, obj);
                    Logger.Log_Trace($"Added new profile for player {e.FromPlayerID} for object type {obj}");
                    return;
                }
                data[e.FromPlayerID] = obj;
            }
        }

        public void ReceiveMessage(ModMessageReceivedEventArgs e)
        {
            AddToData<DateLetter>(ref this.dateLetterData, "DateLetterData", e);
            AddToData<FriendshipMultiplier>(ref this.multiplierData, "MultiplierData", e);

            Logger.Log_Trace($"Received message from {e.FromPlayerID}. Type was {e.Type}");

            if (e.Type == "ClearDateLetterData" && dateLetterData.ContainsKey(e.FromPlayerID))
            {
                dateLetterData.Remove(e.FromPlayerID);
                Logger.Log_Trace($"{e.FromPlayerID} DateLetter was null. Trying to remove the profile...");

            }
            else if (e.Type == "ClearMultiplierData" && multiplierData.ContainsKey(e.FromPlayerID))
            {
                multiplierData.Remove(e.FromPlayerID);
                Logger.Log_Trace($"{e.FromPlayerID} Multiplier was now null. Trying to remove the profile...");
            }
        }

        public void Save()
        {
            if (Context.IsMultiplayer && Context.IsMainPlayer)
            {
                ModEntry.modHelper.Data.WriteSaveData<NetworkDataManager>("NetworkDataManager", this);
                Logger.Log_Trace("Saved network data");
            }
        }

        public void TryLoadingDataForHost()
        {
            long hostId = Game1.player.UniqueMultiplayerID;

            if (dateLetterData.ContainsKey(hostId))
            {
                Logger.Log_Trace("Host key was found in date letter data");
                ModEntry.dateLetter = dateLetterData[hostId];
            }
            if (multiplierData.ContainsKey(hostId))
            {
                Logger.Log_Trace("Host key was found in friendship multiplier data");
                ModEntry.multiplier = multiplierData[hostId];
            }
            else
            {
                Logger.Log_Trace("No data for the host was found. Adding new profile for host...");
                long playerId = Game1.player.UniqueMultiplayerID;

                multiplierData.Add(playerId, new FriendshipMultiplier("None"));
                dateLetterData.Add(playerId, new DateLetter());
            }
        }
    }
}
