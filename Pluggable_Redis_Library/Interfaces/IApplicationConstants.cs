using System;
using System.Collections.Generic;
using System.Text;

namespace Pluggable_Redis_Library.Interfaces
{
  public  interface IApplicationConstants
    {
        string RedisCacheConnection { get; }
        int REDIS_HASHSET_EXPIRY_IN_MINUITES { get; }
    }
}
