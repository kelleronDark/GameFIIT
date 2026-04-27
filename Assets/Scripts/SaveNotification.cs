using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Animator))]
public class SaveNotification : MonoBehaviour
{
    [Header("Настройки")]
    public float displayTime = 2.5f; // Сколько времени иконка крутится
    public float fadeDuration = 0.5f; // Время плавного исчезновения

    private CanvasGroup canvasGroup;
    private Animator anim;

    void Awake()
    {
        // Инициализируем компоненты
        anim = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Скрываем объект при старте игры
        gameObject.SetActive(false);
    }

    public void Show()
    {
        // Включаем объект (теперь корутина запустится без ошибок)
        gameObject.SetActive(true);
        
        // Сбрасываем прозрачность в 1 (видимый)
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // ПРИНУДИТЕЛЬНЫЙ ЗАПУСК АНИМАЦИИ
        // Проигрываем анимацию с 0-го кадра, чтобы шестеренка сразу крутилась
        if (anim != null)
        {
            anim.Play(0, -1, 0f); 
        }

        // Перезапускаем корутину, если сохранение произошло дважды быстро
        StopAllCoroutines();
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // 1. Ждем, пока игрок любуется анимацией
        yield return new WaitForSeconds(displayTime);

        // 2. Плавно гасим иконку через альфа-канал
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            }
            yield return null;
        }

        // 3. Выключаем объект полностью
        gameObject.SetActive(false);
    }
}