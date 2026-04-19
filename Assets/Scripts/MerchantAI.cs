using UnityEngine;

public class MerchantAI : MonoBehaviour
{
    public float speed = 2f;
    public float walkDistance = 3f;
    
    private Vector2 startPos;
    private bool movingRight = true;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        
        // Чтобы точно не упал и не крутился
        if(rb != null) {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }
    }

    void FixedUpdate()
    {
        float leftBoundary = startPos.x - walkDistance;
        float rightBoundary = startPos.x + walkDistance;

        // Используем жесткое переключение, чтобы он не "застревал" на границе
        if (movingRight && transform.position.x >= rightBoundary)
        {
            movingRight = false;
        }
        else if (!movingRight && transform.position.x <= leftBoundary)
        {
            movingRight = true;
        }

        float direction = movingRight ? 1 : -1;
    
        // Прямое изменение velocity часто работает лучше для простых патрулей, 
        // но если оставляем MovePosition, убедись, что Body Type в Rigidbody2D — Kinematic или Dynamic
        Vector2 nextPos = rb.position + new Vector2(direction * speed * Time.fixedDeltaTime, 0);
        rb.MovePosition(nextPos);

        // Разворот (используем Abs, чтобы не сломать масштаб, если он не 1)
        float scaleX = Mathf.Abs(transform.localScale.x) * direction;
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
    }
}