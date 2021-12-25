using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static EnumsAndData;

public class WinScreen : MonoBehaviour
{
    public Transform holderForItems;
    public GameObject winScreenPanel;
    public TMP_Text upperText;

    public void SetUpResults(string message)
    {
        winScreenPanel.SetActive(true);
        ClearHolder();

        EventManager.isAlive = false;
        string[] substrings = message.Split('|');
        int winnerDbId = -1;
        Enum.TryParse(substrings[3], out MatchResult _res);
        if (!_res.Equals(MatchResult.Discarded))
        {
            winnerDbId = Int32.Parse(substrings[1]);
            ConnectionManager.activePlayroom.winnerNickname = substrings[2];
        }
        ConnectionManager.activePlayroom.matchState.Value = MatchState.Finished;
        ConnectionManager.activePlayroom.matchResult = _res;
        
        List<EndResultItem> items = GenerateMatchResultsFromString(substrings[4], winnerDbId);

        int i = 1;
        foreach(var a in items)
        {
            GameObject resultItem = Instantiate(PrefabsHolder.instance.matchFinishedStatsItem_prefab, holderForItems);
            var holder = resultItem.GetComponent<WinScreenHolderForStats>();
            holder.SetUp(i.ToString(), a.nickName, a.killsCount.ToString(), a.deathsCount.ToString(), a.runesCount.ToString(), a.isWinner);
            i++;
        }

        upperText.text = MatchResultToString(_res, substrings[2]);
    }

    public void ClearHolder()
    {
        List<Transform> itemsToDelete = new List<Transform>();
        for (int i = 0; i < holderForItems.childCount; i++)
            itemsToDelete.Add(holderForItems.GetChild(i));

        for (int i = 0; i < itemsToDelete.Count; i++)
            Destroy(itemsToDelete[i].gameObject);
    }

    public List<EndResultItem> GenerateMatchResultsFromString(string message, int winnerDbId)
    {
        List<EndResultItem> items = new List<EndResultItem>();
        string[] resultPerPlayer = message.Split('@');
        foreach(var a in resultPerPlayer)
        {
            string[] data = a.Split(',');
            int dbId = Int32.Parse(data[0]);
            string nickname = data[1];
            int killsCount = Int32.Parse(data[2]);
            int deathsCount = Int32.Parse(data[3]);
            int runesCount = Int32.Parse(data[4]);
            EndResultItem item = new EndResultItem(dbId, nickname, killsCount, deathsCount, runesCount, dbId == winnerDbId);
            items.Add(item);
        }
        List<EndResultItem> sortedItems = items.OrderByDescending(a => a.killsCount).ToList();
        return sortedItems;
    }

    static string MatchResultToString(MatchResult res, string winner = "")
    {
        if (res == MatchResult.PlayerWon)
        {
            return $"The Match Is Finished! {winner} Is Victorious!";
        }
        else if (res == MatchResult.Draw)
        {
            return "The Match Is Finished! DRAW.";
        }
        else if (res == MatchResult.Discarded)
        {
            return "For a number of reasons the match was discarded.";
        }
        return "";
    }

    public class EndResultItem
    {
        public int dbId;
        public string nickName;
        public int killsCount;
        public int deathsCount;
        public int runesCount;
        public bool isWinner;

        public EndResultItem(int dbId, string nickName, int killsCount, int deathsCount, int runesCount, bool isWinner)
        {
            this.dbId = dbId;
            this.nickName = nickName;
            this.killsCount = killsCount;
            this.deathsCount = deathsCount;
            this.runesCount = runesCount;
            this.isWinner = isWinner;
        }
    }
}
