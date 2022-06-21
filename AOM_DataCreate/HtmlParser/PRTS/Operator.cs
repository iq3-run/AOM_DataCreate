using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AOM_DataCreate.HtmlParser.PRTS {
    internal class Operator : HtmlParser {

        static readonly Regex charname_regex = new(@"<div[^>]*id=""charname""[^>]*>(.+?)</div>");
        static readonly Regex charname_en_regex = new (@"<div[^>]*id=""charname-en""[^>]*>(.+?)</div>");
        static readonly Regex class_regex = new(@"<div[^>]*id=""charclasstxt""[^>]*>.*?<a[^>]*>(.+?)</a> -- <a[^>]*>(.+?)</a>.*?</div>", RegexOptions.Singleline);

        public Operator(string source) : base(source) {
        }
        public Operator(Uri url) : base(url) {
        }
        // https://prts.wiki/w/%E8%8A%AC

        public COperator? Parse() {
            COperator opr = new();
            if(Source == null) {
                Wait();
                if(Source == null) {
                    throw new NullReferenceException("ソースの取得に失敗しました");
                }
            }
            Match name_regex = charname_regex.Match(Source);
            opr.Name.Chinese = name_regex.Groups[1].Value.Trim();

            Match name_en_regex = charname_en_regex.Match(Source);
            opr.Name.English = name_en_regex.Groups[1].Value.Trim();

            Match class_match = class_regex.Match(Source);
            opr.Class.Chinese = class_match.Groups[1].Value.Trim();
            opr.SubClass.Chinese = class_match.Groups[2].Value.Trim();

            Regex material_regex = new(@"<a[^>]*title=""([^""]+)""[^>]*>.*?</a><span[^>]*>(\d+)(w?)</span>");
            foreach(Match material_match in material_regex.Matches(Source, Source.IndexOf(@"<span id=""精英化材料"">"))) {
                Console.WriteLine("{0} x{1}",material_match.Groups[1].Value, material_match.Groups[2]);
            }

            return opr;
        }
    }
}