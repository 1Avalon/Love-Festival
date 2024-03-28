using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace LoveFestival
{
    public sealed class ModDate
    {
        public string DateUniqueId;

        public string Condition;

        public string Location;

        public Dictionary<string, string> EventScript;

        public string DateLetterContent;

        public string AcceptDateResponse;

        public int day;

        public string DatePartnerName;

        private readonly string NAME_TOKEN = "{DATEPARTNER_NAME}";

        private readonly string DAYSUNTILDATE_TOKEN = "{DAYS_UNTIL_DATE}";
        public void LoadNpcNameToken()
        {
            DateLetterContent = DateLetterContent.Replace(NAME_TOKEN, DatePartnerName);
            foreach (var item in EventScript)
            {
                EventScript[item.Key] = item.Value.Replace(NAME_TOKEN, DatePartnerName);
            }
        }
        public void LoadDaysUntilDateToken()
        {
            AcceptDateResponse = AcceptDateResponse.Replace(DAYSUNTILDATE_TOKEN, (day - Game1.dayOfMonth).ToString());
        }

    }
}
