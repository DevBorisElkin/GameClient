using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Playroom(){ }
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
        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }

    }

    public enum Map { DefaultMap }
}
