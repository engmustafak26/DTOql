using Demo.Domain;
using DTOql.Interfaces;
using DTOql.Models;

namespace Demo.DTO.Requests
{
    public class UserListDto : IDynamicProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }

        public string RoleName { get; set; }

    

        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            yield return new DynamicProperty
            {
                PropertyName = nameof(RoleName),
                SourcePropertyPath = "Role.Name",
                Allias = nameof(RoleName)
            };
        }
    }
}
