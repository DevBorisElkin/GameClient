using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumsAndData
{
    public static string CODE_SAVED_LOGIN = "$code_saved_login";
    public static string CODE_SAVED_PASSWORD = "$code_saved_password";

    public static string CODE_GRAPHICS_SETTINGS = "code_settings";

    public enum GraphicsSettings { High = 0, Medium = 1, Low = 2 }

    public enum ClientAccessLevel
    {
        LowestLevel = 0,
        Authenticated = 1
    }

    public enum AccessRights
    {
        User,
        Admin,
        SuperAdmin
    }

    public enum ClientStatus { 
        Disconnected,
        Connected,
        Authenticated,
        InPlayRoom 
    }

    // here I will populate different DatabaseRequestResults
    public enum RequestResult
    {
        None = 0,
        Success = 1,
        Fail = 2,
        Fail_NoConnectionToDB = 3,
        Fail_WrongPairLoginPassword = 4,
        Fail_LoginAlreadyTaken = 5,
        Fail_NicknameAlreadyTaken = 6,
        Fail_NoUserWithGivenLogin = 7
    }
    public enum Map { DefaultMap = 0 }
    //public enum Map { DefaultMap = 0, IcyMap = 1, DesertMap = 2 }

    public enum ReasonOfDeath { ByOtherPlayer, Suicide }
    public enum DeathDetails { FellOutOfMap, SalmonRuneFellOutOfMap, TouchedSpikes, SalmonRuneTouchedSpikes, BlackRuneKilled }
    public enum MessageType { Kill, Suicide, Enter, Exit, PickUpRune } // TODO add more events
    public enum MatchState { WaitingForPlayers, InGame, Finished, JustStarting }
    public enum MatchResult { PlayerWon, Draw, Discarded }
    public enum MatchFinishReason { FinishedByKills, FinishedByTime, Discarded }

    public enum Rune
    {
        None = 0,
        Black = 1, // projectile modifier
        SpringGreen = 2, // movement modifier
        DarkGreen = 3, // movement modifier
        LightBlue = 4, // projectile modifier
        Red = 5, // projectile modifier
        Golden = 6, // attack modifier
        RedViolet = 7, // attack modifier
        Salmon = 8 // movement modifier
    }

    public enum MessageParseType { VersionOne, VersionTwo }

    public enum OpponentPointerSettings { Normal, LerpPositionWithSprite, InstantDebugPosition }

    public enum CustomRuneSpawn_Amount
    {
        One,
        Three,
        Five,
        Max
    }
    public enum CustomRuneSpawn_Position
    {
        ClosestSpawn,
        Random
    }
}
