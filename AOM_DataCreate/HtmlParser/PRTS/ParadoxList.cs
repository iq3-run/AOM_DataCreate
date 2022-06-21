using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AOM_DataCreate.HtmlParser.PRTS {
    internal class ParadoxList : HtmlParser {
        const string URL = @"https://prts.wiki/w/%E5%85%B3%E5%8D%A1%E4%B8%80%E8%A7%88";
        static readonly Regex prts_paradox_regex = new(@"<div .*?class=""PDXSIM_2"".*?><a .*?>(.+)</a></div>");
        public ParadoxList() : base(new Uri(URL)) {
        }

        public ParadoxList(string source) : base(source) {
        }

        public List<string> Parse() {
            List<string> list = new();
            if(Source == null) {
                Wait();
                if(Source == null) {
                    throw new NullReferenceException("ソースの取得に失敗しました");
                }
            }
            foreach(Match match in prts_paradox_regex.Matches(Source)) {
                list.Add(match.Groups[1].Value.Trim());
            }
            return list;
        }
    }
}
