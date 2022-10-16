using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using Sirenix.OdinInspector;
public enum ESeedType { Potato, Wheat, Carrot }
public class Crop : MonoBehaviour {
    public ESeedType CropType;
    private int age;
    public bool hasWater;
    public int timeIncrementAge;
    public int Age {
        get { return this.age; }
        set {
            Mathf.Min(this.AgeModels.Length,value);
            UpdateAgeModel();
        }
    }
    public GameObject[] AgeModels;
    public async Task Planted() {
        //Equivalent of Start. 
        await new WaitUntil(() => hasWater);
    }
    public void IncrementAge() {
        Age += 1;
    }
    [Button]
    public void CycleAge() {
        if (Age == this.AgeModels.Length - 1) {
            Age = 0;
        } else {
            Age++;
        }
        UpdateAgeModel();
    }
    void UpdateAgeModel() {
        if (this.AgeModels.Length != 0) {
            for (int i = 0;i < this.AgeModels.Length;i++) {
                GameObject item = this.AgeModels[i];
                if (i != this.Age) {
                    item.SetActive(false);
                } else {
                    item.SetActive(true);
                }
            }
        }
    }
    public bool CanBePickedUp() {
        return this.Age == this.AgeModels.Length - 1;
    }
    public void PickUp() {
        if (this.CanBePickedUp()) {
            //@TODO: put corresponding items in the player's inventory.
            Destroy(this.gameObject);
        }
    }
}
