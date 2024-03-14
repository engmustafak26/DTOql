using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface ILogicFetcher<TDto> where TDto : class
    {
        Task<IEnumerable<dynamic>> ExecuteAsync();
    }
}

