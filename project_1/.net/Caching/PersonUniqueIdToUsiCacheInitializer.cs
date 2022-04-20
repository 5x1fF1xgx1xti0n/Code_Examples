using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EdFi.Ods.Api.IdentityValueMappers;
using EdFi.Ods.Common.Specifications;
using Instructure.Ods.WebApi.Providers;
using log4net;

namespace Instructure.Ods.WebApi.Caching
{
    internal class PersonUniqueIdToUsiCacheInitializer : IPersonUniqueIdToUsiCacheInitializer
    {
        private readonly IRedisCacheProvider _redisCacheProvider;
        private readonly IPersonIdentifiersProvider _personIdentifiersProvider;
        private readonly ILog _logger = LogManager.GetLogger(typeof(PersonUniqueIdToUsiCacheInitializer));
        private readonly ConcurrentDictionary<string, bool> _initializationsInProgress =
            new ConcurrentDictionary<string, bool>();

        public PersonUniqueIdToUsiCacheInitializer(
            IRedisCacheProvider redisCacheProvider,
            IPersonIdentifiersProvider personIdentifiersProvider)
        {
            _redisCacheProvider = redisCacheProvider;
            _personIdentifiersProvider = personIdentifiersProvider;
        }

        public async Task InitializeAsync(
            string personType,
            string uniqueIdByUsiCacheKey,
            string usiByUniqueIdCacheKey,
            DateTime absoluteExpiration,
            TimeSpan slidingExpiration)
        {
            // Validate Person type
            if (!PersonEntitySpecification.IsPersonEntity(personType))
            {
                string validPersonTypes =
                    "'" + string.Join("','", PersonEntitySpecification.ValidPersonTypes) + "'";

                throw new ArgumentException(
                    $"Invalid person type '{personType}'. Valid person types are: {validPersonTypes}");
            }

            if (!_initializationsInProgress.TryAdd(
                $"{personType}_{uniqueIdByUsiCacheKey}_{usiByUniqueIdCacheKey}", true))
            {
                return;
            }

            await InitializePersonTypeValueMapsAsync(
                personType,
                uniqueIdByUsiCacheKey,
                usiByUniqueIdCacheKey,
                absoluteExpiration,
                slidingExpiration);

            _initializationsInProgress.TryRemove(
                $"{personType}_{uniqueIdByUsiCacheKey}_{usiByUniqueIdCacheKey}", out bool _);
        }

        private async Task InitializePersonTypeValueMapsAsync(
            string personType,
            string uniqueIdByUsiCacheKey,
            string usiByUniqueIdCacheKey,
            DateTime absoluteExpiration,
            TimeSpan slidingExpiration)
        {
            try
            {
                // Start building the data
                var uniqueIdByUsi = new Dictionary<string, string>();
                var usiByUniqueId = new Dictionary<string, int>();

                Stopwatch stopwatch = null;

                if (_logger.IsDebugEnabled)
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                }

                foreach (
                    var valueMap in await _personIdentifiersProvider.GetAllPersonIdentifiers(personType))
                {
                    string key1 = valueMap.Usi.ToString();
                    uniqueIdByUsi.TryAdd(key1, valueMap.UniqueId);

                    string key2 = valueMap.UniqueId;
                    usiByUniqueId.TryAdd(key2, valueMap.Usi);
                }

                if (_logger.IsDebugEnabled)
                {
                    stopwatch.Stop();

                    _logger.DebugFormat(
                        "UniqueId/USI cache for {0} initialized {1:n0} entries in {2:n0} milliseconds.",
                        personType,
                        uniqueIdByUsi.Count,
                        stopwatch.ElapsedMilliseconds);
                }

                _redisCacheProvider.Insert(
                    usiByUniqueIdCacheKey,
                    usiByUniqueId,
                    absoluteExpiration,
                    slidingExpiration);

                _redisCacheProvider.Insert(
                    uniqueIdByUsiCacheKey,
                    uniqueIdByUsi,
                    absoluteExpiration,
                    slidingExpiration);
            }
            catch (Exception ex)
            {
                _logger.Error(
                    "An exception occurred while trying to warm the PersonCache. UniqueIds will be retrieved individually.",
                    ex);
            }
        }
    }
}
