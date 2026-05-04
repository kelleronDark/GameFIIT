using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private float cooldown = 2f; // Задержка, чтобы не спамить сохранением
    [SerializeField] private Vector3 respawnOffset = new Vector3(0, 0.5f, 0); // Чтобы не застрять в текстурах
    private float nextSaveTime;
    private bool isUsed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isUsed
            && Time.time >= nextSaveTime)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                isUsed = true;
                nextSaveTime = Time.time + cooldown;

                // 1. Обновляем точку в контроллере игрока
                player.SetCheckpoint(transform.position + respawnOffset); 
                
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.SaveInventoryState();
                }

                // 2. Запускаем анимацию дискеты (наша гордость!)
                if (UIAnimationController.Instance != null)
                {
                    UIAnimationController.Instance.TriggerSaveIcon();
                }

                // 3. Сохраняем данные на диск/в память
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveGame();
                }
                
                Debug.Log("Чекпоинт активирован!");
            }
        }
    }
}