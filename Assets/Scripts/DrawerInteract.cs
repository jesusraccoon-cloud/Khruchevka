using UnityEngine;                                                             // Подключаем базовые функции Unity

public class DrawerInteract : MonoBehaviour                                    // Скрипт для выдвижения ящика
{
    public enum SlideDirection                                                 // Создаем список вариантов направления
    {
        Forward,                                                               // Вперед по локальной оси Z
        Back,                                                                  // Назад по локальной оси Z
        Right,                                                                 // Вправо по локальной оси X
        Left                                                                   // Влево по локальной оси X
    }

    [Header("Interaction Settings")]                                           // Заголовок блока настроек взаимодействия
    public Transform playerCamera;                                             // Камера игрока, откуда идет луч
    public float interactDistance = 3f;                                        // Максимальная дистанция взаимодействия
    public KeyCode interactKey = KeyCode.E;                                    // Кнопка взаимодействия

    [Header("Drawer Movement Settings")]                                       // Заголовок блока настроек движения
    public float slideDistance = 0.4f;                                         // Насколько далеко выдвигать ящик
    public float moveSpeed = 3f;                                               // Скорость движения
    public SlideDirection slideDirection = SlideDirection.Back;                // Направление выезда, выбирается в Inspector

    private Vector3 closedLocalPosition;                                       // Позиция закрытого ящика
    private Vector3 openLocalPosition;                                         // Позиция открытого ящика
    private Vector3 targetLocalPosition;                                       // Целевая позиция
    private bool isOpen = false;                                               // Открыт ли ящик

    void Start()                                                               // Выполняется один раз при старте
    {
        if (playerCamera == null)                                              // Если камера не назначена вручную
        {
            Camera mainCam = Camera.main;                                      // Ищем главную камеру

            if (mainCam != null)                                               // Если нашли
            {
                playerCamera = mainCam.transform;                              // Сохраняем камеру
            }
            else                                                               // Если не нашли
            {
                Debug.LogError("MainCamera not found!");                       // Ошибка в консоль
            }
        }

        closedLocalPosition = transform.localPosition;                         // Запоминаем текущую позицию как закрытую
        openLocalPosition = closedLocalPosition + GetSlideVector() * slideDistance; // Считаем открытую позицию
        targetLocalPosition = closedLocalPosition;                             // В начале цель = закрытая позиция
    }

    void Update()                                                              // Выполняется каждый кадр
    {
        transform.localPosition = Vector3.Lerp(                                // Плавно двигаем ящик
            transform.localPosition,                                           // Из текущей позиции
            targetLocalPosition,                                               // В нужную позицию
            Time.deltaTime * moveSpeed                                         // С учетом времени кадра и скорости
        );

        if (playerCamera == null)                                              // Если камеры нет
        {
            return;                                                            // Выходим
        }

        Vector3 rayOrigin = playerCamera.position + playerCamera.forward * 0.3f; // Сдвигаем старт луча чуть вперед
        Debug.DrawRay(rayOrigin, playerCamera.forward * interactDistance, Color.red); // Рисуем луч в Scene View

        if (Input.GetKeyDown(interactKey))                                     // Если нажата кнопка взаимодействия
        {
            Ray ray = new Ray(rayOrigin, playerCamera.forward);                // Создаем луч
            RaycastHit hit;                                                    // Переменная для информации о попадании

            if (Physics.Raycast(ray, out hit, interactDistance))               // Если луч во что-то попал
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) // Если попали в этот ящик или его дочерний объект
                {
                    ToggleDrawer();                                            // Открываем или закрываем ящик
                }
            }
        }
    }

    void ToggleDrawer()                                                        // Метод переключения состояния ящика
    {
        isOpen = !isOpen;                                                      // Меняем состояние на противоположное

        if (isOpen)                                                            // Если теперь открыт
        {
            targetLocalPosition = openLocalPosition;                           // Двигаем к открытой позиции
        }
        else                                                                   // Если теперь закрыт
        {
            targetLocalPosition = closedLocalPosition;                         // Двигаем к закрытой позиции
        }
    }

    Vector3 GetSlideVector()                                                   // Метод возвращает направление движения
    {
        switch (slideDirection)                                                // Проверяем выбранное направление
        {
            case SlideDirection.Forward:                                       // Если выбрано Forward
                return Vector3.forward;                                        // Возвращаем вперед

            case SlideDirection.Back:                                          // Если выбрано Back
                return Vector3.back;                                           // Возвращаем назад

            case SlideDirection.Right:                                         // Если выбрано Right
                return Vector3.right;                                          // Возвращаем вправо

            case SlideDirection.Left:                                          // Если выбрано Left
                return Vector3.left;                                           // Возвращаем влево

            default:                                                           // Если вдруг что-то пошло не так
                return Vector3.back;                                           // По умолчанию назад
        }
    }
}