using Jemkont.GridSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDraggingSystem : MonoBehaviour {

    public static CardDraggingSystem instance;
    public CardComponent DraggedCard;
    //automatically set from callbacks on CardComponent.cs

    public float DissapearingHeight;
    //The height at which cards disappear when dragged
    public void Awake() {
        instance = this;
    }

    public void Update() {
       /* Implementation suggestion
        if (DraggedCard != null) {
            if (!DraggedCard.isInPlacingMode) {
                if (DraggedCard.transform.position.y >= DissapearingHeight) {
                    //Card starts to disspear;
                    DraggedCard.Dissapear();
                }
            } else {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out RaycastHit hit,maxDistance:Mathf.Infinity)) {
                    Debug.Log(hit.collider.gameObject.name);
                    if(hit.transform.gameObject.TryGetComponent(out Cell cellHit)) {
                        if (cellHit != null) {
                            cellHit.ChangeStateColor(Color.blue);
                            //The cell we are currently hovering with the card turns blue so we know we have it selected.
                            DraggedCard.HoveredCell = cellhit;
                            //Setting the reference inside the card so that we know where to cast the spell.
                            // Next things will happen in CardComponent.OnPointerUp() callback.
                        }
                    }
                }
            }
        }*/
    }

}
