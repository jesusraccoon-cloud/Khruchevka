using UnityEngine; // Подключаем Unity: GameObject, Transform, Debug и другие базовые классы

public class ApartmentFinalSequence : MonoBehaviour // Главный режиссёр финальной сцены квартиры
{
    [Header("Final Objects")] // Блок объектов финального события
    public GameObject fallenWardrobe; // Упавший шкаф, который включится после сбора всех кассет

    [Header("Room 1 Door Break")] // Блок визуальной поломки двери комнаты 1
    public GameObject normalRoomDoor; // Обычная дверь комнаты 1
    public GameObject brokenDoorOnFloor; // Выбитая дверь на полу

    [Header("Bathroom Door")] // Блок двери ванной
    public UniversalDoor bathroomDoor; // Дверь ванной

    [Header("Bathroom Lock")] // Блок замка ванной
    public GameObject bathroomLockCollider; // Коллайдер-замок, который появится после 6/6

    [Header("Monster")] // Блок монстра
    public GameObject monsterObject; // Объект монстра
    public MonsterAI monsterAI; // AI монстра
    public MonsterPatrol monsterPatrol; // Патруль монстра

    public Transform monsterExitBlockPoint; // Точка у выхода

    [Header("Triggers")] // Блок финальных триггеров
    public GameObject hallReturnDeathTrigger; // Триггер смерти при возврате
    public GameObject kitchenFinalTrigger; // Триггер кухни

    [HideInInspector] // Прячем из Inspector
    public bool finalSequenceStarted = false; // Началась ли финальная фаза

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

        if (brokenDoorOnFloor != null) // Если выбитая дверь назначена
        {
            brokenDoorOnFloor.SetActive(true); // Показываем выбитую дверь
        }

        if (bathroomDoor != null) // Если дверь ванной назначена
        {
            bathroomDoor.CloseDoor(); // Закрываем дверь ванной
            bathroomDoor.isLocked = true; // Блокируем дверь ванной
            bathroomDoor.canMonsterOpen = false; // Монстр не может открыть ванную
        }

        if (bathroomLockCollider != null) // Если коллайдер-замок назначен
        {
            bathroomLockCollider.SetActive(true); // Включаем замок после 6/6
        }

        if (hallReturnDeathTrigger != null) // Если триггер смерти назначен
        {
            hallReturnDeathTrigger.SetActive(true); // Включаем триггер смерти
        }

        if (kitchenFinalTrigger != null) // Если кухонный триггер назначен
        {
            kitchenFinalTrigger.SetActive(true); // Включаем кухонный триггер
        }

        BlockExitWithMonster(); // Ставим монстра у выхода

        Debug.Log("Финальная последовательность квартиры запущена"); // Сообщение в Console
    }

    public void BlockExitWithMonster() // Метод блокировки выхода монстром
    {
        if (exitBlocked) return; // Если уже заблокирован — выходим

        exitBlocked = true; // Запоминаем блокировку

        if (monsterObject != null) // Если монстр назначен
        {
            monsterObject.SetActive(true); // Включаем монстра
        }

        if (monsterExitBlockPoint != null && monsterObject != null) // Если точка и монстр есть
        {
            monsterObject.transform.position = monsterExitBlockPoint.position; // Ставим монстра к выходу
            monsterObject.transform.rotation = monsterExitBlockPoint.rotation; // Поворачиваем монстра
        }

        if (monsterAI != null) // Если AI назначен
        {
            monsterAI.isActivated = false; // Отключаем обычный AI
        }

        if (monsterPatrol != null) // Если патруль назначен
        {
            monsterPatrol.isPatrolActive = false; // Отключаем патруль
        }

        Debug.Log("Монстр заблокировал выход"); // Сообщение в Console
    }
}