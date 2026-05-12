using UnityEngine;

public class BayonetTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    // Поставь 150-200, если хочешь, чтобы игрок "сгорал" за полсекунды
    public float damagePerSecond = 150f; 

    private bool isActive = true;
    private Animator animator;
    private float damageAccumulator = 0f;

    public bool IsActive => isActive;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Накапливаем урон дробно
                damageAccumulator += damagePerSecond * Time.deltaTime;

                // Как только накопилась хотя бы 1 единица здоровья
                if (damageAccumulator >= 1f)
                {
                    int damageToApply = Mathf.FloorToInt(damageAccumulator);
                    player.TakeDamage(damageToApply);
                    
                    // Вычитаем только целую часть, чтобы дробный остаток не пропадал
                    damageAccumulator -= damageToApply;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Сбрасываем накопленный "хвост" урона при выходе
            damageAccumulator = 0f;
        }
    }

    public void ToggleTrap()
    {
        isActive = !isActive;
        
        if (animator != null)
        {
            animator.SetBool("isDeactivated", !isActive);
        }
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetBayonetTrapState(!isActive);
        }

        Debug.Log("Ловушка переключена. Активна: " + isActive);
    }
    
    public void SetState(bool deactivated)
    {
        isActive = !deactivated;
    
        // 2. А вот здесь (при загрузке) нам нужно мгновенное состояние
        if (animator != null)
        {
            animator.SetBool("isDeactivated", deactivated);
        
            if (deactivated)
            {
                // Мгновенно выключена
                animator.Play("Deactivated_Idle", 0, 1f);
            }
            else
            {
                // Мгновенно включена (штыки торчат)
                animator.Play("Active_Idle", 0, 1f);
            }
        }
    }
}