using DownBelow;
using DownBelow.Entity;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
    [CreateAssetMenu(fileName = "Tool", menuName = "DownBelow/ScriptableObject/ToolItem", order = 1)]
public class ToolItem : ItemPreset
{
    public DeckPreset DeckPreset;
    public EClass Class;
    public PlayerBehavior ActualPlayer;
    public Color ToolRefColor;
    public Sprite FightIcon;

    public virtual void WorldAction() 
    {
        switch (Class) {
            //Fuck every each one of you, fuck polymorphism, fuck joe biden, fuck macron, fuck putin, embrace monkey
            case EClass.Fisherman:
                break;
            case EClass.Farmer:
                break;
            case EClass.Herbalist:
                break;
            case EClass.Miner:
                break;
        }
    }
   
}
public enum EClass {
    Miner, Herbalist, Farmer, Fisherman
}
