using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private string filePath;
    private Transform player;
    private List<string> activeCheckpointsList = new List<string>();
    private bool bayonetTrapDeactivated = false;
    private int savedMerchantState = 0;
    private int savedBoothmanState = 0;
    private bool hasSeenBoxTutorial = false;
    [HideInInspector] public bool playHatchSoundNext = false;
    
    [HideInInspector] public bool playFinalCutsceneNext = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        filePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    public void SaveGame()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player == null) return;

        PlayerController pc = player.GetComponent<PlayerController>();
        SaveData data = new SaveData();
        
        if (pc != null)
        {
            Vector3 cpPos = pc.GetLastCheckpointPos(); 
            data.checkpointX = cpPos.x;
            data.checkpointY = cpPos.y;
        }

        if (InventoryManager.Instance != null)
        {
            data.inventoryItemNames = InventoryManager.Instance.GetCollectedItemsNames();
        }
        
        data.keyCount = KeyInventory.Instance.GetKeyCount();
        data.activatedCheckpoints = new List<string>(activeCheckpointsList);
        data.isBayonetTrapDeactivated = bayonetTrapDeactivated;
        
        MerchantAI merchant = Object.FindFirstObjectByType<MerchantAI>();
        if (merchant != null)
        {
            savedMerchantState = (int)merchant.GetStoryState();
        }
        data.merchantStoryStateInt = savedMerchantState;
        
        BoothmanAI boothman = Object.FindFirstObjectByType<BoothmanAI>();
        if (boothman != null)
        {
            savedBoothmanState = (int)boothman.GetStoryState();
        }
        data.boothmanStoryStateInt = savedBoothmanState;
        
        data.playFinalCutsceneNext = playFinalCutsceneNext;
        data.hasSeenBoxTutorial = hasSeenBoxTutorial;
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Прогресс (чекпоинт) сохранен!");

        if (UIAnimationController.Instance != null)
        {
            UIAnimationController.Instance.TriggerSaveIcon();
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(filePath)) 
        {
            Debug.LogWarning("Файл сохранения не найден по пути: " + filePath);
            return;
        }

        string json = File.ReadAllText(filePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.LoadInventoryFromNames(data.inventoryItemNames);
            InventoryManager.Instance.RefreshUI();
        }
            
        if (KeyInventory.Instance != null)
        {
            KeyInventory.Instance.RestoreKeys(data.keyCount);
        }
        
        activeCheckpointsList = new List<string>(data.activatedCheckpoints);
        bayonetTrapDeactivated = data.isBayonetTrapDeactivated;

        BayonetTrap trap = Object.FindFirstObjectByType<BayonetTrap>();
        if (trap != null)
        {
            trap.SetState(bayonetTrapDeactivated);
        }

        if (player == null) 
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            player.position = new Vector3(data.checkpointX, data.checkpointY, 0);
            
            var cam = FindFirstObjectByType<CameraFollow>();
            if (cam != null) 
            {
                cam.target = player;
                cam.Warp();
            }
            
            Debug.Log("Позиция игрока восстановлена.");
        }
        
        savedMerchantState = data.merchantStoryStateInt;
        MerchantAI targetMerchant = Object.FindFirstObjectByType<MerchantAI>();
        if (targetMerchant != null)
        {
            targetMerchant.SetStoryState((MerchantStoryState)savedMerchantState);
        }
        
        savedBoothmanState = data.boothmanStoryStateInt;
        BoothmanAI targetBoothman = Object.FindFirstObjectByType<BoothmanAI>();
        if (targetBoothman != null)
        {
            targetBoothman.SetStoryState((BoothKeeperStoryState)savedBoothmanState);
        }
        
        playFinalCutsceneNext = data.playFinalCutsceneNext;
        hasSeenBoxTutorial = data.hasSeenBoxTutorial;
    }
    
    public bool HasSaveFile()
    {
        return File.Exists(filePath);
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("<color=red>Файл сохранения удален для новой игры.</color>");
        }
        
        ResetInMemoryData();
    }
    
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.buildIndex != 0 && scene.name != "Cutscenes" && HasSaveFile())
        {
            Debug.Log("Игровая сцена загружена, восстанавливаем данные...");
            LoadGame();
        }
    }
    
    public void QuickSave()
    {
        if (InventoryManager.Instance != null) InventoryManager.Instance.SaveInventoryState();
        if (KeyInventory.Instance != null) KeyInventory.Instance.SaveKeyState();

        SaveGame();
    }
    
    public Vector3 GetSavedCheckpointPosition()
    {
        if (!File.Exists(filePath)) return Vector3.zero;

        string json = File.ReadAllText(filePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return new Vector3(data.checkpointX, data.checkpointY, 0);
    }
    
    public void RegisterCheckpoint(string id)
    {
        if (!activeCheckpointsList.Contains(id))
        {
            activeCheckpointsList.Add(id);
        }
    }

    public bool IsCheckpointActivated(string id)
    {
        return activeCheckpointsList.Contains(id);
    }
    
    public void SetBayonetTrapState(bool state)
    {
        bayonetTrapDeactivated = state;
    }

    public bool IsBayonetTrapDeactivated()
    {
        return bayonetTrapDeactivated;
    }
    
    public void SetMerchantState(int state)
    {
        savedMerchantState = state;
    }

    public int GetMerchantState()
    {
        return savedMerchantState;
    }
    
    public void SetBoothmanState(int state)
    {
        savedBoothmanState = state;
    }

    public int GetBoothmanState()
    {
        return savedBoothmanState;
    }
    
    public void SetBoxTutorialSeen(bool state)
    {
        hasSeenBoxTutorial = state;
    }

    public bool IsBoxTutorialSeen()
    {
        return hasSeenBoxTutorial;
    }
    
    public void ResetInMemoryData()
    {
        activeCheckpointsList.Clear();
        bayonetTrapDeactivated = false;
        playFinalCutsceneNext = false;
        savedMerchantState = 0;
        savedBoothmanState = 0;
        player = null;
        hasSeenBoxTutorial = false;
        playHatchSoundNext = false;
        
        Debug.Log("<color=cyan>Данные сохранения в RAM успешно сброшены к начальным значениям.</color>");
    }
}