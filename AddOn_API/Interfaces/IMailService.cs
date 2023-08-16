using AddOn_API.DTOs.MailService;

namespace AddOn_API.Interfaces
{
    public interface IMailService
    {
         bool SendMail(MailData mailData);
    }
}