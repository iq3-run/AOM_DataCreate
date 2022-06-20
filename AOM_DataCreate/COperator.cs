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

        private List<COperator> alternative;
        public List<COperator> Alternative { get { return alternative; } }

        private List<CMaterialSet>[] promotion = new List<CMaterialSet>[2];
        public List<CMaterialSet>[] Promotion { get { return promotion; } }

        private List<CMaterialSet>[] skill = new List<CMaterialSet>[6];
        public List<CMaterialSet>[] Skill { get { return skill; } }

        private List<CMaterialSet>[,] skill_sp = new List<CMaterialSet>[3, 3];
        public List<CMaterialSet>[,] SkillSP { get { return skill_sp; } }

        private List<CMaterialSet>[,] module = new List<CMaterialSet>[2, 3];
        public List<CMaterialSet>[,] Module { get { return module; } }

        public bool isGlobal { get; set; }
        public bool isParadox { get; set; }
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
            isGlobal = true;
            isParadox = true;
        }

        public COperator CreateAlter() {
            if(Parent == null) {
                COperator alter = new COperator();
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
                    alternative = other.alternative;
                    foreach(var item in alternative) {
                        item.Parent = this;
                    }
                }
                for(int i = 0; i < promotion.Length; i++) {
                    if(promotion[i].Count == 0) promotion[i] = other.promotion[i];
                }
                for(int i = 0; i < skill.Length; i++) {
                    if(skill[i].Count == 0) skill[i] = other.skill[i];
                }
                for(int i = 0; i < skill_sp.GetLength(0); i++) {
                    for(int j = 0; j < skill_sp.GetLength(1); j++) {
                        if(skill_sp[i, j].Count == 0) skill_sp[i, j] = other.skill_sp[i, j];
                    }
                }
                for(int i = 0; i < module.GetLength(0); i++) {
                    for(int j = 0; j < module.GetLength(1); j++) {
                        if(module[i, j].Count == 0) module[i, j] = other.module[i, j];
                    }
                }
                if(isGlobal) isGlobal = other.isGlobal;
                if(isParadox) isParadox = other.isParadox;
                return true;
            } else {
                return false;
            }
        }

        public string ExportOpeData() {
            StringWriter sw = new StringWriter();
            bool writeed = false;
            int step = 0;
            for(int i = 0; i < promotion.Length; i++) {
                if(promotion[i].Count > 0) {
                    sw.Write("{0}\t{1}", ID, step);
                    int k = 0;
                    foreach(var item in promotion[i]) {
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
            for(int i = 0; i < skill.Length; i++) {
                if(skill[i].Count > 0) {
                    if(!writeed) sw.WriteLine("{0}\t0\t\t\t\t\t\t", ID);
                    sw.Write("{0}\t{1}", ID, step);
                    int k = 0;
                    foreach(var item in skill[i]) {
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
            for(int i = 0; i < skill_sp.GetLength(0); i++) {
                for(int j = 0; j < skill_sp.GetLength(1); j++) {
                    if(skill_sp[i, j].Count > 0) {
                        sw.Write("{0}\t{1}", ID, step);
                        int k = 0;
                        foreach(var item in skill_sp[i, j]) {
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
            }
            for(int i = 0; i < module.GetLength(0); i++) {
                for(int j = 0; j < module.GetLength(1); j++) {
                    if(module[i, j].Count > 0) {
                        sw.Write("{0}\t{1}", ID, step);
                        int k = 0;
                        foreach(var item in module[i, j]) {
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
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            if(obj is COperator other) {
                if(ID.Length > 0 && ID.Equals(other.ID)) return true;
                if(ID.Length == 0 || other.ID.Length == 0) {
                    if(Name.Chinese.Length > 0 && Name.Chinese.Equals(other.Name.Chinese)) return true;
                    if(Name.Japanese.Length > 0 && Name.Japanese.Equals(other.Name.Japanese)) return true;
                    if(Name.English.Length > 0 && Name.English.Equals(other.Name.English)) return true;
                }
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
