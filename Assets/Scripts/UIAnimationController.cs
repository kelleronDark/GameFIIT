using UnityEngine;
using System.Collections;

public class UIAnimationController : MonoBehaviour
{
    public static UIAnimationController Instance;

    [Header("Ссылки")]
    [SerializeField] private CanvasGroup canvasGroup; 
    [SerializeField] private RectTransform iconTransform; 
    [SerializeField] private Animator animator;

    [Header("Параметры появления")]
    [SerializeField] private float animationSpeed = 4f;
    [SerializeField] private float waitTime = 2f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (canvasGroup != null) canvasGroup.alpha = 0;
    }

    public void TriggerSaveIcon()
    {
        // Останавливаем старую анимацию, если она еще шла, и запускаем новую
        StopAllCoroutines();
        StartCoroutine(SaveAnimationRoutine());
    }

    private IEnumerator SaveAnimationRoutine()
    {
        // 1. Запускаем кручение дискеты в Animator сразу
        if (animator != null)
        {
            animator.Play("SaveAnimation", 0, 0f);
        }

        // 2. Плавное появление (Fade In)
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            iconTransform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, t);
            yield return null;
        }

        // 3. Ждем пока игрок полюбуется на крутящуюся дискету
        yield return new WaitForSeconds(waitTime);

        // 4. Плавное исчезновение (Fade Out)
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t);
            iconTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, t);
            yield return null;
        }
        
        canvasGroup.alpha = 0;
    }
}