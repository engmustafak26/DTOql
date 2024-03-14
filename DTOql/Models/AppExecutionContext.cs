using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOql.Models
{
    public class AppExecutionContext
    {
        public Guid RequestId { get; set; } 
        public void GenerateRequestId()
        {
            RequestId = Guid.NewGuid();
        }
    }
}
