using UnityEngine; // Подключаем Unity
using System.Collections; // Подключаем работу с корутинами

public class UniversalDoor : MonoBehaviour // Основной класс скрипта двери
{
    public enum DoorOpenDirection // Направление открытия двери
    {
        Forward, // Открытие в положительную сторону выбранной оси
        Backward // Открытие в отрицательную сторону выбранной оси
    }

    public enum DoorRotationAxis // Ось вращения двери
    {
        X, // Для дверцы вверх/вниз
        Y, // Для обычной двери влево/вправо
        Z  // Запасной вариант
    }

    [Header("Door Settings")] // Заголовок блока настроек двери в Inspector
    public bool isOpen = false; // Открыта ли дверь сейчас

    public bool IsOpen => isOpen; // Безопасное публичное свойство только для чтения, чтобы другие скрипты (например кот) могли узнать состояние двери, не ломая старую систему

    public DoorOpenDirection openDirection = DoorOpenDirection.Forward; // Направление открытия двери

    [Header("Rotation Axis")] // Заголовок блока выбора оси вращения
    public DoorRotationAxis rotationAxis = DoorRotationAxis.Y; // Ось, по которой будет вращаться дверь

    [Header("Open Angle")] // Заголовок блока угла открытия
    public float openAngle = 90f; // На сколько градусов будет открываться дверь

    [Header("Open / Close Speed")] // Заголовок блока скорости открытия и закрытия
    public float openSpeed = 5f; // Скорость открытия двери
    public float closeSpeed = 7f; // Скорость закрытия двери

    [Header("Interaction")] // Заголовок блока взаимодействия
    public float interactDistance = 2f; // Максимальная дистанция взаимодействия лучом
    public KeyCode interactKey = KeyCode.E; // Кнопка взаимодействия
    public LayerMask interactLayers = ~0; // Слои, по которым работает Raycast

    [Header("References")] // Заголовок блока ссылок на объекты
    public Transform defaultOpener; // Камера или объект, откуда идет луч
    public Collider handleInteractZone; // Коллайдер зоны ручки, в который должен попасть луч

    [Header("Handles")] // Заголовок блока ручек двери
    public Transform outsideHandle; // Внешняя ручка двери
    public Transform insideHandle; // Внутренняя ручка двери
    public float handleDownAngle = 20f; // На сколько градусов ручки опускаются вниз
    public float handlePressSpeed = 12f; // Скорость нажатия ручек
    public float handleReturnSpeed = 10f; // Скорость возврата ручек
    public float handleHoldTime = 0.05f; // Маленькая пауза, когда ручка нажата

    [Header("Door Delay")] // Заголовок блока задержки перед движением двери
    public float doorOpenDelay = 0.03f; // Небольшая задержка перед началом открытия

    [Header("Monster Access")] // Заголовок блока доступа монстра
    public bool canMonsterOpen = true; // Может ли монстр открыть эту дверь

    [Header("Tumbler Lock")] // Заголовок блока связи двери с тумблером
    public bool requiresTumbler = false; // Нужно ли, чтобы тумблер был включен для открытия двери
    public TumblerSwitch requiredTumbler; // Ссылка на нужный тумблер

    private Quaternion closedRotation; // Закрытое вращение двери
    private Quaternion openedRotation; // Открытое вращение двери

    private Quaternion outsideHandleStartRotation; // Стартовый поворот внешней ручки
    private Quaternion insideHandleStartRotation; // Стартовый поворот внутренней ручки
    private Quaternion outsideHandlePressedRotation; // Поворот внешней ручки в нажатом состоянии
    private Quaternion insideHandlePressedRotation; // Поворот внутренней ручки в нажатом состоянии

    private bool isBusy = false; // Защита от повторного нажатия во время анимации

    void Start() // Метод вызывается один раз при запуске сцены
    {
        if (defaultOpener == null && Camera.main != null) // Если ссылка на открывающий объект не задана, но есть Main Camera
        {
            defaultOpener = Camera.main.transform; // Автоматически назначаем Main Camera
        }

        closedRotation = transform.localRotation; // Запоминаем текущее локальное вращение как закрытое

        float direction = openDirection == DoorOpenDirection.Forward ? 1f : -1f; // Определяем базовое направление открытия

        Vector3 rotationVector = Vector3.zero; // Будущий поворот по нужной оси

        switch (rotationAxis) // Выбираем ось вращения
        {
            case DoorRotationAxis.X:
                rotationVector = new Vector3(openAngle * direction, 0f, 0f); // Поворот по оси X
                break;

            case DoorRotationAxis.Y:
                rotationVector = new Vector3(0f, openAngle * direction, 0f); // Поворот по оси Y
                break;

            case DoorRotationAxis.Z:
                rotationVector = new Vector3(0f, 0f, openAngle * direction); // Поворот по оси Z
                break;
        }

        openedRotation = closedRotation * Quaternion.Euler(rotationVector); // Вычисляем открытое вращение двери

        if (outsideHandle != null) // Если внешняя ручка назначена
        {
            outsideHandleStartRotation = outsideHandle.localRotation; // Запоминаем ее исходный поворот
            outsideHandlePressedRotation = outsideHandleStartRotation * Quaternion.Euler(0f, 0f, -handleDownAngle); // Считаем поворот нажатой ручки
        }

        if (insideHandle != null) // Если внутренняя ручка назначена
        {
            insideHandleStartRotation = insideHandle.localRotation; // Запоминаем ее исходный поворот
            insideHandlePressedRotation = insideHandleStartRotation * Quaternion.Euler(0f, 0f, -handleDownAngle); // Считаем поворот нажатой ручки
        }
    }

    void Update() // Метод вызывается каждый кадр
    {
        TryInteract(); // Проверяем нажатие игроком
        UpdateDoorRotation(); // Плавно вращаем дверь
    }

    bool CanOpenDoor() // Метод проверки: можно ли открыть дверь
    {
        if (!requiresTumbler) // Если дверь не требует тумблер
        {
            return true; // Открывать можно всегда
        }

        if (requiredTumbler == null) // Если тумблер нужен, но не назначен
        {
            return false; // Открывать нельзя
        }

        return requiredTumbler.isOn; // Открыть можно только если тумблер включен
    }

    void TryInteract() // Метод проверки взаимодействия игрока с дверью
    {
        if (!Input.GetKeyDown(interactKey)) return; // Если не нажата нужная кнопка — выходим
        if (defaultOpener == null) return; // Если не назначен открывающий объект — выходим
        if (handleInteractZone == null) return; // Если не назначена зона ручки — выходим
        if (isBusy) return; // Если дверь занята анимацией — выходим

        Ray ray = new Ray(defaultOpener.position, defaultOpener.forward); // Создаем луч из камеры вперед
        RaycastHit hit; // Переменная для хранения информации о попадании

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayers)) // Если луч попал в объект на нужной дистанции
        {
            if (hit.collider == handleInteractZone || hit.collider.transform.IsChildOf(handleInteractZone.transform)) // Если попали в саму зону ручки или ее дочерний объект
            {
                if (!isOpen) // Если дверь сейчас закрыта
                {
                    if (CanOpenDoor()) // Проверяем, можно ли ее открыть
                    {
                        ToggleDoor(); // Открываем дверь
                    }
                    else
                    {
                        Debug.Log("Дверь не открывается: тумблер не активирован."); // Сообщение в Console
                    }
                }
                else
                {
                    ToggleDoor(); // Если дверь уже открыта — разрешаем закрыть ее
                }
            }
        }
    }

    public void ToggleDoor() // Универсальный метод открыть или закрыть дверь
    {
        if (isBusy) return; // Если дверь занята — ничего не делаем
        StartCoroutine(InteractSequence(!isOpen)); // Запускаем корутину и передаем противоположное состояние
    }

    public void OpenDoor() // Метод открытия двери из других скриптов
    {
        if (isBusy) return; // Если дверь занята — выходим
        if (isOpen) return; // Если дверь уже открыта — выходим

        if (!CanOpenDoor()) return; // Если условия открытия не выполнены — не открываем

        StartCoroutine(InteractSequence(true)); // Открываем дверь
    }

    public void CloseDoor() // Метод закрытия двери из других скриптов
    {
        if (isBusy) return; // Если дверь занята — выходим
        if (!isOpen) return; // Если дверь уже закрыта — выходим
        StartCoroutine(InteractSequence(false)); // Закрываем дверь
    }

    public void OpenDoorForMonster() // Отдельный метод открытия двери монстром
    {
        if (!canMonsterOpen) return; // Если монстру нельзя открывать эту дверь — выходим
        if (isBusy) return; // Если дверь занята — выходим
        if (isOpen) return; // Если дверь уже открыта — выходим

        if (!CanOpenDoor()) return; // Если дверь требует тумблер и он не активирован — монстр тоже не откроет

        StartCoroutine(InteractSequence(true)); // Открываем дверь
    }

    IEnumerator InteractSequence(bool targetOpenState) // Общая последовательность нажатия ручки и смены состояния двери
    {
        isBusy = true; // Блокируем повторное нажатие

        yield return StartCoroutine(PressHandlesDown()); // Сначала опускаем ручки вниз

        if (doorOpenDelay > 0f) // Если задана задержка перед открытием
        {
            yield return new WaitForSeconds(doorOpenDelay); // Ждем немного
        }

        isOpen = targetOpenState; // Меняем состояние двери на нужное

        if (handleHoldTime > 0f) // Если нужна пауза удержания ручек
        {
            yield return new WaitForSeconds(handleHoldTime); // Держим ручки чуть-чуть нажатыми
        }

        yield return StartCoroutine(ReturnHandlesBack()); // Возвращаем ручки обратно

        isBusy = false; // Разрешаем следующее взаимодействие
    }

    IEnumerator PressHandlesDown() // Корутина опускания ручек вниз
    {
        float t = 0f; // Переменная прогресса анимации
        Quaternion outStart = outsideHandle != null ? outsideHandle.localRotation : Quaternion.identity; // Стартовая ротация внешней ручки
        Quaternion inStart = insideHandle != null ? insideHandle.localRotation : Quaternion.identity; // Стартовая ротация внутренней ручки

        while (t < 1f) // Пока анимация не завершена
        {
            t += Time.deltaTime * handlePressSpeed; // Увеличиваем прогресс в зависимости от времени и скорости

            if (outsideHandle != null) // Если внешняя ручка существует
            {
                outsideHandle.localRotation = Quaternion.Lerp(outStart, outsideHandlePressedRotation, t); // Плавно поворачиваем ее вниз
            }

            if (insideHandle != null) // Если внутренняя ручка существует
            {
                insideHandle.localRotation = Quaternion.Lerp(inStart, insideHandlePressedRotation, t); // Плавно поворачиваем ее вниз
            }

            yield return null; // Ждем следующий кадр
        }

        if (outsideHandle != null) outsideHandle.localRotation = outsideHandlePressedRotation; // Жестко ставим внешнюю ручку в финальное положение
        if (insideHandle != null) insideHandle.localRotation = insideHandlePressedRotation; // Жестко ставим внутреннюю ручку в финальное положение
    }

    IEnumerator ReturnHandlesBack() // Корутина возврата ручек в исходное положение
    {
        float t = 0f; // Переменная прогресса анимации
        Quaternion outStart = outsideHandle != null ? outsideHandle.localRotation : Quaternion.identity; // Стартовое положение внешней ручки
        Quaternion inStart = insideHandle != null ? insideHandle.localRotation : Quaternion.identity; // Стартовое положение внутренней ручки

        while (t < 1f) // Пока возврат не завершен
        {
            t += Time.deltaTime * handleReturnSpeed; // Увеличиваем прогресс возврата

            if (outsideHandle != null) // Если внешняя ручка существует
            {
                outsideHandle.localRotation = Quaternion.Lerp(outStart, outsideHandleStartRotation, t); // Возвращаем внешнюю ручку назад
            }

            if (insideHandle != null) // Если внутренняя ручка существует
            {
                insideHandle.localRotation = Quaternion.Lerp(inStart, insideHandleStartRotation, t); // Возвращаем внутреннюю ручку назад
            }

            yield return null; // Ждем следующий кадр
        }

        if (outsideHandle != null) outsideHandle.localRotation = outsideHandleStartRotation; // Жестко ставим внешнюю ручку в стартовое положение
        if (insideHandle != null) insideHandle.localRotation = insideHandleStartRotation; // Жестко ставим внутреннюю ручку в стартовое положение
    }

    void UpdateDoorRotation() // Метод плавного вращения двери
    {
        Quaternion target = isOpen ? openedRotation : closedRotation; // Выбираем целевую ротацию: открытая или закрытая
        float speed = isOpen ? openSpeed : closeSpeed; // Выбираем скорость открытия или закрытия

        transform.localRotation = Quaternion.Slerp( // Плавно вращаем дверь
            transform.localRotation, // Из текущего вращения
            target, // В целевое вращение
            Time.deltaTime * speed // С учетом времени кадра и скорости
        );
    }
}