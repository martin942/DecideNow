using DecideNowServer.DB;
using DecideNowServer.Exceptions;
using DecideNowServer.Models;
using DecideNowServer.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer.service
{
    public class RegisterService
    {

        private static RegisterService instance;


        private RegisterService()
        {

        }

        public async Task<RegisterModel> AddUser(RegisterModel registerModel)
        {
            // encrypts the password with the private key and sets it as the current password
            string cypher = registerModel.password_hash;
            registerModel.password_hash = RSA.DecryptPassword(registerModel.password_hash, registerModel.public_key, registerModel.check_sum);
            if (registerModel.password_hash.Equals(""))
            {
                throw new InternalServerException("Couldn't read password, try again");
            }
            if (!Verifier.IsValidDate(registerModel.create_date))
            {
                throw new BadRequestException("Invalid Date");
            }
            if (!Verifier.IsValidMail(registerModel.email))
            {
                throw new BadRequestException("Invalid Email");
            }
            if (await DbRegister.GetInstance().AddUser(registerModel) == 1)
            {
                registerModel.password_hash = cypher;
                return registerModel;
            }
            return null;
        }

        public static RegisterService GetInstance()
        {
            if (instance == null)
            {
                instance = new RegisterService();
            }
            return instance;
        }

    }
}
