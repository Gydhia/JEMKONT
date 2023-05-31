using System;
using System.Collections;
using System.Collections.Generic;
using DownBelow.Managers;
using UnityEngine;

public class OptionsManager : _baseManager<OptionsManager>
{
    public void UpdateBrightness(float value)
    {
        Screen.brightness = value;
    }

    public void UpdateDisplayMode(DisplayModes display)
    {
        switch (display)
        {
              case DisplayModes.FullScreen:
                  Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                  break;
              case DisplayModes.Borderless:
                  Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                  break;
              case DisplayModes.Windowed:
                  Screen.fullScreenMode = FullScreenMode.Windowed;
                  break;
              default:
                  Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                  break;
        }
        
        Save();
    }
    
    
    
    
    
    public void UpdateGameVolume(float value)
    {
        
    }
    public void UpdateMusicVolume(float value)
    {
        
    }
    public void UpdateEffectsVolume(float value)
    {
        
    }
    
    private void Save()
    {
        //TODO Save les settings
    }
}
