using UnityEngine;
using UnityEngine.UI;

public class BestiaryDetailPanel : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button leftArrow;
    [SerializeField] private Button rightArrow;

    public void Setup(BestiaryEntry entry, UnityEngine.Events.UnityAction onPrev, UnityEngine.Events.UnityAction onNext)
    {
        icon.sprite = entry.enemyIcon;
        nameText.text = entry.enemyName;
        descriptionText.text = entry.description;

        leftArrow.onClick.AddListener(onPrev);
        rightArrow.onClick.AddListener(onNext);
    }
}