using System.Threading.Tasks;


namespace WebRequesters
{
    public interface IWebRequester
    { 
        Task<bool> SendRequest(string url, string data, int timeout);
    }
}
