namespace ClinicReception.Core.Abstractions;

/// <summary>Сповіщення пацієнта. Сервіс не знає, лист це, SMS чи запис у журнал.</summary>
public interface INotifier
{
    /// <summary>Надсилає повідомлення адресатові.</summary>
    /// <param name="recipient">Адреса пацієнта.</param>
    /// <param name="message">Текст повідомлення.</param>
    void Notify(string recipient, string message);
}
