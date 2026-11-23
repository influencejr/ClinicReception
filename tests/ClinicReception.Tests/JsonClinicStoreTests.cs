using ClinicReception.Core.Domain;
using ClinicReception.Infrastructure.Storage;

namespace ClinicReception.Tests;

// Інфраструктура поза межами порогу покриття, але збереження й читання записів
// перевіряються на справжніх файлах у тимчасовій теці.
public sealed class JsonClinicStoreTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "clinic-" + Guid.NewGuid().ToString("N"));

    public JsonClinicStoreTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Fact]
    public void SaveThenLoad_CompletedVisit_RestoresTimeItemsAndStatus()
    {
        Appointment original = new(7, 3, "DOC-CARD-01", VisitTime.At(new DateOnly(2026, 11, 24), new TimeOnly(9, 30), 30));
        original.AddItem(new VisitItem("CONS", 1, 650m, 30));
        original.Confirm();
        original.Complete();
        JsonClinicStore store = new(_folder);
        store.SaveAppointments(new[] { original });
        InMemoryAppointmentRepository loaded = new();

        int count = store.LoadAppointments(loaded);

        Appointment copy = loaded.GetById(7)!;
        Assert.Equal(1, count);
        Assert.Equal(original.Time, copy.Time);
        Assert.Equal(VisitStatus.Completed, copy.Status);
        Assert.Equal(650m, copy.Subtotal);
    }

    [Fact]
    public void LoadAppointments_NoFileYet_ReturnsZero()
    {
        Assert.Equal(0, new JsonClinicStore(_folder).LoadAppointments(new InMemoryAppointmentRepository()));
    }

    [Fact]
    public void LoadDirectory_RepositoryDataFolder_ReadsDoctorsAndPatients()
    {
        string data = Path.Combine(AppContext.BaseDirectory, "data");

        InMemoryClinicDirectory directory = new JsonClinicStore(data).LoadDirectory();

        Assert.Equal(8, directory.Doctors.Count);
        Assert.Equal(60, directory.PatientCount);
        Assert.Equal(new TimeOnly(9, 0), directory.FindDoctor("DOC-CARD-01")!.WorkStart);
    }
}
