namespace Demo.Domain
{
    public class Organization
    {
        public int Id { get; set; } 
        public string Name { get; set; }    

        public ICollection<User> Users { get; set; }    
    }
}
