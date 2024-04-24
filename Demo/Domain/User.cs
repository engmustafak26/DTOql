namespace Demo.Domain
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsOrganizationAdmin { get; set; }
        public string ApiKey { get; set; }


        public ICollection<UserRoles> UserRoles { get; set; }
        public ICollection<UserProfileHint> Hints { get; set; }

        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }

    }
}
