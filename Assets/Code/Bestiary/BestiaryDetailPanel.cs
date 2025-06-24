using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class BestiaryDetailPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button leftArrow;
    [SerializeField] private Button rightArrow;
    [SerializeField] private Button closeButton;

    public void Setup(BestiaryEntry entry, UnityAction onPrev, UnityAction onNext)
    {
        icon.sprite = entry.enemyIcon;
        nameText.text = entry.enemyName;
        descriptionText.text = entry.description;

        leftArrow.onClick.RemoveAllListeners();
        rightArrow.onClick.RemoveAllListeners();

        leftArrow.onClick.AddListener(onPrev);
        rightArrow.onClick.AddListener(onNext);
    }

    public void SetCloseAction(UnityAction onClose)
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(onClose);
        }
    }
}