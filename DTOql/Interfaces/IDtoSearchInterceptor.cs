using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface IDtoSearchInterceptor<TDto> where TDto : class
    {
        Task ExecuteAsync(TDto dto);
    }
}