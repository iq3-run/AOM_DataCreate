// See https://aka.ms/new-console-template for more information
using AOM_DataCreate;
using AOM_DataCreate.HtmlParser;
using PRTS = AOM_DataCreate.HtmlParser.PRTS;
using KuroWiki = AOM_DataCreate.HtmlParser.KuroWiki;
using SiroWiki = AOM_DataCreate.HtmlParser.SiroWiki;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


List<COperator> operator_list = new();

Dictionary<string, int> material_dic = new();
Dictionary<int, CNameSet> material_dic2 = new();
Dictionary<string, int> place_dic = new();
Dictionary<string, int> race_dic = new();
Dictionary<string, int> class_dic = new();
Dictionary<int, Dictionary<string, int>> sub_class_dic = new();

if(args.Length < 1) return;
Directory.SetCurrentDirectory(args[0]);
using(StreamReader sr = new(@"lang.csv")) {
    string? line;
    string mode = "";
    while((line = sr.ReadLine()) != null) {
        string[] lang = line.Split("\t");
        if(lang[0].StartsWith("[")) {
            mode = lang[0][1..^1];
            continue;
        }
        if(mode.Equals("Operator")) continue;
        int id = int.Parse(lang[0]);
        CNameSet name = new(lang[1], lang[2], lang[3]);
        switch(mode) {
            case "Class":
                class_dic[lang[1]] = id;
                class_dic[lang[2]] = id;
                class_dic[lang[3]] = id;
                break;
            case "Race":
                race_dic[lang[1]] = id;
                race_dic[lang[2]] = id;
                race_dic[lang[3]] = id;
                break;
            case "Place":
                place_dic[lang[1]] = id;
                place_dic[lang[2]] = id;
                place_dic[lang[3]] = id;
                string[] bp = lang[1].Split("/");
                foreach(var item in bp) {
                    place_dic[item] = id;
                }
                break;
            case "Material":
                material_dic[lang[1]] = id;
                material_dic[lang[2]] = id;
                material_dic[lang[3]] = id;
                material_dic2[id] = name;
                break;
            case "System":
                break;
            case "Operator":
                break;
            default:
                int class_id = class_dic[mode];
                if(!sub_class_dic.ContainsKey(class_id)) {
                    sub_class_dic.Add(class_id, new Dictionary<string, int>());
                }
                sub_class_dic[class_id][lang[1]] = id;
                sub_class_dic[class_id][lang[2]] = id;
                sub_class_dic[class_id][lang[3]] = id;
                break;
        }
    }
}

Console.WriteLine("PRTS");
PRTS.OperatorList prts_operator_parser = new();
PRTS.ParadoxList paradox_list_parser = new();

List<COperator> prts_charas = prts_operator_parser.GetOperators(material_dic2);
List<string> paradox_list = paradox_list_parser.Parse();

Console.WriteLine("黒Wiki");
KuroWiki.OperatorList kuro_ope_list_parser = new();
operator_list = kuro_ope_list_parser.MargeOperator(prts_charas, material_dic2, paradox_list);

Console.WriteLine("白Wiki");
SiroWiki.ProfileList profileList = new();
profileList.MargeOperators(operator_list);

Regex split_mark_regex = new(@"\W+");

operator_list.Sort((a, b) => b.Rarity.CompareTo(a.Rarity));
StreamReader lang_read = new(@"lang.csv");
StreamWriter lang_write = new(@"lang_new.csv");
StreamWriter opeinfodata_write = new(@"opeinfodata_new.csv");
StreamWriter opedata_write = new(@"opedata_new.csv");
List<string> output_operator_list = new();
int before_rarity = 6;
try {
    opeinfodata_write.WriteLine("Code\tRarity\tClass\tByUse\tRace\tPlace\tAlready\tParadox\tAlternate");
    opedata_write.WriteLine("Code\tStep\tMaterial1\tCount\tMaterial2\tCount\tMaterial3\tCount");

    string? lang_line;
    while((lang_line = lang_read.ReadLine()) != null) {
        lang_write.WriteLine(lang_line);
        if(lang_line.StartsWith("[Operator]")) break;
    }
    while((lang_line = lang_read.ReadLine()) != null) {
        if(lang_line.StartsWith("[Material]")) {
            lang_write.WriteLine(lang_line);
            break;
        }
        string[] line_cols = lang_line.Split("\t");
        COperator? opr = operator_list.Find(o => o.ID.Equals(line_cols[0]) || o.Name.Chinese.Equals(line_cols[3]));
        if(opr == null) continue;
        if(opr.Rarity != before_rarity) {
            
            foreach(var o in operator_list.FindAll(o => o.Rarity == before_rarity && !output_operator_list.Contains(o.ID))) {
                output_operator(o);
                output_operator_list.Add(o.ID);
            }
            before_rarity = opr.Rarity;

        }
        output_operator(opr);
        output_operator_list.Add(opr.ID);
    }
    while((lang_line = lang_read.ReadLine()) != null) {
        lang_write.WriteLine(lang_line);
    }

    void output_operator(COperator opr) {
        lang_write.WriteLine("{0}\t{1}\t{2}\t{3}", opr.ID, opr.Name.Japanese, opr.Name.English, opr.Name.Chinese);
        int class_id = class_dic[opr.Class.Japanese];
        int sub_class_id = sub_class_dic[class_id][opr.SubClass.Japanese];
        bool exist_race_id = false;
        if(race_dic.TryGetValue(opr.Race.Japanese, out int race_id)) {
            exist_race_id = true;
        } else {
            string[] race = split_mark_regex.Split(opr.Race.Japanese);
            foreach(string r in race) {
                if(race_dic.TryGetValue(r, out race_id)) {
                    exist_race_id = true;
                    break;
                }
            }
        }
        if(!exist_race_id) {
            Console.WriteLine("{0} 種族：{1}", opr.Name.Japanese, opr.Race.Japanese);
            race_id = 99;
        }
        bool exist_place_id = false;
        if(place_dic.TryGetValue(opr.Birthplace.Japanese, out int place_id)) {
            exist_place_id = true;
        } else {
            string[] place = split_mark_regex.Split(opr.Birthplace.Japanese);
            foreach(string p in place) {
                if(place_dic.TryGetValue(p, out place_id)) {
                    exist_place_id = true;
                    break;
                }
            }
        }
        if(!exist_place_id) {
            Console.WriteLine("{0} 出身：{1}", opr.Name.Japanese, opr.Birthplace.Japanese);
            place_id = 99;
        }
        opeinfodata_write.WriteLine("{0}\t☆{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", opr.ID, opr.Rarity, class_id, sub_class_id, race_id, place_id, opr.IsGlobal ? 1 : 0, opr.IsParadox ? 1 : 0, 0);
        opedata_write.Write(opr.ExportOpeData());
        foreach(COperator alter in opr.Alternative) {
            class_id = class_dic[alter.Class.Japanese];
            sub_class_id = sub_class_dic[class_id][alter.SubClass.Japanese];
            lang_write.WriteLine("{0}\t{1}\t{2}\t{3}", alter.ID, alter.Name.Japanese, alter.Name.English, alter.Name.Chinese);
            opeinfodata_write.WriteLine("{0}\t☆{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", alter.ID, opr.Rarity, class_id, sub_class_id, race_id, place_id, opr.IsGlobal ? 1 : 0, opr.IsParadox ? 1 : 0, 1);
            opedata_write.Write(alter.ExportOpeData());
        }
    }
} finally {
    lang_read.Close();
    lang_write.Close();
    opeinfodata_write.Close();
    opedata_write.Close();
}

