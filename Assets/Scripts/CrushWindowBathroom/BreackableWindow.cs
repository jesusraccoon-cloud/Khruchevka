using UnityEngine; // Подключаем Unity-классы
using System.Collections; // Подключаем корутины

public class BreakableWindow : MonoBehaviour, IHitInteractable
// Скрипт разбиваемого окна
// IHitInteractable позволяет PlayerInteractor наносить удары по окну
{
    public GameObject windowIntact;
    // Целое окно

    public GameObject windowBroken;
    // Разбитое окно

    public int hitsToBreak = 3;
    // Сколько ударов нужно, чтобы разбить окно

    public float hitDelay = 0.3f;
    // Задержка между ударами, чтобы нельзя было спамить ЛКМ

    private int currentHits = 0;
    // Сколько ударов уже нанесено

    private bool isBroken = false;
    // Разбито окно или нет

    private bool canHit = true;
    // Можно ли сейчас наносить удар

    public bool IsBroken
    {
        get { return isBroken; }
    }
    // Даём другим скриптам узнать, разбито окно или нет

    void Start()
    // Вызывается один раз при запуске сцены
    {
        if (windowIntact != null)
        // Если целое окно назначено
        {
            windowIntact.SetActive(true);
            // Включаем целое окно
        }

        if (windowBroken != null)
        // Если разбитое окно назначено
        {
            windowBroken.SetActive(false);
            // Выключаем разбитое окно
        }
    }

    public void Hit()
    // Метод вызывается PlayerInteractor при ударе ЛКМ
    {
        if (isBroken) return;
        // Если окно уже разбито — ничего не делаем

        if (!canHit) return;
        // Если задержка между ударами ещё не закончилась — выходим

        currentHits++;
        // Добавляем один удар

        Debug.Log("Удар по окну: " + currentHits + " / " + hitsToBreak);
        // Показываем прогресс ударов

        if (currentHits >= hitsToBreak)
        // Если ударов достаточно
        {
            Break();
            // Разбиваем окно
        }
        else
        {
            StartCoroutine(HitCooldown());
            // Запускаем задержку до следующего удара
        }
    }

    IEnumerator HitCooldown()
    // Корутина задержки между ударами
    {
        canHit = false;
        // Временно запрещаем наносить удар

        yield return new WaitForSeconds(hitDelay);
        // Ждём указанное количество секунд

        canHit = true;
        // Снова разрешаем удар
    }

    void Break()
    // Метод разбития окна
    {
        if (isBroken) return;
        // Если окно уже разбито — выходим

        isBroken = true;
        // Помечаем окно как разбитое

        Debug.Log("ОКНО РАЗБИТО!");
        // Сообщение в Console

        if (windowIntact != null)
        // Если целое окно назначено
        {
            windowIntact.SetActive(false);
            // Выключаем целое окно
        }

        if (windowBroken != null)
        // Если разбитое окно назначено
        {
            windowBroken.SetActive(true);
            // Включаем разбитое окно
        }
    }
}