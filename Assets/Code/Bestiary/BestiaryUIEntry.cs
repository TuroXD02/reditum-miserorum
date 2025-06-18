using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BestiaryUIEntry : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    public void Setup(BestiaryEntry entry, bool discovered, UnityAction onClick)
    {
        if (discovered)
        {
            iconImage.sprite = entry.enemyIcon;
            iconImage.color = Color.white;
            button.onClick.AddListener(onClick);
        }
        else
        {
            iconImage.color = new Color(0, 0, 0, 0.3f); // greyed out
            iconImage.sprite = null; // or set a "?" sprite
            button.interactable = false;
        }
    }
}