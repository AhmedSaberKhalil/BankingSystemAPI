using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Domain.EmailService
{
	public class EmailService : IEmailService
	{
		private readonly EmailSettings _emailSettings;

		public EmailService(IOptions<EmailSettings> emailSettings)
		{
			_emailSettings = emailSettings.Value;

		}
		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			var message = new MailMessage();
			message.To.Add(new MailAddress(toEmail));
			message.From = new MailAddress(_emailSettings.SmtpUsername);
			message.Subject = subject;
			message.Body = body;
			message.IsBodyHtml = true;

			using (var smtpClient = new System.Net.Mail.SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
			{
				smtpClient.UseDefaultCredentials = false;
				smtpClient.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
				smtpClient.EnableSsl = true;
				await smtpClient.SendMailAsync(message);
			}
		}
	}
}
