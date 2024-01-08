using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoveFestival
{
    public class LoveLetter
    {
        public string Context
        {
            get { return generateLetterContext.Invoke(); }
        }

        public delegate string GenerateLetter();

        private GenerateLetter generateLetterContext;
        public LoveLetter(GenerateLetter generateLetter)
        {
            generateLetterContext = generateLetter;
        } 
        public LoveLetter(string context) 
        {
            generateLetterContext = () => { return context; };
        }
    }
}
