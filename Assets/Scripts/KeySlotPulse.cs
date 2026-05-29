using UnityEngine;

public class KeySlotPulse : MonoBehaviour
{
    public float pulseScale = 4f;
    public float pulseDuration = 0.4f;
    
    private Vector3 originalScale;
    private bool isPulsing = false;
    private float pulseTimer = 0f;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (isPulsing)
        {
            pulseTimer += Time.deltaTime / pulseDuration;
            
            float scaleMultiplier = Mathf.Lerp(1f, pulseScale, Mathf.Sin(pulseTimer * Mathf.PI));
            transform.localScale = originalScale * scaleMultiplier;
            
            if (pulseTimer >= 1f)
            {
                isPulsing = false;
                transform.localScale = originalScale;
            }
        }
    }

    public void Pulse()
    {
        isPulsing = true;
        pulseTimer = 0f;
    }
}