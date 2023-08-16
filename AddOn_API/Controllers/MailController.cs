using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AddOn_API.DTOs.MailService;
using AddOn_API.Interfaces;
using Microsoft.AspNetCore.Mvc;
//using AddOn_API.Models;

namespace AddOn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService mailService;
        public MailController(IMailService mailService)
        {
            this.mailService = mailService;
        }

        [HttpGet("")]
        public  ActionResult<IEnumerable<MailData>> TestSendMail()
        {
            MailData _data = new MailData{
                EmailForm = "it.helpdesk@rofuth.com",
                EmailTo = "theerayut.l@rofuth.com,theerayut.l@rofuth.com",
                EmailCC = "",
                EmailSubject = "hello email",
                EmailBody = "detail inside email"
            };

            try{

                bool chkmail =  mailService.SendMail(_data);

                if (chkmail != true){
                    return StatusCode((int)HttpStatusCode.NotFound,_data);
                }

                return StatusCode((int)HttpStatusCode.OK,_data);
            }catch(Exception ex){
                return StatusCode((int)HttpStatusCode.NotFound,ex.ToString());
            }

            

             


        }

        [HttpPost("[action]")]
        public ActionResult<MailData> SendMailMessage(MailData model)
        {
            return null;
        }

    }
}