namespace ClinicReception.Core.Legacy;

public class AppointmentManager
{
    private readonly FileAppointmentStore _store = new FileAppointmentStore();
    private readonly SmtpMailer _mailer = new SmtpMailer();

    public decimal Place(LegacyAppointment appointment, string patientType)
    {
        decimal total = 0m;
        foreach (var item in appointment.Items)
            total += item.UnitPrice * item.Quantity;

        if (patientType == "regular")
            total = total * 0.95m;
        else if (patientType == "insured")
            total = total * 0.90m;
        else if (patientType == "staff")
            total = total * 0.70m;

        if (total > 1000m)
            total = total - 50m;

        appointment.Total = total;
        appointment.Status = 1;
        _store.SaveToFile(appointment, "appointments.json");
        _mailer.Send(appointment.Email, "Appointment " + appointment.Id, "" + total);
        return total;
    }

    public string BuildCsvReport(List<LegacyAppointment> appointments)
    {
        var text = "id;total;status" + Environment.NewLine;
        foreach (var a in appointments)
            text += a.Id + ";" + a.Total + ";" + a.Status;

        File.WriteAllText("report.csv", text);
        Console.WriteLine("Звіт збережено");
        return text;
    }

    public void Archive(LegacyAppointment appointment)
    {
        LegacyAppointment archived = new ArchivedAppointment(appointment.Id);
        foreach (var item in appointment.Items)
            archived.AddItem(item);

        _store.SaveToFile(archived, "archive.json");
    }
}
