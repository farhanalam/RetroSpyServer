﻿using System;
using System.Collections.Generic;
using GameSpyLib.Logging;
using GameSpyLib.Database;
using GameSpyLib.Extensions;
using RetroSpyServer.XMLConfig;

namespace RetroSpyServer.DBQueries
{
    public class GPCMDBQuery:DBQueryBase
    {
        public GPCMDBQuery():base()
        {          
        }
        /// <summary>
        /// Use DBQueryBase Constructor to create database connection for us
        /// </summary>
        /// <param name="dbdriver"></param>
        public GPCMDBQuery(DatabaseDriver dbdriver) : base(dbdriver)
        {
        }

        protected  Dictionary<string, object> GetUserDataReal(string AppendFirst, string SecondAppend, string _P0, string _P1)
        {
            var Rows = base.dbdriver.Query("SELECT profiles.profileid, profiles.firstname, profiles.lastname, profiles.publicmask, profiles.latitude, profiles.longitude, " +
                "profiles.aim, profiles.picture, profiles.occupationid, profiles.incomeid, profiles.industryid, profiles.marriedid, profiles.childcount, profiles.interests1, " +
                @"profiles.ownership1, profiles.connectiontype, profiles.sex, profiles.zipcode, profiles.countrycode, profiles.homepage, profiles.birthday, profiles.birthmonth, " +
                @"profiles.birthyear, profiles.location, profiles.icq, profiles.status, users.password, users.userstatus " + AppendFirst +
                " FROM profiles INNER JOIN users ON profiles.userid = users.userid WHERE " + SecondAppend, _P0, _P1);
            return (Rows.Count == 0) ? null : Rows[0];
        }

        public Dictionary<string, object> GetProfileInfo( uint id)
        {
            var Rows = base.dbdriver.Query("SELECT profiles.profileid, profiles.firstname, profiles.lastname, profiles.publicmask, profiles.latitude, profiles.longitude, " +
                "profiles.aim, profiles.picture, profiles.occupationid, profiles.incomeid, profiles.industryid, profiles.marriedid, profiles.childcount, profiles.interests1, " +
                @"profiles.ownership1, profiles.connectiontype, profiles.sex, profiles.zipcode, profiles.countrycode, profiles.homepage, profiles.birthday, profiles.birthmonth, " +
                @"profiles.birthyear, profiles.location, profiles.icq, profiles.status, profiles.nick, profiles.uniquenick, users.email FROM profiles " +
                @"INNER JOIN users ON profiles.userid = users.userid WHERE profileid=@P0", id);
            return (Rows.Count == 0) ? null : Rows[0];
        }

        public  Dictionary<string, object> GetUserFromUniqueNick( string Unick)
        {
            return GetUserDataReal(", profiles.nick, users.email ", "profiles.uniquenick=@P0", Unick, "");
        }

        public Dictionary<string, object> GetUserFromNickname( string Email, string Nick)
        {
            return GetUserDataReal(", profiles.uniquenick ", "profiles.nick=@P0 AND users.email=@P1", Nick, Email);
        }

        public bool UserExists( string Nick)
        {
            return (dbdriver.Query("SELECT profileid FROM profiles WHERE `nickname`=@P0", Nick).Count != 0);
        }

        /// <summary>
        /// Creates a new Gamespy Account
        /// </summary>
        /// <remarks>Used by the login server when a create account request is made</remarks>
        /// <param name="dbdriver">The database connection to use</param>
        /// <param name="Nick">The Account Name</param>
        /// <param name="Pass">The UN-HASHED Account Password</param>
        /// <param name="Email">The Account Email Address</param>
        /// <param name="Country">The Country Code for this Account</param>
        /// <param name="UniqueNick">The unique nickname for this Account</param>
        /// <returns>Returns the Player ID if sucessful, 0 otherwise</returns>
        public uint CreateUser( string Nick, string Pass, string Email, string Country, string UniqueNick)
        {
            dbdriver.Execute("INSERT INTO users(email, password) VALUES(@P0, @P1)", Email, StringExtensions.GetMD5Hash(Pass));
            var Rows = dbdriver.Query("SELECT userid FROM users WHERE email=@P0 and password=@P1", Email, Pass);
            if (Rows.Count < 1)
                return 0;

            dbdriver.Execute("INSERT INTO profiles(userid, nick, uniquenick, countrycode) VALUES(@P0, @P1, @P2, @P3)", Rows[0]["userid"], Nick, UniqueNick, Country);
            Rows = dbdriver.Query("SELECT profileid FROM profiles WHERE uniquenick=@P0", UniqueNick);
            if (Rows.Count < 1)
                return 0;

            return uint.Parse(Rows[0]["profileid"].ToString());
        }

        
    }
}