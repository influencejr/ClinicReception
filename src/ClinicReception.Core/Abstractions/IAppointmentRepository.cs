using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Abstractions;

/// <summary>Сховище записів на прийом. Відповідає лише за зберігання.</summary>
public interface IAppointmentRepository
{
    /// <summary>Зберігає новий запис.</summary>
    /// <param name="appointment">Запис, якого ще немає в сховищі.</param>
    void Add(Appointment appointment);

    /// <summary>Шукає запис за номером.</summary>
    /// <param name="id">Номер запису.</param>
    /// <returns>Знайдений запис або null, якщо такого номера немає.</returns>
    Appointment? GetById(int id);

    /// <summary>Повертає всі збережені записи.</summary>
    /// <returns>Знімок записів на момент виклику.</returns>
    IReadOnlyList<Appointment> GetAll();
}
