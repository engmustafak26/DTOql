using Demo.Domain;
using DTOql.Enums;
using DTOql.Interfaces;
using DTOql.Models;

namespace Demo.DTO.Roles
{
    public class RoleListDto : IDynamicProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            return new DynamicProperty[0];
        }
    }   
}
