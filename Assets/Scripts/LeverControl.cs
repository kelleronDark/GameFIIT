using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LeverControl : MonoBehaviour
{
    public Animator doorAnimator;
    public BoxCollider2D doorCollider;
    public Animator leverAnimator;
    
    [Header("Trap")]
    public BayonetTrap bayonetTrap;
    public bool startsOpened = false; 

    [Header("Audio")]
    public AudioSource audioSource; // 1. Источник звука

    [Header("UI Hint")]
    public GameObject hintPrefab;
    private GameObject currentHint;

    private bool isPlayerNearby = false;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (bayonetTrap != null)
        {
            // Только в этом случае спрашиваем у SaveManager её состояние
            bool isAlreadyDeactivated = SaveManager.Instance != null && SaveManager.Instance.IsBayonetTrapDeactivated();

            if (isAlreadyDeactivated)
            {
                ApplyState(true); // Отключаем ловушку и поворачиваем рычаг навсегда
                return; // Выходим из метода, настройки по умолчанию нам не нужны
            }
        }

        ApplyState(startsOpened); // Настройка уровня по умолчанию (если дверь изначально должна быть открыта)
        // Железно закрываем дверь и выключаем рычаг при загрузке сцены
    }
    
    private void ApplyState(bool isOpen)
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("isOpen", isOpen);
            doorAnimator.Play(isOpen ? "Gate_Opened" : "Gate_Closed", 0, 1f);
        }

        if (doorCollider != null) doorCollider.enabled = !isOpen;

        if (bayonetTrap != null)
        {
            // Используем SetState, чтобы не крутить логику Toggle по кругу
            bayonetTrap.SetState(isOpen); 
        }

        if (leverAnimator != null)
        {
            leverAnimator.SetBool("isActivated", isOpen);
            leverAnimator.Play(isOpen ? "Lever_On" : "Lever_Off", 0, 1f);
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
        // 2. Воспроизводим звук переключения
        if (audioSource != null)
            audioSource.Play();

        // Если есть дверь — работаем с дверью
        if (doorAnimator != null)
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
        }

        // Если есть ловушка — выключаем её
        if (bayonetTrap != null)
        {
            bayonetTrap.ToggleTrap();

            if (leverAnimator != null)
            {
                leverAnimator.SetBool("isActivated", !bayonetTrap.IsActive);
            }
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetBayonetTrapState(!bayonetTrap.IsActive);
            }

            Debug.Log("Ловушка переключена.");
        }

        HideHint();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = true;
            ShowHint();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            isPlayerNearby = false;
            HideHint();
        }
    }

    void ShowHint()
    {
        if (currentHint != null) return;

        currentHint = Instantiate(hintPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
        currentHint.transform.SetParent(transform);

        Canvas canvas = currentHint.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                canvas.worldCamera = mainCamera;
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

        Destroy(currentHint);
        currentHint = null;
    }
}