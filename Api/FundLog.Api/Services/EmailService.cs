using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using Task = System.Threading.Tasks.Task;

namespace FundLog.Api.Services;

public class EmailService : IEmailSender
{
  public async Task SendEmailAsync(string email, string subject, string htmlMessage)
  {
    var client = new SmtpClient("smtp.office365.com", 587) { EnableSsl = true, UseDefaultCredentials = false };
    client.Credentials = new System.Net.NetworkCredential("fake@fake.com", "fake");
    var msg = new MailMessage("fake@fake.com", email)
    {
      Body = htmlMessage,
      Subject = subject
    };

    await client.SendMailAsync(msg);
  }
}
