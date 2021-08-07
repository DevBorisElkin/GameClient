using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Enums
{
    public enum ClientAccessLevel
    {
        LowestLevel = 0,
        Authenticated = 1
    }

    public enum ClientStatus { 
        Disconnected,
        Connected,
        Authenticated,
        WaitingToGetAcceptedToPlayroom,
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
}
