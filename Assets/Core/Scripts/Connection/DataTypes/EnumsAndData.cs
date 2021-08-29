using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnumsAndData
{
    public static string CODE_SAVED_LOGIN = "$code_saved_login";
    public static string CODE_SAVED_PASSWORD = "$code_saved_password";


    public enum ClientAccessLevel
    {
        LowestLevel = 0,
        Authenticated = 1
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
        Fail_NoUserWithGivenLogin = 6
    }
    public enum Map { DefaultMap = 0 }
    //public enum Map { DefaultMap = 0, IcyMap = 1, DesertMap = 2 }
}
