using Demo.Domain;
using DTOql.Enums;
using DTOql.Interfaces;
using DTOql.Models;

namespace Demo.DTO.Organizations
{
    public class OrganizationListDto : IDynamicProperty, IEntityState
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public UserDto[] Users { get; set; }
        public EntityState EntityState { get; set; }

        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            return new DynamicProperty[0];
        }
    }

    public class UserDto : IDynamicProperty, IEntityState
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsOrganizationAdmin { get; set; }
        public string ApiKey { get; set; }


        public UserRolesDto[] UserRoles { get; set; }
        public UserProfileHintDto[] Hints { get; set; }
        public EntityState EntityState { get; set; }

        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            return new DynamicProperty[0];
        }
    }

    public class UserProfileHintDto : IDynamicProperty, IEntityState
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public EntityState EntityState { get; set; }

        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            return new DynamicProperty[0];
        }
    }

    public class UserRolesDto : IDynamicProperty, IEntityState
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public RoleDto Role { get; set; }

        public EntityState EntityState { get; set; }

        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            return new DynamicProperty[0];
        }
    }

    public class RoleDto : IDynamicProperty, IEntityState
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public EntityState EntityState { get; set; }

        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            return new DynamicProperty[0];
        }
    }
}
