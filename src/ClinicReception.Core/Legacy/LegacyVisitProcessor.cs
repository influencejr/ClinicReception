using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Legacy;

public static class LegacyVisitProcessor
{
    public static decimal Process(
        int visitId, string patientName, string patientEmail,
        bool isRegular, List<VisitItem> items,
        DateOnly visitDate, string promoCode, string status,
        bool printToConsole, decimal registrationFee,
        out string newStatus)
    {
        newStatus = status;
        decimal total = 0;
        if (visitId <= 0)
        {
            Console.WriteLine("Помилка: номер візиту");
            return -1;
        }
        if (patientName == null || patientName == "")
        {
            Console.WriteLine("Помилка: немає пацієнта");
            return -1;
        }
        if (patientEmail == null || !patientEmail.Contains("@"))
        {
            Console.WriteLine("Помилка: пошта пацієнта");
            return -1;
        }
        if (items == null || items.Count == 0)
        {
            Console.WriteLine("Помилка: немає послуг");
            return -1;
        }
        if (status != "Scheduled" && status != "Confirmed" &&
            status != "Completed" && status != "Cancelled")
        {
            Console.WriteLine("Помилка: невідомий стан");
            return -1;
        }
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Quantity <= 0)
            {
                Console.WriteLine("Помилка: кількість");
                return -1;
            }
            if (items[i].UnitPrice < 0)
            {
                Console.WriteLine("Помилка: ціна");
                return -1;
            }
            decimal sum = items[i].Quantity * items[i].UnitPrice;
            if (items[i].Quantity >= 10)
                sum = sum * 0.95m;
            total = total + sum;
        }
        if (isRegular)
            total = total * 0.93m;
        if (total > 5000)
            total = total * 0.9m;
        else if (total > 2000)
            total = total * 0.95m;
        if (promoCode == "CHECKUP10")
            total = total - total * 0.1m;
        else if (promoCode == "MINUS200" && total > 1000)
            total = total - 200;
        if (visitDate.DayOfWeek == DayOfWeek.Sunday)
            total = total * 0.98m;
        if (total < 1000)
            total = total + registrationFee;
        if (total < 0)
            total = 0;
        total = Math.Round(total, 2);
        if (status == "Scheduled" && total > 0)
            newStatus = "Confirmed";
        else if (status == "Confirmed")
            newStatus = "Completed";
        else if (status == "Cancelled")
        {
            newStatus = "Cancelled";
            total = 0;
        }
        if (printToConsole)
        {
            Console.WriteLine("Візит № " + visitId);
            Console.WriteLine("Пацієнт: " + patientName);
            Console.WriteLine("Пошта: " + patientEmail);
            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine(items[i].ServiceCode + " x " +
                    items[i].Quantity);
            }
            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }
        return total;
    }
}
