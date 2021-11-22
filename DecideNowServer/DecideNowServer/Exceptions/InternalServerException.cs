using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer.Exceptions
{
    public class InternalServerException : Exception
    {

        public InternalServerException(string message) : base(message)
        {
        }

    }
}
