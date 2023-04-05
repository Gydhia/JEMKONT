using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class PlayerInputs
{
    public static InputAction player_l_click;
    public static InputAction player_r_click;

    public static InputAction player_interact;
    public static InputAction player_escape;

    public static void Init(InputActionAsset asset)
    {
        player_l_click = asset["LeftClick"];
        player_r_click = asset["RightClick"];

        player_interact = asset["Interact"];
        player_escape = asset["Escape"];
    }
}
