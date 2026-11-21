namespace ClinicReception.Core.Domain;

/// <summary>
/// Єдине правило заокруглення грошей (D-14): до копійок, половина копійки - від нуля,
/// як у касі. Банківське заокруглення до парного, яке C# бере за замовчуванням, не вживається.
/// </summary>
public static class Money
{
    /// <summary>Кількість знаків після коми: гривні з копійками.</summary>
    public const int DecimalPlaces = 2;

    /// <summary>Заокруглює суму до копійок. Стану не змінює.</summary>
    /// <param name="amount">Сума в гривнях.</param>
    /// <returns>Сума до копійок: 0,125 стає 0,13, а не 0,12.</returns>
    public static decimal Round(decimal amount) =>
        Math.Round(amount, DecimalPlaces, MidpointRounding.AwayFromZero);
}
