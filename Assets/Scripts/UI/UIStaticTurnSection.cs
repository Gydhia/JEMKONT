using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStaticTurnSection : MonoBehaviour
{
    public Image AllySprite;
    public Image EnemySprite;
    // The item to instantiate
    public Image SpritePrefab;
    // The parent of the entities
    public Transform EntitiesHolder;

    public List<Image> CombatEntities;

    public void Init(List<CharacterEntity> combatEntities)
    {
        combatEntities.Sort((ent, ent2) => ent.IsAlly ? 1 : 2);

        for (int i = 0; i < combatEntities.Count / 2; i++)
        {
            this.CombatEntities.Add( Instantiate(this.SpritePrefab, this.EntitiesHolder, combatEntities[i]) );
            if(i + 1 < combatEntities.Count)
                this.CombatEntities.Add(Instantiate(this.SpritePrefab, this.EntitiesHolder, combatEntities[i + 1]));
        }
    }
}
