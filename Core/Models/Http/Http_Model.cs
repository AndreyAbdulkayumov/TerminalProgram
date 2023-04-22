using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models.Http
{
    internal class Http_Model
    {
        public async Task<string> SendRequest(string ResponseURI)
        {
            try
            {
                HttpClient client = new HttpClient();

                List<Task> Tasks = new List<Task>();

                Task<HttpResponseMessage> HttpRequest = client.GetAsync(ResponseURI);
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
                    "\n\nУказанный URI:\n\n" + ResponseURI);
            }
        }
    }
}
