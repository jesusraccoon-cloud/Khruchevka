using UnityEngine; // Подключаем Unity-классы

public class PlayerInteractor : MonoBehaviour // Центральный скрипт взаимодействия игрока
{
    public Camera playerCamera; // Камера игрока
    public float interactDistance = 3f; // Дистанция взаимодействия
    public KeyCode interactKey = KeyCode.E; // Кнопка взаимодействия
    public LayerMask interactLayers = ~0; // Слои взаимодействия

    private IInteractable currentInteractable; // Объект, который можно активировать
    private ILookInteractable currentLookInteractable; // Объект, который может реагировать на наведение
    private ILookInteractable previousLookInteractable; // Прошлый объект наведения

    private void Start() // Запускается один раз
    {
        if (playerCamera == null) // Если камера не назначена
        {
            playerCamera = Camera.main; // Берём MainCamera
        }
    }

    private void Update() // Каждый кадр
    {
        FindCurrentInteractable(); // Ищем объект перед камерой

        HandleLook(); // Обрабатываем подсветку/наведение

        if (Input.GetKeyDown(interactKey)) // Если нажали E
        {
            if (currentInteractable != null) // Если объект найден
            {
                currentInteractable.Interact(); // Активируем объект
            }
        }

        DrawDebugRay(); // Рисуем луч
    }

    private void FindCurrentInteractable() // Поиск объекта перед игроком
    {
        currentInteractable = null; // Сбрасываем объект активации
        currentLookInteractable = null; // Сбрасываем объект наведения

        if (playerCamera == null) return; // Если камеры нет — выходим

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // Луч из камеры вперёд

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayers)) // Если луч попал
        {
            currentInteractable = hit.collider.GetComponentInParent<IInteractable>(); // Ищем объект для E
            currentLookInteractable = hit.collider.GetComponentInParent<ILookInteractable>(); // Ищем объект для наведения
        }
    }

    private void HandleLook() // Метод обработки наведения
    {
        if (previousLookInteractable != currentLookInteractable) // Если объект наведения изменился
        {
            if (previousLookInteractable != null) // Если раньше был объект
            {
                previousLookInteractable.LookExit(); // Сообщаем ему, что игрок больше не смотрит
            }

            previousLookInteractable = currentLookInteractable; // Запоминаем новый объект
        }

        if (currentLookInteractable != null) // Если сейчас смотрим на объект
        {
            currentLookInteractable.LookUpdate(); // Обновляем его наведение
        }
    }

    private void DrawDebugRay() // Отладочный луч
    {
        if (playerCamera == null) return; // Если камеры нет — выходим

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.green); // Рисуем зелёный луч
    }
}