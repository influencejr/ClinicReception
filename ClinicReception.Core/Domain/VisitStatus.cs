namespace ClinicReception.Core.Domain;

/// <summary>Стан запису на прийом у його життєвому циклі.</summary>
public enum VisitStatus
{
    Scheduled = 0,
    Confirmed = 1,
    Completed = 2,
    Missed = 3,
    Cancelled = 4,
}
