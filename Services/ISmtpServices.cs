using B3C3GRP6.API.Models;

namespace B3C3GRP6.Services
{
    public interface ISmtpServices
    {
        Task SendEmailForEmailConfirmation(EmailModel emailOptionsModel);
    }
}
