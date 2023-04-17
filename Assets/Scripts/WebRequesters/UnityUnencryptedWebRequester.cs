using System.Threading.Tasks;
using UnityEngine.Networking;


namespace WebRequesters
{
    public class UnityUnencryptedWebRequester : IWebRequester
    {
        public async Task<bool> SendRequest(string url, string data, int timeout = 10)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(url, data))
            {
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                www.timeout = timeout;

                www.SendWebRequest().completed += _ =>
                {
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        tcs.SetResult(true);
                    }
                    else
                    {
                        tcs.SetResult(false);
                    }
                };

                return await tcs.Task;
            }
        }
    }
}
