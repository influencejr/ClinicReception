using ClinicReception.Core.Abstractions;
using ClinicReception.Core.Domain;

namespace ClinicReception.Tests.Doubles;

// стаб: стану немає, на будь-яку послугу повертає одну й ту саму ціну
internal sealed class StubPricingPolicy : IPricingPolicy
{
    private readonly decimal _price;

    public StubPricingPolicy(decimal price) => _price = price;

    public decimal PriceOf(VisitItem item) => _price;
}
