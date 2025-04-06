using System.Data;
using Dapper;

namespace Hermes.Data.TypeHandlers
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateTime dateTime => DateOnly.FromDateTime(dateTime),
                string dateString => DateOnly.Parse(dateString),
                _ => throw new ArgumentException($"Não foi possível converter {value} para DateOnly.")
            };
        }
        
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }
    }
}
