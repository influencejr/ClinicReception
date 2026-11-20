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
        VisitGuards.EnsureItemsValid(items);
        newStatus = status;
        decimal total = 0;
        for (int i = 0; i < items.Count; i++)
        {
            decimal sum = items[i].Quantity * items[i].UnitPrice;
            if (items[i].Quantity >= PricingRules.BulkQuantity)
                sum -= sum * PricingRules.BulkRate;
            total = total + sum;
        }
        if (isRegular)
            total -= total * PricingRules.RegularPatientRate;
        if (total > PricingRules.LargeVisitFrom)
            total -= total * PricingRules.LargeVisitRate;
        else if (total > PricingRules.MediumVisitFrom)
            total -= total * PricingRules.MediumVisitRate;
        if (promoCode == PricingRules.CheckupPromoCode)
            total -= total * PricingRules.CheckupPromoRate;
        else if (promoCode == PricingRules.FixedPromoCode && total > PricingRules.FixedPromoFrom)
            total -= PricingRules.FixedPromoAmount;
        if (visitDate.DayOfWeek == DayOfWeek.Sunday)
            total -= total * PricingRules.SundayRate;
        if (total < PricingRules.FreeRegistrationFrom)
            total += registrationFee;
        if (total < 0)
            total = 0;
        total = Math.Round(total, 2);
        if (status == VisitStatus.Scheduled && total > 0)
            newStatus = VisitStatus.Confirmed;
        else if (status == VisitStatus.Confirmed)
            newStatus = VisitStatus.Completed;
        else if (status == VisitStatus.Cancelled)
        {
            newStatus = VisitStatus.Cancelled;
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
