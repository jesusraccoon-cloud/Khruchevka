using UnityEngine; // Подключаем Unity: GameObject, Transform, Debug и другие базовые классы

public class ApartmentFinalSequence : MonoBehaviour // Главный режиссёр финальной сцены квартиры
{
    [Header("Final Objects")] // Блок объектов финального события
    public GameObject fallenWardrobe; // Упавший шкаф, который включится после сбора всех кассет

    [Header("Monster")] // Блок монстра
    public GameObject monsterObject; // Сам объект монстра в сцене
    public MonsterAI monsterAI; // Главный AI монстра
    public MonsterPatrol monsterPatrol; // Патруль монстра

    public Transform monsterExitBlockPoint; // Точка у выхода, куда монстр переместится для блокировки пути игроку

    [Header("Triggers")] // Блок финальных триггеров
    public GameObject exitBlockTrigger; // Триггер сцены у выхода
    public GameObject hallReturnDeathTrigger; // Триггер смерти при попытке вернуться в зал
    public GameObject kitchenFinalTrigger; // Триггер кухни для финальной сцены

    private bool finalStarted = false; // Защита от повторного запуска финала
    private bool exitBlocked = false; // Защита от повторной блокировки выхода

    public void StartFinalSequence() // Метод запуска финала квартиры
    {
        if (finalStarted) return; // Если финал уже запускался — выходим

        finalStarted = true; // Запоминаем, что финал начался

        if (fallenWardrobe != null) // Если упавший шкаф назначен
        {
            fallenWardrobe.SetActive(true); // Включаем упавший шкаф
        }

        if (monsterPatrol != null) // Если патруль монстра назначен
        {
            monsterPatrol.isPatrolActive = false; // Останавливаем обычный патруль
        }

        if (monsterAI != null) // Если AI монстра назначен
        {
            monsterAI.isActivated = false; // Временно выключаем обычный AI
        }

        if (exitBlockTrigger != null) // Если триггер выхода назначен
        {
            exitBlockTrigger.SetActive(true); // Включаем триггер у выхода
        }

        if (hallReturnDeathTrigger != null) // Если триггер смерти назначен
        {
            hallReturnDeathTrigger.SetActive(true); // Включаем опасную зону возврата в зал
        }

        if (kitchenFinalTrigger != null) // Если триггер кухни назначен
        {
            kitchenFinalTrigger.SetActive(true); // Включаем кухонный триггер
        }

        Debug.Log("Финальная последовательность квартиры запущена"); // Сообщение в Console
    }

    public void BlockExitWithMonster() // Метод блокировки выхода монстром
    {
        if (exitBlocked) return; // Если выход уже заблокирован — выходим

        exitBlocked = true; // Запоминаем, что блокировка уже произошла

        if (monsterObject != null) // Если объект монстра назначен
        {
            monsterObject.SetActive(true); // На всякий случай включаем монстра
        }

        if (monsterExitBlockPoint != null && monsterObject != null) // Если есть точка и монстр
        {
            monsterObject.transform.position = monsterExitBlockPoint.position; // Перемещаем монстра к выходу
            monsterObject.transform.rotation = monsterExitBlockPoint.rotation; // Поворачиваем монстра как точку
        }

        if (monsterAI != null) // Если AI монстра назначен
        {
            monsterAI.isActivated = false; // Оставляем AI выключенным, чтобы монстр стоял на месте
        }

        if (monsterPatrol != null) // Если патруль назначен
        {
            monsterPatrol.isPatrolActive = false; // Убеждаемся, что патруль выключен
        }

        Debug.Log("Монстр заблокировал выход"); // Сообщение в Console
    }
}