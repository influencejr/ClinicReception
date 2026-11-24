using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;

namespace ClinicReception.Infrastructure.Storage;

/// <summary>Довідники лікарів і пацієнтів у пам'яті, з пошуком за ключем через словник.</summary>
public sealed class InMemoryClinicDirectory : IClinicDirectory
{
    private readonly Dictionary<string, Doctor> _doctors;
    private readonly Dictionary<int, Patient> _patients;

    /// <summary>Створює довідники з готових списків.</summary>
    /// <param name="doctors">Лікарі; коди не повторюються.</param>
    /// <param name="patients">Пацієнти; номери не повторюються.</param>
    /// <exception cref="ArgumentNullException">Список не передано.</exception>
    /// <exception cref="ArgumentException">Код лікаря або номер пацієнта повторюється.</exception>
    public InMemoryClinicDirectory(IEnumerable<Doctor> doctors, IEnumerable<Patient> patients)
    {
        ArgumentNullException.ThrowIfNull(doctors);
        ArgumentNullException.ThrowIfNull(patients);
        _doctors = doctors.ToDictionary(d => d.Code, StringComparer.OrdinalIgnoreCase);
        _patients = patients.ToDictionary(p => p.Id);
    }

    /// <inheritdoc/>
    public IReadOnlyList<Doctor> Doctors => _doctors.Values.OrderBy(d => d.Code, StringComparer.Ordinal).ToList();

    /// <summary>Кількість пацієнтів у довіднику.</summary>
    public int PatientCount => _patients.Count;

    /// <inheritdoc/>
    public Doctor? FindDoctor(string code) => _doctors.GetValueOrDefault(code);

    /// <inheritdoc/>
    public Patient? FindPatient(int id) => _patients.GetValueOrDefault(id);
}
