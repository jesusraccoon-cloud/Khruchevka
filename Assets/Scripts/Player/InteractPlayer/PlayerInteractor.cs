using UnityEngine; // Подключаем Unity-классы

public class PlayerInteractor : MonoBehaviour // Центральный скрипт взаимодействия игрока
{
    public Camera playerCamera; // Камера игрока

    public float interactDistance = 3f; // Дистанция взаимодействия

    public KeyCode interactKey = KeyCode.E; // Кнопка взаимодействия

    public KeyCode hitKey = KeyCode.Mouse0; // Кнопка удара

    public LayerMask interactLayers = ~0; // Слои взаимодействия

    public float shortClickMaxTime = 0.35f; // Максимальное время короткого клика

    public PlayerHideController playerHideController; // Контроллер пряток

    public ObjectGrabber objectGrabber; // Скрипт захвата предметов

    private IInteractable currentInteractable; // Текущий объект короткого взаимодействия

    private IHitInteractable currentHitInteractable; // Текущий объект удара

    private IHoldInteractable currentHoldInteractable; // Текущий объект удержания

    private ILookInteractable currentLookInteractable; // Текущий объект наведения

    private ILookInteractable previousLookInteractable; // Предыдущий объект наведения

    private IInteractable pressedInteractable; // Объект, на котором началось нажатие

    private IHoldInteractable pressedHoldInteractable; // Объект удержания, на котором началось нажатие

    private bool isPressing = false; // Нажата ли кнопка

    private float pressTimer = 0f; // Таймер удержания

    private void Start() // Запускается один раз
    {
        if (playerCamera == null) // Если камера не назначена
        {
            playerCamera = Camera.main; // Берём MainCamera
        }

        if (playerHideController == null) // Если контроллер пряток не назначен
        {
            playerHideController = GetComponent<PlayerHideController>(); // Ищем на этом объекте
        }

        if (objectGrabber == null && playerCamera != null) // Если ObjectGrabber не назначен
        {
            objectGrabber = playerCamera.GetComponent<ObjectGrabber>(); // Ищем ObjectGrabber на камере
        }
    }

    private void Update() // Каждый кадр
    {
        FindCurrentInteractable(); // Ищем объект перед камерой

        HandleLook(); // Обрабатываем наведение

        HandleInput(); // Обрабатываем E

        HandleHitInput(); // Обрабатываем удар

        DrawDebugRay(); // Рисуем debug-луч
    }

    private void HandleInput() // Обработка E
    {
        if (Input.GetKeyDown(interactKey)) // Если нажали E
        {
            isPressing = true; // Запоминаем нажатие

            pressTimer = 0f; // Сбрасываем таймер

            if (playerHideController != null && playerHideController.isHidden) // Если игрок спрятан
            {
                pressedInteractable = null; // Короткое действие не нужно

                pressedHoldInteractable = playerHideController; // Удержание отправляем в прятки
            }
            else // Если игрок не спрятан
            {
                pressedInteractable = currentInteractable; // Запоминаем короткое взаимодействие

                pressedHoldInteractable = currentHoldInteractable; // Запоминаем удержание
            }
        }

        if (Input.GetKey(interactKey) && isPressing) // Если удерживаем E
        {
            pressTimer += Time.deltaTime; // Увеличиваем таймер

            if (pressedHoldInteractable != null) // Если есть объект удержания
            {
                pressedHoldInteractable.HoldInteract(pressTimer); // Передаём время удержания
            }
        }

        if (Input.GetKeyUp(interactKey) && isPressing) // Если отпустили E
        {
            if (pressTimer <= shortClickMaxTime) // Если это короткое нажатие
            {
                if (pressedInteractable != null) // Если есть обычный интерактив
                {
                    pressedInteractable.Interact(); // Вызываем взаимодействие
                }
                else if (objectGrabber != null) // Если обычного интерактива нет
                {
                    objectGrabber.Interact(); // Пробуем взять или отпустить предмет
                }
            }
            else // Если это было удержание
            {
                if (pressedHoldInteractable != null) // Если был объект удержания
                {
                    pressedHoldInteractable.HoldCancel(pressTimer); // Сообщаем отмену удержания
                }
            }

            isPressing = false; // Сбрасываем состояние

            pressTimer = 0f; // Сбрасываем таймер

            pressedInteractable = null; // Очищаем объект

            pressedHoldInteractable = null; // Очищаем объект удержания
        }
    }

    private void HandleHitInput() // Обработка удара
    {
        if (Input.GetKeyDown(hitKey)) // Если нажали ЛКМ
        {
            if (currentHitInteractable != null) // Если объект можно ударить
            {
                currentHitInteractable.Hit(); // Ударяем
            }
        }
    }

    private void FindCurrentInteractable() // Поиск объекта перед игроком
    {
        currentInteractable = null; // Сбрасываем короткое взаимодействие

        currentHitInteractable = null; // Сбрасываем удар

        currentHoldInteractable = null; // Сбрасываем удержание

        currentLookInteractable = null; // Сбрасываем наведение

        if (playerCamera == null) return; // Если камеры нет — выходим

        if (playerHideController != null && playerHideController.isHidden) return; // Если игрок спрятан — луч наружу не нужен

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // Луч из камеры

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayers, QueryTriggerInteraction.Collide)) // Пускаем луч
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>(); // Ищем IInteractable

            currentHitInteractable = hit.collider.GetComponentInParent<IHitInteractable>(); // Ищем IHitInteractable

            currentHoldInteractable = hit.collider.GetComponentInParent<IHoldInteractable>(); // Ищем IHoldInteractable

            currentLookInteractable = hit.collider.GetComponentInParent<ILookInteractable>(); // Ищем ILookInteractable
        }
    }

    private void HandleLook() // Обработка наведения
    {
        if (previousLookInteractable != currentLookInteractable) // Если объект наведения изменился
        {
            if (previousLookInteractable != null) // Если старый объект был
            {
                previousLookInteractable.LookExit(); // Убираем наведение
            }

            previousLookInteractable = currentLookInteractable; // Запоминаем новый объект
        }

        if (currentLookInteractable != null) // Если есть объект наведения
        {
            currentLookInteractable.LookUpdate(); // Обновляем наведение
        }
    }

    private void DrawDebugRay() // Рисует луч в Scene View
    {
        if (playerCamera == null) return; // Если камеры нет — выходим

        if (playerHideController != null && playerHideController.isHidden) return; // Если игрок спрятан — не рисуем

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.green); // Рисуем зелёный луч
    }
}