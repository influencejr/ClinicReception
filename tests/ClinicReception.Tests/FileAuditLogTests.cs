using ClinicReception.Core.Errors;
using ClinicReception.Core.Storage;

namespace ClinicReception.Tests;

public sealed class FileAuditLogTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"audit-{Guid.NewGuid():N}.log");

    [Fact]
    public void Dispose_ExceptionInsideUsing_FileIsClosed()
    {
        Assert.Throws<DomainRuleException>(() => WriteThenFail(_path));

        // файл закрито: його можна відкрити без спільного доступу
        using (new FileStream(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
        }

        Assert.EndsWith(";book;1", Assert.Single(File.ReadAllLines(_path)), StringComparison.Ordinal);
    }

    [Fact]
    public void Write_AfterDispose_ThrowsObjectDisposed()
    {
        FileAuditLog audit = new(_path);
        audit.Dispose();

        Assert.Throws<ObjectDisposedException>(() => audit.Write("cancel", 1));
    }

    public void Dispose() => File.Delete(_path);

    private static void WriteThenFail(string path)
    {
        using FileAuditLog audit = new(path);
        audit.Write("book", 1);
        throw new DomainRuleException("appointment.exists", "Запис № 7 не знайдено");
    }
}
