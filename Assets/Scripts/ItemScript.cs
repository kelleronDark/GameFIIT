using UnityEngine;
using TMPro;

public class ItemHint : MonoBehaviour
{
    public GameObject hintPrefab;
    private GameObject currentHint;
    
    // Смещение подсказки относительно центра объекта
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentHint == null)
        {
            ShowHint();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HideHint();
        }
    }

    void Update()
    {
        // Если подсказка активна, двигаем её за объектом каждый кадр
        if (currentHint != null)
        {
            currentHint.transform.position = transform.position + offset;
        }
    }

    void ShowHint()
    {
        // Создаем подсказку в мировой позиции (без родителя)
        currentHint = Instantiate(hintPrefab, transform.position + offset, Quaternion.identity);
        
        // ВАЖНО: НЕ делаем SetParent! Оставляем в корне сцены или на слое UI.

        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null && Camera.main != null)
            canvas.worldCamera = Camera.main;

        TextMeshProUGUI text = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null) text.text = "Нажмите F";
    }

    void HideHint()
    {
        if (currentHint != null)
        {
            Destroy(currentHint);
            currentHint = null;
        }
    }
}