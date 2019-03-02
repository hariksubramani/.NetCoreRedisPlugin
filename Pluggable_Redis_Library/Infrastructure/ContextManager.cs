using Microsoft.AspNetCore.Http;
using Pluggable_Redis_Library.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Pluggable_Redis_Library.Infrastructure
{
    public  class ContextManager : IContextManager
    {
        private static IHttpContextAccessor _httpContext = null;
        private static IApplicationConstants _applicationConstant = null;
        // accessed via lock(_threadDbContexts), only required for multi threaded non web applications
        private static readonly Hashtable _threadDbContexts = new Hashtable();
        private static Lazy<ConnectionMultiplexer> LazyConnection;
        private static Lazy<ConfigurationOptions> configOptions = null;
        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        #region Constructor
        public ContextManager(IHttpContextAccessor httpContext, IApplicationConstants applicationConstant)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _applicationConstant = applicationConstant ?? throw new ArgumentNullException(nameof(applicationConstant));          
            //configure the redis 
            configOptions = new Lazy<ConfigurationOptions>(() =>
            {
                var configOptions = new ConfigurationOptions();
                configOptions.EndPoints.Add(_applicationConstant.RedisCacheConnection);
                configOptions.ClientName = "SafeRedisConnection";
                configOptions.ConnectTimeout = 100000;
                configOptions.SyncTimeout = 100000;
                configOptions.AbortOnConnectFail = false;
                return configOptions;
            });
            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configOptions.Value));
        }
        #endregion


       

        /// <summary>
        /// Sets the repository context
        /// </summary>
        /// <param name="repositoryContext">An object representing the repository context</param>
        public void SetRepositoryContext(object repositoryContext, string contextKey)
        {
            try
            {
                if (repositoryContext == null)
                {
                    RemoveCurrentDbContext(contextKey);
                }
                else if (repositoryContext is ConnectionMultiplexer)
                {
                    StoreCurrentDbContext((ConnectionMultiplexer)repositoryContext, contextKey);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }  


        #region - Insert - Context - Methods
        /// <summary>
        /// sets the current session 		
        /// </summary>
        private static void StoreCurrentDbContext(ConnectionMultiplexer DbContext, string contextKey)
        {
            try
            {
                if (_httpContext.HttpContext == null)
                    StoreCurrentThreadDbContext(DbContext, contextKey);
                else
                    StoreCurrentHttpContextDbContext(DbContext, contextKey);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private static void StoreCurrentThreadDbContext(ConnectionMultiplexer DbContext, string contextKey)
        {
            try
            {
                lock (_threadDbContexts.SyncRoot)
                {
                    if (_threadDbContexts.Contains(contextKey))
                        _threadDbContexts[contextKey] = DbContext;
                    else
                        _threadDbContexts.Add(contextKey, DbContext);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        private static void StoreCurrentHttpContextDbContext(ConnectionMultiplexer DbContext, string contextKey)
        {
            try
            {
                if (_httpContext.HttpContext.Items.ContainsKey(contextKey))
                    _httpContext.HttpContext.Items[contextKey] = DbContext;
                else
                    _httpContext.HttpContext.Items.Add(contextKey, DbContext);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #endregion

        #region - Get - Context - Methods
        /// <summary>
        /// Returns the active object context
        /// </summary>
        public ConnectionMultiplexer GetRepositoryContext(string contextKey)
        {
            try
            {
                ConnectionMultiplexer DbContext = GetCurrentDbContext(contextKey);
                if (DbContext == null) // create and store the object context
                {
                    DbContext = Connection;
                    StoreCurrentDbContext(DbContext, contextKey);
                }
                return DbContext;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        /// <summary>
        /// gets the current object context 		
        /// </summary>
        private static ConnectionMultiplexer GetCurrentDbContext(string contextKey)
        {
            try
            {
                ConnectionMultiplexer DbContext = null;
                if (_httpContext.HttpContext == null)
                    DbContext = GetCurrentThreadDbContext(contextKey);
                else
                    DbContext = GetCurrentHttpContextDbContext(contextKey);
                return DbContext;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        /// <summary>
        /// gets the session for the current thread
        /// </summary>
        private static ConnectionMultiplexer GetCurrentThreadDbContext(string contextKey)
        {
            try
            {
                ConnectionMultiplexer DbContext = null;
                Thread threadCurrent = Thread.CurrentThread;
                if (threadCurrent.Name == null)
                    threadCurrent.Name = contextKey;
                else
                {
                    object threadDbContext = null;
                    lock (_threadDbContexts.SyncRoot)
                    {
                        threadDbContext = _threadDbContexts[contextKey];
                    }
                    if (threadDbContext != null)
                        DbContext = (ConnectionMultiplexer)threadDbContext;
                }
                return DbContext;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }



        /// <summary>
        /// gets the object context for the current thread
        /// </summary>
        private static ConnectionMultiplexer GetCurrentHttpContextDbContext(string contextKey)
        {
            try
            {
                ConnectionMultiplexer DbContext = null;
                if (_httpContext.HttpContext.Items.ContainsKey(contextKey))
                    DbContext = (ConnectionMultiplexer)_httpContext.HttpContext.Items[contextKey];
                return DbContext;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        #endregion

        #region - Delete - Context - Methods
        /// <summary>
        /// remove current object context 		
        /// </summary>
        private static void RemoveCurrentDbContext(string contextKey)
        {
            try
            {
                if (_httpContext.HttpContext == null)
                    RemoveCurrentThreadDbContext(contextKey);
                else
                    RemoveCurrentHttpContextDbContext(contextKey);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private static void RemoveCurrentThreadDbContext(string contextKey)
        {
            try
            {
                lock (_threadDbContexts.SyncRoot)
                {
                    if (_threadDbContexts.Contains(contextKey))
                    {
                        ConnectionMultiplexer DbContext = (ConnectionMultiplexer)_threadDbContexts[contextKey];
                        if (DbContext != null)
                        {
                            DbContext = null;
                            //DbContext.Dispose();
                        }
                        _threadDbContexts.Remove(contextKey);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// remove the session for the currennt HttpContext
        /// </summary>
        private static void RemoveCurrentHttpContextDbContext(string contextKey)
        {
            try
            {
                ConnectionMultiplexer DbContext = GetCurrentHttpContextDbContext(contextKey);
                if (DbContext != null)
                {
                    _httpContext.HttpContext.Items.Remove(contextKey);
                    DbContext = null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #endregion
    }
}
