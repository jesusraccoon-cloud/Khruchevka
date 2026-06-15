using UnityEngine; // Подключаем Unity-классы

public class PlayerInteractor : MonoBehaviour // Центральный скрипт взаимодействия игрока
{
    [Header("References")] // Блок ссылок
    public Camera playerCamera; // Камера игрока, из которой выпускаются лучи взаимодействия и удара

    public PlayerHideController playerHideController; // Контроллер пряток игрока, чтобы не искать объекты наружу, когда игрок спрятан

    public ObjectGrabber objectGrabber; // Скрипт захвата предметов, который вызывается если обычного IInteractable перед игроком нет

    [Header("Interaction Settings")] // Блок настроек обычного взаимодействия
    public float interactDistance = 3f; // Дистанция обычного взаимодействия через E, например двери, кассеты, шкафы и панели

    [Header("Hit Settings")] // Блок настроек удара
    public float hitDistance = 2f; // Дистанция удара через ЛКМ, например замки, окна, баррикады и ломаемые объекты

    [Header("Raycast Settings")] // Блок общих настроек лучей
    public LayerMask interactLayers = ~0; // Слои, по которым работают лучи взаимодействия и удара

    public QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide; // Разрешает Raycast попадать в Trigger-коллайдеры, если они используются как зоны взаимодействия

    [Header("Keys")] // Блок кнопок
    public KeyCode interactKey = KeyCode.E; // Кнопка обычного взаимодействия

    public KeyCode hitKey = KeyCode.Mouse0; // Кнопка удара

    [Header("Click / Hold")] // Блок короткого нажатия и удержания
    public float shortClickMaxTime = 0.35f; // Максимальное время, которое считается коротким кликом, а не удержанием

    [Header("Debug")] // Блок отладки
    public bool drawDebugRays = true; // Включает или выключает отображение debug-лучей в Scene View

    public bool showDebugLogs = false; // Включает или выключает debug-логи попаданий Raycast в Console

    private IInteractable currentInteractable; // Текущий объект обычного взаимодействия, найденный лучом interactDistance

    private IHitInteractable currentHitInteractable; // Текущий объект удара, найденный отдельным лучом hitDistance

    private IHoldInteractable currentHoldInteractable; // Текущий объект удержания, найденный лучом interactDistance

    private ILookInteractable currentLookInteractable; // Текущий объект наведения, найденный лучом interactDistance

    private ILookInteractable previousLookInteractable; // Предыдущий объект наведения, чтобы корректно вызвать LookExit

    private IInteractable pressedInteractable; // Объект обычного взаимодействия, который был под прицелом в момент нажатия E

    private IHoldInteractable pressedHoldInteractable; // Объект удержания, который был под прицелом в момент нажатия E

    private bool isPressing = false; // Показывает, удерживается ли сейчас кнопка взаимодействия

    private float pressTimer = 0f; // Считает, сколько времени игрок удерживает кнопку взаимодействия

    private void Start() // Запускается один раз при старте сцены
    {
        if (playerCamera == null) // Если камера не назначена в Inspector
        {
            playerCamera = Camera.main; // Берём главную камеру сцены
        }

        if (playerHideController == null) // Если контроллер пряток не назначен в Inspector
        {
            playerHideController = GetComponent<PlayerHideController>(); // Ищем PlayerHideController на объекте игрока
        }

        if (objectGrabber == null && playerCamera != null) // Если ObjectGrabber не назначен и камера найдена
        {
            objectGrabber = playerCamera.GetComponent<ObjectGrabber>(); // Ищем ObjectGrabber на камере игрока
        }
    }

    private void Update() // Выполняется каждый кадр
    {
        FindCurrentInteractable(); // Ищем объект для E-взаимодействия на дистанции interactDistance

        FindCurrentHitInteractable(); // Ищем объект для удара ЛКМ на дистанции hitDistance

        HandleLook(); // Обрабатываем наведение на объект

        HandleInput(); // Обрабатываем обычное взаимодействие через E

        HandleHitInput(); // Обрабатываем удар через ЛКМ

        DrawDebugRays(); // Рисуем debug-лучи в Scene View
    }

    private void HandleInput() // Обработка кнопки взаимодействия
    {
        if (Input.GetKeyDown(interactKey)) // Если игрок нажал кнопку взаимодействия
        {
            isPressing = true; // Запоминаем, что кнопка взаимодействия нажата

            pressTimer = 0f; // Сбрасываем таймер удержания

            if (playerHideController != null && playerHideController.isHidden) // Если игрок сейчас спрятан
            {
                pressedInteractable = null; // Обычное взаимодействие не используем, потому что игрок внутри укрытия

                pressedHoldInteractable = playerHideController; // Удержание отправляем в систему пряток, чтобы игрок мог выйти
            }
            else // Если игрок не спрятан
            {
                pressedInteractable = currentInteractable; // Запоминаем объект короткого взаимодействия, который был под прицелом в момент нажатия

                pressedHoldInteractable = currentHoldInteractable; // Запоминаем объект удержания, который был под прицелом в момент нажатия
            }
        }

        if (Input.GetKey(interactKey) && isPressing) // Если игрок продолжает удерживать кнопку взаимодействия
        {
            pressTimer += Time.deltaTime; // Увеличиваем таймер удержания

            if (pressedHoldInteractable != null) // Если есть объект, который поддерживает удержание
            {
                pressedHoldInteractable.HoldInteract(pressTimer); // Передаём объекту текущее время удержания
            }
        }

        if (Input.GetKeyUp(interactKey) && isPressing) // Если игрок отпустил кнопку взаимодействия
        {
            if (pressTimer <= shortClickMaxTime) // Если время нажатия меньше лимита короткого клика
            {
                if (pressedInteractable != null) // Если был найден обычный объект взаимодействия
                {
                    pressedInteractable.Interact(); // Вызываем обычное взаимодействие
                }
                else if (objectGrabber != null) // Если обычного объекта нет, но есть ObjectGrabber
                {
                    objectGrabber.Interact(); // Пробуем взять или отпустить физический предмет
                }
            }
            else // Если время нажатия больше лимита короткого клика
            {
                if (pressedHoldInteractable != null) // Если был найден объект удержания
                {
                    pressedHoldInteractable.HoldCancel(pressTimer); // Сообщаем объекту, что удержание завершилось или отменилось
                }
            }

            isPressing = false; // Сбрасываем состояние нажатия

            pressTimer = 0f; // Сбрасываем таймер удержания

            pressedInteractable = null; // Очищаем сохранённый объект обычного взаимодействия

            pressedHoldInteractable = null; // Очищаем сохранённый объект удержания
        }
    }

    private void HandleHitInput() // Обработка кнопки удара
    {
        if (Input.GetKeyDown(hitKey)) // Если игрок нажал кнопку удара
        {
            if (currentHitInteractable != null) // Если перед игроком есть объект, который можно ударить
            {
                currentHitInteractable.Hit(); // Вызываем удар у найденного объекта
            }
        }
    }

    private void FindCurrentInteractable() // Поиск объекта для обычного взаимодействия
    {
        currentInteractable = null; // Сбрасываем найденный IInteractable

        currentHoldInteractable = null; // Сбрасываем найденный IHoldInteractable

        currentLookInteractable = null; // Сбрасываем найденный ILookInteractable

        if (playerCamera == null) return; // Если камера не назначена — прекращаем поиск

        if (playerHideController != null && playerHideController.isHidden) return; // Если игрок спрятан — наружный луч взаимодействия не нужен

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // Создаём луч из камеры вперёд

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayers, triggerInteraction)) // Пускаем луч обычного взаимодействия на interactDistance
        {
            currentInteractable = FindInterfaceInColliderOrParents<IInteractable>(hit.collider); // Ищем IInteractable на объекте попадания или его родителях

            currentHoldInteractable = FindInterfaceInColliderOrParents<IHoldInteractable>(hit.collider); // Ищем IHoldInteractable на объекте попадания или его родителях

            currentLookInteractable = FindInterfaceInColliderOrParents<ILookInteractable>(hit.collider); // Ищем ILookInteractable на объекте попадания или его родителях

            if (showDebugLogs) // Если debug-логи включены
            {
                Debug.Log("INTERACT RAY HIT: " + hit.collider.name); // Показываем имя коллайдера, в который попал луч взаимодействия
            }
        }
    }

    private void FindCurrentHitInteractable() // Поиск объекта для удара
    {
        currentHitInteractable = null; // Сбрасываем найденный IHitInteractable

        if (playerCamera == null) return; // Если камера не назначена — прекращаем поиск

        if (playerHideController != null && playerHideController.isHidden) return; // Если игрок спрятан — удар наружу не нужен

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // Создаём луч из камеры вперёд

        if (Physics.Raycast(ray, out RaycastHit hit, hitDistance, interactLayers, triggerInteraction)) // Пускаем отдельный луч удара на hitDistance
        {
            currentHitInteractable = FindInterfaceInColliderOrParents<IHitInteractable>(hit.collider); // Ищем IHitInteractable на объекте попадания или его родителях

            if (showDebugLogs) // Если debug-логи включены
            {
                Debug.Log("HIT RAY HIT: " + hit.collider.name); // Показываем имя коллайдера, в который попал луч удара
            }
        }
    }

    private T FindInterfaceInColliderOrParents<T>(Collider targetCollider) where T : class // Универсальный поиск интерфейса на коллайдере и родителях
    {
        if (targetCollider == null) return null; // Если коллайдера нет — возвращаем null

        T interfaceOnCollider = targetCollider.GetComponent<T>(); // Сначала ищем нужный интерфейс на самом объекте с коллайдером

        if (interfaceOnCollider != null) // Если интерфейс найден на самом объекте
        {
            return interfaceOnCollider; // Возвращаем найденный интерфейс
        }

        T interfaceInParents = targetCollider.GetComponentInParent<T>(); // Если на коллайдере интерфейса нет — ищем его на родительских объектах

        return interfaceInParents; // Возвращаем найденный интерфейс или null
    }

    private void HandleLook() // Обработка наведения
    {
        if (previousLookInteractable != currentLookInteractable) // Если объект наведения изменился
        {
            if (previousLookInteractable != null) // Если раньше был другой объект наведения
            {
                previousLookInteractable.LookExit(); // Сообщаем старому объекту, что игрок перестал на него смотреть
            }

            previousLookInteractable = currentLookInteractable; // Запоминаем новый объект наведения
        }

        if (currentLookInteractable != null) // Если сейчас есть объект наведения
        {
            currentLookInteractable.LookUpdate(); // Обновляем состояние наведения у текущего объекта
        }
    }

    private void DrawDebugRays() // Рисует debug-лучи в Scene View
    {
        if (!drawDebugRays) return; // Если debug-лучи выключены — ничего не рисуем

        if (playerCamera == null) return; // Если камеры нет — ничего не рисуем

        if (playerHideController != null && playerHideController.isHidden) return; // Если игрок спрятан — наружные лучи не рисуем

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.green); // Рисуем зелёный луч обычного взаимодействия

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * hitDistance, Color.red); // Рисуем красный луч удара
    }
}