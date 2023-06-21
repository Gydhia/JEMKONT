using DownBelow.Managers;
using DownBelow.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DeckBuildingBehavior : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{

    void _addCardToDeck()
    {
        UIManager.Instance.DeckbuildingSystem.TryAddCopy(GetComponent<CardVisual>().CardReference, true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayerInputs.player_l_click.performed += Player_l_click_performed;
    }

    private void Player_l_click_performed(InputAction.CallbackContext obj)=>_addCardToDeck();

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayerInputs.player_l_click.performed -= Player_l_click_performed;
    }
    private void OnDestroy()
    {
        PlayerInputs.player_l_click.performed -= Player_l_click_performed;
    }
    private void OnDisable()
    {
        PlayerInputs.player_l_click.performed -= Player_l_click_performed;
    }
}
