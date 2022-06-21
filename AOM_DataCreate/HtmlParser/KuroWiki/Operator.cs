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
        static readonly Regex material_regex = new(@"(.+?)\s*(?:x|×)\s*(\d+)");

        public Operator(string source) : base(source) {
        }
        public Operator(Uri url) : base(url) {
        }

        public COperator? Parse(Dictionary<string, int> material_dic, List<string> paradox_list) {
            COperator opr = new();
            if(Source == null) {
                Wait();
                if(Source == null) {
                    throw new NullReferenceException("ソースの取得に失敗しました");
                }
            }

            string kurowiki_chara_str2 = HttpUtility.HtmlDecode(HtmlParser.TagRegex.Replace(Source, ""));
            StringReader sr = new(kurowiki_chara_str2);

            bool do_check = false;
            string data_pos = "";
            int skl_kind = -1;
            int mod_kind = -1;
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
                    else if(line.Replace(" ", "").Equals("昇進1")) data_pos = "昇進1";
                    else if(line.Replace(" ", "").Equals("昇進2")) data_pos = "昇進2";
                    else if(line.Equals("1→2")) data_pos = "1to2";
                    else if(line.Equals("2→3")) data_pos = "2to3";
                    else if(line.Equals("3→4")) data_pos = "3to4";
                    else if(line.Equals("4→5")) data_pos = "4to5";
                    else if(line.Equals("5→6")) data_pos = "5to6";
                    else if(line.Equals("6→7")) data_pos = "6to7";
                    else if(line.Equals("7→8")) {
                        data_pos = "7to8";
                        skl_kind++;
                    } else if(line.Equals("8→9")) data_pos = "8to9";
                    else if(line.Equals("9→10")) data_pos = "9to10";
                    else if(line.Equals("モジュール強化")) into_mod = true;
                    else if(into_mod && line.Contains("-X")) mod_kind = 0;
                    else if(into_mod && line.Contains("-Y")) mod_kind = 1;
                    else if(into_mod && line.Equals("Lv1")) data_pos = "modLv1";
                    else if(into_mod && line.Equals("Lv2")) data_pos = "modLv2";
                    else if(into_mod && line.Equals("Lv3")) data_pos = "modLv3";
                    else if(line.Equals("中国wiki攻略ページ")) {
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
                            if(material_dic.ContainsKey(material_name)) {
                                int material_id = material_dic[material_name];
                                switch(data_pos) {
                                    case "昇進1":
                                        opr.Promotion[0].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "昇進2":
                                        opr.Promotion[1].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "1to2":
                                        opr.Skill[0].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "2to3":
                                        opr.Skill[1].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "3to4":
                                        opr.Skill[2].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "4to5":
                                        opr.Skill[3].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "5to6":
                                        opr.Skill[4].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "6to7":
                                        opr.Skill[5].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "7to8":
                                        opr.SkillSP[skl_kind, 0].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "8to9":
                                        opr.SkillSP[skl_kind, 1].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "9to10":
                                        opr.SkillSP[skl_kind, 2].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "modLv1":
                                        opr.Module[mod_kind, 0].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "modLv2":
                                        opr.Module[mod_kind, 1].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    case "modLv3":
                                        opr.Module[mod_kind, 2].Add(new CMaterialSet(material_id, material_name, material_qty));
                                        break;
                                    default:
                                        break;
                                }
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
