using Jemkont.GridSystem;
using Jemkont.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CharacterEntity : MonoBehaviour
{
    public delegate UnityEvent StatModified();

    public event StatModified OnHealthRemoved;
    public event StatModified OnHealthAdded;
    public event StatModified OnStrenghtRemoved;
    public event StatModified OnStrenghtAdded;
    public event StatModified OnDexterityRemoved;
    public event StatModified OnDexterityAdded;
    public event StatModified OnMovementRemoved;
    public event StatModified OnMovementAdded;
    public event StatModified OnManaRemoved;
    public event StatModified OnManaAdded;

    private EntityStats RefStats;

    public bool IsAlly = true;
    public GridPosition PlayerPosition = GridPosition.zero;
    public CombatGrid CurrentGrid;


    public Dictionary<EntityStatistics, int> Statistics;
    public int Health { get => Statistics[EntityStatistics.Health]; private set => Statistics[EntityStatistics.Health] = value; }
    public int Strenght { get => Statistics[EntityStatistics.Strenght]; private set => Statistics[EntityStatistics.Strenght] = value; }
    public int Dexterity { get => Statistics[EntityStatistics.Dexterity]; private set => Statistics[EntityStatistics.Dexterity] = value;}
    public int Movement { get => Statistics[EntityStatistics.Movement]; private set => Statistics[EntityStatistics.Movement] = value;}
    public int Mana { get => Statistics[EntityStatistics.Mana]; private set => Statistics[EntityStatistics.Mana] = value;}

    public void GoTo(Cell destination) 
    {
        this.PlayerPosition = new GridPosition(destination.Datas.heightPos, destination.Datas.widthPos);
        this.transform.position = destination.WorldPosition;
    }

    public void Init(EntityStats stats)
    {
        this.RefStats = stats;
        this.Statistics = new Dictionary<EntityStatistics, int>();

        this.Statistics.Add(EntityStatistics.Health, stats.Health);
        this.Statistics.Add(EntityStatistics.Strenght, stats.Strenght);
        this.Statistics.Add(EntityStatistics.Dexterity, stats.Dexterity);
        this.Statistics.Add(EntityStatistics.Movement, stats.Movement);
        this.Statistics.Add(EntityStatistics.Mana, stats.Mana);
    }

    public void ChangeStatistic(EntityStatistics stat, int value)
    {
        switch (stat)
        {
            case EntityStatistics.Health: this._changeHealth(value); break;
            case EntityStatistics.Mana: this._changeMana(value); break;
            case EntityStatistics.Movement: this._changeMovement(value); break;
            case EntityStatistics.Strenght: this._changeStrenght(value); break;
            case EntityStatistics.Dexterity: this._changeDexterity(value); break;
        }
    }

    private void _changeHealth(int value)
    {
        if (value < 0)
            this._removeHealth(value);
        else
            this._addHealth(value);
    }

    private void _changeStrenght(int value)
    {
        if (value < 0)
            this._removeStrenght(value);
        else
            this._addStrenght(value);
    }
    private void _changeDexterity(int value)
    {
        if (value < 0)
            this._removeDexterity(value);
        else
            this._addDexterity(value);
    }
    private void _changeMana(int value)
    {
        if (value < 0)
            this._removeMana(value);
        else
            this._addMana(value);
    }
    private void _changeMovement(int value)
    {
        if (value < 0)
            this._removeMovement(value);
        else
            this._addMovement(value);
    }

    private void _addHealth(int value)
    {
        this.Health += value;
        this.OnHealthAdded?.Invoke();
    }
    private void _removeHealth(int value)
    {
        this.Health -= value;
        this.OnHealthRemoved?.Invoke();
    }
    private void _addStrenght(int value)
    {
        this.Strenght += value;
        this.OnStrenghtAdded?.Invoke();
    }
    private void _removeStrenght(int value)
    {
        this.Strenght -= value;
        this.OnStrenghtRemoved?.Invoke();
    }
    private void _addDexterity(int value)
    {
        this.Dexterity += value;
        this.OnDexterityAdded?.Invoke();
    }
    private void _removeDexterity(int value)
    {
        this.Dexterity -= value;
        this.OnDexterityRemoved?.Invoke();
    }
    private void _addMana(int value)
    {
        this.Mana += value;
        this.OnManaAdded?.Invoke();
    }
    private void _removeMana(int value)
    {
        this.Mana -= value;
        this.OnManaRemoved?.Invoke();
    }
    private void _addMovement(int value)
    {
        this.Movement += value;
        this.OnMovementAdded?.Invoke();
    }
    private void _removeMovement(int value)
    {
        this.Movement -= value;
        this.OnMovementRemoved?.Invoke();
    }
}
