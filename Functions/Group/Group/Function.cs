using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Group
{
    public class Function : IHttpFunction
    {
        
        public async Task HandleAsync(HttpContext context)
        {
            string method = context.Request.Method;
            switch (method)
            {
                case "GET":
                    await Get(context);
                    break;
                case "POST":
                    await Post(context);
                    break;
                case "PATCH":
                    await Patch(context);
                    break;
                default:
                    break;
            }
        }

        public async Task Get(HttpContext context)
        {

        }

        public async Task Post(HttpContext context)
        {
            string path = context.Request.Path;
            switch (path)
            {
                case "/create":
                    await PostCreate(context);
                    break;
            }
        }

        public async Task Patch(HttpContext context)
        {

        }

        public async Task PostCreate(HttpContext context)
        {
            string token = ((string)context.Request.Query["token"]);
            if (token == null || token.Equals(""))
            {
                await SendResponseMessage(context, new Message("token missing", 400));
                return;
            }
            string userid = await SqlManager.GetInstance().ValidateToken(token, context.Connection.RemoteIpAddress.ToString());
            if (userid == null || userid.Equals(""))
            {
                await SendResponseMessage(context, new Message("invalid token", 400));
                return;
            }
            // todo
        }

        private async Task SendResponseMessage(HttpContext context, object responseObject)
        {
            string response = JsonSerializer.Serialize(responseObject);
            context.Response.ContentType = "application/json";
            if (responseObject.GetType() == typeof(Message))
            {
                context.Response.StatusCode = ((Message)responseObject).statusCode;
            }
            else
            {
                context.Response.StatusCode = 200;
            }
            await context.Response.WriteAsync(response);
        }

    }
}
