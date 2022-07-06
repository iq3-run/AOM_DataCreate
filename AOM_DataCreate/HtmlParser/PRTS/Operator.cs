using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AOM_DataCreate.HtmlParser.PRTS {
    internal class Operator : HtmlParser {

        static readonly Regex charname_regex = new(@"<div[^>]*id=""charname""[^>]*>(.+?)</div>");
        static readonly Regex charname_en_regex = new(@"<div[^>]*id=""charname-en""[^>]*>(.+?)</div>");
        static readonly Regex class_regex = new(@"<div[^>]*id=""charclasstxt""[^>]*>.*?<a[^>]*>(.+?)</a> -- <a[^>]*>(.+?)</a>.*?</div>", RegexOptions.Singleline);
        static readonly Regex material_regex = new(@"<a[^>]*title=""([^""]+)""[^>]*>.*?</a><span[^>]*>([\d.,]+)(w|万)?</span>");

        public Operator(string source) : base(source) {
        }
        public Operator(Uri url) : base(url) {
        }
        // https://prts.wiki/w/%E8%8A%AC

        public COperator Parse(in COperator opr, Dictionary<int, CNameSet> material_dic) {
            GetSource(5);
            if(Source == null) return opr;

            if(opr.Name.Chinese.Length == 0) {
                Match name_regex = charname_regex.Match(Source);
                opr.Name.Chinese = name_regex.Groups[1].Value.Trim();
            }
            if(opr.Name.English.Length == 0) {
                Match name_en_regex = charname_en_regex.Match(Source);
                opr.Name.English = name_en_regex.Groups[1].Value.Trim();
            }

            Match class_match = class_regex.Match(Source);
            opr.Class.Chinese = class_match.Groups[1].Value.Trim();
            opr.SubClass.Chinese = class_match.Groups[2].Value.Trim();

            bool exists_promotion = false;
            bool exists_skill = false;
            bool exists_module = false;
            int start_point = Source.IndexOf(@"<span id=""精英化材料"">");
            if(start_point == -1) {
                start_point = Source.IndexOf(@"<span id=""技能升级材料"">");
            }
            if(start_point == -1) {
                start_point = 0;
            }

            foreach(Match tbody_match in HtmlParser.TableBodyRegex.Matches(Source, start_point)) {
                string mode = "";
                int level = -1;
                int skill = -1;
                int module = -1;

                if(!exists_promotion && tbody_match.Value.Contains("精英阶段")) {
                    mode = "昇進";
                    exists_promotion = true;
                }
                if(!exists_skill && tbody_match.Value.Contains("技能升级")) {
                    mode = "スキル";
                    exists_skill = true;
                }
                if(tbody_match.Value.Contains("解锁需求与材料消耗")) {
                    mode = "モジュール";
                    exists_module = true;
                }
                if(mode.Length == 0) {
                    continue;
                }

                foreach(Match cell_match in HtmlParser.TableCellRegex.Matches(tbody_match.Value)) {
                    if(cell_match.Value.Contains("0→1")) level = 0;
                    else if(cell_match.Value.Contains("1→2")) level = 1;
                    else if(cell_match.Value.Contains("2→3")) level = 2;
                    else if(cell_match.Value.Contains("3→4")) level = 3;
                    else if(cell_match.Value.Contains("4→5")) level = 4;
                    else if(cell_match.Value.Contains("5→6")) level = 5;
                    else if(cell_match.Value.Contains("6→7")) level = 6;
                    else if(cell_match.Value.Contains("等级1")) {
                        mode = "特化";
                        if(level != 0) skill = 0;
                        else skill++;
                        level = 0;
                    } else if(cell_match.Value.Contains("等级2")) {
                        mode = "特化";
                        if(level != 1) skill = 0;
                        else skill++;
                        level = 1;
                    } else if(cell_match.Value.Contains("等级3")) {
                        mode = "特化";
                        if(level != 2) skill = 0;
                        else skill++;
                        level = 2;
                    } else if(cell_match.Value.Contains("-X")) module = 0;
                    else if(cell_match.Value.Contains("-Y")) module = 1;
                    else if(cell_match.Value.Contains("解锁需求与材料消耗")) level = 0;
                    else if(cell_match.Value.Contains("模组升级消耗")) level = 1;

                    if(level < 0) continue;
                    bool add_materia = false;
                    foreach(Match material_match in material_regex.Matches(cell_match.Value)) {
                        string material_name = material_match.Groups[1].Value;
                        if(material_name.Equals("龙门币")) continue;
                        double qty_a = double.Parse(material_match.Groups[2].Value);
                        if(material_match.Groups[3].Success) qty_a *= 10000;
                        int qty = (int)qty_a;
                        var material = material_dic.First(m => m.Value.Equals(material_name));
                        //Console.WriteLine("{0} x{1}", material_name, qty);
                        CMaterialSet material_set = new(material, qty);
                        switch(mode) {
                            case "昇進":
                                opr.Promotion[level].Add(material_set);
                                break;
                            case "スキル":
                                opr.Skill[level - 1].Add(material_set);
                                break;
                            case "特化":
                                opr.SkillSP[skill, level].Add(material_set);
                                break;
                            case "モジュール":
                                opr.Module[module, level].Add(material_set);
                                break;
                            default:
                                break;
                        }
                        add_materia = true;
                    }
                    if(mode.Equals("モジュール") && add_materia) level++;
                }

            }

            return opr;
        }
    }
}