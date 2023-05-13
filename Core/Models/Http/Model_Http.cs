using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Http
{
    public class Model_Http
    {
        public async Task<string> SendRequest(string RequestURI)
        {
            try
            {
                HttpClient client = new HttpClient();

                List<Task> Tasks = new List<Task>();

                Task<HttpResponseMessage> HttpRequest = client.GetAsync(RequestURI);
                Tasks.Add(HttpRequest);

                HttpResponseMessage Response = HttpRequest.Result;
                Response.EnsureSuccessStatusCode();

                Task<string> DecodedResponse = Response.Content.ReadAsStringAsync();
                Tasks.Add(DecodedResponse);

                await Task.WhenAll(Tasks);

                return DecodedResponse.Result;
            }

            catch (Exception error)
            {
                throw new Exception("Ошибка отправки http запроса:\n\n" + error.Message + 
                    "\n\nУказанный URI:\n\n" + RequestURI);
            }
        }
    }
}
