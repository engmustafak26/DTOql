using Demo.Domain;
using DTOql.Interfaces;
using DTOql.Models;
using System.Threading;

namespace Demo.DTO.Roles
{
    public class RoleSearchDto : ISearch
    {
        public Dictionary<string, SearchProperty> SearchProperties { get; } = new Dictionary<string, SearchProperty>();

        public int Id { get; set; }
        public string Name { get; set; }     

        public PagingWithSortModel PaginationWithSort { get; set; }

        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            return new DynamicProperty[0];
        }
    }
}


