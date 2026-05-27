using UnityEngine;
using UnityEngine.UI;

public class KeyInventory : MonoBehaviour
{
    public static KeyInventory Instance;

    [Header("Settings")]
    public int maxKeys = 4;
    private int currentKeys = 0;

    [Header("UI Slots")]
    public Image[] keySlots;
    public Sprite keySprite;

    [Header("Debug")]
    public bool showDebugLogs = true;
    
    private int savedKeysSnapshot = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeUI();
    }

    void InitializeUI()
    {
        currentKeys = 0;

        foreach (var slot in keySlots)
        {
            if (slot == null) continue;

            slot.enabled = true;

            Transform keyIconTransform = slot.transform.Find("KeyIcon");
            if (keyIconTransform != null)
            {
                Image keyIcon = keyIconTransform.GetComponent<Image>();
                if (keyIcon != null)
                    keyIcon.enabled = false;
            }
        }
    }

    public bool AddKey()
    {
        if (currentKeys >= maxKeys)
        {
            if (showDebugLogs)
                Debug.LogWarning("Инвентарь ключей полон! Нельзя взять больше ключей.");
            return false;
        }

        currentKeys++;
        UpdateUI();
        
        if (showDebugLogs)
            Debug.Log($"Ключ подобран! Всего ключей: {currentKeys}/{maxKeys}");
        
        return true;
    }

    public bool UseKey()
    {
        if (currentKeys <= 0)
        {
            if (showDebugLogs)
                Debug.LogWarning("У игрока нет ключей!");
            return false;
        }

        currentKeys--;
        UpdateUI();
        
        if (showDebugLogs)
            Debug.Log($"Ключ использован. Осталось ключей: {currentKeys}/{maxKeys}");
        
        return true;
    }

    public bool HasKeys()
    {
        return currentKeys > 0;
    }

    public int GetKeyCount()
    {
        return currentKeys;
    }

    void UpdateUI()
    {
        for (int i = 0; i < keySlots.Length; i++)
        {
            if (keySlots[i] != null)
            {
                bool hasKey = (i < currentKeys);

                Transform keyIconTransform = keySlots[i].transform.Find("KeyIcon");
                
                if (keyIconTransform != null)
                {
                    Image keyIcon = keyIconTransform.GetComponent<Image>();
                    if (keyIcon != null)
                    {
                        keyIcon.enabled = hasKey;
                        
                        if (hasKey && keySprite != null)
                        {
                            keyIcon.sprite = keySprite;
                        }
                    }
                }

                if (hasKey && i == currentKeys - 1) 
                {
                    KeySlotPulse pulser = keySlots[i].GetComponent<KeySlotPulse>();
                    
                    if (pulser != null)
                    {
                        pulser.Pulse();
                    }
                }
            }
        }
    }

    [ContextMenu("Add Test Key")]
    void AddTestKey()
    {
        AddKey();
    }

    [ContextMenu("Remove Test Key")]
    void RemoveTestKey()
    {
        UseKey();
    }
    
    public void SaveKeyState()
    {
        // savedKeysSnapshot = currentKeys;
        // Debug.Log($"Состояние ключей сохранено: {savedKeysSnapshot}");
    }

    public void ResetKeys()
    {
        // currentKeys = savedKeysSnapshot;
        // UpdateUI();
        // Debug.Log("Ключи восстановлены из сохранения.");
    }
    
    public void RestoreKeys(int count)
    {
        // currentKeys = count;
        // savedKeysSnapshot = count;
        // UpdateUI();
    }
}