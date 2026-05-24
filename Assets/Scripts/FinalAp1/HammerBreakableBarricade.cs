using UnityEngine; // Подключаем Unity-классы

public class HammerBreakableBarricade : MonoBehaviour, IHitInteractable // Баррикада, которую можно сбить ударом
{
    [Header("Final Sequence")] // Блок финальной последовательности
    public ApartmentFinalSequence finalSequence; // Ссылка на финальную фазу квартиры

    [Header("Hit Settings")] // Блок настроек ударов
    public int hitsToBreak = 3; // Сколько ударов нужно для сбивания

    public float hitDelay = 0.3f; // Минимальная пауза между ударами

    [Header("Object Swap")] // Блок замены объектов
    public GameObject standingObject; // Стоящий шкаф до сбивания

    public GameObject fallenObject; // Упавший шкаф после сбивания

    [Header("Optional")] // Дополнительные настройки
    public bool disableThisAfterBreak = true; // Отключить ли этот объект после сбивания

    private int currentHits = 0; // Сколько ударов уже нанесено

    private float lastHitTime = -999f; // Время последнего удара

    private bool isBroken = false; // Сбит ли шкаф уже

    public void Hit() // Метод вызывается PlayerInteractor при ЛКМ
    {
        if (finalSequence == null) return; // Если финальная последовательность не назначена — выходим

        if (!finalSequence.finalSequenceStarted) return; // Если финальная фаза ещё не началась — шкаф не ломается

        if (isBroken) return; // Если шкаф уже сбит — ничего не делаем

        if (Time.time < lastHitTime + hitDelay) return; // Если игрок бьёт слишком быстро — игнорируем удар

        lastHitTime = Time.time; // Запоминаем время удара

        currentHits++; // Увеличиваем количество ударов

        Debug.Log("Удар по баррикаде: " + currentHits + "/" + hitsToBreak); // Пишем прогресс в Console

        if (currentHits >= hitsToBreak) // Если ударов достаточно
        {
            BreakBarricade(); // Сбиваем шкаф
        }
    }

    void BreakBarricade() // Метод сбивания шкафа
    {
        isBroken = true; // Запоминаем, что шкаф уже сбит

        if (standingObject != null) // Если стоящий шкаф назначен
        {
            standingObject.SetActive(false); // Выключаем стоящий шкаф
        }

        if (fallenObject != null) // Если упавший шкаф назначен
        {
            fallenObject.SetActive(true); // Включаем упавший шкаф
        }

        Debug.Log("Баррикада сбита"); // Пишем сообщение в Console

        if (disableThisAfterBreak) // Если нужно отключить сам объект со скриптом
        {
            enabled = false; // Отключаем этот скрипт
        }
    }
}