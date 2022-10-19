using Jemkont.GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jemkont.Managers
{
    public class SettingsManager : _baseManager<SettingsManager>
    {
        public GridsPreset GridsPreset;
        public InputPreset InputPreset;
        public CombatPreset CombatPreset;

        // for test
        public EntityStats PlayerStats;
    }

}
