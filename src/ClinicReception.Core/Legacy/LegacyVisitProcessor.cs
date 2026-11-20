using ClinicReception.Core.Billing;
using ClinicReception.Core.Domain;

namespace ClinicReception.Core.Legacy;

public static class LegacyVisitProcessor
{
    public static decimal Process(
        int visitId, string patientName, string patientEmail,
        bool isRegular, IReadOnlyList<VisitItem> items,
        DateOnly visitDate, string promoCode, VisitStatus status,
        bool printToConsole, decimal registrationFee,
        out VisitStatus newStatus)
    {
        VisitGuards.EnsureHeaderValid(visitId, patientName, patientEmail);
        decimal total = VisitTotalCalculator.SubtotalOf(items);
        total -= VisitTotalCalculator.RegularPatientDiscountOf(total, isRegular);
        total -= VisitTotalCalculator.VolumeDiscountOf(total);
        total -= VisitTotalCalculator.PromoDiscountOf(total, promoCode);
        total -= VisitTotalCalculator.SundayDiscountOf(total, visitDate);
        total += VisitTotalCalculator.RegistrationFeeOf(total, registrationFee);
        total = status == VisitStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), 2);
        newStatus = VisitTotalCalculator.NextStatusOf(status, total);
        if (printToConsole)
        {
            Console.WriteLine("Візит № " + visitId);
            Console.WriteLine("Пацієнт: " + patientName);
            Console.WriteLine("Пошта: " + patientEmail);
            foreach (VisitItem item in items)
            {
                Console.WriteLine(item.ServiceCode + " x " + item.Quantity);
            }
            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }
        return total;
    }
}
