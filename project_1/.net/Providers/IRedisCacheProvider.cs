using System;
using System.Collections.Generic;
using EdFi.Ods.Common.Caching;

namespace Instructure.Ods.WebApi.Providers
{
    public interface IRedisCacheProvider : ICacheProvider
    {
        bool TryGetCachedObjectFromHash<T>(string key, string hashField, out T value);

        void InsertToHash(string key, string hashField, object value);

        void Insert<T>(
            string key,
            IDictionary<string, T> dictionary,
            DateTime absoluteExpiration,
            TimeSpan slidingExpiration);

        bool KeyExists(string key);

        bool TryGetCachedObject<T>(string key, out T value, int db = 0);

        void Insert(string key,
            object value,
            DateTime absoluteExpiration,
            TimeSpan slidingExpiration,
            int db = 0);
    }
}
