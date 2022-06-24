using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AOM_DataCreate.HtmlParser {
    internal abstract class HtmlParser {
        public static readonly Regex TableBodyRegex = new(@"<tbody>(.+?)</tbody>", RegexOptions.Singleline);
        public static readonly Regex TableLineRegex = new(@"<tr>(.+?)</tr>", RegexOptions.Singleline);
        public static readonly Regex TableCellRegex = new(@"<(?:td|th)[^>]*?>(.+?)</(?:td|th)>", RegexOptions.Singleline);
        public static readonly Regex TagRegex = new(@"<[^>]+?>", RegexOptions.Singleline);
        public string? Source { get; set; }
        private Task<string?>? task;
        private Uri? uri;
        public HtmlParser(string source) {
            Source = source;
        }

        public HtmlParser(Uri uri) {
            this.uri = uri;
            task = HttpClientWrapper.GetPage(uri);
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

        public bool GetSource(int retry_count = 0) {
            if(Source != null) {
                return true;
            }
            Wait();
            if(Source != null) {
                return true;
            }
            if(retry_count <= 0) {
                return false;
            }
            if(ReTry()) {
                return GetSource(retry_count - 1);
            } else return false;
        }
        public bool ReTry(Uri uri) {
            this.uri = uri;
            task = HttpClientWrapper.GetPage(uri);
            return true;
        }
        public bool ReTry() {
            if(uri != null) {
                task = HttpClientWrapper.GetPage(uri);
                return true;
            }
            return false;
        }
    }
}
