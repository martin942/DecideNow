using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecideNowServer.Models
{
    public class ErrorModel : ModelBase
    {
        

        public string message { get; set; }
        public int status { get; set; }

        public ErrorModel()
        {
        }

        public ErrorModel(string message, int status)
        {
            this.message = message;
            this.status = status;
        }

    }
}
