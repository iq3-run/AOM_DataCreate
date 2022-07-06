using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AOM_DataCreate.HtmlParser.SiroWiki {
    internal class Operator : HtmlParser {
        static readonly Regex chinese_name_regex = new(@"<p>中国語名(?:：|:)(.+?)[　\s<]");
        static readonly Regex english_name_regex = new(@"英語名(?:：|:)(.+?)[　\s<]");
        static readonly Regex russian_name_regex = new(@"ロシア語名(?:：|:)(.+?)[　\s<]");
        static readonly Regex profile_regex = new(@"<table[^>]*?><tbody>(.+?プロフィール.+?)(?:</tbody></table>|$)", RegexOptions.Singleline);
        static readonly Regex p_material_regex = new(@"(.+)x(\d+)");
        static readonly Regex item_title_regex = new(@"<img [^>]*title=""(.+?)""[^>]*>");
        static readonly Regex skill_qty_regex = new(@"x?(\d+)");
        static readonly Regex module_material_regex = new(@"<a[^>]*?><img[^>]*?title=""(.+?)""[^>]*?></a>x(\d+)");
        static readonly Regex material_img_regex = new(@"(d32|\w+?)(\d+)?(?:_.*?)?\.png");
        static readonly Regex skill_title_regex = new(@"<h2[^>]*>スキルランク素材一覧");

        public Operator(string source) : base(source) {
        }
        public Operator(Uri url) : base(url) {
        }

        public COperator? Parse(Dictionary<int, CNameSet> material_dic) {
            return Parse(new COperator(), material_dic);
        }
        
        public COperator? Parse(COperator opr, Dictionary<int, CNameSet> material_dic) {
            GetSource(5);
            if(Source == null) return null;

            Match chinese_name_match = chinese_name_regex.Match(Source);
            if(!chinese_name_match.Success) {
                return null;
            }
            opr.Name.Chinese = HttpUtility.HtmlDecode(chinese_name_match.Groups[1].Value).Trim();
            Match english_name_match = english_name_regex.Match(Source);
            if(english_name_match.Success) {
                opr.Name.English = HttpUtility.HtmlDecode(english_name_match.Groups[1].Value).Trim();
            }
            Match profile_match = profile_regex.Match(Source, Source.IndexOf("基本情報"));
            if(!profile_match.Success) {
                return null;
            }
            string profile_str = profile_match.Groups[1].Value;
            foreach(Match line_match in HtmlParser.TableLineRegex.Matches(profile_str)) {
                string[] column = HtmlParser.TagRegex.Replace(line_match.Groups[1].Value.Replace("</th>", " "), "").Replace("?", "").Trim().Split(" ");
                //Match profile_column_match = sirowiki_profile_column_regex.Match(line_match.Groups[1].Value);
                //if(!profile_column_match.Success) {
                //    continue;
                //}
                //string v = profile_column_match.Groups[2].Value.Trim();
                //switch(profile_column_match.Groups[1].Value.Trim()) {
                string v = column[^1];
                switch(column[0]) {
                    case "コードネーム":
                        opr.Name.Japanese = v;
                        break;
                    case "陣営":
                        opr.Camp.Japanese = v;
                        break;
                    case "出身":
                        opr.Birthplace.Japanese = v;
                        break;
                    case "産地":
                        opr.Birthplace.Japanese = v;
                        opr.Race.Japanese = "ロボット";
                        break;
                    case "種族":
                        opr.Race.Japanese = v;
                        break;
                    case "職業":
                        opr.Class.Japanese = v;
                        break;
                    case "職分":
                        opr.SubClass.Japanese = v;
                        break;
                    default:
                        break;
                }
            }

            int promotion_pos = Source.IndexOf(">昇進  ");
            if(promotion_pos != -1) {
                Match promotion_match = HtmlParser.TableBodyRegex.Match(Source, promotion_pos);
                if(promotion_match.Success) {
                    int promotion = -1;
                    foreach(Match p_cell_match in TableCellRegex.Matches(promotion_match.Value)) {
                        string cell_value = TagRegex.Replace(p_cell_match.Groups[1].Value, "").Trim();
                        if(cell_value.Equals("昇進1")) promotion = 0;
                        else if(cell_value.Equals("昇進2")) promotion = 1;
                        Match p_material_match = p_material_regex.Match(cell_value);
                        if(p_material_match.Success) {
                            string m_name = p_material_match.Groups[1].Value.Trim();
                            int m_qty = int.Parse(p_material_match.Groups[2].Value);
                            var material = material_dic.First(m => m.Value.Equals(m_name));
                            CMaterialSet m_set = new(material, m_qty);
                            opr.Promotion[promotion].Add(m_set);
                        }
                    }
                }
            }

            int skill_pos = Source.IndexOf(">スキルランク素材一覧  ");
            if(skill_pos != -1) {
                Match skill_match = HtmlParser.TableBodyRegex.Match(Source, skill_pos);
                if(skill_match.Success) {
                    CMaterialSet?[] skill_material = new CMaterialSet[9];
                    int common_cnt = -1;
                    int skill = -1;
                    foreach(Match skill_line_match in TableLineRegex.Matches(skill_match.Value)) {
                        if(skill_line_match.Value.Contains("Rank")) continue;
                        int i = -1;
                        foreach(Match skill_cell_match in TableCellRegex.Matches(skill_line_match.Value)) {
                            string cell_value = TagRegex.Replace(skill_cell_match.Groups[1].Value, "").Trim();
                            if(cell_value.Equals("共通")) {
                                SetMaterial(skill_material);
                                skill_material = new CMaterialSet[9];
                                common_cnt++;
                                continue;
                            } else if(cell_value.Equals("スキル1") || cell_value.Equals("スキル１")) {
                                SetMaterial(skill_material);
                                skill_material = new CMaterialSet[9];
                                skill = 0;
                                continue;
                            } else if(cell_value.Equals("スキル2") || cell_value.Equals("スキル２")) {
                                SetMaterial(skill_material);
                                skill_material = new CMaterialSet[9];
                                skill = 1;
                                continue;
                            } else if(cell_value.Equals("スキル3") || cell_value.Equals("スキル３")) {
                                SetMaterial(skill_material);
                                skill_material = new CMaterialSet[9];
                                skill = 2;
                                continue;
                            }
                            i++;
                            if(cell_value.Equals("-")) continue;
                            Match material_name_match = item_title_regex.Match(skill_cell_match.Groups[1].Value);
                            if(material_name_match.Success) {
                                CMaterialSet material = new();
                                Match material_img_match = material_img_regex.Match(material_name_match.Groups[1].Value);
                                if(material_img_match.Success) {
                                    int id = 0;
                                    if(material_img_match.Groups[2].Success) {
                                        id = int.Parse(material_img_match.Groups[2].Value);
                                    }
                                    switch(material_img_match.Groups[1].Value.Trim()) {
                                        case "rock":
                                            id += 30010;
                                            break;
                                        case "sugar":
                                            id += 30020;
                                            break;
                                        case "polyester":
                                            id += 30030;
                                            break;
                                        case "iron":
                                            id += 30040;
                                            break;
                                        case "aketon":
                                            id += 30050;
                                            break;
                                        case "gadget":
                                            id += 30060;
                                            break;
                                        case "alcohol":
                                            id += 30072;
                                            break;
                                        case "manganese":
                                            id += 30082;
                                            break;
                                        case "polishstone":
                                            id += 30092;
                                            break;
                                        case "rma":
                                            id += 30102;
                                            break;
                                        case "mtl":
                                            id = 30115;
                                            break;
                                        case "nanosheet":
                                            id = 30125;
                                            break;
                                        case "d32":
                                            id = 30135;
                                            break;
                                        case "pgel":
                                            id += 31010;
                                            break;
                                        case "iam":
                                            id += 31020;
                                            break;
                                        case "crystal":
                                            id += 31032;
                                            if(id == 31035) id = 30145;
                                            break;
                                        case "solvent":
                                            id += 31042;
                                            break;
                                        case "fluid":
                                            id += 31052;
                                            break;

                                        case "mod_unlock_token":
                                            id = 3000;
                                            break;

                                        case "skillbook":
                                            id += 10;
                                            break;
                                        default:
                                            continue;
                                    }

                                    material.SetMaterial(id, material_dic[id]);
                                } else {
                                    string material_name = material_name_match.Groups[1].Value.Trim().Replace(" ", "").Replace("III", "3").Replace("II", "2").Replace("I", "1");
                                    material_name = material_name.Replace("Ⅰ", "1").Replace("Ⅱ", "2").Replace("Ⅲ", "3");
                                    material.SetMaterial(material_dic.First(m => m.Value.Equals(material_name)));
                                }
                                skill_material[i] = material;
                            }
                            Match qty_match = skill_qty_regex.Match(cell_value);
                            if(qty_match.Success) {
                                var material = skill_material[i];
                                if(material != null) {
                                    material.Quantity = int.Parse(qty_match.Groups[1].Value);
                                }
                            }
                        }
                    }
                    SetMaterial(skill_material);

                    void SetMaterial(CMaterialSet?[] material) {
                        for(int j = 0; j < material.Length; j++) {
                            int level = j / 3;
                            var m = skill_material[j];
                            if(m == null) continue;
                            if(skill < 0) {
                                level += common_cnt * 3;
                                opr.Skill[level].Add(m);
                            } else {
                                opr.SkillSP[skill, level].Add(m);
                            }
                        }
                    }
                }
            }

            int mod_pos = Source.IndexOf(">モジュール  ");
            if(mod_pos != -1) {
                Match module_match = HtmlParser.TableRegex.Match(Source, mod_pos);
                if(module_match.Success) {
                    int module = -1;
                    foreach(Match p_cell_match in TableCellRegex.Matches(module_match.Value)) {
                        string cell_value = TagRegex.Replace(p_cell_match.Groups[1].Value, "").Trim();
                        if(cell_value.Contains("-X")) module = 0;
                        else if(cell_value.Contains("-Y")) module = 1;
                        foreach(Match m_material_match in module_material_regex.Matches(p_cell_match.Groups[1].Value)) {
                            string m_name = m_material_match.Groups[1].Value.Trim();
                            int m_qty = int.Parse(m_material_match.Groups[2].Value);
                            var material = material_dic.First(m => m.Value.Equals(m_name));
                            CMaterialSet m_set = new(material, m_qty);
                            opr.Module[module, 0].Add(m_set);
                        }
                    }
                }
            }
            return opr;
        }
    }
}