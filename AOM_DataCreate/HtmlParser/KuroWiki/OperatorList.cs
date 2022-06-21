using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AOM_DataCreate.HtmlParser.KuroWiki {
    internal class OperatorList : HtmlParser {
        const string URL = @"https://wiki3.jp/arknightsjp/page/14";

        static readonly Regex kurowiki_list_regex = new(@"<a\s+.*?href=""(.+)"".*?>☆(\d)\s+(.+)</a>");

        public OperatorList() : base(new Uri(URL)) {
        }

        public OperatorList(string source) : base(source) {
        }

        public List<COperator> GetOperators(Dictionary<string, int> material_dic, List<string> paradox_list) {
            List<COperator> operators = new();
            if(Source == null) {
                Wait();
                if(Source == null) {
                    throw new NullReferenceException("ソースの取得に失敗しました");
                }
            }


            foreach(Match match in kurowiki_list_regex.Matches(HttpUtility.HtmlDecode(Source))) {
                Console.WriteLine(match.Groups[3].Value);
                Operator operator_parser = new(new Uri(match.Groups[1].Value.Trim()));
                COperator? opr = operator_parser.Parse(material_dic, paradox_list);
                if(opr == null) continue;
                if(!operators.Contains(opr)) operators.Add(opr);
            }
            return operators;
        }

        public List<COperator> MargeOperator(List<COperator> base_operators, Dictionary<string, int> material_dic, List<string> paradox_list) {
            List<COperator> add_operators = GetOperators(material_dic, paradox_list);
            List<COperator> new_list = new();
            foreach(var opr_add in add_operators) {
                COperator? opr_base = base_operators.Find(o => o.Equals(opr_add));
                if(opr_base == null) {
                    continue;
                }
                opr_add.TryMerge(opr_base);
                new_list.Add(opr_add);
            }
            return new_list;
        }
    }
}

