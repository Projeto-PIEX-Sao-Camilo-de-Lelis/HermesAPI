using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hermes.Core.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Role
    {
        [EnumMember(Value = "Admin")]
        [Display(Name = "Administrador")]
        Admin,
        [EnumMember(Value = "User")]
        [Display(Name = "Usuário")]
        User
    }
}
