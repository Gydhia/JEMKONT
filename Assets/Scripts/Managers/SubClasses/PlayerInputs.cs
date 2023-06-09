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

    public static InputAction player_select_1;
    public static InputAction player_select_2;
    public static InputAction player_select_3;
    public static InputAction player_select_4;
    public static InputAction player_reselect;

    public static InputAction player_scroll;

    public static InputAction player_alt;
    public static InputAction player_F4;

    public static void Init(InputActionAsset asset)
    {
        player_l_click = asset["LeftClick"];
        player_r_click = asset["RightClick"];

        player_interact = asset["Interact"];
        player_escape = asset["Escape"];
        player_scroll = asset["MouseScrollY"];

        player_select_1 = asset["Select1"];
        player_select_2 = asset["Select2"];
        player_select_3 = asset["Select3"];
        player_select_4 = asset["Select4"];

        player_reselect = asset["Reselect"];

        player_alt = asset["Alt"];
        player_F4 = asset["F4"];
    }
}
