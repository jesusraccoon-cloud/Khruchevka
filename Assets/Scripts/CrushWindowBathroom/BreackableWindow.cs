using UnityEngine; // Подключаем Unity-классы
using System.Collections; // Подключаем корутины

public class BreakableWindow : MonoBehaviour, IHitInteractable // Окно получает удары от PlayerInteractor
{
    public GameObject windowIntact; // Целая версия окна
    public GameObject windowBroken; // Разбитая версия окна

    public int hitsToBreak = 3; // Сколько ударов нужно для разбития
    public float hitDelay = 0.3f; // Задержка между ударами

    private int currentHits = 0; // Текущее количество ударов
    private bool isBroken = false; // Разбито ли окно
    private bool canHit = true; // Можно ли сейчас ударить

    public bool IsBroken // Свойство для других скриптов
    {
        get { return isBroken; } // Возвращаем состояние окна
    }

    void Start() // Вызывается при запуске сцены
    {
        if (windowIntact != null) // Если целое окно назначено
        {
            windowIntact.SetActive(true); // Показываем целое окно
        }

        if (windowBroken != null) // Если разбитое окно назначено
        {
            windowBroken.SetActive(false); // Прячем разбитое окно
        }
    }

    public void Hit() // Вызывается при ударе по окну
    {
        if (isBroken) return; // Если уже разбито — выходим
        if (!canHit) return; // Если кулдаун не прошел — выходим

        currentHits++; // Добавляем один удар

        if (currentHits >= hitsToBreak) // Если ударов достаточно
        {
            Break(); // Разбиваем окно
        }
        else // Если ударов пока недостаточно
        {
            StartCoroutine(HitCooldown()); // Запускаем задержку до следующего удара
        }
    }

    IEnumerator HitCooldown() // Корутина задержки между ударами
    {
        canHit = false; // Запрещаем новый удар

        yield return new WaitForSeconds(hitDelay); // Ждем указанное время

        canHit = true; // Снова разрешаем удар
    }

    void Break() // Метод разбития окна
    {
        if (isBroken) return; // Если уже разбито — выходим

        isBroken = true; // Помечаем окно как разбитое

        if (windowIntact != null) // Если целое окно назначено
        {
            windowIntact.SetActive(false); // Выключаем целое окно
        }

        if (windowBroken != null) // Если разбитое окно назначено
        {
            windowBroken.SetActive(true); // Включаем разбитое окно
        }
    }
}