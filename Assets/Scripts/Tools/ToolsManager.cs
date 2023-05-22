using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;

namespace DownBelow.Managers
{
    public class ToolsManager : _baseManager<ToolsManager>
    {
        [OdinSerialize()] public Dictionary<EClass, ToolItem> ToolInstances = new();//Je vois des choses

        public List<ToolItem> AvailableTools;

        [HideInInspector]
        public Dictionary<Guid, ToolItem> ToolPresets;

        public void Init()
        {
            this.ToolPresets = new Dictionary<Guid, ToolItem>();
            foreach (var tool in this.AvailableTools)
            {
                this.ToolPresets.Add(tool.UID, tool);
            }
        }

        public void AddToInstance(ToolItem toolToAdd)
        {
            if (ToolInstances.TryGetValue(toolToAdd.Class, out ToolItem tool))
            {
                toolToAdd.DeckPreset = tool.DeckPreset; toolToAdd.Class = tool.Class;
                //This might be shitty, we'll see afterwards.
                //TODO: Photon?
            }
            else
            {
                ToolInstances.Add(toolToAdd.Class, toolToAdd);
            }
        }
    }

}
