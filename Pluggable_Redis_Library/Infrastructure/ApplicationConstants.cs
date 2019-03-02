using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pluggable_Redis_Library.Infrastructure
{
    /// <summary>
    /// This class is responsible for reading the values and assigning it to the properties which can be used in various places
    /// </summary>
    public class ApplicationConstants
    {
        #region PrivateMembers
        private IConfiguration _configuration = null;
        #endregion

        #region Constructor
        public ApplicationConstants(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
        }
        #endregion

        #region Public Properties
        // Connection string of your redis cache
        public string RedisCacheConnection => _configuration.GetConnectionString("RedisCacheConnection");
            
        //cache expiration time
        public int REDIS_HASHSET_EXPIRY_IN_MINUITES => Convert.ToInt32(_configuration.
            GetSection("AppSettings:REDIS_HASHSET_EXPIRY_IN_MINUITES").Value);
      
        #endregion
    }
}
