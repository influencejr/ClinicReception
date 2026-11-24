using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;

namespace ClinicReception.Infrastructure.Storage;

/// <summary>
/// Файли даних у теці data: довідники doctors.json і patients.json лише читаються,
/// записи на прийом читаються з appointments.json і зберігаються туди ж.
/// </summary>
public sealed class JsonClinicStore
{
    private const string DoctorsFile = "doctors.json";
    private const string PatientsFile = "patients.json";
    private const string AppointmentsFile = "appointments.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _directory;

    /// <summary>Створює сховище над текою даних.</summary>
    /// <param name="dataDirectory">Тека з файлами JSON.</param>
    /// <exception cref="ArgumentException">Шлях порожній.</exception>
    public JsonClinicStore(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        _directory = dataDirectory;
    }

    /// <summary>Повний шлях до файлу записів.</summary>
    public string AppointmentsPath => Path.Combine(_directory, AppointmentsFile);

    /// <summary>Читає довідники лікарів і пацієнтів.</summary>
    /// <returns>Довідники в пам'яті.</returns>
    /// <exception cref="FileNotFoundException">Файлу довідника немає.</exception>
    /// <exception cref="JsonException">Файл пошкоджено.</exception>
    public InMemoryClinicDirectory LoadDirectory()
    {
        IEnumerable<Doctor> doctors = Read<List<StoredDoctor>>(DoctorsFile).Select(ToDoctor);
        IEnumerable<Patient> patients = Read<List<StoredPatient>>(PatientsFile).Select(ToPatient);
        return new InMemoryClinicDirectory(doctors, patients);
    }

    /// <summary>Читає збережені записи в сховище; якщо файлу ще немає, нічого не робить.</summary>
    /// <param name="target">Сховище, куди додати записи.</param>
    /// <returns>Кількість прочитаних записів.</returns>
    /// <exception cref="JsonException">Файл пошкоджено.</exception>
    public int LoadAppointments(IAppointmentRepository target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!File.Exists(AppointmentsPath))
        {
            return 0;
        }

        List<StoredAppointment> stored = Read<List<StoredAppointment>>(AppointmentsFile);
        stored.ForEach(a => target.Add(ToAppointment(a)));
        return stored.Count;
    }

    /// <summary>Зберігає всі записи; спершу пише тимчасовий файл, щоб збій не зіпсував попередній.</summary>
    /// <param name="appointments">Записи для збереження.</param>
    /// <exception cref="IOException">Файл недоступний для запису.</exception>
    public void SaveAppointments(IEnumerable<Appointment> appointments)
    {
        ArgumentNullException.ThrowIfNull(appointments);
        List<StoredAppointment> stored = appointments.OrderBy(a => a.Id).Select(ToStored).ToList();
        string temporary = AppointmentsPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(stored, Options));
        File.Move(temporary, AppointmentsPath, overwrite: true);
    }

    private static Doctor ToDoctor(StoredDoctor d) =>
        new(d.Code, d.FullName, d.ConsultationFee, d.SlotMinutes)
        {
            Specialty = d.Specialty,
            WorkStart = d.WorkStart,
            WorkEnd = d.WorkEnd,
        };

    private static Patient ToPatient(StoredPatient p) =>
        new(p.Id, p.FullName, p.Email) { Phone = p.Phone, IsRegular = p.IsRegular, HasInsurance = p.HasInsurance };

    private static Appointment ToAppointment(StoredAppointment a)
    {
        Appointment appointment = new(a.Id, a.PatientId, a.DoctorCode, VisitTime.At(a.VisitDate, a.StartTime, a.DurationMinutes));
        foreach (StoredItem item in a.Items)
        {
            appointment.AddItem(new VisitItem(item.ServiceCode, item.Quantity, item.UnitPrice, item.DurationMinutes));
        }

        RestoreStatus(appointment, a.Status);
        return appointment;
    }

    // стан відновлюється тими самими переходами, що й у роботі, тож файл не обійде правил агрегата
    private static void RestoreStatus(Appointment appointment, VisitStatus status)
    {
        if (status is VisitStatus.Cancelled)
        {
            appointment.Cancel();
            return;
        }

        if (status is not VisitStatus.Scheduled)
        {
            appointment.Confirm();
        }

        if (status is VisitStatus.Completed)
        {
            appointment.Complete();
        }
        else if (status is VisitStatus.Missed)
        {
            appointment.MarkMissed();
        }
    }

    private static StoredAppointment ToStored(Appointment a) => new()
    {
        Id = a.Id,
        PatientId = a.PatientId,
        DoctorCode = a.DoctorCode,
        VisitDate = a.VisitDate,
        StartTime = a.StartTime,
        DurationMinutes = a.Slot.DurationMinutes,
        Status = a.Status,
        Items = a.Items.Select(i => new StoredItem
        {
            ServiceCode = i.ServiceCode,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            DurationMinutes = i.DurationMinutes,
        }).ToList(),
    };

    private T Read<T>(string file)
    {
        string json = File.ReadAllText(Path.Combine(_directory, file));
        return JsonSerializer.Deserialize<T>(json, Options)
            ?? throw new JsonException($"Файл {file} порожній");
    }
}
