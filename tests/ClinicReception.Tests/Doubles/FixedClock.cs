namespace ClinicReception.Tests.Doubles;

// фіксований годинник: тест сам задає «зараз», тож результат не залежить від дня запуску
internal sealed class FixedClock : TimeProvider
{
    private readonly DateTimeOffset _now;

    public FixedClock(DateTimeOffset now) => _now = now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => _now;
}
