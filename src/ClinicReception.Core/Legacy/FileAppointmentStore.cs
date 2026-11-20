using System.Text.Json;

namespace ClinicReception.Core.Legacy;

public class FileAppointmentStore : IAppointmentStore
{
    private readonly List<LegacyAppointment> _all = new List<LegacyAppointment>();

    public void Add(LegacyAppointment appointment) => _all.Add(appointment);

    public LegacyAppointment GetById(int id) => _all.First(a => a.Id == id);

    public void SaveToFile(LegacyAppointment appointment, string path)
    {
        File.AppendAllText(path, JsonSerializer.Serialize(appointment) + Environment.NewLine);
    }

    public void SendEmail(string to, string body) => throw new NotImplementedException();

    public string ExportCsv() => throw new NotImplementedException();

    public void Backup(string folder) => throw new NotImplementedException();

    public void PrintInvoice(LegacyAppointment appointment) => throw new NotImplementedException();
}
