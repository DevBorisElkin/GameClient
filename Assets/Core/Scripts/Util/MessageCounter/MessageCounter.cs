using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MessageCounter
{
    static List<MessageCounterInstance> counterInstances = new List<MessageCounterInstance>();
    public static void StartCounting(string key)
    {
        MessageCounterInstance counterInstance = new MessageCounterInstance(key);
        counterInstances.Add(counterInstance);
    }

    public static void UpdateCounter(string key)
    {
        MessageCounterInstance correctInstance = GetInstanceByKey(key);
        if (correctInstance != null)
        {
            correctInstance.counter++;
            if ((DateTime.Now - correctInstance.countInceptionDate).TotalMilliseconds > 1000)
            {
                Debug.Log($"[{key}] Messages count: {correctInstance.counter}");
                correctInstance.countInceptionDate = DateTime.Now;
                correctInstance.counter = 0;
            }
        }
        else Debug.Log($"Error, counter instance for key {key} is null");
    }

    static MessageCounterInstance GetInstanceByKey(string key)
    {
        foreach (var a in counterInstances)
            if (a.key == key) return a;
        return null;
    }

    class MessageCounterInstance
    {
        public string key;
        public DateTime countInceptionDate;
        public int counter;

        public MessageCounterInstance(string key)
        {
            this.key = key;
            countInceptionDate = DateTime.Now;
            counter = 0;
        }
    }
}
