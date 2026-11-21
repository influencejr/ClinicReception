using ClinicReception.Core.Abstractions;

namespace ClinicReception.Tests.Doubles;

// стаб, що імітує недоступний поштовий сервер: кожен виклик кидає IOException
internal sealed class ThrowingNotifier : INotifier
{
    public void Notify(string recipient, string message) =>
        throw new IOException("Поштовий сервер недоступний.");
}
