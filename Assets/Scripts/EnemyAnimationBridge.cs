using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    private EnemyAI enemyAI;

    void Start()
    {
        enemyAI = GetComponentInParent<EnemyAI>();
        
        if (enemyAI == null)
        {
            Debug.LogError($"AnimationBridge на {gameObject.name}: Не найден EnemyAI на родительских объектах!");
        }
    }

    public void OnAttackAnimationHit()
    {
        if (enemyAI != null)
        {
            enemyAI.OnAttackAnimationHit();
        }
    }
}