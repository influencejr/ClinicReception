using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Abstractions;

/// <summary>Довідники лікарів і пацієнтів. Реалізацію дає інфраструктура (файли JSON).</summary>
public interface IClinicDirectory
{
    /// <summary>Усі лікарі довідника.</summary>
    IReadOnlyList<Doctor> Doctors { get; }

    /// <summary>Знаходить лікаря за кодом. Стану не змінює.</summary>
    /// <param name="code">Код лікаря, наприклад «DOC-THER-01».</param>
    /// <returns>Лікар або null, якщо коду немає в довіднику.</returns>
    Doctor? FindDoctor(string code);

    /// <summary>Знаходить пацієнта за номером. Стану не змінює.</summary>
    /// <param name="id">Номер пацієнта.</param>
    /// <returns>Пацієнт або null, якщо номера немає в довіднику.</returns>
    Patient? FindPatient(int id);
}
