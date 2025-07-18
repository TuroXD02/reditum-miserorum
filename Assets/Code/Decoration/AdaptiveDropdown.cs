using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AdaptiveDropdown : TMP_Dropdown
{
    [Tooltip("Max number of items visible before scrolling")]
    [SerializeField] private int maxVisibleItems = 5;

    [Tooltip("Height of each item (must match template item height)")]
    [SerializeField] private float itemHeight = 60f;

    [Tooltip("Extra height padding to prevent clipping")]
    [SerializeField] private float extraHeightPadding = 30f;

    protected override GameObject CreateDropdownList(GameObject template)
    {
        GameObject list = base.CreateDropdownList(template);

        int itemCount = options.Count;
        int visibleCount = Mathf.Min(itemCount, maxVisibleItems);
        float desiredHeight = (visibleCount * itemHeight) + extraHeightPadding;

        RectTransform listRect = list.transform.GetChild(0).GetComponent<RectTransform>();
        if (listRect != null)
        {
            listRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, desiredHeight);
        }

        return list;
    }
}