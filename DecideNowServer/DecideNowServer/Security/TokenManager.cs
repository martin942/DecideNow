using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer.Security
{
    public class TokenManager
    {

        private static TokenManager instance;
        private List<Token> tokens = new List<Token>();
        TokenHandler th;

        private TokenManager()
        {
            th = new TokenHandler(tokens);
        }

        public Token AddToken(string userId, string clientIp)
        {
            Token token = new Token(userId, clientIp);
            tokens.Add(token);
            return token;
        }

        public Token GetTokenByToken(string token)
        {
            foreach (Token t in tokens)
            {
                if (t.token.Equals(token))
                {
                    return t;
                }
            }
            return null;
        }

        public Token GetTokenByUserId(string userId)
        {
            foreach (Token t in tokens)
            {
                if (t.userId.Equals(userId))
                {
                    return t;
                }
            }
            return null;
        }

        public static TokenManager GetInstance()
        {
            if (instance == null)
            {
                instance = new TokenManager();
            }
            return instance;
        }


    }

    public class Token
    {

        public string token { get; }
        public string userId { get; }
        public long create_timestamp { get; }
        public string clientIp { get; }

        public Token(string userId, string clientIp)
        {
            this.userId = userId;
            this.clientIp = clientIp;
            token = Guid.NewGuid().ToString();
            create_timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
        }

    }

    public class TokenHandler
    {

        private List<Token> tokens;
        private static bool running = true;

        public TokenHandler(List<Token> tokens)
        {
            this.tokens = tokens;
            Start();
        }

        private void Start()
        {
            RemoveOldTokens();
        }

        private void RemoveOldTokens()
        {
            Task.Run(() =>
            {
                while (running)
                {
                    List<Token> tokensToRemove = new List<Token>();
                    DateTime currentTime = DateTime.Now;
                    foreach (Token token in tokens)
                    {
                        DateTime createTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        createTime = createTime.AddSeconds(token.create_timestamp);
                        if (currentTime.AddHours(24) < createTime)
                        {
                            tokensToRemove.Add(token);
                        }
                    }
                    for (int i = 0; i < tokensToRemove.Count;i++)
                    {
                        tokens.Remove(tokensToRemove[i]);
                    }
                    tokensToRemove.Clear();

                    Task.Delay(3600000);
                }
            });
        }

        public static void Stop( object sender, EventArgs e)
        {
            running = false;
        }

    }

}
