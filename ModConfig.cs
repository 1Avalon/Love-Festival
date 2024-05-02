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
    }
}
