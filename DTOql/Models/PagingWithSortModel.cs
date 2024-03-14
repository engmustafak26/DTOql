using System;
using System.Text.Json.Serialization;

namespace DTOql.Models
{
    public class PagingWithSortModel
    {
        public PagingWithSortModel()
        {
            SortField = "Id";
            SortOrder = SortOrders.Asc;
            CurrentPage = 1;
            PageSize = int.MaxValue;
        }

        public string SortField { get; set; }
        public SortOrders SortOrder { get; set; }
        public int CurrentPage { get; set; }
        public int? TotalRowsCount { get; set; }
        public int? TotalPages => (int)Math.Ceiling((double)((double)TotalRowsCount / (PageSize == default ? 1 : PageSize)));
        public int PageSize { get; set; }
        [JsonIgnore]
        public string SortBy { get { return SortField + " " + SortOrder.ToString(); } }
    }

    public enum SortOrders : byte
    {
        Asc = 1,
        Desc = 2
    }
}