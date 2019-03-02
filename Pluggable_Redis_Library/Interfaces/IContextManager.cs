using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pluggable_Redis_Library.Interfaces
{
    public interface IContextManager
    {
       
        ConnectionMultiplexer GetRepositoryContext(string contextKey);
        void SetRepositoryContext(object repositoryContext, string contextKey);
    }
}
