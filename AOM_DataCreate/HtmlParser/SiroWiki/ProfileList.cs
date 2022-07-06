using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AOM_DataCreate.HtmlParser.SiroWiki {
    internal class ProfileList : HtmlParser {
        const string URL = @"https://arknights.wikiru.jp/index.php?%A5%ED%A5%C9%A5%B9%BF%CD%BB%F6%B2%DD%A5%D5%A5%A1%A5%A4%A5%EB";
        static readonly Regex anker_regex = new(@"<a[^>]*href=""(.+?)""[^>]*>");

        public ProfileList() : base(new Uri(URL)) {
        }

        public ProfileList(string source) : base(source) {
        }
        public List<COperator> GetOperators(Dictionary<int, CNameSet> material_dic, bool check_detail = true) {
            List<COperator> operators = new();
            GetSource(5);
            if(Source == null) return operators;

            foreach(Match match in TableBodyRegex.Matches(Source, Source.IndexOf("個人情報一覧"))) {
                foreach(Match line in TableLineRegex.Matches(match.Value)) {
                    COperator opr = new();
                    string[] profile_data = line.Groups[1].Value.Split(@"</td>");
                    for(int i = 0; i < profile_data.Length; i++) {
                        profile_data[i] = TagRegex.Replace(profile_data[i], "").Trim();
                    }
                    Console.WriteLine(profile_data[1]);
                    if(profile_data.Length == 21) {
                        opr.ID = profile_data[(int)ProfileColumn.図鑑コード];
                        opr.Name.Japanese = profile_data[(int)ProfileColumn.コードネーム];
                        opr.Race.Japanese = profile_data[(int)ProfileColumn.種族];
                        opr.Birthplace.Japanese = profile_data[(int)ProfileColumn.出身地];
                        operators.Add(opr);
                    } else if(profile_data.Length == 17) {
                        opr.ID = profile_data[(int)RobotColumn.図鑑コード];
                        opr.Name.Japanese = profile_data[(int)RobotColumn.コードネーム];
                        opr.Race.Japanese = "ロボット";
                        opr.Birthplace.Japanese = profile_data[(int)RobotColumn.産地];
                        operators.Add(opr);
                    }
                    if(check_detail) {
                        Match anker_match = anker_regex.Match(line.Groups[1].Value);
                        if(anker_match.Success) {
                            Uri operator_url = new(anker_match.Groups[1].Value.Trim());
                            Operator operator_perser = new(operator_url);
                            operator_perser.Parse(opr, material_dic);
                        }
                    }
                }
            }

            return operators;
        }

        public void MargeOperators(List<COperator> base_operators, Dictionary<int, CNameSet> material_dic) {
            List<COperator> add_operators = GetOperators(material_dic);
            foreach(COperator opr_base in base_operators) {
                COperator? opr_add = add_operators.Find(oa => oa.Equals(opr_base));
                if(opr_add != null) {
                    opr_base.ID = opr_add.ID;
                    opr_base.TryMerge(opr_add);
                    opr_base.Name.Japanese = opr_add.Name.Japanese;
                }
            }
        }

        enum ProfileColumn {
            画像,
            コードネーム,
            中文表記,
            性別,
            誕生日,
            身長,
            戦闘経験,
            源石融合率,
            血液中源石密度,
            出身地,
            種族,
            背景ロゴ_所属,
            図鑑コード,
            物理強度,
            戦場機動,
            生理的耐性,
            戦術立案,
            戦闘技術,
            アーツ適性,
            追加
        }
        enum RobotColumn {
            画像,
            コードネーム,
            性別設定,
            使用年月,
            製造元,
            産地,
            出荷日,
            高さ,
            重量,
            最高速度,
            図鑑コード,
            登坂能力,
            制動効率,
            走行性,
            航続力,
            構造安定性
        }
    }
}
