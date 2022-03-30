using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Explore
{
    public class Function : IHttpFunction
    {
        public async Task HandleAsync(HttpContext context)
        {
            string method = context.Request.Method;
            if (method.Equals("GET"))
            {
                await Get(context);
            }
            else if (method.Equals("POST"))
            {
                await Post(context);
            }
            else if (method.Equals("DELETE"))
            {
                await Delete(context);
            }
        }

        public async Task Get(HttpContext context)
        {
            string path = context.Request.Path;
            if (path.Equals("/search"))
            {
                await GetSearch(context);
            }
        }

        public async Task Post(HttpContext context)
        {

        }

        public async Task Delete(HttpContext context)
        {

        }

        public async Task GetSearch(HttpContext context)
        {
            string searchValue = context.Request.Query["searchValue"];
            string filter = context.Request.Query["filter"];

            if (string.IsNullOrEmpty(searchValue))
            {
                await SendMessage(context, new Message("no search value", 400));
                return;
            }

            List<string> usersNames = await SqlManager.GetInstance().GetUsers(searchValue);
            await SendMessage(context, new Message(usersNames, 200));
        }

        public async Task SendMessage(HttpContext context, Message message)
        {
            string response = JsonSerializer.Serialize(message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = message.statusCode;
            await context.Response.WriteAsync(response);
        }

    }
}
