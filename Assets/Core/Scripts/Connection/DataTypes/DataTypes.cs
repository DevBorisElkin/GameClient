using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using static EnumsAndData;
using System.Linq;

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

        public ReactiveProperty<MatchState> matchState;
        public int playersToStart;
        public int killsToFinish;
        public ReactiveProperty<int> totalTimeToFinishInSeconds;
        public int totalTimeToFinishInSecUnchanged;
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
                matchState = new ReactiveProperty<MatchState>();
                matchState.Value = _matchState;

                playersToStart = Int32.Parse(substrings[8]);

                totalTimeToFinishInSeconds = new ReactiveProperty<int>();
                int timeTillFinish = Int32.Parse(substrings[9]);
                totalTimeToFinishInSeconds.Value = timeTillFinish;

                totalTimeToFinishInSecUnchanged = Int32.Parse(substrings[10]);
                killsToFinish = Int32.Parse(substrings[11]);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
    [System.Serializable]
    public class UserData
    {
        public int db_id;
        public string login;
        public string password;
        public string nickname;
        public string ip;
        public AccessRights accessRights;

        //additional data
        public int total_games;
        public int total_victories;
        public int kills;
        public int deaths;
        public int runes_picked_up;

        public RequestResult requestResult;

        public UserData() { }
        public UserData(RequestResult requestResult)
        {
            this.requestResult = requestResult;
        }
        public UserData(int id, string login, string password, string nickname, string ip, AccessRights accessRights, int totalGames, int totalVictories,
            int kills, int deaths, int runes_picked_up, RequestResult requestResult = RequestResult.Success)
        {
            this.db_id = id;
            this.login = login;
            this.password = password;
            this.nickname = nickname;
            this.ip = ip;
            this.accessRights = accessRights;
            this.requestResult = requestResult;
            this.total_games = totalGames;
            this.total_victories = totalVictories;
            this.kills = kills;
            this.deaths = deaths;
            this.runes_picked_up = runes_picked_up;
        }

        public static UserData ParseUserDataFromString(string raw)
        {
            try
            {
                string[] userData = raw.Split(',');
                AccessRights access;
                try
                {
                    Enum.TryParse(FirstCharToUpper(userData[5]), out AccessRights accessRights);
                    access = accessRights;
                }
                catch (Exception e) { access = AccessRights.User; }

                return new UserData(Int32.Parse(userData[0]), userData[1], userData[2], userData[3], userData[4], access,
                                Int32.Parse(userData[6]), Int32.Parse(userData[7]), Int32.Parse(userData[8]), Int32.Parse(userData[9]), Int32.Parse(userData[10]));
            }
            catch(Exception e)
            {
                Debug.Log(e);
                return null;
            }
        }

        public override string ToString()
        {
            return $"id:[{db_id}], login:[{login}], password:[{password}], nickname:[{nickname}], ip:[{ip}], accessRights:[{accessRights}]" +
                $", totalGames:[{total_games}], totalVictories:[{total_victories}], kills:[{kills}], deaths:[{deaths}], runesPickedUp:[{runes_picked_up}]";
        }
        public string ToNetworkString()
        {
            return $"{db_id},{login},{password},{nickname},{ip},{accessRights},{total_games},{total_victories},{kills},{deaths},{runes_picked_up}";
        }

        public bool IsAdmin()
        {
            if (accessRights.Equals(AccessRights.User)) return false;
            else return true;
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
    public static string FirstCharToUpper(string input)
    {
        if (String.IsNullOrEmpty(input))
            throw new ArgumentException("ARGH!");
        return input.First().ToString().ToUpper() + input.Substring(1);
    }

}
