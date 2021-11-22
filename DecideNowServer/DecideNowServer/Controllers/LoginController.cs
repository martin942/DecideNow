using DecideNowServer.Exceptions;
using DecideNowServer.Models;
using DecideNowServer.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer.Controllers
{
    [Controller]
    [Route("login")]
    public class LoginController : ControllerBase
    {

        // get client ip address
        // HttpContext.Connection.RemoteIpAddress.ToString();


        [HttpGet]
        [Route("{email}")]
        public async Task<IActionResult> GetChallenge(string email)
        {
            LoginService loginService = LoginService.GetInstance();
            try
            {
                string challenge = await loginService.GetChallenge(email);
                return StatusCode(200, challenge);
            }
            catch (InternalServerException ex)
            {
                return StatusCode(500, new ErrorModel(ex.Message, 500));
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostToken(LoginModel loginModel)
        {
            LoginService loginservice = LoginService.GetInstance();
            try
            {
                string token = await loginservice.GetToken(loginModel.challenge, HttpContext.Connection.RemoteIpAddress.ToString());
                return StatusCode(200, token);
            }
            catch (BadRequestException ex)
            {
                return StatusCode(400, new ErrorModel(ex.Message, 400));
            }

        }
    }
}
