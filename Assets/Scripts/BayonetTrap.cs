using UnityEngine;

public class BayonetTrap : MonoBehaviour
{
    [Header("Trap Settings")]
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
                damageAccumulator += damagePerSecond * Time.deltaTime;

                if (damageAccumulator >= 1f)
                {
                    int damageToApply = Mathf.FloorToInt(damageAccumulator);
                    player.TakeDamage(damageToApply);
                    
                    damageAccumulator -= damageToApply;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
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
    
        if (animator != null)
        {
            animator.SetBool("isDeactivated", deactivated);
        
            if (deactivated)
            {
                animator.Play("Deactivated_Idle", 0, 1f);
            }
            else
            {
                animator.Play("Active_Idle", 0, 1f);
            }
        }
    }
}