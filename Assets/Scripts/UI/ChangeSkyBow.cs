using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChangeSkyBow : MonoBehaviour
{
    // Start is called before the first frame update
    public List<Material> Skyboxes = new List<Material>();

    private float tymeure = 0f;

    public float timeBeforeChange = 10f;
    // Update is called once per frame
    void Update()
    {
        tymeure += Time.deltaTime;
        if(tymeure >= timeBeforeChange)
        {
            ChangeTheSkybox();
            tymeure = 0;
        }
        

    }

    private void ChangeTheSkybox()
    {
        int result = Random.Range(0, Skyboxes.Count);
    
        RenderSettings.skybox = Skyboxes[result];
    }

}
