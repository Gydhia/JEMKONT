using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jemkont.Managers
{
    public class CombatManager : _baseManager<CombatManager>
    {
        public Dictionary<Guid, EntitySpawn> EntitiesSpawnsSO;

        private void Awake()
        {
            this._loadEveryEntities();
        }

        private void _loadEveryEntities()
        {
            var entities = Resources.LoadAll<EntitySpawn>("Presets/Entity/").ToList();

            this.EntitiesSpawnsSO = new Dictionary<Guid, EntitySpawn>();

            foreach (var entity in entities)
                this.EntitiesSpawnsSO.Add(entity.UID, entity);
        }
        public void StartCombat()
        {

        }
    }
}