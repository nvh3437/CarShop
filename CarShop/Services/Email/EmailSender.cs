using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CarShop.Services.Email
{
    public class EmailSender: IEmailSender
    {
        private readonly MailInfo mailInfo;
        private readonly ILogger<EmailSender> logger;
        public EmailSender(ILogger<EmailSender> _logger, IOptions<MailInfo> _mailInfo)
        {
            _logger.LogInformation("Create EmailSender Service");
            logger = _logger;
            mailInfo = _mailInfo.Value;
        }

        public async Task SendEmailAsync(string email, string title, string content)
        {
            var message = new MimeMessage();

            message.Sender = new MailboxAddress(mailInfo.DisplayName, mailInfo.Mail);
            message.From.Add(new MailboxAddress(mailInfo.DisplayName, mailInfo.Mail));
            message.To.Add(new MailboxAddress(email, email));

            message.Subject = title;

            var BodyBuilder = new BodyBuilder();
            BodyBuilder.HtmlBody = content;
            message.Body = BodyBuilder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                smtp.Connect(mailInfo.Host, mailInfo.Port, SecureSocketOptions.StartTls);
                smtp.Authenticate(mailInfo.Mail, mailInfo.Password);
                await smtp.SendAsync(message);
                logger.LogInformation($"Send mail to {email}");
            }
            catch (Exception ex)
            {
                Directory.CreateDirectory("ErrorMail");
                var fileName = string.Format(@"ErrorMail/{0}-{1}.eml", email,Guid.NewGuid());
                await message.WriteToAsync(fileName);
                logger.LogInformation(ex.Message);
            }
            await smtp.DisconnectAsync(true);
        }
    }
}
