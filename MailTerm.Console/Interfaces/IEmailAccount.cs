namespace MailTerm.Console.Interfaces;

public interface IEmailAccount
{
    string Name { get; set; }
    string EmailAddress { get; set; }
}