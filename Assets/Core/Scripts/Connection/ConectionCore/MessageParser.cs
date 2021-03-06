using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static OnlineGameManager;
using static EnumsAndData;
using static DataTypes;

public static class MessageParser
{
    public static char GetSplitStringByParseType(MessageParseType messageParseType = MessageParseType.VersionOne)
    {
        if (messageParseType == MessageParseType.VersionOne) return '/';
        else if (messageParseType == MessageParseType.VersionTwo) return ',';
        return '/';
    }

    public static Vector3 StrToVector3(string msg, MessageParseType messageParseType = MessageParseType.VersionOne)
    {
        string[] positions = msg.Split(GetSplitStringByParseType(messageParseType));
        return new Vector3(
            float.Parse(positions[0], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(positions[1], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(positions[2], CultureInfo.InvariantCulture.NumberFormat));
    }

    // message to players - shows shot data
    // code|posOfShootingPoint|rotationAtRequestTime|dbIdOfShootingPlayer|activeRuneModifiers
    // activeRuneModifiers: rune@rune@rune  or "none"
    // "shot_result|123/45/87|543/34/1|13|Black/LightBlue/Red";
    public static void ParseOnShotMessage(string message, out Vector3 posOfShot, out Quaternion rotOfShot, out int dbIdOfPlayerWhoMadeShot, out List<Rune> activeRuneModifiers)
    {
        Debug.Log("OnShotMessage:" + message);
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

            activeRuneModifiers = new List<Rune>();
            if (!substrings[4].Equals("none"))
            {
                string[] runeModifiers = substrings[4].Split('/');
                foreach(var a in runeModifiers)
                {
                    Enum.TryParse(a, out Rune rune);
                    activeRuneModifiers.Add(rune);
                }
            }
        }catch(Exception e)
        {
            Debug.Log(e);
            posOfShot = new Vector3();
            rotOfShot = new Quaternion();
            dbIdOfPlayerWhoMadeShot = -1;
            activeRuneModifiers = new List<Rune>();
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

    public static void ParseOnRuneSpawnedMessage(string message, out Vector3 spawnPosition, out Rune rune, out int runeId, out PlayerData runeInvoker)
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
            string dbIdOfInvoker = msg[4];

            if (dbIdOfInvoker.Equals("none")) runeInvoker = null;
            else
            {
                int dbInvokerId = Int32.Parse(dbIdOfInvoker);
                if (dbInvokerId == OnlineGameManager.instance.current_player.playerData.db_id)
                    runeInvoker = OnlineGameManager.instance.current_player.playerData;
                else
                    runeInvoker = OnlineGameManager.instance.FindPlayerByDbId(dbInvokerId);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            spawnPosition = new Vector3();
            rune = Rune.None;
            runeId = -1;
            runeInvoker = null;
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
    public static List<SpawnedRuneInstance> ParseOnRunesInfoMessage(string message)
    {
        List<SpawnedRuneInstance> runes = new List<SpawnedRuneInstance>();
        try
        {
            string[] msg = message.Split('|');
            string[] runesRaw = msg[1].Split('@');
            foreach(string a in runesRaw)
            {
                string[] runeData = a.Split(',');
                Vector3 runePosition = StrToVector3(runeData[0]);
                Enum.TryParse(runeData[1], out Rune rune);
                int runeId = Int32.Parse(runeData[2]);
                runes.Add(new SpawnedRuneInstance(runePosition, rune, runeId));
            }
        }
        catch (Exception e) { runes = new List<SpawnedRuneInstance>(); Debug.Log(e); }
        return runes;
    }

    //  code|rune_effect_data@rune_effect_data
    // "rune_effects_info|player_db_id,runeType, runeType,runeType@player_db_id,runeType
    public const string RUNE_EFFECTS_INFO = "rune_effects_info";

    public static List<RuneEffectInfo> ParseOnRuneEffectsMessage(string message)
    {
        List<RuneEffectInfo> runeEffects = new List<RuneEffectInfo>();
        try
        {
            string[] msg = message.Split('|');
            string[] runesEffectsRaw = msg[1].Split('@');
            foreach (string a in runesEffectsRaw)
            {
                string[] runeEffectsData = a.Split(',');
                if (runeEffectsData[1].Equals("none")) continue;

                List<Rune> runeEffectsOnPlayer = new List<Rune>();

                int playerDbId = Int32.Parse(runeEffectsData[0]);
                for (int i = 1; i < runeEffectsData.Length; i++)
                {
                    Enum.TryParse(runeEffectsData[i], out Rune rune);
                    runeEffectsOnPlayer.Add(rune);
                }
                runeEffects.Add(new RuneEffectInfo(playerDbId, runeEffectsOnPlayer));
            }
        }
        catch (Exception e) { runeEffects = new List<RuneEffectInfo>(); Debug.Log(e); }
        return runeEffects;
    }

    // code|playerWhoGotDebuffsDbId|LightBlue
    // "player_received_debuffs|12|LightBlue
    public static void ParseOnDebuffMessage(string message, out int playerDbId, Rune debuff)
    {
        try
        {
            string[] msg = message.Split('|');
            playerDbId = Int32.Parse(msg[1]);
            Enum.TryParse(msg[2], out Rune rune);
            debuff = rune;
        }
        catch (Exception e) { playerDbId = -1; debuff = Rune.None; Debug.Log(e); }
    }

    // code|playerWhoGotDebuffsDbId|LightBlue
    // "player_debuff_ended|12|LightBlue
    public static void ParseOnDebuffsMessage(string message, out int playerDbId, out Rune debuff)
    {
        try
        {
            string[] msg = message.Split('|');
            playerDbId = Int32.Parse(msg[1]);
            Enum.TryParse(msg[2], out Rune rune);
            debuff = rune;
        }
        catch (Exception e) { playerDbId = -1; debuff = Rune.None; Debug.Log(e); }
    }

    //  code|windowType|messageType|message
    public static void ParceMessageFromServer(string message, out string messageBody, out MessageFromServer_WindowType windowType,
        out MessageFromServer_MessageType messageType)
    {
        try
        {
            string[] msg = message.Split('|');
            Enum.TryParse(msg[1], out MessageFromServer_WindowType window_type);
            Enum.TryParse(msg[2], out MessageFromServer_MessageType message_type);
            windowType = window_type;
            messageType = message_type;
            messageBody = msg[3];
        }
        catch(Exception e)
        {
            messageBody = "none";
            windowType = MessageFromServer_WindowType.LightWindow;
            messageType = MessageFromServer_MessageType.Info;
        }
    }
}
