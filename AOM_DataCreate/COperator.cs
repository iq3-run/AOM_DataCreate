using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOM_DataCreate {
    internal class COperator {

        string id;
        public string ID {
            get {
                if(Parent != null) {
                    int n = Parent.alternative.IndexOf(this);
                    return string.Format("{0}_{1}", Parent.id, n + 2);
                } else
                    return id;
            }
            set {
                id = value;
                if(Parent != null) Parent.id = value;
            }
        }
        public int Rarity { get; set; }

        CNameSet name;
        public CNameSet Name {
            get {
                if(Parent != null) {
                    string format = "{0}({1})";
                    return new CNameSet(
                        string.Format(format, Parent.name.Japanese, Class.Japanese),
                        string.Format(format, Parent.name.English, Class.English),
                        string.Format(format, Parent.name.Chinese, Class.Chinese)
                        );
                } else return name;
            }
        }
        public CNameSet Class { get; set; }
        public CNameSet SubClass { get; set; }
        public CNameSet Birthplace { get; set; }
        public CNameSet Nation { get; set; }
        public CNameSet Camp { get; set; }
        public CNameSet Race { get; set; }

        private readonly List<COperator> alternative;
        public List<COperator> Alternative { get { return alternative; } }

        public List<CMaterialSet>[] Materials {
            get {
                List<List<CMaterialSet>> m = new();
                for(int i = 0; i < promotion.Length; i++) {
                    m.Add( promotion[i]);
                }
                for(int i = 0; i < skill.Length; i++) {
                    m.Add(skill[i]);
                }
                for(int i = 0; i < skill_sp.GetLength(0); i++) {
                    for(int j = 0; j < skill_sp.GetLength(1); j++) {
                        m.Add(skill_sp[i, j]);
                    }
                }
                for(int i = 0; i < module.GetLength(0); i++) {
                    for(int j = 0; j < module.GetLength(1); j++) {
                        m.Add( module[i, j]);
                    }
                }
                return m.ToArray();
            }
        }

        private readonly List<CMaterialSet>[] promotion = new List<CMaterialSet>[2];
        public List<CMaterialSet>[] Promotion { get { return promotion; } }

        private readonly List<CMaterialSet>[] skill = new List<CMaterialSet>[6];
        public List<CMaterialSet>[] Skill { get { return skill; } }

        private readonly List<CMaterialSet>[,] skill_sp = new List<CMaterialSet>[3, 3];
        public List<CMaterialSet>[,] SkillSP { get { return skill_sp; } }

        private readonly List<CMaterialSet>[,] module = new List<CMaterialSet>[2, 3];
        public List<CMaterialSet>[,] Module { get { return module; } }

        public bool IsGlobal { get; set; }
        public bool IsParadox { get; set; }
        public COperator? Parent { get; set; }

        public COperator() {
            name = new CNameSet();
            id = "";
            Class = new CNameSet();
            SubClass = new CNameSet();
            Birthplace = new CNameSet();
            Nation = new CNameSet();
            Camp = new CNameSet();
            Race = new CNameSet();

            for(int i = 0; i < promotion.Length; i++) {
                promotion[i] = new List<CMaterialSet>();
            }

            for(int i = 0; i < skill.Length; i++) {
                skill[i] = new List<CMaterialSet>();
            }
            for(int i = 0; i < skill_sp.GetLength(0); i++) {
                for(int j = 0; j < skill_sp.GetLength(1); j++) {
                    skill_sp[i, j] = new List<CMaterialSet>();
                }
            }
            for(int i = 0; i < module.GetLength(0); i++) {
                for(int j = 0; j < module.GetLength(1); j++) {
                    module[i, j] = new List<CMaterialSet>();
                }
            }
            alternative = new List<COperator>();
            IsGlobal = true;
            IsParadox = true;
        }

        public COperator CreateAlter() {
            if(Parent == null) {
                COperator alter = new();
                alternative.Add(alter);
                alter.name = name;
                alter.ID = ID;
                alter.Rarity = Rarity;
                alter.Birthplace = Birthplace;
                alter.Nation = Nation;
                alter.Camp = Camp;
                alter.Race = Race;
                alter.Parent = this;
                return alter;
            } else {
                return Parent.CreateAlter();
            }
        }

        public void AddAlter(COperator alter) {
            if(Parent == null) {
                alternative.Add(alter);
                alter.Parent = this;
                foreach(var item in alter.Alternative) {
                    this.AddAlter(item);
                }
                alter.alternative.Clear();
            } else {
                Parent.AddAlter(alter);
            }
        }

        public bool TryMerge(COperator other) {
            if(this.Equals(other)) {
                if(ID.Length == 0) ID = other.ID;
                if(Rarity == 0) Rarity = other.Rarity;
                if(Name.Japanese.Length == 0) Name.Japanese = other.Name.Japanese;
                if(Name.English.Length == 0) Name.English = other.Name.English;
                if(Name.Chinese.Length == 0) Name.Chinese = other.Name.Chinese;
                if(Class.Japanese.Length == 0) Class.Japanese = other.Class.Japanese;
                if(Class.English.Length == 0) Class.English = other.Class.English;
                if(Class.Chinese.Length == 0) Class.Chinese = other.Class.Chinese;
                if(SubClass.Japanese.Length == 0) SubClass.Japanese = other.SubClass.Japanese;
                if(SubClass.English.Length == 0) SubClass.English = other.SubClass.English;
                if(SubClass.Chinese.Length == 0) SubClass.Chinese = other.SubClass.Chinese;
                if(Birthplace.Japanese.Length == 0) Birthplace.Japanese = other.Birthplace.Japanese;
                if(Birthplace.English.Length == 0) Birthplace.English = other.Birthplace.English;
                if(Birthplace.Chinese.Length == 0) Birthplace.Chinese = other.Birthplace.Chinese;
                if(Nation.Japanese.Length == 0) Nation.Japanese = other.Nation.Japanese;
                if(Nation.English.Length == 0) Nation.English = other.Nation.English;
                if(Nation.Chinese.Length == 0) Nation.Chinese = other.Nation.Chinese;
                if(Camp.Japanese.Length == 0) Camp.Japanese = other.Camp.Japanese;
                if(Camp.English.Length == 0) Camp.English = other.Camp.English;
                if(Camp.Chinese.Length == 0) Camp.Chinese = other.Camp.Chinese;
                if(Race.Japanese.Length == 0) Race.Japanese = other.Race.Japanese;
                if(Race.English.Length == 0) Race.English = other.Race.English;
                if(Race.Chinese.Length == 0) Race.Chinese = other.Race.Chinese;
                if(alternative.Count == 0) {
                    alternative.AddRange(other.alternative);
                    foreach(var item in alternative) {
                        item.Parent = this;
                    }
                } else {
                    for(int i = 0; i < alternative.Count && i < other.alternative.Count; i++) {
                        alternative[i].TryMerge(other.alternative[i]);
                    }
                }

                for(int i = 0; i < Materials.Length; i++) {
                    if(Materials[i].Count == 0) Materials[i].AddRange(other.Materials[i]);
                    else {
                        foreach(var material_f in Materials[i]) {
                            if(material_f.Name.Equals("龍門幣")) continue;
                            CMaterialSet? material_t = other.Materials[i].Find(m => m.ID == material_f.ID);
                            if(material_t == null) {
                                Console.WriteLine("<{0} {1}-{2}: {3}", ID, Name.Japanese, (Step)i, material_f.Name.Japanese);
                                continue;
                            }
                            if(material_f.Quantity != material_t.Quantity) {
                                Console.WriteLine("!{0} {1}-{2}: {3} x{4} <> x{5}", ID, Name.Japanese, (Step)i, material_f.Name.Japanese, material_f.Quantity, material_t.Quantity);
                            }
                        }
                        foreach(var material_t in other.Materials[i].FindAll(mt => !Materials[i].Exists(mf => mt.ID == mf.ID))) {
                            Console.WriteLine(">{0} {1}-{2}: {3}", ID, Name.Japanese, (Step)i, material_t.Name.Japanese);
                        }
                    }
                }

                if(IsGlobal) IsGlobal = other.IsGlobal;
                if(IsParadox) IsParadox = other.IsParadox;
                return true;
            } else {
                return false;
            }
        }


        public string ExportOpeData() {
            StringWriter sw = new();
            bool writeed = false;
            int step = 0;
            for(int i = 0; i < Materials.Length; i++) {
                if(Materials[i].Count > 0) {
                    if(i == (int)Step.SLv1to2 && !writeed) sw.WriteLine("{0}\t0\t\t\t\t\t\t", ID);
                    sw.Write("{0}\t{1}", ID, step);
                    int k = 0;
                    foreach(var item in Materials[i]) {
                        if(!item.Name.Equals("龍門幣")) {
                            sw.Write("\t{0}\t{1}", item.ID, item.Quantity);
                            k++;
                        }
                    }
                    for(; k < 3; k++) {
                        sw.Write("\t\t");
                    }
                    sw.WriteLine();
                    writeed = true;
                }
                step++;
            }
            if(!writeed) sw.WriteLine("{0}\t-\t\t\t\t\t\t", ID);
            return sw.ToString();
        }

        public string[] ExportLang() {
            return new string[] { ID, Name.Japanese, Name.English, Name.Chinese };
        }

        override public string ToString() {
            return string.Format("{0} {1}", ID, Name.Japanese);
        }

        public override bool Equals(object? obj) {
            if(obj == null) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj is COperator other) {
                if(ID.Length > 0 && ID.Equals(other.ID)) return true;
                if(ID.Length == 0 || other.ID.Length == 0) {
                    if(Name.Chinese.Length > 0 && Name.Chinese.Equals(other.Name.Chinese)) return true;
                    if(Name.Japanese.Length > 0 && Name.Japanese.Equals(other.Name.Japanese)) return true;
                    if(Name.English.Length > 0 && Name.English.Equals(other.Name.English)) return true;
                }
            }
            if(obj is string value) {
                if(ID.Length > 0 && ID.Equals(value)) return true;
                if(Name.Chinese.Length > 0 && Name.Chinese.Equals(value)) return true;
                if(Name.Japanese.Length > 0 && Name.Japanese.Equals(value)) return true;
                if(Name.English.Length > 0 && Name.English.Equals(value)) return true;
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public enum Step {
            昇進1,
            昇進2,
            SLv1to2,
            SLv2to3,
            SLv3to4,
            SLv4to5,
            SLv5to6,
            SLv6to7,
            S1特化1,
            S1特化2,
            S1特化3,
            S2特化1,
            S2特化2,
            S2特化3,
            S3特化1,
            S3特化2,
            S3特化3,
            モジュールXLv1,
            モジュールXLv2,
            モジュールXLv3,
            モジュールYLv1,
            モジュールYLv2,
            モジュールYLv3,
        }
    }
}
