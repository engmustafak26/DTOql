using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface IUnitOfWork
    {
        int SaveChanges();
        Task<int> SaveChangesAsync();
    }
}