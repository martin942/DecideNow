using DecideNowServer.DB;
using DecideNowServer.Exceptions;
using DecideNowServer.Models;
using DecideNowServer.Security;
using DecideNowServer.service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DecideNowServer.Controllers
{
    [Controller]
    [Route("register")]
    public class RegisterController : ControllerBase
    {

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(RSA.GetPublicKey());
        }

        [HttpPost]
        public async Task<IActionResult> Post(RegisterModel registerModel)
        {
            RegisterService registerService = RegisterService.GetInstance();
            try
            {
                await registerService.AddUser(registerModel);
            }
            catch (ObjectDisposedException ex)
            {
                return StatusCode(500, new ErrorModel("Somethiong went wrong", 500));
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new ErrorModel("Somethiong went wrong", 500));
            }
            catch (InvalidCastException ex)
            {
                return StatusCode(500, new ErrorModel("Somethiong went wrong", 500));
            }
            catch (IOException ex)
            {
                return StatusCode(500, new ErrorModel("Somethiong went wrong", 500));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, new ErrorModel("Somethiong went wrong", 500));
            }
            catch (InternalServerException ex)
            {
                return StatusCode(500, new ErrorModel(ex.Message, 500));
            }
            catch (BadRequestException ex)
            {
                return StatusCode(400, new ErrorModel(ex.Message, 400));
            }
        
            return StatusCode(500, new ErrorModel("Somethiong went wrong", 500));
        }




    }
}
