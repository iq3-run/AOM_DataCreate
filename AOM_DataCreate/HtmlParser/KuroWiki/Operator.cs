using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AOM_DataCreate.HtmlParser.KuroWiki {
    internal class Operator : HtmlParser {
        static readonly Regex kurowiki_chara_name_regex = new(@"(.+?)\s*(?:\(|（)(.+)(?:\)|）)\s*\[(.+)\]");
        static readonly Regex sub_class_regex = new(@"\[(.+)\](?:\s|&\w+;)*(.+)");
        static readonly Regex material_regex = new(@"(.+?)\s*(?:x|×)\s*([\d,]+)");

        public Operator(string source) : base(source) {
        }
        public Operator(Uri url) : base(url) {
        }

        public COperator? Parse(Dictionary<int, CNameSet> material_dic, List<string> paradox_list) {
            COperator opr = new();
            GetSource(5);
            if(Source == null) return null;
            string kurowiki_chara_str2 = HttpUtility.HtmlDecode(HtmlParser.TagRegex.Replace(Source, ""));
            StringReader sr = new(kurowiki_chara_str2);

            bool do_check = false;
            string data_pos = "";
            int skl_kind = -1;
            int mod_kind = -1;
            int level = -1;
            bool into_mod = false;
            string? line;

            while((line = sr.ReadLine()) != null) {
                line = line.Trim();
                if(line.Length == 0) continue;

                if(line.StartsWith("ページ名：")) {
                    do_check = true;
                    continue;
                }
                if(line.Equals("昇格（ジョブチェンジシステム）")) {
                    do_check = true;
                    opr = opr.CreateAlter();
                    data_pos = "";
                    skl_kind = -1;
                    mod_kind = -1;
                    continue;
                }

                if(do_check) {
                    if(opr.Name.Japanese.Length == 0 && kurowiki_chara_name_regex.IsMatch(line)) {
                        GroupCollection chara_name = kurowiki_chara_name_regex.Match(line).Groups;
                        opr.Name.Chinese = chara_name[3].Value.Trim();
                        opr.Name.Japanese = chara_name[1].Value.Trim();
                        opr.Name.English = chara_name[2].Value.Trim();
                    }
                    if(line.Equals("コメント")) do_check = false;
                    else if(opr.Class.Japanese.Length == 0 && (line.Equals("クラス") || line.Equals("兵種"))) {
                        opr.Class.Japanese = (sr.ReadLine() ?? "").Trim();
                        opr.SubClass.Japanese = sub_class_regex.Match(sr.ReadLine() ?? "").Groups[2].Value.Trim();
                    } else if(opr.Birthplace.Japanese.Length == 0 && line.Equals("出身")) opr.Birthplace.Japanese = (sr.ReadLine() ?? "").Trim();
                    else if(opr.Race.Japanese.Length == 0 && line.Equals("種族")) opr.Race.Japanese = (sr.ReadLine() ?? "").Trim();
                    else if(opr.Camp.Japanese.Length == 0 && line.Equals("陣営")) opr.Camp.Japanese = (sr.ReadLine() ?? "").Trim();
                    else if(line.Replace(" ", "").Equals("昇進1")) {
                        data_pos = "昇進";
                        level = 0;
                    } else if(line.Replace(" ", "").Equals("昇進2")) {
                        data_pos = "昇進";
                        level = 1;
                    } else if(line.Equals("1→2")) {
                        data_pos = "スキル";
                        level = 0;
                    } else if(line.Equals("2→3")) {
                        data_pos = "スキル";
                        level = 1;
                    } else if(line.Equals("3→4")) {
                        data_pos = "スキル";
                        level = 2;
                    } else if(line.Equals("4→5")) {
                        data_pos = "スキル";
                        level = 3;
                    } else if(line.Equals("5→6")) {
                        data_pos = "スキル";
                        level = 4;
                    } else if(line.Equals("6→7")) {
                        data_pos = "スキル";
                        level = 5;
                    } else if(line.Equals("7→8")) {
                        data_pos = "特化";
                        skl_kind++;
                        level = 0;
                    } else if(line.Equals("8→9")) {
                        data_pos = "特化";
                        level = 1;
                    } else if(line.Equals("9→10")) {
                        data_pos = "特化";
                        level = 2;
                    } else if(line.Equals("モジュール強化")) into_mod = true;
                    else if(into_mod && line.Contains("-X")) mod_kind = 0;
                    else if(into_mod && line.Contains("-Y")) mod_kind = 1;
                    else if(into_mod && line.Equals("Lv1")) {
                        data_pos = "モジュール";
                        level = 0;
                    } else if(into_mod && line.Equals("Lv2")) {
                        data_pos = "モジュール";
                        level = 1;
                    } else if(into_mod && line.Equals("Lv3")) {
                        data_pos = "モジュール";
                        level = 2;
                    } else if(line.Equals("中国wiki攻略ページ")) {
                        //string? prts_chara_url = sr.ReadLine();
                        //if(prts_chara_url != null) {
                        //    string[] u = prts_chara_url.Split('/');
                        //    Task<string?> pcp = GetPage(new Uri("https://prts.wiki/index.php?curid=" + u[u.Length - 1]));
                        //    pcp.Wait();
                        //    string? result = pcp.Result;
                        //    if(result != null) {
                        //        if(result.Contains("悖论模拟")) {
                        //            base_opr.isParadox = true;
                        //        } else {
                        //            base_opr.isParadox = false;
                        //        }
                        //    }
                        //}
                    } else if(line.Equals("基地スキル")) {
                        data_pos = "";
                        do_check = false;
                    }

                    if(data_pos != "") {
                        foreach(Match m in material_regex.Matches(line)) {
                            string material_name = m.Groups[1].Value.Trim().Replace(" ", "").Replace("III", "3").Replace("II", "2").Replace("I", "1");
                            material_name = material_name.Replace("Ⅰ", "1").Replace("Ⅱ", "2").Replace("Ⅲ", "3");
                            int material_qty = int.Parse(m.Groups[2].Value.Trim());
                            try {
                                var material = material_dic.First(m => m.Value.Equals(material_name));
                                CMaterialSet material_set = new(material, material_qty);
                                switch(data_pos) {
                                    case "昇進":
                                        opr.Promotion[level].Add(material_set);
                                        break;
                                    case "スキル":
                                        opr.Skill[level].Add(material_set);
                                        break;
                                    case "特化":
                                        opr.SkillSP[skl_kind, level].Add(material_set);
                                        break;
                                    case "モジュール":
                                        opr.Module[mod_kind, level].Add(material_set);
                                        break;
                                    default:
                                        break;
                                }
                            } catch {
                                continue;
                            }
                        }
                    }
                }
            }
            if(opr.Parent != null) opr = opr.Parent;
            if(opr.Name.Japanese.Length > 0) {
                if(paradox_list.Count > 0) {
                    if(paradox_list.Contains(opr.Name.Chinese)) opr.IsParadox = true;
                    else opr.IsParadox = false;
                }

                return opr;
            }
            return null;
        }
    }
}
