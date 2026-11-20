namespace ClinicReception.Core.Legacy;

public interface IAppointmentStore
{
    void Add(LegacyAppointment appointment);
    LegacyAppointment GetById(int id);
    void SaveToFile(LegacyAppointment appointment, string path);
    void SendEmail(string to, string body);
    string ExportCsv();
    void Backup(string folder);
    void PrintInvoice(LegacyAppointment appointment);
}
