using Dapper;
using System.Data;

namespace ET_Backend.Repository.Event;

// ---------- DateOnly ----------
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
        => parameter.Value = value.ToDateTime(TimeOnly.MinValue);     // als DateTime speichern

    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateTime dt        => DateOnly.FromDateTime(dt),
            string   s         => DateOnly.Parse(s[..10]),            // "2025-05-25" oder "2025-05-25 00:00:00"
            long     ticks     => DateOnly.FromDateTime(new DateTime(ticks)),
            _                  => throw new DataException(
                $"Cannot convert {value.GetType()} to DateOnly")
        };
    }
}

// ---------- TimeOnly ----------
public sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        => parameter.Value = DateTime.Today.Add(value.ToTimeSpan());  // als DateTime speichern

    public override TimeOnly Parse(object value)
    {
        return value switch
        {
            DateTime dt        => TimeOnly.FromDateTime(dt),
            string   s         => TimeOnly.Parse(s.Split(' ').Last()), // "10:00:00" aus evtl. '2025-05-25 10:00:00'
            long     ticks     => TimeOnly.FromDateTime(new DateTime(ticks)),
            _                  => throw new DataException(
                $"Cannot convert {value.GetType()} to TimeOnly")
        };
    }
}