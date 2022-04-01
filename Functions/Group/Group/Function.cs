using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace Group
{
    public class Function : IHttpFunction
    {
        
        public async Task HandleAsync(HttpContext context)
        {
            await context.Response.WriteAsync("Hello, Functions Framework.");
        }
    }
}
