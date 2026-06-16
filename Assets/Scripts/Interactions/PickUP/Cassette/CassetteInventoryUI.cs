using UnityEngine; // Подключаем Unity-классы
using TMPro; // Подключаем TextMeshPro

public class CassetteInventoryUI : MonoBehaviour // Скрипт счетчика кассет
{
    [Header("UI")] // Блок UI
    public TextMeshProUGUI cassetteCounterText; // Текст счетчика кассет

    [Header("Settings")] // Блок настроек
    public int currentCassetteCount = 0; // Текущее количество кассет
    public int maxCassetteCount = 6; // Максимум кассет

    [Header("Monster")] // Блок монстра
    public MonsterAI monsterAI; // Ссылка на главный скрипт монстра

    [Header("Hall Doors")] // Блок дверей в зал
    public UniversalDoor[] hallDoors; // Массив дверей в зал

    [Header("Final Sequence")] // Блок финальной последовательности
    public ApartmentFinalSequence finalSequence; // Ссылка на режиссёрский скрипт финала квартиры

    public int activateMonsterAt = 4; // На какой кассете активировать монстра

    private bool monsterActivated = false; // Защита от повторной активации монстра
    private bool finalEventTriggered = false; // Защита от повторного запуска финального события

    void Start() // При старте сцены
    {
        UpdateUI(); // Обновляем UI
    }

    public void AddCassette() // Добавить кассету
    {
        currentCassetteCount = Mathf.Clamp(currentCassetteCount + 1, 0, maxCassetteCount); // Увеличиваем счетчик и не даём выйти за максимум

        if (!monsterActivated && currentCassetteCount >= activateMonsterAt) // Если монстр ещё не активирован и собрано 4+ кассеты
        {
            ActivateMonster(); // Активируем монстра и открываем доступ в зал
        }

        if (!finalEventTriggered && currentCassetteCount >= maxCassetteCount) // Если финал ещё не запущен и собраны все кассеты
        {
            TriggerFinalEvent(); // Запускаем финальную последовательность
        }

        UpdateUI(); // Обновляем текст счетчика
    }

    void ActivateMonster() // Метод события на 4/6 кассет
    {
    monsterActivated = true; // Запоминаем, что событие 4/6 уже запущено

    if (hallDoors != null) // Если массив дверей существует
    {
        foreach (UniversalDoor door in hallDoors) // Проходим по каждой двери
        {
            if (door != null) // Если дверь назначена
            {
                door.SetLocked(false); // Разблокируем дверь через метод, а не напрямую через переменную
            }
        }
    }

    if (finalSequence != null) // Если ApartmentFinalSequence назначен
    {
        finalSequence.StartEarlyHallDoorBreakSequence(); // Запускаем выламывание дверей монстром
    }
    else if (monsterAI != null) // Если финальный режиссёр не назначен, но монстр есть
    {
        monsterAI.ActivateMonster(); // Запасной вариант: просто активируем монстра
    }
    else // Если ничего не назначено
    {
        Debug.LogWarning("CassetteInventoryUI: не назначен ApartmentFinalSequence или MonsterAI"); // Пишем предупреждение
    }
    }

    void TriggerFinalEvent() // Метод запуска финального события
    {
        finalEventTriggered = true; // Запоминаем, что финал уже был запущен

        if (finalSequence != null) // Если режиссёр финала назначен
        {
            finalSequence.StartFinalSequence(); // Запускаем финальную последовательность квартиры
        }
        else // Если ссылка не назначена
        {
            Debug.LogWarning("ApartmentFinalSequence не назначен в CassetteInventoryUI"); // Пишем предупреждение в Console
        }
    }

    void UpdateUI() // Обновление UI
    {
        if (cassetteCounterText == null) return; // Если текста нет — выходим

        cassetteCounterText.text = currentCassetteCount + "/" + maxCassetteCount; // Показываем 0/6, 1/6 и т.д.
    }
    public void SetCassetteCountDebug(int newCount) // Debug-установка количества кассет
    {
        currentCassetteCount = Mathf.Clamp(newCount, 0, maxCassetteCount); // Ставим нужное количество кассет и ограничиваем максимумом

        if (!monsterActivated && currentCassetteCount >= activateMonsterAt) // Если монстр ещё не активирован и кассет 4 или больше
        {
            ActivateMonster(); // Активируем монстра и разблокируем двери в зал
        }

        if (!finalEventTriggered && currentCassetteCount >= maxCassetteCount) // Если финал ещё не запускался и кассет 6 или больше
        {
            TriggerFinalEvent(); // Запускаем финальную последовательность
        }

        UpdateUI(); // Обновляем текст счетчика
    }
}