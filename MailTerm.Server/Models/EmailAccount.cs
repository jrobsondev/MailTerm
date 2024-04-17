using MailTerm.Server.Interfaces;
using MimeKit;

namespace MailTerm.Server.Models;

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