using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LeverControl : MonoBehaviour
{
    public Animator doorAnimator;
    public BoxCollider2D doorCollider;
    public Animator leverAnimator;
    
    public bool startsOpened = false; 
    private bool isPlayerNearby = false;

    [Header("UI Hint")]
    public GameObject hintPrefab; // Префаб подсказки
    private GameObject currentHint;

    void Start()
    {
        if (startsOpened)
        {
            doorAnimator.Play("Gate_Opened", 0, 1f); 
            doorAnimator.SetBool("isOpen", true);

            if (leverAnimator != null)
            {
                leverAnimator.Play("Lever_On", 0, 1f); 
                leverAnimator.SetBool("isActivated", true);
            }
            
            if (doorCollider != null) doorCollider.enabled = false; 
        }
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleGate();
        }
    }

    private void ToggleGate()
    {
        bool newState = !doorAnimator.GetBool("isOpen");

        doorAnimator.SetBool("isOpen", newState);
        if (doorCollider != null) 
        {
            doorCollider.enabled = !newState; 
        }

        if (leverAnimator != null)
        {
            leverAnimator.SetBool("isActivated", newState);
        }

        Debug.Log("Рычаг и дверь переключены. Состояние открыто: " + newState);
        
        HideHint(); // Скрываем подсказку после активации
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = true;
            ShowHint(); // Показываем подсказку
            Debug.Log("Нажми F, чтобы активировать");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = false;
            HideHint(); // Скрываем подсказку
        }
    }

    // === ЕДИНЫЙ МЕТОД SHOWHINT (как в Chest.cs) ===

    void ShowHint()
    {
        if (currentHint != null) return; // Уже есть подсказка

        currentHint = Instantiate(hintPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        currentHint.transform.SetParent(transform);

        // Назначаем камеру
        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
            }
            else
            {
                Debug.LogError("❌ Камера не найдена для подсказки рычага!");
            }
        }

        TextMeshProUGUI hintText = currentHint.GetComponentInChildren<TextMeshProUGUI>();
        if (hintText != null)
        {
            hintText.text = "Нажмите F";
        }
    }

    void HideHint()
    {
        if (currentHint == null) return;

        Destroy(currentHint); // Простое удаление — как было изначально
        currentHint = null;
    }
}