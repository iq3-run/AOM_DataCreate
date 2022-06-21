using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AOM_DataCreate.HtmlParser.SiroWiki {
    internal class Operator : HtmlParser {
        static readonly Regex sirowiki_chinese_name_regex = new(@"<p>中国語名(?:：|:)(.+?)[　\s]");
        static readonly Regex sirowiki_profile_regex = new(@"<table[^>]*?><tbody>(.+?プロフィール.+?)(?:</tbody></table>|$)", RegexOptions.Singleline);
        public Operator(string source) : base(source) {
        }
        public Operator(Uri url) : base(url) {
        }

        public COperator? Parse() {
            COperator opr = new();
            if(Source == null) {
                Wait();
                if(Source == null) {
                    throw new NullReferenceException("ソースの取得に失敗しました");
                }
            }
            Match chinese_name_match = sirowiki_chinese_name_regex.Match(Source);
            if(!chinese_name_match.Success) {
                return null;
            }
            string chinese_name = chinese_name_match.Groups[1].Value.Trim();
            opr.Name.Chinese = chinese_name;
            Match profile_match = sirowiki_profile_regex.Match(Source, Source.IndexOf("基本情報"));
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
            return opr;
        }
    }
}