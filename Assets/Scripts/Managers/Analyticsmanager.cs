using System;
using System.Collections;
using System.Collections.Generic;
using DownBelow.Managers;
using UnityEngine;
using UnityEngine.Analytics;
using GameAnalyticsSDK;

public class AnalyticsManager : _baseManager<AnalyticsManager>
{
    #region EventsNames
    public static readonly string Custom_Player_Per_Lobby = "Custom_Player_Per_Lobby";
    public static readonly string Custom_Tasks_Done_By_Class = "Custom_Tasks_Done_By_Class";
    public static readonly string Custom_Map_Exploration = "Custom_Map_Exploration";
    public static readonly string Custom_Time_In_Shop = "Custom_Time_In_Shop";
    public static readonly string Custom_Dropped_Item_Quantity = "Custom_Dropped_Item_Quantity";
    public static readonly string Custom_Session_Length = "Custom_Dropped_Item_Quantity";
    public static readonly string Custom_Used_Card = "Custom_Used_Card";
    public static readonly string Custom_Damage_Dealt_As_Minor = "Custom_Damage_Dealt_As_Minor";
    public static readonly string Custom_Damage_Dealt_As_Farmer = "Custom_Damage_Dealt_As_Farmer";
    public static readonly string Custom_Damage_Dealt_As_Herborist = "Custom_Damage_Dealt_As_Herborist";
    public static readonly string Custom_Damage_Dealt_As_Fisherman = "Custom_Damage_Dealt_As_Fisherman";
    
    
    #endregion
    
    public void Init()
    {
        base.Awake();

        GameAnalytics.Initialize();
    }
    
    public void SendEventPlayerPerLobbyEvent(int playersCount)
    {
        Dictionary<string, object> players = new Dictionary<string, object>();
        players.Add("Players", playersCount);
        GameAnalytics.NewDesignEvent(Custom_Player_Per_Lobby, players);
    }
    
    public void SendEventTasksDoneByClass(int tasksDone)
    {
        Dictionary<string, object> tasks = new Dictionary<string, object>();
        tasks.Add("Tasks", tasksDone);
        GameAnalytics.NewDesignEvent(Custom_Tasks_Done_By_Class, tasks);
    }
    
    public void SendEventMapExploration(List<Vector2> casesCoordinates)
    {
        Dictionary<string, object> coordinates = new Dictionary<string, object>();
        coordinates.Add("CoordinatesTraveled", casesCoordinates);
        GameAnalytics.NewDesignEvent(Custom_Map_Exploration, coordinates);
    }
    
    public void SendEventTimeInShop(float time)
    {
        Dictionary<string, object> timeInShop = new Dictionary<string, object>();
        timeInShop.Add("TimeInShop", time);
        GameAnalytics.NewDesignEvent(Custom_Time_In_Shop, timeInShop);
    }
    
    public void SendEventSessionLength(float time)
    {
        Dictionary<string, object> timeInShop = new Dictionary<string, object>();
        timeInShop.Add("TimeInSession", time);
        GameAnalytics.NewDesignEvent(Custom_Session_Length, timeInShop);
    }
    
    public void SendUsedCardEvent(string ID)
    {
        Dictionary<string, object> usedCard = new Dictionary<string, object>();
        usedCard.Add("TimeInSession", ID);
        GameAnalytics.NewDesignEvent(Custom_Used_Card, usedCard);
    }
    
    public void SendEventDamageDealtAsMinor(float damages)
    {
        Dictionary<string, object> damagesInflicted = new Dictionary<string, object>();
        damagesInflicted.Add("DamagesAsMinor", damages);
        GameAnalytics.NewDesignEvent(Custom_Damage_Dealt_As_Minor, damages);
    }
    
    public void SendEventDamageDealtAsFarmer(float damages)
    {
        Dictionary<string, object> damagesInflicted = new Dictionary<string, object>();
        damagesInflicted.Add("DamagesAsFarmer", damages);
        GameAnalytics.NewDesignEvent(Custom_Damage_Dealt_As_Farmer, damages);
    }
    
    public void SendEventDamageDealtAsHerborist(float damages)
    {
        Dictionary<string, object> damagesInflicted = new Dictionary<string, object>();
        damagesInflicted.Add("DamagesAsHerborist", damages);
        GameAnalytics.NewDesignEvent(Custom_Damage_Dealt_As_Herborist, damages);
    }
    
    public void SendEventDamageDealtAsFisherman(float damages)
    {
        Dictionary<string, object> damagesInflicted = new Dictionary<string, object>();
        damagesInflicted.Add("DamagesAsFisherman", damages);
        GameAnalytics.NewDesignEvent(Custom_Damage_Dealt_As_Fisherman, damages);
    }

    private void OnApplicationQuit()
    {
        SendEventSessionLength(Time.realtimeSinceStartup);
    }
}
