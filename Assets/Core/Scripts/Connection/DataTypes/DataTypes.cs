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

        public MatchState matchState;
        public int playersToStart;
        public int killsToFinish;
        public int totalTimeToFinishInSeconds;
        public MatchResult matchResult;

        public string winnerNickname;

        public Playroom() { }
        // "confirm_enter_playroom|id/nameOfRoom/is_public/password/map/currentPlayers/maxPlayers/
        // matchState/playersToStart/totalTimeToFinishInSeconds/killsToFinish";

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

                Enum.TryParse(substrings[7], out MatchState _matchState);
                matchState = _matchState;
                playersToStart = Int32.Parse(substrings[8]);
                totalTimeToFinishInSeconds = Int32.Parse(substrings[9]);
                killsToFinish = Int32.Parse(substrings[10]);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
    public class UserData
    {
        public int db_id;
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
        public UserData(int dbID, string login, string password, string nickname, string ip, RequestResult requestResult = RequestResult.Success)
        {
            this.db_id = dbID;
            this.login = login;
            this.password = password;
            this.nickname = nickname;
            this.ip = ip;
            this.requestResult = requestResult;
        }

        public override string ToString()
        {
            return $"db_id:[{db_id}], login:[{login}], password:[{password}], nickname:[{nickname}], ip:[{ip}]";
        }
        public string ToNetworkString()
        {
            return $"{db_id},{login},{password},{nickname},{ip}";
        }
    }

    [System.Serializable]
    public class RuneColor
    {
        public Rune rune;
        public Material material;
    }

    [System.Serializable]
    public class RuneIcon
    {
        public Rune rune;
        public Sprite sprite;
    }

    public class SpawnedRuneInstance
    {
        public Vector3 position;
        public Rune runeType;
        public int uniqueId;

        public SpawnedRuneInstance(Vector3 position, Rune runeType, int uniqueId)
        {
            this.position = position;
            this.runeType = runeType;
            this.uniqueId = uniqueId;
        }
    }

    public class RuneEffectInfo
    {
        public int playerDbId;
        public List<Rune> runeEffects;

        public RuneEffectInfo(int playerDbId, List<Rune> runeEffects)
        {
            this.playerDbId = playerDbId;
            this.runeEffects = runeEffects;
        }
    }

}
