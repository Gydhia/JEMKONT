using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;


public class ToolsManager : _baseManager <ToolsManager>{
    [OdinSerialize()] public Dictionary<EClass,Tool> ToolInstances = new();//Je vois des choses
    [OdinSerialize()] public Dictionary<EClass,EntityStats> ToolStats = new();
    public void AddToInstance(Tool toolToAdd) {
        if (ToolInstances.TryGetValue(toolToAdd.Class,out Tool tool)) {
            toolToAdd.Deck = tool.Deck; toolToAdd.Class = tool.Class;
            //This might be shitty, we'll see afterwards.
            //TODO: Photon?
        } else {
            ToolInstances.Add(toolToAdd.Class,toolToAdd);
        }
    }
}
