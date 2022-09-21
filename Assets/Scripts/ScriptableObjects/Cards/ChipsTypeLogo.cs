using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(menuName = "DEV/ChipLogoEnum")]
public class ChipsTypeLogo : ScriptableObject
{
    public List<LogoEnum> Logos = new List<LogoEnum>();
}
[Serializable]
public class LogoEnum {
    public EChipType ChipType;
    public Sprite Logo;
}