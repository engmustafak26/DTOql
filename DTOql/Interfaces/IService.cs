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
        Task<BaseResponseDto<IEnumerable<dynamic>>> GetAsync(Type ListModel, ISearch searchModel);
        Task<BaseResponseDto<dynamic>> GetAsync(Type ListModel, long id);

        Task<BaseResponseDto<object>> AddAsync<T>(T dto) where T : class;
        Task<BaseResponseDto<object>> EditAsync<T>(T dto) where T : class;
        Task<BaseResponseDto<object>> RemoveAsync(object id, bool restore = false);

        Task<BaseResponseDto<object>> SaveRangeAsync<T>(IEnumerable<T> dto) where T : class, IEntityState;

    }
}