using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOM_DataCreate {
    internal static class HttpClientWrapper {

        static HttpClientWrapper() {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public static async Task<string?> GetPage(Uri uri) {
            using(HttpClient client = new HttpClient(new HttpClientHandler {
                AllowAutoRedirect = true,
            })) {
                client.DefaultRequestHeaders.Add("User-Agent",
                   "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
                client.DefaultRequestHeaders.Add("Accept-Language", "ja-JP");
                //client.Timeout = TimeSpan.FromSeconds(10.0);
                try {
                    return await client.GetStringAsync(uri);
                } catch(Exception e) {
                    Console.WriteLine("##########################");
                    Console.WriteLine("例外発生:{0}", e);
                    Console.WriteLine("##########################");
                }
                return null;
            }
        }
    }
}
