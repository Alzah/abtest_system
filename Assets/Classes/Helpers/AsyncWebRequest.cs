using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

namespace Classes.Helpers
{
    /// <summary>
    /// Асинхронный загрузчик данных
    /// </summary>
    public static class AsyncWebRequest
    {
        public static async Task<string> Download(string url, int timeout = 0)
        {
            var response = await DownloadGetAsync(url, timeout);
            switch (response.Status)
            {
                case 200:
                    //Debug.Log("[AsyncWebRequest] Download success");
                    break;
                default:
                    Debug.Log($"[AsyncWebRequest] Download failed with status: {response.Status}");
                    break;
            }

            return response.Data;
        }

        private static async Task<WebResponse> DownloadGetAsync(string url, int timeout = 0)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.certificateHandler = AcceptAllCertificatesHandler.Instance;
                request.timeout = timeout;
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return new WebResponse(request.downloadHandler.text, (int) request.responseCode);
                }
                else
                {
                    return new WebResponse(string.Empty, (int) request.responseCode);
                }
            }
        }

    }

    public class WebResponse
    {
        public string Data;
        public int Status;

        public WebResponse(string data, int status)
        {
            Data = data;
            Status = status;
        }
    }
}
