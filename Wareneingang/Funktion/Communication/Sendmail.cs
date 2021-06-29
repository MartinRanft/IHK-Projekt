using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Wareneingang.Funktion.Communication
{
    internal class Mailer
    {
        private const string ReceiverEmail = "it@ba.de";

        /// <summary>
        /// Will send a mail over smtp to the Developer automatical with fail text.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="test"></param>
        public static void send(string modul, string message, int test = 0)
        {
            MailMessage mailMessage = new MailMessage();

            SmtpClient smtpClient = new SmtpClient("smtp..de");

            mailMessage.From = new MailAddress("maintenance");
            mailMessage.To.Add(Mailer.ReceiverEmail);
            mailMessage.Priority = MailPriority.High;

            if (test == 1)
            {
                mailMessage.Subject = "TEST MAIL GENERATET IN C#";
                mailMessage.Body = "THIS MAIL HAS BIN MADE BY C# FOR TESTING";
            }
            else
            {
                mailMessage.Subject = "Failure in APP Wareneingang at Modul " + modul;
                mailMessage.Body = message;
            }

            smtpClient.Port = 587;
            smtpClient.Credentials = new System.Net.NetworkCredential("maintenance", @"PASSWORD");
            smtpClient.Send(mailMessage);
        }
    }
}