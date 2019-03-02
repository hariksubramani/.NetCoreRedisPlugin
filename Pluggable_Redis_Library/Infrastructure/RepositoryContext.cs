using System;
using Pluggable_Redis_Library.Interfaces;
using StackExchange.Redis;


namespace Pluggable_Redis_Library.Infrastructure
{
    public class RepositoryContext : IRepositoryContext
    {
        #region PrivateMembers
        // this key is used to identify the context of the objects
        private const string OBJECT_CONTEXT_KEY = "Redis_Objects";
        private IContextManager _contextManager = null;
        #endregion

        #region Constructor
        public RepositoryContext(IContextManager contextManager)
        {
            _contextManager = contextManager ?? throw new ArgumentNullException(nameof(contextManager));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the active object context
        /// </summary>
        public ConnectionMultiplexer ObjectContext
        {
            get
            {
                return _contextManager.GetRepositoryContext(OBJECT_CONTEXT_KEY);
            }
        }

        public void Terminate()
        {
            try
            {
                _contextManager.SetRepositoryContext(null, OBJECT_CONTEXT_KEY);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        #endregion
    }
}
