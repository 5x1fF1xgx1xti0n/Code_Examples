using System;
using EdFi.Common.Utils;
using EdFi.Ods.Api.IdentityValueMappers;
using EdFi.Ods.Common.Caching;
using EdFi.Ods.Common.Providers;
using Instructure.Ods.WebApi.Providers;

namespace Instructure.Ods.WebApi.Caching
{
    public class PersonUniqueIdToUsiCache : IPersonUniqueIdToUsiCache
    {
        private readonly IRedisCacheProvider _redisCacheProvider;
        private readonly IEdFiOdsInstanceIdentificationProvider _edFiOdsInstanceIdentificationProvider;

        private readonly IPersonUniqueIdToUsiCacheInitializer _cacheInitializer;
        private readonly IUniqueIdToUsiValueMapper _uniqueIdToUsiValueMapper;

        private readonly TimeSpan _slidingExpiration;
        private readonly TimeSpan _absoluteExpirationPeriod;

        /// <summary>
        /// Provides cached translations between UniqueIds and USI values.
        /// </summary>
        /// <param name="redisCacheProvider">The cache where the database-specific maps (dictionaries) are stored, expiring after 4 hours of inactivity.</param>
        /// <param name="edFiOdsInstanceIdentificationProvider">Identifies the ODS instance for the current call.</param>
        /// <param name="uniqueIdToUsiValueMapper">A component that maps between USI and UniqueId values.</param>
        /// <param name="cacheInitializer">A component that initializes the cache with USI and UniqueId values for a specific person type and context</param>
        /// <param name="slidingExpiration">Indicates how long the cache values will remain in memory after being used before all the cached values are removed.</param>
        /// <param name="absoluteExpirationPeriod">Indicates the maximum time that the cache values will remain in memory before being refreshed.</param>
        public PersonUniqueIdToUsiCache(
            IRedisCacheProvider redisCacheProvider,
            IEdFiOdsInstanceIdentificationProvider edFiOdsInstanceIdentificationProvider,
            IUniqueIdToUsiValueMapper uniqueIdToUsiValueMapper,
            IPersonUniqueIdToUsiCacheInitializer cacheInitializer,
            TimeSpan slidingExpiration,
            TimeSpan absoluteExpirationPeriod)
        {
            _redisCacheProvider = redisCacheProvider;
            _edFiOdsInstanceIdentificationProvider = edFiOdsInstanceIdentificationProvider;
            _uniqueIdToUsiValueMapper = uniqueIdToUsiValueMapper;
            _cacheInitializer = cacheInitializer;

            if (slidingExpiration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slidingExpiration), "TimeSpan cannot be a negative value.");
            }

            if (absoluteExpirationPeriod < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(absoluteExpirationPeriod), "TimeSpan cannot be a negative value.");
            }

            // Use sliding expiration value, if both are set.
            if (slidingExpiration > TimeSpan.Zero && absoluteExpirationPeriod > TimeSpan.Zero)
            {
                absoluteExpirationPeriod = TimeSpan.Zero;
            }

            _slidingExpiration = slidingExpiration;
            _absoluteExpirationPeriod = absoluteExpirationPeriod;
        }

        /// <summary>
        /// Gets the externally defined UniqueId for the specified type of person and the ODS-specific surrogate identifier.
        /// </summary>
        /// <param name="personType">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="usi">The integer-based identifier for the specified representation of the person,
        /// specific to a particular ODS database instance.</param>
        /// <returns>The UniqueId value assigned to the person if found; otherwise <b>null</b>.</returns>
        public string GetUniqueId(string personType, int usi)
        {
            if (usi == default)
            {
                return default;
            }

            int instanceId = _edFiOdsInstanceIdentificationProvider.GetInstanceIdentification();

            string uniqueIdByUsiCacheKey = GetUniqueIdByUsiCacheKey(personType, instanceId);
            string usiByUniqueIdCacheKey = GetUsiByUniqueIdCacheKey(personType, instanceId);

            string key1 = usi.ToString();

            if (_redisCacheProvider.TryGetCachedObjectFromHash(
                uniqueIdByUsiCacheKey,
                key1,
                out string cachedUniqueId))
            {
                return cachedUniqueId;
            }

            if (!_redisCacheProvider.KeyExists(uniqueIdByUsiCacheKey))
            {
                _cacheInitializer.InitializeAsync(
                    personType,
                    uniqueIdByUsiCacheKey,
                    usiByUniqueIdCacheKey,
                    GetAbsoluteExpiration(),
                    _slidingExpiration);
            }

            // Call the value mapper for the individual value
            var valueMap = _uniqueIdToUsiValueMapper.GetUniqueId(personType, usi);

            // Save the value
            if (valueMap.UniqueId != null)
            {
                _redisCacheProvider.InsertToHash(uniqueIdByUsiCacheKey, key1, valueMap.UniqueId);

                string key2 = valueMap.UniqueId;

                _redisCacheProvider.InsertToHash(usiByUniqueIdCacheKey, key2, usi);
            }

            return valueMap.UniqueId;
        }

        /// <summary>
        /// Gets the ODS-specific integer identifier for the specified type of person and their UniqueId value.
        /// </summary>
        /// <param name="personType">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="uniqueId">The UniqueId value associated with the person.</param>
        /// <returns>The ODS-specific integer identifier for the specified type of representation of
        /// the person if found; otherwise 0.</returns>
        public int GetUsi(string personType, string uniqueId)
        {
            var usi = GetUsi(personType, uniqueId, false);
            return usi.GetValueOrDefault();
        }

        /// <summary>
        /// Gets the ODS-specific integer identifier for the specified type of person and their UniqueId value.
        /// </summary>
        /// <param name="personTypeName">The type of the person (e.g. Staff, Student, Parent).</param>
        /// <param name="uniqueId">The UniqueId value associated with the person.</param>
        /// <returns>The ODS-specific integer identifier for the specified type of representation of
        /// the person if found; otherwise <b>null</b>.</returns>
        public int? GetUsiNullable(string personTypeName, string uniqueId)
        {
            var usi = GetUsi(personTypeName, uniqueId, true);

            return usi.HasValue && usi.Value == default
                ? null
                : usi;
        }

        private int? GetUsi(string personType, string uniqueId, bool isNullable)
        {
            if (string.IsNullOrWhiteSpace(uniqueId))
            {
                return isNullable
                    ? default(int?)
                    : default(int);
            }

            int instanceId = _edFiOdsInstanceIdentificationProvider.GetInstanceIdentification();

            string usiByUniqueIdCacheKey = GetUsiByUniqueIdCacheKey(personType, instanceId);
            string uniqueIdByUsiCacheKey = GetUniqueIdByUsiCacheKey(personType, instanceId);

            string key1 = uniqueId;


            if (_redisCacheProvider.TryGetCachedObjectFromHash(
                    usiByUniqueIdCacheKey,
                    key1,
                    out int cachedUsi) &&
                cachedUsi != default)
            {
                return cachedUsi;
            }

            if (!_redisCacheProvider.KeyExists(usiByUniqueIdCacheKey))
            {
                _cacheInitializer.InitializeAsync(
                    personType,
                    uniqueIdByUsiCacheKey,
                    usiByUniqueIdCacheKey,
                    GetAbsoluteExpiration(),
                    _slidingExpiration);
            }

            var valueMap = _uniqueIdToUsiValueMapper.GetUsi(personType, uniqueId);

            // Save the value
            if (valueMap.Usi != default)
            {
                _redisCacheProvider.InsertToHash(usiByUniqueIdCacheKey, key1, valueMap.Usi);

                string key2 = valueMap.Usi.ToString();

                _redisCacheProvider.InsertToHash(uniqueIdByUsiCacheKey, key2, uniqueId);
            }

            return valueMap.Usi;
        }

        private static string GetUsiByUniqueIdCacheKey(string personType, int instanceId)
            => $"IdentityValueMaps_{personType}_UsiByUniqueId_from_{instanceId}";

        private static string GetUniqueIdByUsiCacheKey(string personType, int instanceId)
            => $"IdentityValueMaps_{personType}_UniqueIdByUsi_from_{instanceId}";

        private DateTime GetAbsoluteExpiration() => _absoluteExpirationPeriod == TimeSpan.Zero
            ? DateTime.MaxValue
            : SystemClock.Now().Add(_absoluteExpirationPeriod);
    }
}
