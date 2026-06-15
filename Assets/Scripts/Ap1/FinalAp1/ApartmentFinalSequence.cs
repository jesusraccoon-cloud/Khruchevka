using UnityEngine; // Подключаем Unity

public class ApartmentFinalSequence : MonoBehaviour // Главный режиссёр финальной сцены квартиры
{
    [Header("Final Objects")] // Блок финальных объектов
    public GameObject fallenWardrobe; // Упавший шкаф

    [Header("Closet Fall")] // Блок падения шкафа
    public ClosetPhysicalFall closetPhysicalFall; // Скрипт падения шкафа

    [Header("Room 1 Door Break")] // Блок поломки двери комнаты
    public GameObject normalRoomDoor; // Обычная дверь комнаты
    public GameObject brokenDoorOnFloor; // Выбитая дверь комнаты

    [Header("Bathroom Door")] // Блок двери ванной
    public UniversalDoor bathroomDoor; // Дверь ванной

    [Header("Bathroom Lock")] // Блок замка ванной
    public GameObject bathroomLockCollider; // Коллайдер замка ванной

    [Header("Monster")] // Блок монстра
    public GameObject monsterObject; // Объект монстра
    public MonsterAI monsterAI; // AI монстра
    public MonsterPatrol monsterPatrol; // Патруль монстра
    public Transform monsterExitBlockPoint; // Точка блокировки выхода

    [Header("Window First Hit Reaction")] // Блок реакции на первый удар по окну
    public GameObject finalNormalDoor; // Обычная дверь перед реакцией
    public GameObject finalBrokenDoor; // Сломанная дверь после реакции
    public Rigidbody fallenWardrobeRigidbody; // Rigidbody шкафа
    public Vector3 wardrobeForceDirection = new Vector3(1f, 0.2f, 0f); // Направление толчка шкафа
    public float wardrobeForce = 4f; // Сила толчка шкафа
    public float wardrobeTorque = 2f; // Сила вращения шкафа
    public Transform monsterAfterWindowHitPoint; // Точка монстра после удара по окну

    [Header("Triggers")] // Блок триггеров
    public GameObject hallReturnDeathTrigger; // Триггер смерти при возврате в коридор
    public GameObject kitchenFinalTrigger; // Триггер кухни
    public GameObject bathroomExitChaseTrigger; // Триггер выхода из ванной
    public GameObject apartmentExitCompleteTrigger; // Триггер завершения квартиры после выхода

    [Header("Apartment Completion")] // Блок завершения квартиры
    public UniversalDoor apartmentExitDoor; // Входная дверь квартиры
    public bool lockApartmentDoorAfterExit = true; // Блокировать ли дверь после выхода

    [HideInInspector] public bool finalSequenceStarted = false; // Финал начался
    [HideInInspector] public bool apartmentCompleted = false; // Квартира завершена
    [HideInInspector] public bool readyToDisableByTumbler = false; // Можно отключить квартиру тумблером УМПСР

    private bool finalStarted = false; // Финал уже запускался
    private bool exitBlocked = false; // Выход уже блокировался
    private bool windowFirstHitReactionStarted = false; // Реакция на первый удар уже была
    private bool playerEscapedThroughWindow = false; // Игрок перелез через окно
    private bool bathroomExitTriggered = false; // Триггер выхода из ванной уже сработал

    private void Start() // При старте сцены
    {
        if (closetPhysicalFall != null) closetPhysicalFall.canFall = false; // Запрещаем падение шкафа до финала

        if (bathroomExitChaseTrigger != null) bathroomExitChaseTrigger.SetActive(false); // Выключаем триггер выхода из ванной на старте

        if (apartmentExitCompleteTrigger != null) apartmentExitCompleteTrigger.SetActive(false); // Выключаем триггер завершения квартиры до финала
    }

    public void StartFinalSequence() // Запуск финала
    {
        if (finalStarted) return; // Если финал уже был — выходим

        finalStarted = true; // Запоминаем запуск финала
        finalSequenceStarted = true; // Сообщаем другим скриптам, что финал начался

        if (closetPhysicalFall != null) closetPhysicalFall.canFall = true; // Разрешаем падение шкафа

        if (fallenWardrobe != null) fallenWardrobe.SetActive(true); // Включаем упавший шкаф

        if (normalRoomDoor != null) normalRoomDoor.SetActive(false); // Прячем обычную дверь комнаты

        if (brokenDoorOnFloor != null) brokenDoorOnFloor.SetActive(true); // Показываем выбитую дверь

        if (bathroomDoor != null) // Если дверь ванной назначена
        {
            bathroomDoor.CloseDoor(); // Закрываем дверь ванной
            bathroomDoor.SetLocked(true); // Блокируем дверь ванной
            bathroomDoor.canMonsterOpen = false; // Запрещаем монстру открыть ванную
        }

        if (bathroomLockCollider != null) bathroomLockCollider.SetActive(true); // Включаем замок ванной

        if (hallReturnDeathTrigger != null) hallReturnDeathTrigger.SetActive(true); // Включаем триггер смерти

        if (kitchenFinalTrigger != null) kitchenFinalTrigger.SetActive(true); // Включаем кухонный триггер

        if (bathroomExitChaseTrigger != null) bathroomExitChaseTrigger.SetActive(false); // Пока держим триггер ванной выключенным

        if (apartmentExitCompleteTrigger != null) apartmentExitCompleteTrigger.SetActive(true); // Включаем триггер завершения квартиры

        BlockExitWithMonster(); // Отправляем монстра блокировать выход

        Debug.Log("Финальная последовательность квартиры запущена"); // Debug
    }

    public void BlockExitWithMonster() // Монстр идёт блокировать выход
    {
        if (exitBlocked) return; // Если уже блокировал — выходим

        exitBlocked = true; // Запоминаем блокировку

        if (monsterObject != null) monsterObject.SetActive(true); // Включаем монстра

        if (monsterAI != null && monsterExitBlockPoint != null) monsterAI.GoToPointAndStop(monsterExitBlockPoint); // Отправляем монстра к выходу

        if (monsterPatrol != null) monsterPatrol.isPatrolActive = false; // Выключаем патруль

        Debug.Log("Монстр пошёл блокировать выход"); // Debug
    }

    public void OnFinalWindowFirstHit() // Первый удар по окну
    {
        if (windowFirstHitReactionStarted) return; // Если реакция уже была — выходим

        windowFirstHitReactionStarted = true; // Запоминаем реакцию

        if (finalNormalDoor != null) finalNormalDoor.SetActive(false); // Прячем обычную дверь

        if (finalBrokenDoor != null) finalBrokenDoor.SetActive(true); // Показываем сломанную дверь

        if (fallenWardrobeRigidbody != null) // Если Rigidbody шкафа назначен
        {
            fallenWardrobeRigidbody.isKinematic = false; // Включаем физику шкафа
            fallenWardrobeRigidbody.AddForce(wardrobeForceDirection.normalized * wardrobeForce, ForceMode.Impulse); // Толкаем шкаф
            fallenWardrobeRigidbody.AddTorque(Random.insideUnitSphere * wardrobeTorque, ForceMode.Impulse); // Добавляем вращение
        }

        if (monsterObject != null) monsterObject.SetActive(true); // Включаем монстра

        if (monsterAI != null && monsterAfterWindowHitPoint != null) monsterAI.StartFinalWindowThreat(monsterAfterWindowHitPoint); // Запускаем угрозу у окна

        Debug.Log("Первый удар по окну: монстр начал угрозу у окна"); // Debug
    }

    public void OnPlayerEscapedThroughWindow() // Игрок перелез через окно
    {
        if (!finalSequenceStarted) return; // Если финал не начался — выходим

        playerEscapedThroughWindow = true; // Запоминаем, что игрок перелез

        if (bathroomExitChaseTrigger != null) bathroomExitChaseTrigger.SetActive(true); // Включаем триггер погони после ванной

        Debug.Log("Игрок перелез через окно, триггер выхода из ванной включён"); // Debug
    }

    public void OnBathroomExitTrigger() // Игрок вышел из ванной
    {
        if (bathroomExitTriggered) return; // Если уже сработало — выходим

        if (!finalSequenceStarted) return; // Если финал не начался — выходим

        if (!playerEscapedThroughWindow) return; // Если игрок не перелез через окно — выходим

        bathroomExitTriggered = true; // Запоминаем срабатывание

        if (monsterObject != null) monsterObject.SetActive(true); // Гарантированно включаем монстра

        if (monsterPatrol != null) monsterPatrol.isPatrolActive = false; // Гарантированно выключаем патруль

        if (monsterAI != null) monsterAI.ForceChasePlayer(); // Запускаем постоянную финальную погоню

        if (bathroomExitChaseTrigger != null) bathroomExitChaseTrigger.SetActive(false); // Отключаем триггер

        Debug.Log("Игрок вышел из ванной, монстр начал финальную погоню"); // Debug
    }

    public void TryCompleteApartmentAfterExit() // Игрок вышел из квартиры после финала
    {
        if (apartmentCompleted) return; // Если квартира уже завершена — выходим

        if (!finalSequenceStarted) // Если финал ещё не начался
        {
            Debug.Log("Квартиру нельзя завершить: финал 6/6 ещё не запущен"); // Debug

            return; // Выходим
        }

        apartmentCompleted = true; // Запоминаем завершение квартиры
        readyToDisableByTumbler = true; // Разрешаем отключение через тумблер УМПСР

        if (apartmentExitDoor != null) // Если входная дверь квартиры назначена
        {
            apartmentExitDoor.CloseDoor(); // Закрываем дверь квартиры

            if (lockApartmentDoorAfterExit) apartmentExitDoor.SetLocked(true); // Блокируем дверь квартиры
        }

        Debug.Log("Квартира завершена. Теперь её можно отключить тумблером УМПСР"); // Debug
    }
}