using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Web;
using StackExchange.Redis;
using WebApplication1.Models;

namespace WebApplication1.DataAccess
{
    public class CacheRepository
    {

        private IDatabase db;
        public CacheRepository()
        {
            // var connection = ConnectionMultiplexer.Connect("localhost"); //localhost if cache server is installed locally after downloaded from https://github.com/rgl/redis/downloads 
            // connection to your REDISLABS.com db as in the next line NOTE: DO NOT USE MY CONNECTION BECAUSE I HAVE A LIMIT OF 30MB...CREATE YOUR OWN ACCOUNT
            var connection = ConnectionMultiplexer.Connect("redis-11494.c11.us-east-1-3.ec2.cloud.redislabs.com:11494,password=zxkZT63znNm11suNantC5qI5EUbTyOO7"); //<< connection here should be to your redis database from REDISLABS.COM
            db = connection.GetDatabase();
        }

        /// <summary>
        /// store a list of products in cache
        /// </summary>
        /// <param name="files"></param>
        public void UpdateCache(List<File> files)
        {
            if (db.KeyExists("files") == true)
                db.KeyDelete("files");

            string jsonString;
            jsonString = JsonSerializer.Serialize(files); 

            db.StringSet("files", jsonString);
        }
        /// <summary>
        /// Gets a list of products from cache
        /// </summary>
        /// <returns></returns>
        public List<File> GetFilesFromCache()
        {
            if (db.KeyExists("files") == true)
            {
                var files = JsonSerializer.Deserialize<List<File>>(db.StringGet("files"));
                return files;
            }
            else
            {
                return new List<File>();
            }
        }
    }
}