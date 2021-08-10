using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumsAndData;

public class UserData
{
    public int id;
    public string login;
    public string password;
    public string nickname;

    public RequestResult requestResult;

    public UserData() { }
    public UserData(RequestResult requestResult)
    {
        this.requestResult = requestResult;
    }
    public UserData(int id, string login, string password, string nickname, RequestResult requestResult = RequestResult.Success)
    {
        this.id = id;
        this.login = login;
        this.password = password;
        this.nickname = nickname;
        this.requestResult = requestResult;
    }

    public override string ToString()
    {
        return $"id:[{id}], login:[{login}], password:[{password}], nickname:[{nickname}]";
    }
    public string ToNetworkString()
    {
        return $"{id},{login},{password},{nickname}";
    }
}

