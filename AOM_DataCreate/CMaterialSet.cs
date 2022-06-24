using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOM_DataCreate {
    internal class CMaterialSet {
        public int ID { get; set; }
        public CNameSet Name { get; set; }
        public int Quantity { get; set; }

        public CMaterialSet() {
            Name = new();
            Quantity = 0;
        }

        public CMaterialSet(int id, CNameSet name, int quantity) {
            ID = id;
            Name = name;
            Quantity = quantity;
        }

        public CMaterialSet(KeyValuePair<int, CNameSet> material, int quantity) {
            ID = material.Key;
            Name = material.Value;
            Quantity = quantity;
        }

        override public string ToString() {
            return string.Format("{0} x{1}", Name, Quantity);
        }

        public string ToTSV() {
            if(Quantity > 0) return string.Format("{0}\t{1}", ID, Quantity);
            else return "\t";
        }
    }
}
