using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public sealed class ModConfig
    {
        public string TestNpcDateName { get; set; } = "Haley";

        public bool SpouseAlwaysGivingLetter { get; set; } = true;

        public int MinRequiredHearts { get; set; } = 2;

        public int ChancePerHeart { get; set; } = 10;

        public bool ChangeMoneyTextColorInLetter { get; set; } = true;

        public int NpcLimit { get; set; } = 12; //allow users to set a limit, later the festival may take too much time and the rewards are not useful anymore at that progression point

        public bool EnableModdedNPCs { get; set; } = true;
    }
}
