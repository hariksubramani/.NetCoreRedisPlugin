using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pluggable_Redis_Library.Interfaces
{
    public interface IRepositoryContext
    {
        /// <summary>
        /// Get Radis Db Context here
        /// </summary>
        ConnectionMultiplexer ObjectContext { get; }

        /// <summary>
        /// Terminates the current repository context
        /// </summary>
        void Terminate();
    }
}
