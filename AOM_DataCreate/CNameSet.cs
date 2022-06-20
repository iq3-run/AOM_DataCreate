using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOM_DataCreate
{
    internal class CNameSet
    {
        public string Japanese { get; set; }
        public string English { get; set; }
        public string Chinese { get; set; }

        public CNameSet()
        {
            Japanese = "";
            English = "";
            Chinese = "";
        }

        public CNameSet(string japanese, string english, string chinese) {
            Japanese = japanese;
            English = english;
            Chinese = chinese;
        }

        public CNameSet Clone() {
            return new CNameSet(Japanese, English, Chinese);
        }

        public override string ToString() {
            return Japanese + "\t" + English + "\t" + Chinese;
        }
    }
}
