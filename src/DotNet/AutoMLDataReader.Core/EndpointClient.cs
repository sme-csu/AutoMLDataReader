using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AutoMLDataReader.Core
{
    public class EndpointClient
    {
        public string Url { get; private set; }
        public string APIKey { get; set; }
        public EndpointClient(string url)
        {
            Url = url;
        }
        public async Task<string> InvokeRequestResponseService(IEnumerable<Dictionary<string,string>> dataItems)
        {
            var handler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
            };
            using (var client = new HttpClient(handler))
            {
                var scoreRequest = new Dictionary<string, List<Dictionary<string, string>>>()
                {
                    {
                        "data",
                        dataItems.ToList()
                    },
                };

                if (!string.IsNullOrWhiteSpace(APIKey))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", APIKey);
                }
                
                client.BaseAddress = new Uri(Url);

                // WARNING: The 'await' statement below can result in a deadlock
                // if you are calling this code from the UI thread of an ASP.Net application.
                // One way to address this would be to call ConfigureAwait(false)
                // so that the execution does not attempt to resume on the original context.
                // For instance, replace code such as:
                //      result = await DoSomeTask()
                // with the following:
                //      result = await DoSomeTask().ConfigureAwait(false)

                var requestString = JsonConvert.SerializeObject(scoreRequest);
                var content = new StringContent(requestString);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    //Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    //Console.WriteLine(response.Headers.ToString());

                    return await response.Content.ReadAsStringAsync();
                    //Console.WriteLine(responseContent);
                }
            }
        }

        public static async Task<IEnumerable<string>>  ParseSwaggerAsync(string swaggerUrl)
        {
            HttpClient client = new HttpClient();
            var content=await client.GetStringAsync(swaggerUrl);
            JObject root = JObject.Parse(content);
            var t=root.SelectToken("$.definitions.ServiceInput..items.properties");
            List<string> result = new List<string>();
            foreach (var item in t)
            {
                result.Add(((JProperty)item).Name);
            }
            return result;
        }
    }
}
