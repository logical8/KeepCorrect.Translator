using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace KeepCorrect.Translator
{
    public class Search
    {
        public static async Task<SearchResult> GetSearchResult(string word)
        {
            var content = new
            {
                word = word,
                ajax_action = "ajax_balloon_Show",
                piece_index = 0,
                external = 1,
                partner_id = 2676311
            };

            try
            {
                var @string = await "https://puzzle-english.com" // shortcut for Request().AppendPathSegments(...)
                    .SetQueryParam("word", word)
                    .SetQueryParam("ajax_action", "ajax_balloon_Show")
                    .GetStringAsync();

                var result = JsonConvert.DeserializeObject<SearchResult>(@string,
                    new JsonSerializerSettings
                    {
                        Error = (se, ev) => { ev.ErrorContext.Handled = true; }
                    });
                return result;
            
                var t = await "https://puzzle-english.com" // shortcut for Request().AppendPathSegments(...)
                    .SetQueryParam("word", word)
                    .SetQueryParam("ajax_action", "ajax_balloon_Show")
                    .GetJsonAsync<SearchResult>();
                return t;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
           
            var task1 = "https://puzzle-english.com".PostJsonAsync(content).ReceiveString().Result;
            //return result.ReceiveJson<SearchResult>();
            //var task2 = await result.GetStringAsync();
            //var result = Task.WaitAll(task1.Ru);
            return new SearchResult();

            /*return await "https://puzzle-english.com"
                .SetQueryParam("word", word)
                .SetQueryParam("ajax_action", "ajax_balloon_Show")
                .SetQueryParam("piece_index", 0)
                .SetQueryParam("external", 1)
                //.SetQueryParam("partner_id", 2676311)
                .PostAsync();*/
        }

        public static List<Dictionary<string, object>> ParseGlobalSearchResult(string data)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(data);

            var result = new List<Dictionary<string, object>>();

            var divs = doc.DocumentNode.Descendants("div")
                .Where(div => div.GetAttributeValue("data-target", "") != "")
                .ToList();

            foreach (var div in divs)
            {
                var word = div.GetAttributeValue("data-word", "");
                var translation = div.GetAttributeValue("data-translation", "");
                var saved = div.SelectSingleNode(".//span[@class='global_search_add_word']") == null;

                var item = new Dictionary<string, object>
                {
                    { "word", word },
                    { "translation", translation },
                    { "saved", saved }
                };

                result.Add(item);
            }

            return result;
        }
    }
}