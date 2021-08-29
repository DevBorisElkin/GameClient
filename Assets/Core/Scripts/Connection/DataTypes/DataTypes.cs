using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;

public static class DataTypes
{
    public class Playroom
    {
        //        0      1         2         3     4        5             6
        // data: id/nameOfRoom/is_public/password/map/currentPlayers/maxPlayers
        public int id;
        public string name;
        public bool isPublic;
        public Map map = Map.DefaultMap;
        public int playersCurrAmount;
        public int maxPlayers;

        public Playroom() { }
        public Playroom(string createFrom)
        {
            try
            {
                string[] substrings = createFrom.Split('/');
                id = Int32.Parse(substrings[0]);
                name = substrings[1];
                isPublic = bool.Parse(substrings[2]);

                Enum.TryParse(substrings[4], out Map _map);
                map = _map;
                playersCurrAmount = Int32.Parse(substrings[5]);
                maxPlayers = Int32.Parse(substrings[6]);

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

        }
    }
    public class UserData
    {
        public int id;
        public string login;
        public string password;
        public string nickname;
        public string ip;

        public RequestResult requestResult;

        public UserData() { }
        public UserData(RequestResult requestResult)
        {
            this.requestResult = requestResult;
        }
        public UserData(int id, string login, string password, string nickname, string ip, RequestResult requestResult = RequestResult.Success)
        {
            this.id = id;
            this.login = login;
            this.password = password;
            this.nickname = nickname;
            this.ip = ip;
            this.requestResult = requestResult;
        }

        public override string ToString()
        {
            return $"id:[{id}], login:[{login}], password:[{password}], nickname:[{nickname}], ip:[{ip}]";
        }
        public string ToNetworkString()
        {
            return $"{id},{login},{password},{nickname},{ip}";
        }
    }
}
