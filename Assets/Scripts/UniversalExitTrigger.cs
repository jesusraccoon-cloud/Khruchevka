using UnityEngine; // Подключаем Unity

public class UniversalExitTrigger : MonoBehaviour // Универсальный сценарный триггер
{
    [Header("Trigger Settings")] // Настройки триггера
    public bool triggerOnce = true; // Сработать только один раз

    [Header("Objects")] // Объекты для включения и выключения
    public GameObject[] objectsToEnable; // Объекты для включения
    public GameObject[] objectsToDisable; // Объекты для выключения

    [Header("Door")] // Настройки двери
    public UniversalDoor doorToClose; // Дверь которую нужно закрыть
    public bool lockDoorAfterClose = false; // Нужно ли заблокировать дверь после закрытия

    [Header("Final Sequence")] // Финальный сценарий квартиры
    public ApartmentFinalSequence finalSequence; // Режиссёр квартиры

    [Header("Events")] // Какие события вызвать
    public bool callBathroomExit = false; // Игрок вышел из ванной
    public bool tryCompleteApartment = false; // Игрок завершил квартиру

    private bool triggered = false; // Срабатывал ли триггер

    private void OnTriggerEnter(Collider other) // Когда кто-то входит в триггер
    {
        if (triggered && triggerOnce) // Если уже сработал и одноразовый
        {
            return; // Выходим
        }

        if (!other.CompareTag("Player")) // Если это не игрок
        {
            return; // Выходим
        }

        foreach (GameObject obj in objectsToEnable) // Перебираем объекты для включения
        {
            if (obj != null) // Если объект существует
            {
                obj.SetActive(true); // Включаем объект
            }
        }

        foreach (GameObject obj in objectsToDisable) // Перебираем объекты для выключения
        {
            if (obj != null) // Если объект существует
            {
                obj.SetActive(false); // Выключаем объект
            }
        }

        if (doorToClose != null) // Если дверь назначена
        {
            doorToClose.CloseDoor(); // Закрываем дверь

            if (lockDoorAfterClose) // Если нужно заблокировать дверь
            {
                doorToClose.isLocked = true; // Блокируем дверь
            }
        }

        if (finalSequence != null) // Если назначен режиссёр квартиры
        {
            if (callBathroomExit) // Если нужно сообщить о выходе из ванной
            {
                finalSequence.OnBathroomExitTrigger(); // Вызываем событие
            }

            if (tryCompleteApartment) // Если нужно завершить квартиру
            {
                finalSequence.TryCompleteApartmentAfterExit(); // Разрешаем отключение квартиры через тумблер
            }
        }

        if (triggerOnce) // Если триггер одноразовый
        {
            triggered = true; // Запоминаем срабатывание

            if (!tryCompleteApartment) // Если это не триггер завершения квартиры
            {
                gameObject.SetActive(false); // Отключаем триггер
            }
        }
    }
}