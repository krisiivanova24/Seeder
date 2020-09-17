using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebsiteSearchPrices
{
    public class Global
    {
        List<string> emails = new List<string>();

        public Global(List<string> emails)
        {
            this.emails = emails;
        }

        internal static void SendError(Exception v)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.ConnectionStrings["EmailAdmin"].ConnectionString,
                    ConfigurationManager.ConnectionStrings["PasswordAdmin"].ConnectionString);

                MailMessage mm = new MailMessage("donotreply@domain.com", ConfigurationManager.ConnectionStrings["EmailAdmin"].ConnectionString, "Auto Error Email", v.ToString());
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);

                client.Dispose();
                MessageBox.Show($"Установена е грешка в системата. Успешно е изпратен имейл до разработчиците.", "Email sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }

        public void SendEmailWithChange(Sites item, string price)
        {
            try
            {
                SmtpClient client1 = new SmtpClient();
                client1.Port = 587;
                client1.Host = "smtp.gmail.com";
                client1.EnableSsl = true;
                client1.DeliveryMethod = SmtpDeliveryMethod.Network;
                client1.UseDefaultCredentials = false;
                client1.Credentials = new System.Net.NetworkCredential(ConfigurationManager.ConnectionStrings["EmailAdmin"].ConnectionString,
                        ConfigurationManager.ConnectionStrings["PasswordAdmin"].ConnectionString);

                foreach (var email in emails)
                {
                    MailMessage mm = new MailMessage("donotreply@domain.com", email, $"Засечена е промяна в цената на {DateTime.Now}", $"\nИме:{item.Name} \nАртикул: {item.Url} \nОт сайт: {item.SiteType} \nПредишна цена: {item.Price} на дата: {item.Date} \nСегашна цена: {price} \nВ базата данни ще бъде записана новата стойност");
                    mm.BodyEncoding = UTF8Encoding.UTF8;
                    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    client1.Send(mm);
                }
                client1.Dispose();
            }
            catch (Exception x)
            {
                SendError(x);
            }

        }
    }
}
