using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MessageCounter
{
    static DateTime countInceptionDate;
    static string key;
    public static void StartCounting(string key)
    {
        MessageCounter.key = key;
        countInceptionDate = DateTime.Now;
        counter = 0;
    }

    static int counter;
    public static void UpdateCounter()
    {
        counter++;

        if((DateTime.Now - countInceptionDate).TotalMilliseconds > 1000)
        {
            Debug.Log($"[{key}] Messages count: {counter}");
            countInceptionDate = DateTime.Now;
            counter = 0;
        }
    }
    
}
