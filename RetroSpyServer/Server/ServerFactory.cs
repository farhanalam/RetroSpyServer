﻿using System;
using System.Collections.Generic;
using System.Net;
using GameSpyLib.Database;
using GameSpyLib.Network;
using GameSpyLib.Log;

namespace RetroSpyServer.Server
{
    /// <summary>
    /// A factory that create the instance of servers
    /// </summary>
    public class ServerFactory : IDisposable
    {
        protected Dictionary<string, TemplateServer> servers;

        private DatabaseDriver databaseDriver = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ServerFactory()
        {
            servers = new Dictionary<string, TemplateServer>();
        }

        /// <summary>
        /// Default deconstructor
        /// </summary>
        ~ServerFactory()
        {
            Dispose();
        }

        /// <summary>
        /// Creates the server factory
        /// </summary>
        /// <param name="engine">The database engine</param>
        public void Create(DatabaseEngine engine, string connectionString)
        {
            // Determine which database is using and create the database connection
            switch (engine)
            {
                case DatabaseEngine.Mysql:
                    databaseDriver = new MySqlDatabaseDriver(connectionString);
                    break;
                case DatabaseEngine.Sqlite:
                    databaseDriver = new SqliteDatabaseDriver(connectionString);
                    break;
                default:
                    throw new Exception("Unknown database engine!");
            }

            try
            {
                databaseDriver.Connect();
            }
            catch (Exception ex)
            {
                LogWriter.Log.Write(ex.Message, LogLevel.Fatal);
                Environment.Exit(0); // Without database the server cannot start
            }

            LogWriter.Log.Write("Successfully connected to the database!", LogLevel.Info);

            //if (engine == DatabaseEngine.Sqlite)
            //    CreateDatabaseTables();

            // Add all servers
            servers.Add("GPSP", new GPSPServer(databaseDriver));
            servers.Add("GPCM", new GPCMServer(databaseDriver));
        }

        /// <summary>
        /// Free all resources created by the server factory
        /// </summary>
        public void Dispose()
        {
            StopAllServers();
            
            foreach (KeyValuePair<string, TemplateServer> server in servers)
                server.Value.Dispose();

            databaseDriver.Dispose();
        }

        /// <summary>
        /// Stop all servers included in the server factory
        /// </summary>
        public void StopAllServers()
        {
            foreach (KeyValuePair<string, TemplateServer> server in servers)
                server.Value.Stop();
        }

        /// <summary>
        /// Checks if a specific server is running
        /// </summary>
        /// <param name="serverName">The specific server name</param>
        /// <returns></returns>
        public bool IsServerRunning(string serverName)
        {
            serverName = serverName.ToUpper();

            if (!servers.ContainsKey(serverName))
                return false;

            return servers[serverName].IsRunning;
        }

        /// <summary>
        /// Starts a specific server
        /// </summary>
        /// <param name="serverName">The specific server name</param>
        /// <param name="defaultPort">A default port if no port is specified</param>
        public void StartServer(string serverName, int defaultPort)
        {
            string serverIP = "";
            int serverPort = -1, maxConnections = -1;

            serverName = serverName.ToUpper();

            if (XMLConfiguration.ServerConfig.ContainsKey(serverName))
            {
                serverIP = XMLConfiguration.ServerConfig[serverName].ip;
                serverPort = XMLConfiguration.ServerConfig[serverName].port;
                maxConnections = XMLConfiguration.ServerConfig[serverName].maxConnections;
            }

            if (serverIP.Length < 1)
                serverIP = XMLConfiguration.DefaultIP;

            if (serverPort < 1)
                serverPort = defaultPort;

            if (maxConnections < 1)
                maxConnections = XMLConfiguration.DefaultMaxConnections;

            LogWriter.Log.Write("Starting {2} Player Server at {0}:{1}...", LogLevel.Info,serverIP, serverPort, serverName);
            LogWriter.Log.Write("Maximum connections allowed for server {0} are {1}.", LogLevel.Info, serverName, maxConnections);

            if (!servers.ContainsKey(serverName))
                return;

            if (servers[serverName] == null)
                return;

            servers[serverName].Start(serverIP, serverPort, maxConnections);
        }

        /// <summary>
        /// Starts a specific server
        /// </summary>
        /// <param name="serverName">The name of the server</param>
        /// <param name="endPoint">The IP endpoint</param>
        /// <param name="maxConnections">Max number of connections</param>
        public void StartServer(string serverName, IPEndPoint endPoint, int maxConnections)
        {
            serverName = serverName.ToUpper();

            if (!servers.ContainsKey(serverName))
                return;

            if (servers[serverName] == null)
                return; // A server instance cannot be null

            servers[serverName].Start(endPoint, maxConnections);
        }

        /// <summary>
        /// Stop a specific server
        /// </summary>
        /// <param name="serverName">The name of the server</param>
        public void StopServer(string serverName)
        {
            serverName = serverName.ToUpper();

            if (!servers.ContainsKey(serverName))
                return;

            servers[serverName].Stop();
        }


        /// <summary>
        /// Indicates if the server is running or not
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            foreach (KeyValuePair<string, TemplateServer> server in servers)
            {
                if (server.Value.IsRunning)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Creates the tables on the database
        /// </summary>
 //       private void CreateDatabaseTables()
 //       {
 //           databaseDriver.Query(
 //               @"CREATE TABLE IF NOT EXISTS `users` (" +
 //               @"`userid` INTEGER PRIMARY KEY AUTOINCREMENT," +
 //               @"`email` TEXT NOT NULL," +
 //               @"`password` TEXT NOT NULL," +
 //               @"`userstatus` INTEGER DEFAULT(0)" +
 //               @")"
 //           );

 //           databaseDriver.Query(
 //               @"CREATE TABLE IF NOT EXISTS `profiles` (" +
 //               @"`profileid` INTEGER PRIMARY KEY AUTOINCREMENT," +
 //               @"`userid` INTEGER NOT NULL," +
 //               @"`sesskey` INTEGER NOT NULL," +
 //               @"`uniquenick` TEXT NOT NULL," +
 //               @"`nick` TEXT NOT NULL," +
 //               @"`firstname` TEXT DEFAULT NULL," +
 //               @"`lastname` TEXT DEFAULT NULL," +
 //               @"`publicmask` TEXT DEFAULT 0," +
 //               @"`latitude` REAL," +
 //               @"`longitude` REAL," +
 //               @"`aim` TEXT DEFAULT NULL," +
 //               @"`picture` INTEGER DEFAULT 0," +
 //               @"`occupationid` INTEGER DEFAULT 0," +
 //               @"`incomeid` INTEGER DEFUALT 0," +
 //               @"`industryid` INTEGER DEFAULT 0," +
 //               @"`marriedid` INTEGER DEFAULT 0" +
 //               @"`childcount` INTEGER DEFAULT 0," +
 //               @"`interests1` INTEGER DEFAULT 0," +
 //               @"`ownership1` INTEGER DEFAULT 0," +
 //               @"`connectiontype` INTEGER DEFAULT 0," +
 //               @"`sex` TEXT DEFAULT('PAT')," +
 //               @"`zipcode` TEXT DEFAULT('00000')," +
 //               @"`countrycode` TEXT DEFAULT NULL," +
 //               @"`homepage` TEXT DEFAULT NULL," +
 //               @"`birthday` INTEGER DEFAULT 0," +
 //               @"`birthmonth` INTEGER DEFAULT 0," +
 //               @"`birthyear` INTEGER DEFAULT 0," +
 //               @"`location` TEXT DEFAULT NULL," +
 //               @"`icq` INTEGER DEFAULT 0," +
 //               @"`status` INTEGER DEFAULT 0," +
 //               @"`lastip` TEXT," +
 //               @"`lastonline` INTEGER" +
 //               @")"
 //           );

 //           databaseDriver.Query(@"INSERT INTO users(id, email, password, status) VALUES(1, 'spyguy@gamespy.com', '4c3cbcadf7b8a9ae2932afc00560a0d6', 1)");
 //           databaseDriver.Query(@"INSERT INTO `profiles` (`profileid`, `userid`, `uniquenick`, `nick`, `firstname`, `lastname`, `publicmask`, `deleted`, `latitude`, `longitude`, `aim`, `picture`, `occupationid`, `incomeid`, `industryid`, `marriedid`, `childcount`, `interests1`, `ownership1`, `connectiontype`, `sex`, `zipcode`, `countrycode`, `homepage`, `birthday`, `birthmonth`, `birthyear`, `location`, `icq`) VALUES
	//(1, 1, 'SpyGuy', 'SpyGuy', 'Spy', 'Guy', 0, 0, 40.7142, -74.0064, 'spyguy', 0, 0, 0, 0, 0, 0, 0, 0, 3, 'MALE', '10001', 'US', 'https://www.gamespy.com/', 20, 3, 1980, 'New York', 0)");
 //       }
    }
}
