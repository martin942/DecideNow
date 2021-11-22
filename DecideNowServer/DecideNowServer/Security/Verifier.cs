using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DecideNowServer.Security
{
    public class Verifier
    {
        public static bool IsValidMail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsValidDate(string date)
        {
            if (date == null)
            {
                return false;
            }
            Regex rx = new Regex("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$");
            if (rx.IsMatch(date))
            {
                return true;
            }
            else return false;
        }

    }
}
