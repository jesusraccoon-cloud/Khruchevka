using UnityEngine; // Подключаем Unity: GameObject, Transform, Debug и другие базовые классы

public class ApartmentFinalSequence : MonoBehaviour // Главный режиссёр финальной сцены квартиры
{
    [Header("Final Objects")] // Блок объектов финального события
    public GameObject fallenWardrobe; // Упавший шкаф, который включится после сбора всех кассет

    [Header("Room 1 Door Break")] // Блок визуальной поломки двери комнаты 1
    public GameObject normalRoomDoor; // Обычная дверь комнаты 1, которая исчезнет после 6/6 кассет
    public GameObject brokenDoorOnFloor; // Выбитая дверь на полу, которая появится после 6/6 кассет

    [Header("Bathroom Door")] // Блок двери ванной
    public UniversalDoor bathroomDoor; // Дверь ванной, которая заблокируется после 6/6 кассет

    [Header("Monster")] // Блок монстра
    public GameObject monsterObject; // Сам объект монстра в сцене
    public MonsterAI monsterAI; // Главный AI монстра
    public MonsterPatrol monsterPatrol; // Патруль монстра

    public Transform monsterExitBlockPoint; // Точка у выхода, куда монстр переместится для блокировки пути игроку

    [Header("Triggers")] // Блок финальных триггеров
    public GameObject hallReturnDeathTrigger; // Триггер смерти при попытке вернуться в зал
    public GameObject kitchenFinalTrigger; // Триггер кухни для финальной сцены

    [HideInInspector] // Прячем переменную из Inspector, чтобы случайно не менять её руками
    public bool finalSequenceStarted = false; // Началась ли финальная фаза квартиры

    private bool finalStarted = false; // Защита от повторного запуска финала
    private bool exitBlocked = false; // Защита от повторной блокировки выхода

    public void StartFinalSequence() // Метод запуска финала квартиры
    {
        if (finalStarted) return; // Если финал уже запускался — выходим

        finalStarted = true; // Запоминаем, что финал начался

        finalSequenceStarted = true; // Сообщаем другим скриптам, что финальная фаза началась

        if (fallenWardrobe != null) // Если упавший шкаф назначен
        {
            fallenWardrobe.SetActive(true); // Включаем упавший шкаф
        }

        if (normalRoomDoor != null) // Если обычная дверь комнаты назначена
        {
            normalRoomDoor.SetActive(false); // Прячем обычную дверь
        }

        if (brokenDoorOnFloor != null) // Если выбитая дверь на полу назначена
        {
            brokenDoorOnFloor.SetActive(true); // Показываем выбитую дверь на полу
        }

        if (bathroomDoor != null) // Если дверь ванной назначена
        {
            bathroomDoor.CloseDoor(); // Закрываем дверь ванной
            bathroomDoor.isLocked = true; // Блокируем дверь ванной
            bathroomDoor.canMonsterOpen = false; // Запрещаем монстру открывать дверь ванной
        }

        if (hallReturnDeathTrigger != null) // Если триггер смерти назначен
        {
            hallReturnDeathTrigger.SetActive(true); // Включаем опасную зону возврата в зал
        }

        if (kitchenFinalTrigger != null) // Если триггер кухни назначен
        {
            kitchenFinalTrigger.SetActive(true); // Включаем кухонный триггер
        }

        BlockExitWithMonster(); // Сразу ставим монстра у выхода после сбора всех кассет

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