using System.Threading.Tasks;

namespace Instructure.Ods.WebApi.Security.Authorization
{
    public interface IEducationOrganizationCacheInitializer
    {
        Task InitializeAsync(string cacheKey);
    }
}
