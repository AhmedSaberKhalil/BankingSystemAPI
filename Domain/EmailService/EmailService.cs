using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;


namespace Domain.EmailService
{
	public class EmailService : IEmailService
	{
		private readonly EmailSettings _emailSettings;

		public EmailService(IOptions<EmailSettings> emailSettings)
		{
			_emailSettings = emailSettings.Value;

		}
        //public async Task SendEmailAsync(string toEmail, string subject, string body)
        //{
        //	var message = new MailMessage();
        //	message.To.Add(new MailAddress(toEmail));
        //	message.From = new MailAddress(_emailSettings.SmtpUsername);
        //	message.Subject = subject;
        //	message.Body = body;
        //	message.IsBodyHtml = true;

        //	using (var smtpClient = new System.Net.Mail.SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
        //	{
        //		smtpClient.UseDefaultCredentials = false;
        //		smtpClient.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
        //		smtpClient.EnableSsl = true;
        //		await smtpClient.SendMailAsync(message);
        //	}
        //}
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromAddress));
                emailMessage.To.Add(new MailboxAddress(string.Empty, toEmail)); // Correct constructor usage
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = message };

                using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                    await client.SendAsync(emailMessage);
                }
                catch (Exception ex)
                {
                    // Log the exception or handle it accordingly
                    throw new InvalidOperationException("Failed to send email", ex);
                }
                finally
                {
                    await client.DisconnectAsync(true);
                }
            }
            
        }
    }
}
