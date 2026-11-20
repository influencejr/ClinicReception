using System.Net.Mail;

namespace ClinicReception.Core.Legacy;

public class SmtpMailer
{
    public void Send(string to, string subject, string body)
    {
        using var client = new SmtpClient("smtp.clinic.local");
        client.Send("reception@clinic.local", to, subject, body);
    }
}
