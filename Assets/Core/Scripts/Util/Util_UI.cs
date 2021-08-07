using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class Util_UI : MonoBehaviour
{
    public static string InternetRequestResultToString(RequestResult result)
    {
        switch (result)
        {
            case RequestResult.None: return "Response negative";
            case RequestResult.Success: return "Response is positive";
            case RequestResult.Fail: return "Failed to perform operation";
            case RequestResult.Fail_NoConnectionToDB: return "Couldn't read data, no database connection";
            case RequestResult.Fail_WrongPairLoginPassword: return "Given login and password are incorrect";
            case RequestResult.Fail_LoginAlreadyTaken: return "Can't register account with such login because it's already taken";
            case RequestResult.Fail_NoUserWithGivenLogin: return "Didn't find user with such login";
            default: return "Response negative";
        }
    }
}
