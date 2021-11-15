using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static BorisUnityDev.Networking.ConnectionUtil;
using UniRx;

public class Ping_Counter : MonoBehaviour
{
    public TMP_Text pingTxt;

    int lastTcpPing;
    int lastUdpPing;

    List<System.IDisposable> LifetimeDisposables;

    private void Start()
    {
        LifetimeDisposables = new List<System.IDisposable>();

        LastTcpPing.Subscribe(_ => { lastTcpPing = _; OnDataReceived(); }).AddTo(LifetimeDisposables);
        LastUdpPing.Subscribe(_ => { lastUdpPing = _; /*OnDataReceived();*/ }).AddTo(LifetimeDisposables);
    }

    private void OnDestroy()
    {
        foreach(var a in LifetimeDisposables)
            a.Dispose();
    }

    void OnDataReceived()
    {
        int averagePing = ((lastTcpPing + lastUdpPing) / 2);
        pingTxt.text = $"PING: {averagePing}";
    }
}
