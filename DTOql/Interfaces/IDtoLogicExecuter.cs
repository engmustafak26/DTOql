using DTOql.Continuations;
using DTOql.Enums;
using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface IDtoLogicExecuter<TDto> where TDto : class
    {
        public EntityState[] ApplyWhenStates { get; }

        Task<DTOqlBaseResponseDto<object>> ExecuteAsync(TDto dto);
    }
}