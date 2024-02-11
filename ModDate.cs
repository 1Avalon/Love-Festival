using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public sealed class ModDate
    {

        public string condition;

        public string eventScript;

        public string mapName;

        public string[] forks;

        public string dateLetterContent;

        public string acceptingDateResponse;

        public ModDate(string condition, string eventScript, params string[] forks)
        {
            this.condition = condition;
            this.eventScript = eventScript;
            this.forks = forks;
        }
    }
}
