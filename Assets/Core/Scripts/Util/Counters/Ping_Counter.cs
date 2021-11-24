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

    bool activated;
    private void Update()
    {
        if (activated) return;

        SetUpPingCounters();
    }

    void SetUpPingCounters()
    {
        if (LastTcpPing != null && LastUdpPing != null)
        {
            LifetimeDisposables = new List<System.IDisposable>();
            LastTcpPing.Subscribe(_ => { lastTcpPing = _; OnDataReceived(); }).AddTo(LifetimeDisposables);
            LastUdpPing.Subscribe(_ => { lastUdpPing = _; /*OnDataReceived();*/ }).AddTo(LifetimeDisposables);
            activated = true;
        }
    }

    private void OnDestroy()
    {
        if(LifetimeDisposables != null && LifetimeDisposables.Count > 0)
            foreach(var a in LifetimeDisposables)
                a.Dispose();
    }

    void OnDataReceived()
    {
        int averagePing = ((lastTcpPing + lastUdpPing) / 2);
        pingTxt.text = $"PING: {averagePing}";
    }
}
