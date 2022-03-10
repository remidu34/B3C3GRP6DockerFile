using B3C3GRP6.Data.Models;
using Microsoft.EntityFrameworkCore;
using Shyjus.BrowserDetection;

namespace B3C3GRP6.Data.Providers
{
    public class CompteProvider : ICompteProvider
    {
        #region FIELDS
        private readonly B3C3GRP6Context _context;
        #endregion

        #region Constructor
        public CompteProvider(B3C3GRP6Context b3C3GRP6Context)
        {
            _context = b3C3GRP6Context;
        }
        #endregion

        #region Authentification

        public Compte? GetAuthentificationByLogin(string login)
        {
            return _context.Comptes.Local.SingleOrDefault(a => a.Login == login) ??
                _context.Comptes.SingleOrDefault(a => a.Login == login);
        }

        public Compte? GetAuthentificationByLoginAndPassword(string login, string password)
        {
            return _context.Comptes.Local.SingleOrDefault(a => a.Login == login && a.Password == password) ??
                    _context.Comptes.SingleOrDefault(a => a.Login == login && a.Password == password);
        }

        public Compte GetCompteByid(int? id)
        {
            return _context.Comptes.SingleOrDefault(a => a.IdCompte.Equals(id));
        }

        public void UpdateBrowser(string browser, int idCompte)
        {
            var compte = _context.Comptes.FirstOrDefault(x => x.IdCompte.Equals(idCompte));
            if (compte == null)
                Console.WriteLine("Compte not found");

            compte.BrowserName = browser;
            _context.SaveChanges();

        }
        public void UpdateIp(string ip, int idCompte)
        {
            var compte = _context.Comptes.FirstOrDefault(x => x.IdCompte.Equals(idCompte));
            if (compte == null)
                Console.WriteLine("Compte not found");

            compte.IpPublic = ip;
            _context.SaveChanges();

        }
        public string? GetDelayByLogin(string login)
        {
            if (!string.IsNullOrEmpty(login))
            {
                var compte = _context.Comptes.FirstOrDefault(x => x.Login.Equals(login));
                if (compte == null)
                    return null;

                string delay = compte.IncrementDelay;
                return delay;
            }
            return null;
        }
        public void UpdateIncrementDelay(int delay, string login)
        {
            string updateIncrementDelay = delay.ToString();
            var compte = _context.Comptes.FirstOrDefault(x => x.Login.Equals(login));
            if (compte == null)
                Console.WriteLine("Compte not found");

            compte.IncrementDelay = updateIncrementDelay;
            _context.SaveChanges();
        }

        public void InsertUserInDb(string email, string password)
        {
            Compte compte = new Compte();
            compte.Login = email;
            compte.Password = password;
            compte.IncrementDelay = "1";
            
            _context.Comptes.Add(compte);
            _context.SaveChanges();

        }
        #endregion
    }
}
