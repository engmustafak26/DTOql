using System;
using System.Collections;
using System.Collections.Generic;

namespace DTOql.Models
{
    public class DataListWithPagingInfo
    {
        public IEnumerable<dynamic> List { get; set; }
        public PagingWithSortModel Paging { get; set; }
    }
}