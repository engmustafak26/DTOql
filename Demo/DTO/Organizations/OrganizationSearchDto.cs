using Demo.Domain;
using DTOql.Interfaces;
using DTOql.Models;
using System.Threading;

namespace Demo.DTO.Organizations
{
    public class OrganizationSearchDto : ISearch
    {
        public Dictionary<string, SearchProperty> SearchProperties { get; } = new Dictionary<string, SearchProperty>();

        public int Id { get; set; }
        public string Name { get; set; }

        public string UserName { get; set; }
       // public string UserEmail { get; set; }

        public PagingWithSortModel PaginationWithSort { get; set; }

        public IEnumerable<DynamicProperty> GetOverrideProperties()
        {
            return new DynamicProperty[] {
                       new DynamicProperty
                       {
                           PropertyName=nameof(UserName),
                           Allias=nameof(UserName), 
                           SourcePropertyPath= "Users[].Name",
                       }
           };
        }
    }
}


