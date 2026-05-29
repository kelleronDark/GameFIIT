using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    
    private Sprite[] savedSpritesSnapshot = new Sprite[4];
    
    private PlayerController playerController;

    [Header("UI Slots")]
    public Image[] slots;

    [Header("Settings")]
    public int maxItems = 4;
    private Sprite[] collectedSprites = new Sprite[4];
    
    [Header("Audio")]
    public AudioSource pickupAudioSource;
    public AudioClip pickupClip; 
    
    private int currentKeys = 0;
    private int savedKeysSnapshot = 0;

    void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
            playerController = playerObj.GetComponent<PlayerController>();

        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void PlayPickupSound()
    {
        if (pickupAudioSource != null && pickupClip != null)
        {
            pickupAudioSource.PlayOneShot(pickupClip);
        }
    }

    
    public void SaveInventoryState()
    {
        for (int i = 0; i < collectedSprites.Length; i++)
        {
            savedSpritesSnapshot[i] = collectedSprites[i];
        }
        Debug.Log("Снимок инвентаря для сохранения сделан.");
    }
    
    // public void ResetInventory()
    // {
    //     if (playerController != null)
    //         playerController.ForceDropItem();
    //     
    //     currentKeys = savedKeysSnapshot;
    //
    //     for (int i = 0; i < collectedSprites.Length; i++)
    //     {
    //         collectedSprites[i] = savedSpritesSnapshot[i];
    //     
    //         if (collectedSprites[i] != null)
    //         {
    //             slots[i].sprite = collectedSprites[i];
    //             slots[i].enabled = true;
    //         }
    //         else
    //         {
    //             slots[i].sprite = null;
    //             slots[i].enabled = false;
    //         }
    //     }
    //     Debug.Log("Инвентарь откачен к состоянию последнего сохранения.");
    // }

    void Start()
    {
        if (isLoaded)
        {
            RefreshUI();
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].enabled = false;
                slots[i].sprite = null;
            }
        }
    }

    public bool PickupItem(Sprite itemSprite)
    {
        if (itemSprite == null)
        {
            Debug.LogWarning("Попытка подобрать предмет с null спрайтом!");
            return false;
        }

        for (int i = 0; i < collectedSprites.Length; i++)
        {
            if (collectedSprites[i] == null)
            {
                collectedSprites[i] = itemSprite;
                slots[i].sprite = itemSprite;
                slots[i].enabled = true;

                Debug.Log($"Подобрана деталь в слот {i}: {itemSprite.name}");
                
                PlayPickupSound();
                
                return true;
            }
        }

        Debug.LogWarning("Инвентарь полон! Нельзя подобрать ещё одну деталь");
        return false;
    }
    
    public void ClearInventory()
    {
        for (int i = 0; i < collectedSprites.Length; i++)
        {
            collectedSprites[i] = null;
            if (slots[i] != null)
            {
                slots[i].sprite = null;
                slots[i].enabled = false;
            }
        }
    }

    public bool IsSlotOccupied(int index)
    {
        if (index < 0 || index >= collectedSprites.Length) return false;
        return collectedSprites[index] != null;
    }
    
    public List<string> GetCollectedItemsNames()
    {
        List<string> names = new List<string>();
        foreach (var sprite in collectedSprites)
        {
            if (sprite != null) names.Add(sprite.name);
        }
        return names;
    }
    
    public bool HasItem(string itemName)
    {
        if (collectedSprites == null) return false;
    
        foreach (var sprite in collectedSprites)
        {
            if (sprite != null && sprite.name == itemName)
            {
                return true;
            }
        }
        return false;
    }
    
    public bool isLoaded = false;
    
    public void LoadInventoryFromNames(List<string> itemNames)
    {
        isLoaded = false; 
        ClearInventory();
    
        if (itemNames == null || itemNames.Count == 0) 
        {
            isLoaded = true; 
            return; 
        }
    
        Sprite[] allSprites = Resources.LoadAll<Sprite>("generated-removebg-preview");

        for (int i = 0; i < itemNames.Count && i < collectedSprites.Length; i++)
        {
            string targetName = itemNames[i];
            Sprite found = System.Array.Find(allSprites, s => s.name == targetName);

            if (found != null)
            {
                collectedSprites[i] = found; 
                slots[i].sprite = found;
                
                slots[i].enabled = true; 
                slots[i].color = new Color(1, 1, 1, 1);
                slots[i].gameObject.SetActive(true);
    
                Debug.Log($"[UI-FIX] Слот {i} активирован для {found.name}");
            }
        }
    
        for (int i = 0; i < collectedSprites.Length; i++)
        {
            savedSpritesSnapshot[i] = collectedSprites[i];
        }
    
        isLoaded = true; 
        RefreshUI();
        Debug.Log("Инвентарь загружен и снимок обновлен данными из файла.");
    }
    
    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (collectedSprites[i] != null)
            {
                slots[i].sprite = collectedSprites[i];
                slots[i].enabled = true;
                slots[i].color = Color.white;
                slots[i].gameObject.SetActive(true);
            }
            else
            {
                slots[i].enabled = false;
            }
        }
    }
}