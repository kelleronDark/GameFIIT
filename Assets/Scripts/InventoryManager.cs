using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public Image[] slots;
    private bool[] collected = new bool[4];

    void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].color = new Color(0.5f, 0.5f, 0.5f, 1f);
                collected[i] = false;
            }
        }
    }

    public void PickupItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;
        if (collected[slotIndex]) return;

        collected[slotIndex] = true;
        slots[slotIndex].color = Color.white;
        Debug.Log("Подобрана деталь для слота " + slotIndex);
    }
}