using B3C3GRP6.API.Models;
using B3C3GRP6.Data.Models;
using B3C3GRP6.Data.Providers;
using B3C3GRP6.Helper;
using B3C3GRP6.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Shyjus.BrowserDetection;
using System.DirectoryServices;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace B3C3GRP6.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthentificationController : ControllerBase
    {
        #region Constantes
        public const string InvalidCredential = "Invalid credentials";
        #endregion

        #region Attributs
        private readonly IConfiguration _configuration;
        private readonly ICompteProvider _compteProvider;
        private readonly ISmtpServices _smtpServices;
        private readonly IBrowserDetector _browserDetector;
        #endregion

        #region Constructeur
        public AuthentificationController(IConfiguration configuration, CompteProvider compteProvider, ISmtpServices smtpServices, IBrowserDetector browserDetector)
        {
            _configuration = configuration;
            _compteProvider = compteProvider;
            _smtpServices = smtpServices;
            _browserDetector = browserDetector;
        }
        #endregion

        /// <summary>
        /// Login and send OTP by mail
        /// </summary>
        /// <param name="authenticateModel"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] AuthenticateModel authenticateModel)
        {
            try
            {
                if (authenticateModel.Login != null && authenticateModel.Password != null)
                {
                    /*var resultBool = VerifyAccesAd(authenticateModel.Login, authenticateModel.Password);
                    if (!resultBool)
                        return Unauthorized("error ad");*/

                    _compteProvider.InsertUserInDb(authenticateModel.Login, authenticateModel.Password);
                    // incremental delay to prevent brute force attacks
                    var delayForUser = GetDelay(authenticateModel.Login);

                    if (string.IsNullOrEmpty(delayForUser))
                        return Unauthorized("error delay");

                    int incrementalDelay = int.Parse(delayForUser);
                    await Task.Delay(incrementalDelay * 1000);
                    // Verify user
                    var user = await GetAuthentication(authenticateModel.Login, authenticateModel.Password);
                    if (user != null)
                    {
                        // Verify browser
                        var currentBrowser = _browserDetector.Browser;
                        var result = GetLastBrowser(user.IdCompte, currentBrowser.Name);

                        // Récupérer le nom de l'hôte
                        string host = Dns.GetHostName();
                        // Récupérer l'adresse IP
                        string ip = Dns.GetHostEntry(host).AddressList[0].ToString();
                        // Verify Ip
                        var resultIp = GetLastIp(user.IdCompte, ip);

                        if (result == true && resultIp == true)
                        {
                            // is logged
                            _compteProvider.UpdateIncrementDelay(1, authenticateModel.Login);
                            return Ok(GenerateToken(user, true));
                        }
                        // TODO : Transaction SQL
                        _compteProvider.UpdateBrowser(currentBrowser.Name, user.IdCompte);
                        _compteProvider.UpdateIp(ip, user.IdCompte);
                        _compteProvider.UpdateIncrementDelay(1, authenticateModel.Login);

                        //SendEmailSuspect(user.Login);
                        return Ok(GenerateToken(user, false, false, true));
                    }
                    else
                    { 
                        _compteProvider.UpdateIncrementDelay(incrementalDelay * 2, authenticateModel.Login);
                        return Unauthorized(InvalidCredential);
                    }
                }
                else
                {
                    return Unauthorized("login pwd empty");
                }
            }catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Unauthorized("test catch");
            }
        }

        [Authorize(Policy = "RequireMailOnly")]
        [HttpPost("Mail")]
        public async Task<ActionResult> GetMail([FromBody] string email)
        {
            try
            {
                if (!string.IsNullOrEmpty(email))
                {
                    string id = AuthentificationHelper.GetIdInClaims(base.User);
                    int idCompte = Convert.ToInt16(id);
                    Compte compte = new Compte
                    {
                        IdCompte = idCompte
                    };
                    await SendMail(email);
                    return Ok(GenerateToken(compte, false, true));
                }
                return BadRequest();
            }
            catch(Exception ex) 
            {
                Console.Error.WriteLine(ex);
                return Unauthorized();
            }
        }

        [Authorize(Policy = "RequireOtpOnly")]
        [HttpPost("OTP")]
        public async Task<ActionResult> SendMailVerification([FromBody] int? code)
        {
            try
            {
                bool checkOtp = false;
                if (code != null)
                {
                    string id = AuthentificationHelper.GetIdInClaims(base.User);
                    int idCompte = Convert.ToInt16(id);
                    Compte compte = new Compte
                    {
                        IdCompte = idCompte
                    };

                    var compteWithEmail = GetEmailWithId(idCompte);

                    if (compteWithEmail != null)
                    {
                        string? email = compteWithEmail.Login;
                        checkOtp = OtpHelper.GetInstance().VerifyOtp(code, email);
                        if (checkOtp == true)
                        {
                            return Ok(GenerateToken(compte, true));

                        }
                    }
                    return NotFound();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return Unauthorized();
            }
        }

        #region METHODS - PRIVATE
        /// <summary>
        /// Send email with OTP
        /// </summary>
        /// <param name="email"></param>
        private async Task SendMail(string email)
        {
            try
            {
                // Generate OTP
                string otp = OtpHelper.GetInstance().GenerateOtp(email);
                EmailModel options = new EmailModel
                {
                    ToEmails = email,
                    PlaceHolders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("{{email}}", email),
                        new KeyValuePair<string, string>("{{code}}", otp)
                }
                };

                await _smtpServices.SendEmailForEmailConfirmation(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
        /// <summary>
        /// Send email if ip or browser are update
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task SendEmailSuspect(string email)
        {
            try
            {
                EmailModel options = new EmailModel
                {
                    ToEmails = email,
                    PlaceHolders = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("{{email}}", email)
                }
                };
                await _smtpServices.SendEmailForEmailConfirmation(options);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
        private string GenerateToken(Compte compte, bool isLog, bool canEnterOtp = false, bool canEnterMail = false)
        {
            var claims = new[]
            {
                // created claims for the customer for a unique identifier associated to the JWT
                new Claim(JwtRegisteredClaimNames.Sub,_configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),
                new Claim("Id", compte.IdCompte.ToString()),
                new Claim("IsLogged", isLog.ToString(), "bool"),
                new Claim("canEnterOtp", canEnterOtp.ToString(), "bool"),
                new Claim("canEnterMail", canEnterMail.ToString(), "bool"),
            };

            // keys generated using symmetric algorithms
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Issuer"]));

            // encryption key and generate a digital signature
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT security token and encode it.
            var token = new JwtSecurityToken(
                 _configuration["Jwt:Issuer"],
                 _configuration["Jwt:Audience"],
                 claims,
                 expires: DateTime.Now.AddMinutes(20),
                 signingCredentials: signIn);

            // Generation token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool VerifyAccesAd(string login, string password)
        {
            try

            {
                string path = _configuration.GetSection("ConnectionActiveDirectory").GetValue<string>("Url");
                DirectoryEntry Ldap = new DirectoryEntry(path, login, password);
                DirectorySearcher searcher = new DirectorySearcher(Ldap);
                searcher.Filter = "(objectClass=user)";

                foreach (SearchResult result in searcher.FindAll())
                {
                    DirectoryEntry DirEntry = result.GetDirectoryEntry();

                }
                return true;


            }

            catch (Exception Ex)

            {
                Console.WriteLine(Ex.Message);
                return false;
            }
        }

        private string GetDelay(string login)
        {
            if (!string.IsNullOrEmpty(login))
            {
                string delay = _compteProvider.GetDelayByLogin(login);
                return delay;
            }
            return string.Empty;
        }
        private Task<Compte> GetAuthentication(string login, string password)
        {
            if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password))
            {
                var pwd = _compteProvider.GetAuthentificationByLogin(login)?.Password;

                if (pwd == null)
                    throw new Exception(InvalidCredential);

                //Check login and password in database
                var testLogin = _compteProvider.GetAuthentificationByLoginAndPassword(login, password);

                return Task.FromResult(testLogin);
            }

            throw new Exception("Login or password is empty");
        }

        private Compte GetEmailWithId(int? id)
        {
            if (id != null)
            {

                var compte = _compteProvider.GetCompteByid(id);

                return compte;
            }

            throw new Exception("Error with id");
        }

        private bool GetLastBrowser(int id, string? currentBrowser)
        {
            try
            {
                var lastBrowser = _compteProvider.GetCompteByid(id);
                if (lastBrowser.BrowserName == currentBrowser)
                {
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {           
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private bool GetLastIp(int id, string? currentIp)
        {
            try
            {
                var lastIp = _compteProvider.GetCompteByid(id);
                if(lastIp.IpPublic == currentIp)
                {
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        #endregion
    }
}
