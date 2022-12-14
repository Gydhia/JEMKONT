using DownBelow.GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownBelow.Managers
{
    public class SettingsManager : _baseManager<SettingsManager>
    {
        public GridsPreset GridsPreset;
        public InputPreset InputPreset;
        public CombatPreset CombatPreset;
        public GameUIPreset GameUIPreset;

        // for test
        public EntityStats PlayerStats;
    }

}
