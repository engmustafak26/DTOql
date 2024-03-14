using DTOql.Enums;
using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface ILogicExecuter<TEntity> where TEntity : class
    {
        EntityState[] ApplyWhenStates { get; }
        Task ExecuteAsync(TEntity entity);
    }
}