using DownBelow.Managers;
using DownBelow.Mechanics;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SmallCardDeckbuilding : MonoBehaviour 
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Cost;
    public TextMeshProUGUI Number;

    [SerializeField] private Button _button;
    private UnityEngine.Events.UnityAction LeftClick;
    private ScriptableCard card;

    private void Update() {
        if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Mouse.current.position.ReadValue())) {
         /*   if (LeftClick != null && Mouse.current.leftButton.wasPressedThisFrame) {
                LeftClick();
            }*/
            if (card != null && Mouse.current.rightButton.wasPressedThisFrame) {
                RightClick();
            }
        }
    }

    private void SendLeftClickEvent()
    {
        UIManager.Instance.DeckbuildingSystem.CurrentDeckLayoutGroup.enabled = false;
        LeftClick();
        UIManager.Instance.DeckbuildingSystem.CurrentDeckLayoutGroup.enabled = true;
    }
    public void Init(ScriptableCard card, int number, UnityEngine.Events.UnityAction call) {
        Number.text = number.ToString();
        Name.text = card.Title.ToString();
        Cost.text = card.Cost.ToString();
        LeftClick = call;
        
        _button.onClick.AddListener(SendLeftClickEvent);
    }
    /// <summary>
    /// TODO: on est d'accord que qd tu dis "Right clicking a card makes it take a larger portion
    /// of the screen and shows additional information."
    /// c'est basiquement afficher une popup de la carte, genre litt�ralement la m�me carte qu'on joue IG?
    /// Yep c'est ca, en un peu plus zoom�, comme dans hearthstone si tu vois
    /// </summary>
    public void RightClick() {
        Debug.Log("PopupCard");
    }
}
