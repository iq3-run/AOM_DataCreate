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
 
        static readonly Regex sirowiki_list_regex = new(@"<a href=""([^""]+?)""[^>]*title=""([^""]+?)""[^>]*>\2</a>");

        public OperatorList() : base(new Uri(URL)) {
        }

        public OperatorList(string source) : base(source) {
        }

        public List<COperator> GetOperators() {
            List<COperator> operators = new();
            if(Source == null) {
                Wait();
                if(Source == null) {
                    throw new NullReferenceException("ソースの取得に失敗しました");
                }
            }

            foreach(Match match in sirowiki_list_regex.Matches(Source, Source.IndexOf("募集タグ"))) {
                Console.WriteLine(match.Groups[2].Value);
                Operator operator_parser = new(new Uri(match.Groups[1].Value.Trim()));
                COperator? opr = operator_parser.Parse();
                if(opr == null) continue;
                opr.Name.Japanese = match.Groups[2].Value.Trim();
                operators.Add(opr);
            }
            return operators;
        }
    }
}
