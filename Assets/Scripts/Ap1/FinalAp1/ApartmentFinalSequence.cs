using UnityEngine; // Подключаем Unity

public class ApartmentFinalSequence : MonoBehaviour // Главный режиссёр финальной сцены квартиры
{
    [Header("Final Objects")] // Блок финальных объектов
    public GameObject fallenWardrobe; // Упавший шкаф

    [Header("Closet Fall")] // Блок ручного падения шкафа
    public ClosetPhysicalFall closetPhysicalFall; // Скрипт падения шкафа

    [Header("Room 1 Door Break")] // Блок поломки двери комнаты
    public GameObject normalRoomDoor; // Обычная дверь комнаты
    public GameObject brokenDoorOnFloor; // Выбитая дверь

    [Header("Bathroom Door")] // Блок двери ванной
    public UniversalDoor bathroomDoor; // Дверь ванной

    [Header("Bathroom Lock")] // Блок замка ванной
    public GameObject bathroomLockCollider; // Замок ванной

    [Header("Monster")] // Блок монстра
    public GameObject monsterObject; // Объект монстра
    public MonsterAI monsterAI; // AI монстра
    public MonsterPatrol monsterPatrol; // Патруль монстра
    public Transform monsterExitBlockPoint; // Точка блокировки выхода

    [Header("Window First Hit Reaction")] // Блок реакции на первый удар по окну
    public GameObject finalNormalDoor; // Обычная дверь
    public GameObject finalBrokenDoor; // Выбитая дверь
    public Rigidbody fallenWardrobeRigidbody; // Rigidbody шкафа
    public Vector3 wardrobeForceDirection = new Vector3(1f, 0.2f, 0f); // Направление силы
    public float wardrobeForce = 4f; // Сила толчка
    public float wardrobeTorque = 2f; // Сила вращения
    public Transform monsterAfterWindowHitPoint; // Точка у окна

    [Header("Triggers")] // Блок триггеров
    public GameObject hallReturnDeathTrigger; // Триггер смерти при возврате
    public GameObject kitchenFinalTrigger; // Триггер кухни
    public GameObject bathroomExitChaseTrigger; // Триггер выхода из ванной

    [HideInInspector] // Прячем в Inspector
    public bool finalSequenceStarted = false; // Финал начался

    private bool finalStarted = false; // Финал уже запускался
    private bool exitBlocked = false; // Выход уже блокировался
    private bool windowFirstHitReactionStarted = false; // Первый удар по окну уже был
    private bool playerEscapedThroughWindow = false; // Игрок перелез через окно
    private bool bathroomExitTriggered = false; // Триггер выхода из ванной уже сработал

    private void Start() // При старте сцены
    {
        if (closetPhysicalFall != null) // Если шкаф назначен
        {
            closetPhysicalFall.canFall = false; // Запрещаем уронить шкаф до 6/6
        }
    }

    public void StartFinalSequence() // Запуск финала
    {
        if (finalStarted) return; // Если финал уже был — выходим

        finalStarted = true; // Запоминаем запуск
        finalSequenceStarted = true; // Сообщаем другим скриптам

        if (closetPhysicalFall != null) // Если шкаф назначен
        {
            closetPhysicalFall.canFall = true; // Разрешаем уронить шкаф после 6/6
        }

        if (fallenWardrobe != null) fallenWardrobe.SetActive(true); // Включаем шкаф

        if (normalRoomDoor != null) normalRoomDoor.SetActive(false); // Прячем обычную дверь

        if (brokenDoorOnFloor != null) brokenDoorOnFloor.SetActive(true); // Показываем выбитую дверь

        if (bathroomDoor != null) // Если дверь ванной назначена
        {
            bathroomDoor.CloseDoor(); // Закрываем дверь
            bathroomDoor.isLocked = true; // Блокируем дверь
            bathroomDoor.canMonsterOpen = false; // Монстр не открывает ванную
        }

        if (bathroomLockCollider != null) bathroomLockCollider.SetActive(true); // Включаем замок

        if (hallReturnDeathTrigger != null) hallReturnDeathTrigger.SetActive(true); // Включаем триггер смерти

        if (kitchenFinalTrigger != null) kitchenFinalTrigger.SetActive(true); // Включаем кухонный триггер

        if (bathroomExitChaseTrigger != null) bathroomExitChaseTrigger.SetActive(false); // Пока триггер выхода из ванной выключен

        BlockExitWithMonster(); // Монстр идёт блокировать выход

        Debug.Log("Финальная последовательность квартиры запущена"); // Debug
    }

    public void BlockExitWithMonster() // Монстр идёт к выходу
    {
        if (exitBlocked) return; // Если уже запускали — выходим

        exitBlocked = true; // Запоминаем

        if (monsterObject != null) monsterObject.SetActive(true); // Включаем монстра

        if (monsterAI != null && monsterExitBlockPoint != null) // Если AI и точка есть
        {
            monsterAI.GoToPointAndStop(monsterExitBlockPoint); // Монстр идёт к выходу
        }

        if (monsterPatrol != null) monsterPatrol.isPatrolActive = false; // Выключаем патруль

        Debug.Log("Монстр пошёл блокировать выход"); // Debug
    }

    public void OnFinalWindowFirstHit() // Первый удар по окну
    {
        if (windowFirstHitReactionStarted) return; // Если уже было — выходим

        windowFirstHitReactionStarted = true; // Запоминаем

        if (finalNormalDoor != null) finalNormalDoor.SetActive(false); // Прячем дверь

        if (finalBrokenDoor != null) finalBrokenDoor.SetActive(true); // Показываем выбитую дверь

        if (fallenWardrobeRigidbody != null) // Если шкаф назначен
        {
            fallenWardrobeRigidbody.isKinematic = false; // Включаем физику
            fallenWardrobeRigidbody.AddForce(wardrobeForceDirection.normalized * wardrobeForce, ForceMode.Impulse); // Толкаем шкаф
            fallenWardrobeRigidbody.AddTorque(Random.insideUnitSphere * wardrobeTorque, ForceMode.Impulse); // Вращаем шкаф
        }

        if (monsterObject != null) monsterObject.SetActive(true); // Включаем монстра

        if (monsterAI != null && monsterAfterWindowHitPoint != null) // Если AI и точка есть
        {
            monsterAI.StartFinalWindowThreat(monsterAfterWindowHitPoint); // Монстр угрожает у окна
        }

        Debug.Log("Первый удар по окну: монстр начал угрозу у окна"); // Debug
    }

    public void OnPlayerEscapedThroughWindow() // Игрок перелез через окно
    {
        if (!finalSequenceStarted) return; // Если финал не начался — выходим

        playerEscapedThroughWindow = true; // Запоминаем, что игрок перелез

        if (bathroomExitChaseTrigger != null) // Если триггер выхода из ванной назначен
        {
            bathroomExitChaseTrigger.SetActive(true); // Включаем триггер после окна
        }

        Debug.Log("Игрок перелез через окно, триггер выхода из ванной включён"); // Debug
    }

    public void OnBathroomExitTrigger() // Игрок вышел из ванной
    {
        if (bathroomExitTriggered) return; // Если уже сработало — выходим

        if (!finalSequenceStarted) return; // Если финал не начался — выходим

        if (!playerEscapedThroughWindow) return; // Если игрок ещё не перелез через окно — выходим

        bathroomExitTriggered = true; // Запоминаем срабатывание

        if (monsterAI != null) // Если монстр назначен
        {
            monsterAI.ForceChasePlayer(); // Принудительно запускаем погоню
        }

        if (bathroomExitChaseTrigger != null) // Если триггер назначен
        {
            bathroomExitChaseTrigger.SetActive(false); // Выключаем триггер
        }

        Debug.Log("Игрок вышел из ванной, монстр начал преследование"); // Debug
    }
}