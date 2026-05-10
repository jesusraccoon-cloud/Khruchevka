using UnityEngine; // Подключаем Unity-классы

public class PlayerInteractor : MonoBehaviour // Центральный скрипт взаимодействия игрока
{
    public Camera playerCamera; // Камера игрока

    public float interactDistance = 3f; // Дистанция взаимодействия

    public KeyCode interactKey = KeyCode.E; // Кнопка взаимодействия

    public KeyCode hitKey = KeyCode.Mouse0; // Кнопка удара, по умолчанию левая кнопка мыши

    public LayerMask interactLayers = ~0; // Слои взаимодействия

    public float shortClickMaxTime = 0.35f; // Максимальная длительность короткого клика

    public PlayerHideController playerHideController; // Ссылка на контроллер пряток игрока

    private IInteractable currentInteractable; // Объект для короткого взаимодействия

    private IHitInteractable currentHitInteractable; // Объект, по которому можно ударить

    private IHoldInteractable currentHoldInteractable; // Объект для удержания

    private ILookInteractable currentLookInteractable; // Объект для наведения

    private ILookInteractable previousLookInteractable; // Предыдущий объект наведения

    private IInteractable pressedInteractable; // Объект, на котором началось нажатие

    private IHoldInteractable pressedHoldInteractable; // Объект удержания, на котором началось нажатие

    private bool isPressing = false; // Нажата ли сейчас кнопка

    private float pressTimer = 0f; // Время удержания кнопки

    private void Start() // Запускается один раз
    {
        if (playerCamera == null) // Если камера не назначена
        {
            playerCamera = Camera.main; // Берём MainCamera
        }

        if (playerHideController == null) // Если контроллер пряток не назначен
        {
            playerHideController = GetComponent<PlayerHideController>(); // Ищем его на этом же объекте
        }
    }

    private void Update() // Каждый кадр
    {
        FindCurrentInteractable(); // Ищем объект перед камерой

        HandleLook(); // Обрабатываем наведение

        HandleInput(); // Обрабатываем короткое E и удержание E

        HandleHitInput(); // Обрабатываем удар мышкой

        DrawDebugRay(); // Рисуем луч для проверки
    }

    private void HandleInput() // Обработка короткого нажатия и удержания
    {
        if (Input.GetKeyDown(interactKey)) // Если игрок нажал E
        {
            isPressing = true; // Запоминаем, что кнопка нажата

            pressTimer = 0f; // Сбрасываем таймер удержания

            if (playerHideController != null && playerHideController.isHidden) // Если игрок сейчас внутри шкафа
            {
                pressedInteractable = null; // Короткое действие внутри шкафа не нужно

                pressedHoldInteractable = playerHideController; // Удержание отправляем в PlayerHideController
            }
            else // Если игрок не спрятан
            {
                pressedInteractable = currentInteractable; // Запоминаем объект короткого действия

                pressedHoldInteractable = currentHoldInteractable; // Запоминаем объект удержания
            }
        }

        if (Input.GetKey(interactKey) && isPressing) // Если игрок держит E
        {
            pressTimer += Time.deltaTime; // Увеличиваем время удержания

            if (pressedHoldInteractable != null) // Если есть объект удержания
            {
                pressedHoldInteractable.HoldInteract(pressTimer); // Передаём ему время удержания
            }
        }

        if (Input.GetKeyUp(interactKey) && isPressing) // Если игрок отпустил E
        {
            if (pressTimer <= shortClickMaxTime) // Если это короткий клик
            {
                if (pressedInteractable != null) // Если объект короткого действия есть
                {
                    pressedInteractable.Interact(); // Выполняем короткое действие
                }
            }
            else // Если это было удержание
            {
                if (pressedHoldInteractable != null) // Если объект удержания есть
                {
                    pressedHoldInteractable.HoldCancel(pressTimer); // Сообщаем, что удержание закончилось
                }
            }

            isPressing = false; // Сбрасываем состояние кнопки

            pressTimer = 0f; // Сбрасываем таймер

            pressedInteractable = null; // Очищаем короткое действие

            pressedHoldInteractable = null; // Очищаем удержание
        }
    }

    private void HandleHitInput() // Метод обработки удара
    {
        if (Input.GetKeyDown(hitKey)) // Если игрок нажал кнопку удара
        {
            if (currentHitInteractable != null) // Если перед игроком есть объект, по которому можно ударить
            {
                currentHitInteractable.Hit(); // Наносим удар по объекту
            }
        }
    }

    private void FindCurrentInteractable() // Поиск объекта перед игроком
    {
        currentInteractable = null; // Сбрасываем короткое взаимодействие

        currentHitInteractable = null; // Сбрасываем объект удара

        currentHoldInteractable = null; // Сбрасываем удержание

        currentLookInteractable = null; // Сбрасываем наведение

        if (playerCamera == null) return; // Если камеры нет — выходим

        if (playerHideController != null && playerHideController.isHidden) return; // Если игрок спрятан — луч наружу не нужен

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // Создаём луч из камеры вперёд

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayers, QueryTriggerInteraction.Collide)) // Если луч попал в объект
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>(); // Ищем короткое взаимодействие

            currentHitInteractable = hit.collider.GetComponentInParent<IHitInteractable>(); // Ищем объект удара

            currentHoldInteractable = hit.collider.GetComponentInParent<IHoldInteractable>(); // Ищем удержание

            currentLookInteractable = hit.collider.GetComponentInParent<ILookInteractable>(); // Ищем наведение
        }
    }

    private void HandleLook() // Обработка наведения
    {
        if (previousLookInteractable != currentLookInteractable) // Если объект наведения изменился
        {
            if (previousLookInteractable != null) // Если старый объект был
            {
                previousLookInteractable.LookExit(); // Убираем наведение со старого объекта
            }

            previousLookInteractable = currentLookInteractable; // Запоминаем новый объект
        }

        if (currentLookInteractable != null) // Если есть объект наведения
        {
            currentLookInteractable.LookUpdate(); // Обновляем наведение
        }
    }

    private void DrawDebugRay() // Рисуем отладочный луч
    {
        if (playerCamera == null) return; // Если камеры нет — выходим

        if (playerHideController != null && playerHideController.isHidden) return; // В шкафу луч не рисуем

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.green); // Рисуем зелёный луч
    }
}