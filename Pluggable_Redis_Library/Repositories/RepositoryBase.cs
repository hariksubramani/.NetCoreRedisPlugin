using Newtonsoft.Json;
using Pluggable_Redis_Library.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Pluggable_Redis_Library.Repositories
{
    public abstract class RepositoryBase<TKey, TValue> : IRepository<TKey, TValue>
    {
        #region PrivateMembers
        private IDatabase _dbSet;
        private string _redisKey;
        private IRepositoryContext _repositoryContext = null;
        private IApplicationConstants _applicationConstants = null;
        public string RedisKey { get { return _redisKey; } set { _redisKey = value; } }
        #endregion

        #region Constructor
        public RepositoryBase(IRepositoryContext repositoryContext, IApplicationConstants applicationConstants, string redisKey)
        {
            _repositoryContext = repositoryContext ?? throw new ArgumentNullException(nameof(repositoryContext));
            _applicationConstants = applicationConstants;
            _baseContext = repositoryContext.ObjectContext;
            _dbSet = BaseContext.GetDatabase();
            _redisKey = redisKey;
        }
        #endregion

        #region Methods
        public IDatabase DbSet
        {
            get
            {
                return BaseContext.GetDatabase();
            }
        }
        public IDatabase GetDataBase()
        {
            return DbSet;
        }
        public ConnectionMultiplexer _baseContext;
        public ConnectionMultiplexer BaseContext { get { return _baseContext; } }
        private string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);

        }
        private T Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized);


        }
        public void Add(TKey key, TValue value)
        {
            try
            {

                _dbSet.HashSetAsync(RedisKey, Serialize(key), Compressor.Compress(Serialize(value)));
                //_dbSet.HashSetAsync(RedisKey, Serialize(key), Serialize(value));
                _dbSet.KeyExpire(RedisKey, TimeSpan.FromMinutes(_applicationConstants.REDIS_HASHSET_EXPIRY_IN_MINUITES), CommandFlags.FireAndForget);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public bool ContainsKey(TKey key)
        {
            try
            {
                return _dbSet.HashExists(RedisKey, Serialize(key));
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public bool Remove(TKey key)
        {
            try
            {
                return _dbSet.HashDelete(RedisKey, Serialize(key));
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        /// <summary>
        /// Get all values of a table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            try
            {
                var redisValue = _dbSet.HashGet(RedisKey, Serialize(key));
                if (redisValue.IsNull)
                {
                    value = default(TValue);
                    return false;
                }
                // value = Deserialize<TValue>(redisValue.ToString());
                value = Deserialize<TValue>(Compressor.Compress(redisValue.ToString()));
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        /// <summary>
        /// Get Value of each key in table
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetKeyValue(TKey key, out TValue value)
        {
            try
            {
                var redisValue = _dbSet.HashGet(RedisKey, Serialize(key));
                if (redisValue.IsNull)
                {
                    value = default(TValue);
                    return false;
                }
                value = Deserialize<TValue>(Compressor.Decompress(redisValue.ToString()));

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public ICollection<TValue> Values
        {

            get { return new Collection<TValue>(_dbSet.HashValues(_redisKey).Select(h => Deserialize<TValue>(h.ToString())).ToList()); }
        }
        public ICollection<TKey> Keys
        {
            get
            {
                try
                {
                    return new Collection<TKey>(_dbSet.HashKeys(Serialize(_redisKey)).Select(h => Deserialize<TKey>(h.ToString())).ToList());
                }
                catch (Exception ex)
                {

                    throw ex;
                }


            }
        }
        public TValue this[TKey key]
        {
            get
            {
                try
                {
                    var redisValue = _dbSet.HashGet(RedisKey, Serialize(key));
                    return redisValue.IsNull ? default(TValue) : Deserialize<TValue>(redisValue.ToString());
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }
            set
            {
                Add(key, value);
            }
        }
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                Add(item.Key, item.Value);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void Clear()
        {
            try
            {
                _dbSet.KeyDelete(Serialize(_redisKey));
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                return _dbSet.HashExists(Serialize(_redisKey), Serialize(item.Key));
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            try
            {
                _dbSet.HashGetAll(Serialize(_redisKey)).CopyTo(array, arrayIndex);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public int Count
        {
            get
            {
                try
                {
                    return (int)_dbSet.HashLength(Serialize(_redisKey));
                }
                catch (Exception ex)
                {

                    throw ex;
                }

            }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            try
            {
                return Remove(item.Key);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {

            var db = _dbSet;
            foreach (var hashKey in db.HashKeys(Serialize(_redisKey)))
            {
                var redisValue = db.HashGet(Serialize(_redisKey), hashKey);
                yield return new KeyValuePair<TKey, TValue>(Deserialize<TKey>(hashKey.ToString()), Deserialize<TValue>(redisValue.ToString()));
            }


        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return GetEnumerator();
        }
        public void AddMultiple(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            try
            {
                _dbSet.HashSet(Serialize(_redisKey), items.Select(i => new HashEntry(Serialize(i.Key), Serialize(i.Value))).ToArray());
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        #endregion
    }
}
