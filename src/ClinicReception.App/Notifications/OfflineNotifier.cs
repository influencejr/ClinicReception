using ClinicReception.Core.Abstractions;

namespace ClinicReception.App.Notifications;

/// <summary>
/// Сповіщувач, що імітує недоступний поштовий сервер. Потрібен для
/// демонстрації: сервіс має зберегти запис і записати збій у журнал.
/// </summary>
public sealed class OfflineNotifier : INotifier
{
    /// <inheritdoc/>
    /// <exception cref="IOException">Завжди: сервер недоступний.</exception>
    public void Notify(string recipient, string message) =>
        throw new IOException($"Поштовий сервер не відповідає, лист для {recipient} не відправлено.");
}
