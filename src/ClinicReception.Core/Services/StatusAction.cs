namespace ClinicReception.Core.Services;

/// <summary>Подія, що змінює стан запису (UC-03).</summary>
public enum StatusAction
{
    /// <summary>Пацієнт підтвердив запис.</summary>
    Confirm,

    /// <summary>Прийом відбувся.</summary>
    Complete,

    /// <summary>Пацієнт не прийшов.</summary>
    MarkMissed,
}
