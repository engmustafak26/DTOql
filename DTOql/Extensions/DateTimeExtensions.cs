using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOql.Extensions
{
    public static class DateTimeExtensions
    {

        public static long ConvertToUniversalTimeStamp(this DateTime date, bool toMilliseconds = true)
        {
            var toUtcDate = date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            return toMilliseconds ? (long)toUtcDate.TotalMilliseconds : (long)toUtcDate.TotalSeconds;
        }


    }
}
