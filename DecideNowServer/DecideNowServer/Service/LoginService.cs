using DecideNowServer.DB;
using DecideNowServer.Exceptions;
using DecideNowServer.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer.Service
{
    public class LoginService
    {

        private static LoginService instance;

        private LoginService()
        {

        }


        public async Task<string> GetChallenge(string email)
        {
            string userId = await DbLogin.GetInstance().getUserIdByEmail(email);
            if (userId.Equals(""))
            {
                throw new InternalServerException("User not found try to register");
            }
            return await ChallengeManager.GetChallenge(userId);
        }

        public async Task<string> GetToken(string solvedChallenge, string ip)
        {
            Challenge challenge = ChallengeManager.GetChallengeBySolvedChallenge(solvedChallenge);
            ChallengeManager.RemoveChallenge(challenge);
            if (challenge == null)
            {
                throw new BadRequestException("Wrong password, try again");
            }
            Token token = TokenManager.GetInstance().AddToken(challenge.userId, ip);
            await DbLogin.GetInstance().SetLastLogin(challenge.userId);
            return token.token;
        }


        public static LoginService GetInstance()
        {
            if (instance == null)
            {
                instance = new LoginService();
            }
            return instance;
        }

    }
}
