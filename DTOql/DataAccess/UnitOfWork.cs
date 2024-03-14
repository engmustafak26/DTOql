using DTOql.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DTOql.DataAccess
{ 
    public class DbUnitOfWork : IUnitOfWork
    {
        #region Properties

        public DbContext DatabaseContext { get; private set; }

        #endregion Properties

        #region ctor

        public DbUnitOfWork(DbContext databaseContext)
        {
            DatabaseContext = databaseContext;
        }

        #endregion ctor

        #region Execute

        public int SaveChanges()
        {
            return DatabaseContext.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return DatabaseContext.SaveChangesAsync();
        }

        #endregion Execute
    }
}