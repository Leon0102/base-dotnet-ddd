using Storage.Interface;

namespace Storage.Implementation;

public class MailSender : IMailSender
{
    public Task SendEmailAsync(string title, string body)
    {
        throw new NotImplementedException();
    }
}