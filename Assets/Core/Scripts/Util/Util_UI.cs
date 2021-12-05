using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static EnumsAndData;

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
            case RequestResult.Fail_WrongPairLoginPassword: return "Can't authenticate: login or password - incorrect";
            case RequestResult.Fail_LoginAlreadyTaken: return "Can't register account because such login is already taken";
            case RequestResult.Fail_NicknameAlreadyTaken: return "Can't register account because such nickname is already taken";
            case RequestResult.Fail_NoUserWithGivenLogin: return "Didn't find user with such login";
            default: return "Response negative";
        }
    }
    static string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$";

    public static bool IsStringEnglishCompatible(string toCheck)
    {
        if (Regex.IsMatch(toCheck, pattern, RegexOptions.IgnoreCase))
            return false;
        else return true;
    }

    public static bool IsStringCompatible(string toCheck)
    {
        Regex rgx = new Regex("[^A-Za-z0-9_]");
        return !(rgx.IsMatch(toCheck));
    }

    public static bool StringStarstsFromNumberOrUnderscore(string toCheck)
    {
        string input = toCheck.Substring(0, 1);
        bool isDigitPresent = input.Any(c => char.IsDigit(c));
        bool startsWithUnderscore = toCheck.StartsWith("_");
        return (isDigitPresent || startsWithUnderscore);
    }

    public enum Input_Field { Login, Password, Nickname, Lobby_Name, Promocode}
    public static bool IsStringClearFromErrors(string stringToCheck, TMP_Text errorField, Input_Field typeOfInput, int MinLength = 5, int MaxLength = 12)
    {
        if (IsStringCompatible(stringToCheck) && stringToCheck.Length >= MinLength && stringToCheck.Length <= MaxLength && !StringStarstsFromNumberOrUnderscore(stringToCheck))
            return true;
        else
        {
            Debug.Log("Test wasn't passed, checking why");
            if (errorField != null)
            {
                string txtToAdd = "";
                switch (typeOfInput)
                {
                    case Input_Field.Login:
                        txtToAdd = "login field";
                        break;
                    case Input_Field.Password:
                        txtToAdd = "password field";
                        break;
                    case Input_Field.Nickname:
                        txtToAdd = "nickname field";
                        break;
                    case Input_Field.Lobby_Name:
                        txtToAdd = "lobby name field";
                        break;
                }

                if (string.IsNullOrEmpty(stringToCheck))
                {
                    errorField.text = txtToAdd + " is empty";
                }
                if (!IsStringCompatible(stringToCheck))
                {
                    errorField.text = txtToAdd + " can contain only english characters and digits";
                }
                else if (stringToCheck.Length < MinLength)
                {
                    errorField.text = $"{txtToAdd} can't be less than {MinLength} characters";
                }
                else if (stringToCheck.Length > MaxLength)
                {
                    errorField.text = $"{txtToAdd} can't be longer than {MaxLength} characters";
                }
                else if (StringStarstsFromNumberOrUnderscore(stringToCheck))
                {
                    errorField.text = $"{txtToAdd} can't start with digit or underscore";
                }
            }
            else
            {
                string txtToAdd = "";
                switch (typeOfInput)
                {
                    case Input_Field.Login: txtToAdd = "Login field"; break;
                    case Input_Field.Password: txtToAdd = "Password field"; break;
                    case Input_Field.Nickname: txtToAdd = "Nickname field"; break;
                    case Input_Field.Lobby_Name: txtToAdd = "Lobby name field"; break;
                    default: txtToAdd = "Input field"; break;
                }
                string completeText = "";

                if (string.IsNullOrEmpty(stringToCheck))
                    completeText = txtToAdd + " is empty";
                if (!IsStringCompatible(stringToCheck))
                    completeText = txtToAdd + " can contain only english characters and digits";
                else if (stringToCheck.Length < MinLength)
                    completeText = $"{txtToAdd} can't be less than {MinLength} characters";
                else if (stringToCheck.Length > MaxLength)
                    completeText = $"{txtToAdd} can't be longer than {MaxLength} characters";
                else if (StringStarstsFromNumberOrUnderscore(stringToCheck))
                    completeText = $"{txtToAdd} can't start with digit or underscore";

                GameObject spawnedObj = Instantiate(PrefabsHolder.instance.ui_msgFromServer_lightWindow);
                var ui = spawnedObj.GetComponent<UI_MessageFromServer_Light>();

                ui.SetUp(completeText, MessageFromServer_MessageType.Error);
            }
            return false;
        }
    }
}

