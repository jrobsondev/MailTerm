using MailTerm.Console.Interfaces;
using MimeKit;

namespace MailTerm.Console.Models;

public class EmailAccount : IEmailAccount
{
    public string Name { get; set; }
    public string EmailAddress { get; set; }

    public EmailAccount(MailboxAddress mailboxAddress)
    {
        Name = mailboxAddress.Name;
        EmailAddress = mailboxAddress.Address;
    }
}