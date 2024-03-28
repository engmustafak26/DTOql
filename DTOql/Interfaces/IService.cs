using DTOql.Continuations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface IService<TEntity> where TEntity : class
    {
        List<ILogicExecuter<TEntity>> Executers { get; }
        void SetLogicExecuters(params ILogicExecuter<TEntity>[] executers);
        Type GetEntityType() => typeof(TEntity);
        Task<DTOqlBaseResponseDto<IEnumerable<dynamic>>> GetAsync(Type ListModel, ISearch searchModel);
        Task<DTOqlBaseResponseDto<dynamic>> GetAsync(Type ListModel, long id);

        Task<DTOqlBaseResponseDto<object>> AddAsync<T>(T dto) where T : class;
        Task<DTOqlBaseResponseDto<object>> EditAsync<T>(T dto) where T : class;
        Task<DTOqlBaseResponseDto<object>> RemoveAsync(object id, bool restore = false);

        Task<DTOqlBaseResponseDto<object>> SaveRangeAsync<T>(IEnumerable<T> dto) where T : class, IEntityState;

    }
}