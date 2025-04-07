using System.Data;
using System.Reflection;
using System.Runtime.Serialization;
using Dapper;
using Hermes.Core.Enums;

namespace Hermes.Data.TypeHandlers
{
    public class RoleTypeHandler : SqlMapper.TypeHandler<Role>
    {
        public override Role Parse(object value)
        {
            string stringValue = value.ToString()!;

            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                var memberAttr = typeof(Role)
                    .GetField(role.ToString())!
                    .GetCustomAttribute<EnumMemberAttribute>();

                if (memberAttr != null && memberAttr.Value == stringValue)
                    return role;
            }

            if (Enum.TryParse<Role>(stringValue, true, out var result))
            {
                return result;
            }

            throw new ArgumentException($"Não foi possível converter '{stringValue}' para o enum Role");
        }

        public override void SetValue(IDbDataParameter parameter, Role value)
        {
            var enumMemberValue = value.GetType()
                .GetField(value.ToString())!
                .GetCustomAttribute<EnumMemberAttribute>()?.Value;

            parameter.Value = $"{enumMemberValue ?? value.ToString()}::role";
            parameter.DbType = DbType.String;
        }
    }
}