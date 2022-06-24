using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AOM_DataCreate.HtmlParser.SiroWiki {
    internal class Operator : HtmlParser {
        static readonly Regex chinese_name_regex = new(@"<p>中国語名(?:：|:)(.+?)[　\s]");
        static readonly Regex english_name_regex = new(@"英語名(?:：|:)(.+?)[　\s]");
        static readonly Regex russian_name_regex = new(@"ロシア語名(?:：|:)(.+?)[　\s]");
        static readonly Regex profile_regex = new(@"<table[^>]*?><tbody>(.+?プロフィール.+?)(?:</tbody></table>|$)", RegexOptions.Singleline);
        static readonly Regex p_material_regex = new(@"(.+)x(\d+)");
        public Operator(string source) : base(source) {
        }
        public Operator(Uri url) : base(url) {
        }

        public COperator? Parse(Dictionary<int, CNameSet> material_dic) {
            COperator opr = new();
            GetSource(5);
            if(Source == null) return null;

            Match chinese_name_match = chinese_name_regex.Match(Source);
            if(!chinese_name_match.Success) {
                return null;
            }
            opr.Name.Chinese = chinese_name_match.Groups[1].Value.Trim();
            Match english_name_match = english_name_regex.Match(Source);
            if(english_name_match.Success) {
                opr.Name.English = english_name_match.Groups[1].Value.Trim();
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

            Match promotion_match = HtmlParser.TableBodyRegex.Match(Source, Source.IndexOf("昇進  "));
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
            Match skill_match = HtmlParser.TableBodyRegex.Match(Source, Source.IndexOf("スキルランク素材一覧  "));
            if(skill_match.Success) {
            }
            return opr;
        }
    }
}