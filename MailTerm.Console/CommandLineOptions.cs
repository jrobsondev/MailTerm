using CommandLine;

public class CommandLineOptions
{
    [Option('p', "port", Required = false, Default = 2225, HelpText = "Set SMTP server port")]
    public int Port { get; set; }

    [Option('a', "attachments", Required = false, HelpText = "Attachments save location")]
    public string? AttachmentFilePath { get; set; }
}