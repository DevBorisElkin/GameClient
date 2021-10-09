using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static OnlineGameManager;
using static EnumsAndData;

public static class MessageParser
{
    // message to players - shows shot data
    // code|posOfShootingPoint|rotationAtRequestTime|dbIdOfShootingPlayer
    // "shot_result|123/45/87|543/34/1|13";
    public static void ParseOnShotMessage(string message, out Vector3 posOfShot, out Quaternion rotOfShot, out int dbIdOfPlayerWhoMadeShot)
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
            dbIdOfPlayerWhoMadeShot = Int32.Parse(substrings[3]);
        }catch(Exception e)
        {
            Debug.Log(e);
            posOfShot = new Vector3();
            rotOfShot = new Quaternion();
            dbIdOfPlayerWhoMadeShot = -1;
        }
    }
    // "players_positions_in_playroom|nickname,db_id,position,rotation@nickname,db_id,position,rotation@enc..."
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
                playerData.db_id = Int32.Parse(subdata[1]);
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
    public static void ParseOnSpawnDeathParticlesMessage(string message, out Vector3 spawnPosition, out Quaternion spawnRotation)
    {
        try
        {
            string[] msg = message.Split('|');
            string[] position = msg[1].Split('/');
            string[] rotation = msg[2].Split('/');

            spawnPosition = new Vector3(
                float.Parse(position[0], CultureInfo.InvariantCulture),
                float.Parse(position[1], CultureInfo.InvariantCulture),
                float.Parse(position[2], CultureInfo.InvariantCulture)
                );
            spawnRotation = Quaternion.Euler(
                float.Parse(rotation[0], CultureInfo.InvariantCulture),
                float.Parse(rotation[1], CultureInfo.InvariantCulture),
                float.Parse(rotation[2], CultureInfo.InvariantCulture)
                );
        }
        catch (Exception e)
        {
            Debug.Log(e);
            spawnPosition = new Vector3();
            spawnRotation = new Quaternion();
        }
    }

    public static void ParseOnRuneSpawnedMessage(string message, out Vector3 spawnPosition, out Rune rune, out int runeId)
    {
        try
        {
            string[] msg = message.Split('|');
            string[] position = msg[1].Split('/');
            spawnPosition = new Vector3(
                    float.Parse(position[0], CultureInfo.InvariantCulture),
                    float.Parse(position[1], CultureInfo.InvariantCulture),
                    float.Parse(position[2], CultureInfo.InvariantCulture)
                    );
            Enum.TryParse(msg[2], out Rune runeType);
            rune = runeType;
            runeId = Int32.Parse(msg[3]);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            spawnPosition = new Vector3();
            rune = Rune.None;
            runeId = -1;
        }
    }

    public static void ParseOnRunePickedUpMessage(string message, out int runeId, out int playerPicked_db_id, out Rune runeType, out string nickOfGatherPlayer, out float effectDuration)
    {
        try
        {
            string[] msg = message.Split('|');
            runeId = Int32.Parse(msg[1]);
            playerPicked_db_id = Int32.Parse(msg[2]);
            Enum.TryParse(msg[3], out Rune rune);
            runeType = rune;
            nickOfGatherPlayer = msg[4];
            effectDuration = float.Parse(msg[5], CultureInfo.InvariantCulture);
            
        }
        catch (Exception e)
        {
            Debug.Log(e);
            runeId = -1;
            playerPicked_db_id = -1;
            runeType = Rune.None;
            nickOfGatherPlayer = "none";
            effectDuration = 0;
        }
    }

    public static void ParseOnRuneEffectExpiredMessage(string message, out int affectedPlayerDbId, out Rune runeType)
    {
        try
        {
            string[] msg = message.Split('|');
            affectedPlayerDbId = Int32.Parse(msg[1]);
            Enum.TryParse(msg[2], out Rune rune);
            runeType = rune;

        }
        catch (Exception e)
        {
            Debug.Log(e);
            affectedPlayerDbId = -1;
            runeType = Rune.None;
        }
    }
}
