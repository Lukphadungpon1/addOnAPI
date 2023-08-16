using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddOn_API.DTOs.Account;
using AddOn_API.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
//using AddOn_API.Models;

namespace AddOn_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
  
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        public AccountController(IAccountService accountService)
        {
           
            this.accountService = accountService;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAll()
        {
            var _accountre = (await accountService.FindAll());
            var _result = new List<AccountResponse>();
            _result = _accountre.Adapt<List<AccountResponse>>();
           
            if (_result == null){
                return StatusCode((int)HttpStatusCode.NotFound,_result);
            }
            else{
                return StatusCode((int)HttpStatusCode.OK,_result);
            }
            
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<AccountResponse>> Login(LoginRequest model)
        {

            var _account = (await accountService.Login(model));

            
        
             if (_account == null){
                return StatusCode((int)HttpStatusCode.Unauthorized,"Username or Password is wrong.");
            }
            else{

                return StatusCode((int)HttpStatusCode.OK,new { token = accountService.GenerateToken(_account)});
            }

            
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> info()
        {
            // TODO: Your code here
            var accessToken = await HttpContext.GetTokenAsync("access_token");
           if (accessToken == null){
            return Unauthorized();
           }
            var account = accountService.GetInfo(accessToken);
            var responser = account.Adapt<AccountResponse>();
            responser.EmpCode = string.IsNullOrEmpty(responser.EmpCode) ? "0" : responser.EmpCode;
            responser.EmpNameTh =  string.IsNullOrEmpty(responser.EmpNameTh) ? "NameTH" : responser.EmpNameTh;
            responser.EmpSex = string.IsNullOrEmpty(responser.EmpSex) ? "Sex" : responser.EmpSex;

            responser.RoleName = string.IsNullOrEmpty(responser.RoleName) ? "User" : responser.RoleName;
            return Ok(responser);
        }
        
        

    
    }
}