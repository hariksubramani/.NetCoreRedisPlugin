# .NetCoreRedisPlugin
This is the plugin which will be used to the people who want to use the redis for caching
The code is ready to use in production, just do the below changes in appsettings.json
"ConnectionStrings": {
    "RedisCacheConnection": "localhost:6379"
  }
 Replace the localhost:6379 with your redis server connection string
 
   "RedisSettings": {
    "REDIS_HASHSET_EXPIRY_IN_MINUITES": "120"
    }
    
   You can replace 120 with any number which best suits your requirement for cache expiration
  
   You can create the override the servicebase class as per your requirement and use the service whereever it is required.
