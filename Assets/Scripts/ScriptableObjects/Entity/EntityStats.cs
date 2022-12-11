using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entity/Statistics")]
public class EntityStats : SerializedScriptableObject
{
    public int Health;
    public int BaseShield;

    public int Strenght;
    public int Dexterity;

    public int Mana;
    public int Movement;
}
