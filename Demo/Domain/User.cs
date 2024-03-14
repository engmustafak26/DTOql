namespace Demo.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }        
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; }

        public int? RoleId { get; set; }
        public Role Role { get; set; }
    }
}
