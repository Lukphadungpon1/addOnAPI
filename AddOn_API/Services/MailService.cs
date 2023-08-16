using AddOn_API.DTOs.MailService;
using AddOn_API.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AddOn_API.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
    public MailService(IOptions<MailSettings> mailSettingsOptions)
    {
        _mailSettings = mailSettingsOptions.Value;
    }

        public bool SendMail(MailData mailData)
        {
            try
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {

                List<MailboxAddress> emailToList = new List<MailboxAddress>();
                List<MailboxAddress> emailCcList = new List<MailboxAddress>();

                MailboxAddress emailFrom = new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail);

                if (!String.IsNullOrEmpty(mailData.EmailForm) ){

                    string[] spmailfrom = mailData.EmailForm.Split("@");

                     MailboxAddress emailFromFix = new MailboxAddress(spmailfrom[0], mailData.EmailForm);

                    emailMessage.From.Add(emailFromFix);
                }                
                else{
                     emailMessage.From.Add(emailFrom);
                }


                //// email to
                if (mailData.EmailTo.Contains(",")){
                    string[] _listto = mailData.EmailTo.Split(",");

                    foreach(string item in _listto){
                        if (!string.IsNullOrEmpty(item)){
                            string[] spmailto = item.Split("@");
                            MailboxAddress _emailto = new MailboxAddress(spmailto[0], item);
                            emailToList.Add(_emailto);
                        }
                    }
                }else{
                    if (!string.IsNullOrEmpty(mailData.EmailTo)){
                        string[] spmailto = mailData.EmailTo.Split("@");
                        MailboxAddress _emailto = new MailboxAddress(spmailto[0], mailData.EmailTo);
                        emailToList.Add(_emailto);
                    }
                   
                }



                 //// email cc
                if (mailData.EmailCC.Contains(",")){
                    string[] _listcc = mailData.EmailCC.Split(",");

                    foreach(string item in _listcc){
                        if (!string.IsNullOrEmpty(item)){
                            string[] spmailcc = item.Split("@");
                            MailboxAddress _emailcc = new MailboxAddress(spmailcc[0], item);
                            emailCcList.Add(_emailcc);
                        }
                    }
                }else{
                    if (!string.IsNullOrEmpty(mailData.EmailCC)){
                        string[] spmailcc = mailData.EmailCC.Split("@");
                        MailboxAddress _emailcc = new MailboxAddress(spmailcc[0], mailData.EmailCC);
                        emailCcList.Add(_emailcc);
                    }
                    
                }


               

               
              //  MailboxAddress emailTo = new MailboxAddress(mailData.EmailToName, mailData.EmailTo);

                //emailMessage.To.Add(emailTo);
                emailMessage.To.AddRange(emailToList);

                //emailMessage.Cc.Add(new MailboxAddress("Cc Receiver", "theerayut.l@rofuth.com"));
                emailMessage.Cc.AddRange(emailCcList);

               
                //emailMessage.Bcc.Add(new MailboxAddress("Bcc Receiver", "bcc@example.com"));

                emailMessage.Subject = "[ADD ON System.] : " + mailData.EmailSubject;

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = mailData.EmailBody;
               // emailBodyBuilder.TextBody = mailData.EmailBody;

                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                //this is the SmtpClient from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                using (SmtpClient mailClient = new SmtpClient())
                {

                    mailClient.Connect(_mailSettings.Server, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    mailClient.Authenticate(_mailSettings.UserName, _mailSettings.Password);
                    mailClient.Send(emailMessage);
                    mailClient.Disconnect(true);

                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // Exception Details
            return false;
        }
        }
    }
}

