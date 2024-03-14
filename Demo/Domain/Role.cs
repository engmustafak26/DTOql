using System.Data;
using System.Text.Json.Serialization;

namespace Demo.Domain
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

    }
}
