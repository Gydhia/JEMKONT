using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Analytics;
using GameAnalyticsSDK;

public class AnalyticsManager : Singleton<AnalyticsManager>
{
    #region EventsNames
    public static readonly string Custom_Player_Per_Lobby = "Custom_Player_Per_Lobby";
    public static readonly string Custom_Tasks_Done_By_Class = "Custom_Tasks_Done_By_Class";
    public static readonly string Custom_Map_Exploration = "Custom_Map_Exploration";
    public static readonly string Custom_Time_In_Shop = "Custom_Time_In_Shop";
    public static readonly string Custom_Dropped_Item_Quantity = "Custom_Dropped_Item_Quantity";
    #endregion
    
    
    private void Start()
    {
        GameAnalytics.Initialize();
    }

    public void SendEventPlayerPerLobbyEvent(int playersCount)
    {
        Dictionary<string, object> players = new Dictionary<string, object>();
        players.Add("players", playersCount);
        GameAnalytics.NewDesignEvent(Custom_Player_Per_Lobby, players);
    }
    
    public void SendEventTasksDoneByClass(int tasksDone)
    {
        Dictionary<string, object> tasks = new Dictionary<string, object>();
        tasks.Add("tasks", tasksDone);
        GameAnalytics.NewDesignEvent(Custom_Player_Per_Lobby, tasks);
    }
    
    public void SendEventMapExploration(List<Vector2> casesCoordinates)
    {
        Dictionary<string, object> coordinates = new Dictionary<string, object>();
        coordinates.Add("coordinates", casesCoordinates);
        GameAnalytics.NewDesignEvent(Custom_Player_Per_Lobby, coordinates);
    }
    
    public void SendEventTimeInShop(float time)
    {
        Dictionary<string, object> timeInShop = new Dictionary<string, object>();
        timeInShop.Add("coordinates", time);
        GameAnalytics.NewDesignEvent(Custom_Player_Per_Lobby, timeInShop);
    }
    
}
