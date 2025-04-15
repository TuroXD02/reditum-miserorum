using UnityEngine;
using UnityEngine.EventSystems;

public class BlockClicks : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    // Called when a pointer clicks on the GameObject.
    public void OnPointerClick(PointerEventData eventData)
    {
        eventData.Use(); // Consume the click event.
    }

    // Called when a pointer is pressed down on the GameObject.
    public void OnPointerDown(PointerEventData eventData)
    {
        eventData.Use(); // Consume the pointer down event.
    }

    // Called when a pointer is released over the GameObject.
    public void OnPointerUp(PointerEventData eventData)
    {
        eventData.Use(); // Consume the pointer up event.
    }
}
