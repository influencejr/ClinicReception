using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;

namespace ClinicReception.Tests.Doubles;

// фейк довідника: два лікарі й три пацієнти без жодного файлу
internal sealed class FakeClinicDirectory : IClinicDirectory
{
    public const string Therapist = "DOC-THER-01";
    public const string Cardiologist = "DOC-CARD-01";

    private readonly List<Patient> _patients = new()
    {
        new Patient(1, "Іваненко Андрій Петрович", "patient001@example.com"),
        new Patient(2, "Петренко Олена Петрівна", "patient002@example.com"),
        new Patient(3, "Коваленко Олег Іванович", "patient003@example.com"),
    };

    public IReadOnlyList<Doctor> Doctors { get; } = new List<Doctor>
    {
        new(Therapist, "Ткаченко Ірина Олегівна", 450m, 20) { WorkStart = new TimeOnly(8, 0), WorkEnd = new TimeOnly(10, 0) },
        new(Cardiologist, "Бондар Оксана Ігорівна", 650m, 30) { WorkStart = new TimeOnly(9, 0), WorkEnd = new TimeOnly(12, 0) },
    };

    public Doctor? FindDoctor(string code) => Doctors.FirstOrDefault(d => d.Code == code);

    public Patient? FindPatient(int id) => _patients.FirstOrDefault(p => p.Id == id);
}
