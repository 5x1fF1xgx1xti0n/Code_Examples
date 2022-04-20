using System;
using System.Threading.Tasks;

namespace Instructure.Ods.WebApi.Caching
{
    public interface IPersonUniqueIdToUsiCacheInitializer
    {
        Task InitializeAsync(
            string personType,
            string uniqueIdByUsiCacheKey,
            string usiByUniqueIdCacheKey,
            DateTime absoluteExpiration,
            TimeSpan slidingExpiration);
    }
}
