using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Sprite activeSprite; 
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null)
            {
                isActivated = true; // Сразу блокируем повторные входы

                // 1. Сохраняем позицию
                player.SetNewCheckPoint(transform.position);
                
                // 2. Меняем спрайт на активный
                if (activeSprite != null) 
                {
                    // Ищем SpriteRenderer в самом объекте или в дочерних
                    SpriteRenderer sr = GetComponent<SpriteRenderer>();
                    if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.sprite = activeSprite;
                }

                // 3. Вызываем сохранение и UI
                player.SaveGame(); 

                // 4. ГЛАВНОЕ: Отключаем физический триггер. 
                // Теперь игрок может ходить сквозь него, и событие больше не вызовется.
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                Debug.Log("Чекпоинт активирован и физически отключен.");
            }
        }
    }
}