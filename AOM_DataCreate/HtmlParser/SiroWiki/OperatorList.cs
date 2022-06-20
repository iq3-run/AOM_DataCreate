using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AOM_DataCreate.HtmlParser.SiroWiki {
    internal class OperatorList : HtmlParser {
        const string URL = @"https://arknights.wikiru.jp/index.php?%A5%AD%A5%E3%A5%E9%A5%AF%A5%BF%A1%BC%B0%EC%CD%F7";
 
        static Regex sirowiki_list_regex = new Regex(@"<a href=""([^""]+?)""[^>]*title=""([^""]+?)""[^>]*>\2</a>");
        static Regex sirowiki_chinese_name_regex = new Regex(@"<p>中国語名(?:：|:)(.+?)[　\s]");
        static Regex sirowiki_profile_regex = new Regex(@"<table[^>]*?><tbody>(.+?プロフィール.+?)(?:</tbody></table>|$)", RegexOptions.Singleline);
        static Regex sirowiki_profile_column_regex = new Regex(@"<th.*?>(.+)</th><td.*?>(?:<a.*?>)?([^<>]+)(?:</a.*?>)?</td>");

        public OperatorList() : base(new Uri(URL)) {
        }

        public OperatorList(string source) : base(source) {
        }

        public List<COperator> getOperators() {
            List<COperator> operators = new List<COperator>();
            if(Source == null) {
                Wait();
                if(Source == null) {
                    throw new NullReferenceException("ソースの取得に失敗しました");
                }
            }

            foreach(Match match in sirowiki_list_regex.Matches(Source, Source.IndexOf("募集タグ"))) {
                COperator opr = new COperator();
                Console.WriteLine(match.Groups[2].Value);
                Task<string?> sirowiki_chara = HttpClientWrapper.GetPage(new Uri(match.Groups[1].Value.Trim()));
                sirowiki_chara.Wait();
                string? sirowiki_chara_str = HttpUtility.HtmlDecode(sirowiki_chara.Result);
                if(sirowiki_chara_str == null) {
                    continue;
                }
                Match chinese_name_match = sirowiki_chinese_name_regex.Match(sirowiki_chara_str);
                if(!chinese_name_match.Success) {
                    continue;
                }
                string chinese_name = chinese_name_match.Groups[1].Value.Trim();
                opr.Name.Chinese = chinese_name;
                opr.Name.Japanese = match.Groups[2].Value.Trim();
                Match profile_match = sirowiki_profile_regex.Match(sirowiki_chara_str, sirowiki_chara_str.IndexOf("基本情報"));
                if(!profile_match.Success) {
                    continue;
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
                    string v = column[column.Length - 1];
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
                operators.Add(opr);
            }
            return operators;
        }
    }
}
