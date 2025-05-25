using Dapper;
using System.Data;

namespace ET_Backend.Repository.Event;

/// <summary>
/// <para>Custom <strong>Dapper type-handler</strong> für <see cref="DateOnly"/>.</para>
/// <para>
///  • Schreibt <c>DateOnly</c> als <see cref="DateTime"/> in die DB (SQLite &amp; SQL-Server kompatibel).  
///  • Liest Werte aus <see cref="DateTime"/>, <see cref="string"/> („yyyy-MM-dd[ HH:mm:ss]“)  
///   und <see cref="long"/> (Ticks) sicher ein.
/// </para>
/// </summary>
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    /// <inheritdoc />
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
        => parameter.Value = value.ToDateTime(TimeOnly.MinValue);     // in DB immer als DateTime speichern

    /// <inheritdoc />
    public override DateOnly Parse(object value) => value switch
    {
        DateTime dt => DateOnly.FromDateTime(dt),

        // Unterstützt „2025-05-25“ oder „2025-05-25 00:00:00“
        string s    => DateOnly.Parse(s[..10]),                       

        long ticks  => DateOnly.FromDateTime(new DateTime(ticks)),

        _           => throw new DataException(
                         $"DateOnlyTypeHandler: Typ {value.GetType().Name} kann nicht in DateOnly gewandelt werden.")
    };
}

/// <summary>
/// <para>Custom <strong>Dapper type-handler</strong> für <see cref="TimeOnly"/>.</para>
/// <para>
///  • Speichert <c>TimeOnly</c> als <see cref="DateTime"/> (heutiges Datum + Zeit).  
///  • Liest Werte aus <see cref="DateTime"/>, <see cref="string"/> („HH:mm[:ss]“)  
///   und <see cref="long"/> (Ticks) ein.
/// </para>
/// </summary>
public sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    /// <inheritdoc />
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        => parameter.Value = DateTime.Today.Add(value.ToTimeSpan());  // DateTime mit heutigem Date-Teil

    /// <inheritdoc />
    public override TimeOnly Parse(object value) => value switch
    {
        DateTime dt => TimeOnly.FromDateTime(dt),

        // Unterstützt reine Zeit („10:00:00“) oder DateTime-String („2025-05-25 10:00:00“)
        string s    => TimeOnly.Parse(s.Split(' ').Last()),           

        long ticks  => TimeOnly.FromDateTime(new DateTime(ticks)),

        _           => throw new DataException(
                         $"TimeOnlyTypeHandler: Typ {value.GetType().Name} kann nicht in TimeOnly gewandelt werden.")
    };
}
