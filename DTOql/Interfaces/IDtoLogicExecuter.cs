using DTOql.Enums;
using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface IDtoLogicExecuter<TDto> where TDto : class
    {
        public EntityState[] ApplyWhenStates { get; }

        Task ExecuteAsync(TDto dto);
    }
}