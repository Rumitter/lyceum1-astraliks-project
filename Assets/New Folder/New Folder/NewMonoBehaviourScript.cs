using UnityEngine;

public class DropZone : MonoBehaviour
{
    [Tooltip("Если true — только один блок в этой зоне")]
    public bool singleSlot = false;

    public bool AcceptBlock(WordBlock block)
    {
        if (singleSlot && transform.childCount > 0)
        {
            // поменять местами
            var existing = transform.GetChild(0).GetComponent<WordBlock>();
            existing.transform.SetParent(block.originalParent);
            existing.GetComponent<RectTransform>().anchoredPosition = existing.originalPosition;
        }
        return true;
    }
}