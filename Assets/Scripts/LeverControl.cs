using UnityEngine;
using UnityEngine.InputSystem; // Важно! Добавляем доступ к новой системе

public class LeverControl : MonoBehaviour
{
    public Animator doorAnimator;    // Сюда тащим Дверь
    public BoxCollider2D doorCollider; // Сюда тащим Дверь
    public Animator leverAnimator;   // Сюда тащим САМ РЫЧАГ
    
    public bool startsOpened = false; 
    private bool isPlayerNearby = false;

    void Start()
    {
        if (startsOpened)
        {
            // Мгновенно ставим аниматор двери в финальный кадр открытого состояния
            // Замени "Gate_Opened" на точное имя стейта в твоем Animator
            doorAnimator.Play("Gate_Opened", 0, 1f); 
            doorAnimator.SetBool("isOpen", true);

            if (leverAnimator != null)
            {
                // То же самое для рычага, чтобы он сразу был нажат
                leverAnimator.Play("Lever_On", 0, 1f); 
                leverAnimator.SetBool("isActivated", true);
            }
            
            if (doorCollider != null) doorCollider.enabled = false; 
        }
    }

    void Update()
    {
        // Проверка нажатия кнопки F по стандартам новой системы
        if (isPlayerNearby && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ToggleGate();
        }
    }

    private void ToggleGate()
    {
        // 1. Узнаем текущее состояние двери и инвертируем его
        bool newState = !doorAnimator.GetBool("isOpen");

        // 2. Управляем дверью (анимация и физика)
        doorAnimator.SetBool("isOpen", newState);
        if (doorCollider != null) 
        {
            doorCollider.enabled = !newState; 
        }

        // 3. Управляем анимацией самого рычага
        if (leverAnimator != null)
        {
            leverAnimator.SetBool("isActivated", newState);
        }

        Debug.Log("Рычаг и дверь переключены. Состояние открыто: " + newState);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = true;
            Debug.Log("Нажми F, чтобы активировать");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = false;
        }
    }
}