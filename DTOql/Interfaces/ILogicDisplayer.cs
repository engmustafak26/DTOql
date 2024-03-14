using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOql.Interfaces
{
    public interface ILogicDisplayer<TDto> where TDto : class
    {
        Task ExecuteAsync(TDto dto);
    }
}

