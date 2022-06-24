using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace AOM_DataCreate.HtmlParser.PRTS {
    internal class OperatorList : HtmlParser {
        const string URL = @"https://prts.wiki/w/%E5%B9%B2%E5%91%98%E4%B8%80%E8%A7%88";

        static readonly Regex prts_list_regex = new(@"<div\s+(.*?class=""smwdata"".*?)>.*?</div>");
        static readonly Regex param_regex = new(@"([-\w]+)=""(.*?)""");

        public OperatorList() : base(new Uri(URL)) {
        }

        public OperatorList(string source) : base(source) {
        }
        public List<COperator> GetOperators(Dictionary<int, CNameSet> material_dic) {
            List<COperator> operators = new();
            GetSource(5);
            if(Source == null) return operators;

            foreach(Match match in prts_list_regex.Matches(Source)) {
                COperator @operator = new();
                foreach(Match param in param_regex.Matches(match.Groups[1].Value)) {
                    string value = HttpUtility.HtmlDecode(param.Groups[2].Value);
                    switch(param.Groups[1].Value) {
                        case "data-cn":
                            @operator.Name.Chinese = value;
                            break;
                        case "data-en":
                            @operator.Name.English = value;
                            break;
                        case "data-jp":
                            @operator.Name.Japanese = value;
                            if(value.Length == 0) @operator.IsGlobal = false;
                            else @operator.IsGlobal = true;
                            break;
                        case "data-race":
                            @operator.Race.Chinese = value;
                            break;
                        case "data-rarity":
                            @operator.Rarity = int.Parse(value) + 1;
                            break;
                        case "data-class":
                            @operator.Class.Chinese = value;
                            break;
                        case "data-camp":
                            @operator.Camp.Chinese = value;
                            break;
                        case "data-birthplace":
                            @operator.Birthplace.Chinese = value;
                            break;
                        case "data-nation":
                            @operator.Nation.Chinese = value;
                            break;
                        case "data-index":
                            @operator.ID = value;
                            break;
                        default:
                            break;
                    }
                }
                Console.WriteLine("{0}", @operator.Name);
                Operator operator_parser = new(new Uri(System.Web.HttpUtility.UrlPathEncode(@"https://prts.wiki/w/" + @operator.Name.Chinese)));
                operator_parser.Parse(@operator, material_dic);
                COperator? alter = operators.Find(o => o.Equals(@operator));
                if(alter == null) {
                    operators.Add(@operator);
                } else {
                    alter.AddAlter(@operator);
                    operators.Add(alter);
                }
            }

            return operators;
        }
    }
}

/*
 * <div
class="smwdata"
data-cn="酸糖"
data-position="远程位"
data-en="Aciddrop"
data-sex="女"
data-tag="输出"
data-race="黎博利"
data-rarity="3"
data-class="狙击"
data-approach="标准寻访"
data-camp="哥伦比亚"
data-team=""
data-des="罗德岛干员酸糖，自由的滑板斗士。"
data-feature="高精度的近距离射击"
data-str="标准"
data-flex="优良"
data-tolerance="标准"
data-plan="普通"
data-skill="标准"
data-adapt="标准"
data-moredes="绕脚一周，命中对手。"
data-icon="//prts.wiki/images/e/e9/%E5%A4%B4%E5%83%8F_%E9%85%B8%E7%B3%96.png"
data-half="//prts.wiki/images/thumb/8/82/%E5%8D%8A%E8%BA%AB%E5%83%8F_%E9%85%B8%E7%B3%96_1.png/110px-%E5%8D%8A%E8%BA%AB%E5%83%8F_%E9%85%B8%E7%B3%96_1.png"
data-ori-hp="671"
data-ori-atk="313"
data-ori-def="79"
data-ori-res="0"
data-ori-dt="70s"
data-ori-dc="14→16→18"
data-ori-block="1"
data-ori-cd="1.6s"
data-index="R139"
data-sort_id="154"
data-jp="アシッドドロップ"
data-birthplace="哥伦比亚"
data-nation="哥伦比亚"
data-group=""
*/