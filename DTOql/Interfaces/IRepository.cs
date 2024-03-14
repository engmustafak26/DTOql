using DTOql.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {

        List<ILogicExecuter<TEntity>> Executers { get; }
        void SetLogicExecuters(params ILogicExecuter<TEntity>[] executers);

        Task<IEnumerable<dynamic>> GetAsync(Type ListModel, ISearch searchModel);
        Task<dynamic?> GetAsync(Type ListModel, long id);
        void Add(TEntity entity); 
        void Edit(TEntity entity);
        void Remove(TEntity entity);

    }
}