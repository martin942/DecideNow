using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Follow
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
            string token = ((string)context.Request.Query["token"]);
            if (token == null || token.Equals(""))
            {
                await SendResponseMessage(context, new Message("token missing", 400));
                return;
            }

            string userId = await Sql.GetInstance().GetUserIdByToken(token, context.Connection.RemoteIpAddress.ToString());
            if (userId == null || userId.Equals(""))
            {
                await SendResponseMessage(context, new Message("invalid token", 400));
                return;
            }

            string path = context.Request.Path.ToString();

            if (path.Equals("/isfollowing"))
            {
                string isFollowing = ((string)context.Request.Query["username"]);

                if (isFollowing != null && !isFollowing.Equals(""))
                {
                    bool isFollowinValue = await IsFollowing(userId, isFollowing);
                    await SendResponseMessage(context, new Message(isFollowinValue.ToString(), 200));
                }
                else
                {
                    await SendResponseMessage(context, new Message("missing parameter 'username'", 500));
                }
            }
            else if (path.Equals("/followingcount"))
            {
                string followingCount = ((string)context.Request.Query["username"]);

                if (followingCount != null && !followingCount.Equals(""))
                {
                    int followingCountValue = await FollowingCount(followingCount);
                    await SendResponseMessage(context, new Message(followingCountValue.ToString(), 200));
                }
                else
                {
                    await SendResponseMessage(context, new Message("missing parameter 'username'", 500));
                }
            }
            else if (path.Equals("/followerscount"))
            {
                string followersCount = ((string)context.Request.Query["username"]);

                if (followersCount != null && !followersCount.Equals(""))
                {
                    int followersCountValue = await FollowersCount(followersCount);
                    await SendResponseMessage(context, new Message(followersCountValue.ToString(), 200));
                }
                else
                {
                    await SendResponseMessage(context, new Message("missing parameter 'username'", 500));
                }
            }
            else if (path.Equals("/following"))
            {
                string following = ((string)context.Request.Query["username"]);

                if (following != null && !following.Equals(""))
                {
                    List<string> followingList = await Following(following);
                    await SendResponseMessage(context, new Message(followingList, 200));
                }
                else
                {
                    await SendResponseMessage(context, new Message("missing parameter 'username'", 500));
                }
            }
            else if (path.Equals("/followers"))
            {
                string followers = ((string)context.Request.Query["username"]);

                if (followers != null && !followers.Equals(""))
                {
                    List<string> followeresList = await Followers(followers);
                    await SendResponseMessage(context, new Message(followeresList, 200));
                }
                else
                {
                    await SendResponseMessage(context, new Message("missing parameter 'username'", 500));
                }
            }
            else if(path.Equals("/myinfo"))
            {
                string username = await Sql.GetInstance().GetUsernameByUserId(userId);
                await SendResponseMessage(context, new Message(username, 200));
            }
        }

        public async Task Post(HttpContext context)
        {
            string token = ((string)context.Request.Query["token"]);
            if (token == null || token.Equals(""))
            {
                await SendResponseMessage(context, new Message("token missing", 400));
                return;
            }

            string userId = await Sql.GetInstance().GetUserIdByToken(token, context.Connection.RemoteIpAddress.ToString());
            if (userId == null || userId.Equals(""))
            {
                await SendResponseMessage(context, new Message("invalid token", 400));
                return;
            }

            string follow = ((string)context.Request.Query["follow"]);

            Message followMessage = await Follow(userId, follow);

            await SendResponseMessage(context, followMessage);

        }

        private async Task Delete(HttpContext context)
        {
            string token = ((string)context.Request.Query["token"]);
            if (token == null || token.Equals(""))
            {
                await SendResponseMessage(context, new Message("token missing", 400));
                return;
            }

            string userId = await Sql.GetInstance().GetUserIdByToken(token, context.Connection.RemoteIpAddress.ToString());
            if (userId == null || userId.Equals(""))
            {
                await SendResponseMessage(context, new Message("invalid token", 400));
                return;
            }

            string unfollow = ((string)context.Request.Query["unfollow"]);

            Message unfollowMessage = await Unfollow(userId, unfollow);

            await SendResponseMessage(context, unfollowMessage);
        }

        private async Task<Message> Follow(string userId, string follow)
        {
            if (follow == null || follow.Equals(""))
            {
                return null;
            }
            string followUserId = await Sql.GetInstance().GetUserIdByUsername(follow);
            if (followUserId == null || followUserId.Equals(""))
            {
                return new Message($"couldn't find user '{follow}'", 400);
            }
            if (followUserId.Equals(userId))
            {
                return new Message($"can't follow user '{follow}'", 400);
            }

            int rows = await Sql.GetInstance().FollowUser(userId, followUserId);
            if (rows == 1)
            {
                return new Message("follow successfull", 200);
            }
            return new Message($"can't follow user '{follow}'", 500);
        }

        private async Task<Message> Unfollow(string userId, string unfollow)
        {
            if (unfollow == null || unfollow.Equals(""))
            {
                return null;
            }
            string unfollowUserId = await Sql.GetInstance().GetUserIdByUsername(unfollow);
            if (unfollowUserId == null || unfollowUserId.Equals(""))
            {
                return new Message($"couldn't find user '{unfollow}'", 400);
            }
            if (unfollowUserId.Equals(userId))
            {
                return new Message($"can't unfollow user '{unfollow}'", 400);
            }

            int rows = await Sql.GetInstance().UnfollowUser(userId, unfollowUserId);
            if (rows == 1)
            {
                return new Message("unfollow successfull", 200);
            }
            return new Message($"can't unfollow user '{unfollow}'", 500);
        }

        // return list of names
        private async Task<List<string>> Following(string userName)
        {
            string followUserId = await Sql.GetInstance().GetUserIdByUsername(userName);
            if (followUserId == null || followUserId.Equals(""))
            {
                return null;
            }

            List<string> followingList = await Sql.GetInstance().Following(followUserId);


            return followingList;
        }

        private async Task<List<string>> Followers(string userName)
        {
            string followUserId = await Sql.GetInstance().GetUserIdByUsername(userName);
            if (followUserId == null || followUserId.Equals(""))
            {
                return null;
            }

            List<string> followingList = await Sql.GetInstance().Followers(followUserId);

            return followingList;
        }

        private async Task<int> FollowingCount(string userName)
        {
            string followUserId = await Sql.GetInstance().GetUserIdByUsername(userName);
            if (followUserId == null || followUserId.Equals(""))
            {
                return -1;
            }

            int count = await Sql.GetInstance().FollowingCount(followUserId);

            return count;
        }

        private async Task<int> FollowersCount(string userName)
        {
            string followUserId = await Sql.GetInstance().GetUserIdByUsername(userName);
            if (followUserId == null || followUserId.Equals(""))
            {
                return -1;
            }

            int count = await Sql.GetInstance().FollowersCount(followUserId);

            return count;
        }

        private async Task<bool> IsFollowing(string userId, string isFollowingUsername)
        {
            string isFollowingUserId = await Sql.GetInstance().GetUserIdByUsername(isFollowingUsername);
            if (isFollowingUserId == null || isFollowingUserId.Equals(""))
            {
                return false;
            }
            if (isFollowingUserId.Equals(userId))
            {
                return false;
            }
            bool isFollowing = await Sql.GetInstance().IsFollowing(userId, isFollowingUserId);

            return isFollowing;
        }


        private async Task SendResponseMessage(HttpContext context, Message responseObject)
        {
            string response = JsonSerializer.Serialize(responseObject);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = responseObject.statusCode;
            await context.Response.WriteAsync(response);
        }

    }

    public class Message
    {
        public object message { get; set; }
        public int statusCode { get; set; }

        public Message(object message, int statusCode)
        {
            this.message = message;
            this.statusCode = statusCode;
        }
    }
}
