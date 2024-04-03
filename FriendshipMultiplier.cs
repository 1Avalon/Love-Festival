using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class FriendshipMultiplier
    {
        public string targetName;

        public float amount = 1.2f;

        public int expiresAt = ModEntry.festivalDate + 7;

        public FriendshipMultiplier(string targetName) 
        { 
            this.targetName = targetName;
        }
    }
}
