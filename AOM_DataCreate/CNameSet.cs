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

        public override bool Equals(object? obj) {
            if(obj == null) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj is CNameSet other) {
                if(other.Chinese.Equals(Chinese)) return true;
                if(other.Japanese.Equals(Japanese)) return true;
                if(other.English.Equals(English)) return true;
            }
            if(obj is string str) {
                if(Chinese.Equals(str)) return true;
                if(Japanese.Equals(str)) return true;
                if(English.Equals(str)) return true;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
