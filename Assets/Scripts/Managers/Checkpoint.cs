using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private float cooldown = 2f; // Задержка, чтобы не спамить сохранением
    [SerializeField] private Vector3 respawnOffset = new Vector3(0, 0.5f, 0); // Чтобы не застрять в текстурах
    private float nextSaveTime;
    public string checkpointID;
    private bool isActivated = false;
    
    void Start()
    {
        if (string.IsNullOrEmpty(checkpointID)) 
        {
            Debug.LogWarning($"[Checkpoint] На объекте {gameObject.name} не установлен CheckpointID!");
            return; 
        }
        
        if (SaveManager.Instance != null && SaveManager.Instance.IsCheckpointActivated(checkpointID))
        {
            isActivated = true;
            GetComponent<Collider2D>().enabled = false; 
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.timeSinceLevelLoad < 2f) return;
        
        if (other.CompareTag("Player") && !isActivated
            && Time.time >= nextSaveTime)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (string.IsNullOrEmpty(checkpointID))
                {
                    Debug.LogError($"[Checkpoint] Нельзя активировать чекпоинт {gameObject.name} без ID!");
                    return;
                }
    
                isActivated = true;
                
                nextSaveTime = Time.time + cooldown;
                
                if (SaveManager.Instance != null) 
                {
                    SaveManager.Instance.RegisterCheckpoint(checkpointID);
                }

                // 1. Обновляем точку в контроллере игрока
                player.SetCheckpoint(transform.position + respawnOffset); 
                
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.SaveInventoryState();
                }
                
                // if (KeyInventory.Instance != null)
                // {
                //     KeyInventory.Instance.SaveKeyState();
                // }

                // 3. Сохраняем данные на диск/в память
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveGame();
                }
                
                // 4. Запускаем анимацию дискеты (наша гордость!)
                if (UIAnimationController.Instance != null)
                {
                    UIAnimationController.Instance.TriggerSaveIcon();
                }
                
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                
                Debug.Log("Чекпоинт активирован!");
            }
        }
    }
}