using DecideNowServer.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DecideNowServer.Security
{
    public static class ChallengeManager
    {

        private static List<Challenge> challenges = new List<Challenge>();
        
        public static async Task<string> GetChallenge(string userId)
        {
            if (userId == null) return null;
            removeChallengeIfExists(userId);
            string newChallenge = GenerateChallenge();
            string password = await GetPassword(userId);
            string encrypted = EncryptChallenge(newChallenge, password);
            Console.WriteLine("DecideNowServer/Security/ChallengeManager.GetHallenge(): "+ encrypted);
            challenges.Add(new Challenge(userId, newChallenge, encrypted));
            return newChallenge;
        }

        private static string EncryptChallenge(string challenge, string password)
        {
            byte[] challegeBytes = Convert.FromBase64String(challenge);
            byte[] passwordBytes = Convert.FromBase64String(password);
            HashAlgorithm alogirith = new SHA256Managed();
            byte[] challengeWithPass = new byte[challegeBytes.Length + passwordBytes.Length];
            for (int i = 0; i < challegeBytes.Length;i++)
            {
                challengeWithPass[i] = challegeBytes[i];
            }
            for (int i = 0; i < passwordBytes.Length;i++)
            {
                challengeWithPass[i + challegeBytes.Length] = passwordBytes[i];
            }

            return Convert.ToBase64String(alogirith.ComputeHash(challengeWithPass));
        }

        private static async Task<string> GetPassword(string userId)
        {
            string password = await DbLogin.GetInstance().GetPasswordByUserId(userId);
            return password;
        }

        private static void removeChallengeIfExists(string userId)
        {
            challenges.Remove(GetChallengeByUserId(userId));
        }

        private static string GenerateChallenge()
        {
            byte[] challenge = new byte[32];
            new Random().NextBytes(challenge);
            return Convert.ToBase64String(challenge);
        }


        public static Challenge GetChallengeByUserId(string userId)
        {
            foreach (Challenge i in challenges)
            {
                if (i.userId.Equals(userId))
                {
                    return i;
                }
            }
            return null;
        }

        public static Challenge GetChallengeByChallenge(string challenge)
        {
            foreach (Challenge i in challenges)
            {
                if (i.challenge.Equals(challenge))
                {
                    return i;
                }
            }
            return null;
        }

        public static Challenge GetChallengeBySolvedChallenge(string solvedChallenge)
        {
            foreach (Challenge i in challenges)
            {
                if (i.solvedChallenge.Equals(solvedChallenge))
                {
                    return i;
                }
            }
            return null;
        }

        public static void RemoveChallenge(Challenge challenge)
        {
            challenges.Remove(challenge);
        }

    }

    public class Challenge
    {
        public string userId { get; }
        public string challenge { get; }
        public string solvedChallenge { get; }
        

        public Challenge(string userId, string challenge, string solvedChallenge)
        {
            this.userId = userId;
            this.challenge = challenge;
            this.solvedChallenge = solvedChallenge;
        }
    }

}
