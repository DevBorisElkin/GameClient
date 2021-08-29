using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static OnlineGameManager;

public static class MessageParser
{
    // message to players - shows shot data
    // code|posOfShootingPoint|rotationAtRequestTime|ipOfShootingPlayer
    // "shot_result|123/45/87|543/34/1|198.0.0.126";
    public static void ParseOnShotMessage(string message, out Vector3 posOfShot, out Quaternion rotOfShot, out string ipOfPlayerWhoMadeShot)
    {
        try
        {
            string[] substrings = message.Split('|');
            string[] positions = substrings[1].Split('/');
            posOfShot = new Vector3(
                float.Parse(positions[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(positions[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(positions[2], CultureInfo.InvariantCulture.NumberFormat));

            string[] rotations = substrings[2].Split('/');
            rotOfShot = Quaternion.Euler(
                float.Parse(rotations[0], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(rotations[1], CultureInfo.InvariantCulture.NumberFormat),
                float.Parse(rotations[2], CultureInfo.InvariantCulture.NumberFormat)
                );
            ipOfPlayerWhoMadeShot = substrings[3];
        }catch(Exception e)
        {
            Debug.Log(e);
            posOfShot = new Vector3();
            rotOfShot = new Quaternion();
            ipOfPlayerWhoMadeShot = "";
        }
    }
    // "players_positions_in_playroom|nickname,ip,position,rotation@nickname,ip,position,rotation@enc..."
    public static List<PlayerData> ParseOnPositionsMessage(string message)
    {
        try
        {
            List<PlayerData> playersDataList = new List<PlayerData>();
            string[] substrings = message.Split('|');
            string[] playersData = substrings[1].Split('@');
            foreach (string a in playersData)
            {
                string[] subdata = a.Split(',');
                string[] position = subdata[2].Split('/');
                string[] rotation = subdata[3].Split('/');

                Vector3 pos = new Vector3(
                    float.Parse(position[0], CultureInfo.InvariantCulture),
                    float.Parse(position[1], CultureInfo.InvariantCulture),
                    float.Parse(position[2], CultureInfo.InvariantCulture));

                Quaternion rot = Quaternion.Euler(
                    float.Parse(rotation[0], CultureInfo.InvariantCulture),
                    float.Parse(rotation[1], CultureInfo.InvariantCulture),
                    float.Parse(rotation[2], CultureInfo.InvariantCulture));

                PlayerData playerData = new PlayerData();
                playerData.nickname = subdata[0];
                playerData.ip = subdata[1];
                playerData.position = pos; // TODO No initial connection of position with server
                playerData.rotation = rot;

                playersDataList.Add(playerData);
            }
            return playersDataList;
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        return null; 
    }
}
