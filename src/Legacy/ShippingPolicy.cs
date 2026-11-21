namespace ClinicReception.Legacy;

/// <summary>Вартість доставки результатів аналізів додому.</summary>
public sealed class ShippingPolicy
{
    /// <summary>Доставка безкоштовна, якщо сума до сплати не менша за поріг.</summary>
    /// <param name="payable">Сума до сплати після знижки, грн.</param>
    /// <returns>Вартість доставки, грн.</returns>
    public decimal For(decimal payable) =>
        payable < PricingRules.FreeShipFrom ? PricingRules.ShipCost : 0m;
}
