using System;
using System.Net;
using System.Net.Mail;
using UIBase.Properties;

namespace UIBase
{
    public class EmailNotify
    {
        public static void Send(string subject, string text, string emailTo, string filePath)
        {
            if (string.IsNullOrEmpty(emailTo))
                return;

            try
            {
                var message = new MailMessage {From = new MailAddress(Settings.Default.SmtpLogin)};

                message.To.Add(emailTo);
                message.IsBodyHtml = true;
                message.Subject = subject;
                message.Body = text;

                if (filePath != null)
                    message.Attachments.Add(new Attachment(filePath));

                Send(message);
            }
            catch (Exception e)
            {
                NLog.LogManager.GetCurrentClassLogger().Error("Ошибка при отправке уведомления", e);
            }
        }

        public static void Send(MailMessage message)
        {
            using (var sc = new SmtpClient("smtp.yandex.ru", 587))
            {
                sc.EnableSsl = true;
                sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                sc.UseDefaultCredentials = false;
                sc.Credentials = new NetworkCredential(Settings.Default.SmtpLogin, Settings.Default.SmtpPassword);

                sc.Send(message);
            }
        }
    }
}
