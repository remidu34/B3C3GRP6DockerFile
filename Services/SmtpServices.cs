using B3C3GRP6.API.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace B3C3GRP6.Services
{
    public class SmtpServices : ISmtpServices, IDisposable
    {
        private readonly string _mailFrom;
        private readonly string _smtpLogin;
        private readonly string _smtpPassword;
        private readonly int _port;
        private readonly string _host;
        private readonly bool _enableSsl;
        private readonly SmtpClient _client;

        #region Constructor
        public SmtpServices(IConfiguration configuration)
        {
            _smtpLogin = configuration["SmtpConfiguration:smtpLogin"];
            _smtpPassword = configuration["SmtpConfiguration:smtpPassword"];
            _mailFrom = configuration["SmtpConfiguration:mailFrom"];
            _port = configuration.GetSection("SmtpConfiguration").GetValue<int?>("smtpPort") ?? 25;
            _host = configuration["SmtpConfiguration:smtpHost"];
            _enableSsl = configuration.GetSection("SmtpConfiguration").GetValue<bool?>("smtpEnableSSL") ?? false;
            _client = new SmtpClient()
            {
                Host = _host,
                Port = _port,
                EnableSsl = _enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = string.IsNullOrWhiteSpace(_smtpLogin),
                Credentials = string.IsNullOrWhiteSpace(_smtpLogin) ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(_smtpLogin, _smtpPassword)
            };

        }
        #endregion

        #region METHODS - PUBLIC
        public async Task SendEmailForEmailConfirmation(EmailModel emailModel)
        {
            emailModel.Subject = UpdatePlaceHolders("Demande de confirmation de connexion", emailModel.PlaceHolders);

            emailModel.Body = UpdatePlaceHolders(GetEmailBody("EmailConfirm"), emailModel.PlaceHolders);

            await SendEmail(emailModel);
        }
        private string UpdatePlaceHolders(string text, List<KeyValuePair<string, string>> keyValuePairs)
        {
            if (!string.IsNullOrEmpty(text) && keyValuePairs != null)
            {
                foreach (var placeholder in keyValuePairs)
                {
                    if (text.Contains(placeholder.Key))
                    {
                        text = text.Replace(placeholder.Key, placeholder.Value);
                    }
                }
            }

            return text;
        }
        private string GetEmailBody(string templateName)
        {
            var body = File.ReadAllText(Path.Combine("EmailTemplate", $"{templateName}.html"));
            return body;
        }
        private async Task SendEmail(EmailModel emailOptionsModel)
        {

            MailMessage mail = new MailMessage(
               _mailFrom,
               emailOptionsModel.ToEmails,
               emailOptionsModel.Subject,
               emailOptionsModel.Body);

            mail.IsBodyHtml = true;
            mail.BodyEncoding = Encoding.Default;

            await _client.SendMailAsync(mail);
        }
        #endregion
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
