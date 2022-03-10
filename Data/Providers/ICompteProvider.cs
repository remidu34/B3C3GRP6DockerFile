using B3C3GRP6.Data.Models;
using Shyjus.BrowserDetection;

namespace B3C3GRP6.Data.Providers
{
    public interface ICompteProvider
    {
        Compte? GetAuthentificationByLogin(string login);
        Compte? GetAuthentificationByLoginAndPassword(string login, string password);
        Compte? GetCompteByid(int? id);
        void UpdateBrowser(string browser, int idCompte);
        void UpdateIp(string ip, int idCompte);
        string? GetDelayByLogin(string login);
        void UpdateIncrementDelay(int delay, string login);
    }
}
