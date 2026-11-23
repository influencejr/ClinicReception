using System.Globalization;
using ClinicReception.Core.Domain;
using ClinicReception.Core.Errors;
using ClinicReception.Core.Reporting;
using ClinicReception.Core.Services;
using ClinicReception.Tests.Doubles;

namespace ClinicReception.Tests;

// Пошук (UC-04) і звіт «завантаження лікаря за день» (UC-05) на фейкових сховищі й довіднику.
public class ScheduleQueriesTests
{
    private static readonly DateOnly Monday = new(2026, 11, 23);

    private readonly FakeAppointmentRepository _repository = new();
    private readonly ScheduleQueries _queries;

    public ScheduleQueriesTests()
    {
        _queries = new ScheduleQueries(_repository, new FakeClinicDirectory());
    }

    [Fact]
    public void Find_NoAppointments_ReturnsEmpty()
    {
        Assert.Empty(_queries.Find(new AppointmentQuery { PatientId = 1 }));
    }

    [Fact]
    public void Find_PatientAndPeriod_ReturnsOnlyMatching()
    {
        Add(1, 1, FakeClinicDirectory.Therapist, Monday, 8, 0);
        Add(2, 2, FakeClinicDirectory.Therapist, Monday, 8, 20);
        Add(3, 1, FakeClinicDirectory.Therapist, Monday.AddDays(7), 8, 0);

        IReadOnlyList<Appointment> found = _queries.Find(new AppointmentQuery { PatientId = 1, To = Monday.AddDays(1) });

        Assert.Equal(1, Assert.Single(found).Id);
    }

    [Fact]
    public void Find_SeveralDays_OrdersByDateThenTime()
    {
        Add(1, 1, FakeClinicDirectory.Therapist, Monday.AddDays(1), 8, 0);
        Add(2, 2, FakeClinicDirectory.Therapist, Monday, 9, 0);
        Add(3, 3, FakeClinicDirectory.Therapist, Monday, 8, 20);

        IEnumerable<int> ids = _queries.Find(new AppointmentQuery { DoctorCode = "doc-ther-01" }).Select(a => a.Id);

        Assert.Equal("3,2,1", string.Join(',', ids));
    }

    [Fact]
    public void Find_ByStatus_SkipsOtherStates()
    {
        Add(1, 1, FakeClinicDirectory.Therapist, Monday, 8, 0).Cancel();
        Add(2, 2, FakeClinicDirectory.Therapist, Monday, 8, 20);

        Assert.Equal(1, Assert.Single(_queries.Find(new AppointmentQuery { Status = VisitStatus.Cancelled })).Id);
    }

    [Fact]
    public void LoadOf_TwoVisitsAndCancelled_CountsActiveAndFreeWindows()
    {
        Add(1, 1, FakeClinicDirectory.Therapist, Monday, 8, 20);
        Add(2, 2, FakeClinicDirectory.Therapist, Monday, 9, 0);
        Add(3, 3, FakeClinicDirectory.Therapist, Monday, 9, 20).Cancel();

        DoctorDayLoad load = _queries.LoadOf(FakeClinicDirectory.Therapist, Monday);

        Assert.Equal("1,2", string.Join(',', load.Visits.Select(v => v.AppointmentId)));
        Assert.Equal(40, load.BusyMinutes);
        Assert.Equal(33.3m, load.LoadPercent);
        Assert.Equal("08:00-08:20 08:40-09:00 09:20-10:00", string.Join(' ', load.FreeWindows.Select(Text)));
    }

    [Fact]
    public void LoadOf_UnknownDoctor_ThrowsDomainRule()
    {
        Assert.Equal("doctor.exists", Assert.Throws<DomainRuleException>(() => _queries.LoadOf("DOC-NONE-01", Monday)).Rule);
    }

    [Fact]
    public void TextOf_OneVisit_PrintsRowsTotalsAndWindows()
    {
        Add(1, 1, FakeClinicDirectory.Therapist, Monday, 8, 0);

        string text = DoctorLoadReport.TextOf(_queries.LoadOf(FakeClinicDirectory.Therapist, Monday));

        Assert.Equal(
            "Завантаження лікаря DOC-THER-01 (Ткаченко Ірина Олегівна) на 23.11.2026\n"
            + "Робочий час 08:00-10:00, 120 хв\n"
            + "  08:00-08:20  № 1    Іваненко Андрій Петрович         Scheduled\n"
            + "Зайнято 20 хв з 120 (16.7 %)\n"
            + "Вільні проміжки: 08:20-10:00",
            text.Replace("\r\n", "\n", StringComparison.Ordinal));
    }

    private static string Text(TimeSlot slot) =>
        string.Create(CultureInfo.InvariantCulture, $"{slot.StartTime:HH:mm}-{slot.EndTime:HH:mm}");

    private Appointment Add(int id, int patientId, string doctorCode, DateOnly date, int hour, int minute)
    {
        Appointment a = new(id, patientId, doctorCode, VisitTime.At(date, new TimeOnly(hour, minute)));
        _repository.Add(a);
        return a;
    }
}
