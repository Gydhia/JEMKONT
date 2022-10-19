using EODE.Wonderland;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FishingGame : MonoBehaviour {
    public FishingData[] fishes;
    public Slider FishingGauge;
    public RectTransform GreenZoneTransform;
    public float valueIncrement;
    public float valueDecrement;
    private float realMax;
    private float realMin;
    private FishingData actualFish;
    private float timer;
    private FishingData RandomFish() {
        return fishes.Random();
    }
    public void RandomFishGame() {
        InitGame(RandomFish());
    }
    private void InitGame(FishingData data) {
        actualFish = data;
        float center = UnityEngine.Random.Range((data.GreenZoneSize / 2) + 10,90 - (data.GreenZoneSize / 2));
        float max = center + (data.GreenZoneSize / 2);
        float min = center - (data.GreenZoneSize / 2);
        realMax = max+2;
        realMin = min-2;
        max /= 100f;
        min /= 100f;
        GreenZoneTransform.anchorMax = new Vector2(max,GreenZoneTransform.anchorMax.y);
        GreenZoneTransform.anchorMin = new Vector2(min,GreenZoneTransform.anchorMin.y);
        FishingGauge.value = 50;
        FishingGauge.gameObject.SetActive(true);

    }
    public void Update() {
        if (FishingGauge.gameObject.activeSelf) {
            if(Input.GetKeyDown(KeyCode.E))
            { FishingGauge.value += valueIncrement; }
            FishingGauge.value -= valueDecrement;
            if(FishingGauge.value<=realMax && FishingGauge.value >= realMin) {
                timer += Time.deltaTime;
                if(timer >= (actualFish.reelingTimeNeccessary / 1000)) {
                    //Fished up!
                    //TODO: send fish 
                    timer = 0;
                    actualFish = null;
                    FishingGauge.gameObject.SetActive(false);
                }
            }
        }
    }


}
[Serializable]
public class FishingData {
    public string Name;
    [Tooltip("Note: the whole slider goes from 0 to 100.")] public float GreenZoneSize;
    public float reelingTimeNeccessary;

}
