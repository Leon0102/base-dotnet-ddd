namespace Storage.Interface;

public interface IMailSender
{
    Task SendEmailAsync(string title, string body);
}