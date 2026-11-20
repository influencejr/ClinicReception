namespace ClinicReception.Core.Domain;

/// <summary>Стан запису на прийом у його життєвому циклі.</summary>
public enum VisitStatus
{
    /// <summary>Запис створено, пацієнта ще не обдзвонено.</summary>
    Scheduled = 0,

    /// <summary>Пацієнт підтвердив, що прийде.</summary>
    Confirmed = 1,

    /// <summary>Прийом відбувся.</summary>
    Completed = 2,

    /// <summary>Пацієнт не прийшов на прийом.</summary>
    Missed = 3,

    /// <summary>Запис скасовано до прийому.</summary>
    Cancelled = 4,
}
