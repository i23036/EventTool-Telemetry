using Dapper;
using System.Data;

namespace ET_Backend.Repository.Event;


public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }

    public override DateOnly Parse(object value)
    {
        return DateOnly.FromDateTime((DateTime)value);
    }
}

public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.Value = DateTime.Today.Add(value.ToTimeSpan());
    }

    public override TimeOnly Parse(object value)
    {
        return TimeOnly.FromDateTime((DateTime)value);
    }
}
