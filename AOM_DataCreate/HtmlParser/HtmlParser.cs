using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AOM_DataCreate.HtmlParser {
    internal abstract class HtmlParser {
        public static readonly Regex TableBodyRegex = new(@"<tbody>(.+?)</tbody>");
        public static readonly Regex TableLineRegex = new(@"<tr>(.+?)</tr>");
        public static readonly Regex TagRegex = new(@"<.+?>");
        public string? Source { get; set; }
        private Task<string?>? task;

        public HtmlParser(string source) {
            Source = source;
        }

        public HtmlParser(Uri url) {
            task = HttpClientWrapper.GetPage(url);
        }

        public void Wait() {
            if(task != null) {
                task.Wait();
                string? s = task.Result;
                if(s != null) {
                    Source = s;
                }
            }
        }

        public void ReTry(Uri uri) {
            task = HttpClientWrapper.GetPage(uri);
        }
    }
}
