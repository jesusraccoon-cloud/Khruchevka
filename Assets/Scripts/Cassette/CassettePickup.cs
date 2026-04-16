using UnityEngine; // Подключаем базовые классы Unity (Transform, Vector3, MonoBehaviour и т.д.)

public class CassettePickup : MonoBehaviour // Создаём скрипт, который можно повесить на объект
{
    [Header("Interaction")] // Заголовок в Inspector

    [SerializeField] private Transform player; // Ссылка на игрока (обычно камера)
    [SerializeField] private float interactDistance = 3f; // Дистанция взаимодействия лучом
    [SerializeField] private KeyCode interactKey = KeyCode.E; // Кнопка взаимодействия
    [SerializeField] private LayerMask interactLayers = ~0; // Слои для Raycast

    [Header("Move Settings")] // Заголовок
    [SerializeField] private Transform ejectPoint; // Точка, куда будет двигаться кассета
    [SerializeField] private float moveSpeed = 5f; // Скорость движения кассеты

    [Header("Inventory")] // Заголовок
    [SerializeField] private CassetteInventoryUI inventoryUI; // Ссылка на UI счётчика кассет

    [Header("Optional Auto Find")] // Заголовок
    [SerializeField] private bool autoFindPlayer = true; // Автоматически искать игрока
    [SerializeField] private bool autoFindInventoryUI = true; // Автоматически искать UI
    [SerializeField] private bool autoFindEjectPoint = true; // Автоматически искать точку выезда
    [SerializeField] private string ejectPointName = "EjectPoint"; // Имя объекта точки
    [SerializeField] private string playerTag = "MainCamera"; // Тег игрока

    private Vector3 targetPosition; // Позиция, куда должна поехать кассета
    private bool isPickingUp = false; // Флаг: кассета сейчас движется
    private bool isCollected = false; // Флаг: кассета уже собрана

    private void Awake() // Вызывается при создании объекта
    {
        TryFindReferences(); // Пытаемся автоматически найти ссылки
    }

    private void Start() // Вызывается перед первым кадром
    {
        ValidateSetup(); // Проверяем, всё ли настроено правильно
    }

    private void Update() // Вызывается каждый кадр
    {
        if (isCollected) return; // Если уже собрана — ничего не делаем

        if (isPickingUp) // Если кассета уже движется
        {
            MoveToEjectPoint(); // Продолжаем движение
            return; // Выходим из Update
        }

        if (Input.GetKeyDown(interactKey) && CanInteract()) // Если нажали кнопку и можно взаимодействовать
        {
            StartPickup(); // Начинаем подбор
        }
    }

    private void TryFindReferences() // Автоматический поиск ссылок
    {
        if (autoFindPlayer && player == null) // Если включён автопоиск и игрок не назначен
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag); // Ищем объект по тегу
            if (playerObject != null) player = playerObject.transform; // Если нашли — сохраняем Transform
        }

        if (autoFindInventoryUI && inventoryUI == null) // Если включён автопоиск UI
        {
            inventoryUI = FindFirstObjectByType<CassetteInventoryUI>(); // Ищем UI в сцене
        }

        if (autoFindEjectPoint && ejectPoint == null) // Если включён автопоиск точки
        {
            Transform foundPoint = transform.parent != null // Проверяем есть ли родитель
                ? transform.parent.Find(ejectPointName) // Ищем EjectPoint внутри родителя
                : null;

            if (foundPoint != null) ejectPoint = foundPoint; // Если нашли — сохраняем
        }
    }

    private void ValidateSetup() // Проверка корректности настроек
    {
        if (player == null) Debug.LogWarning($"{gameObject.name}: Player не найден."); // Предупреждение если нет игрока
        if (inventoryUI == null) Debug.LogWarning($"{gameObject.name}: UI не найден."); // Предупреждение если нет UI
        if (ejectPoint == null) Debug.LogWarning($"{gameObject.name}: EjectPoint не найден."); // Предупреждение если нет точки
    }

    private bool CanInteract() // Проверка: можно ли взаимодействовать через луч
    {
        if (player == null || ejectPoint == null) return false; // Если нет ссылок — нельзя

        Ray ray = new Ray(player.position, player.forward); // Создаём луч из позиции игрока вперёд
        RaycastHit hit; // Данные попадания луча

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayers)) // Если луч попал во что-то
        {
            if (hit.collider == null) return false; // Если коллайдер почему-то пустой — нельзя

            if (hit.collider.gameObject == gameObject) return true; // Если попали прямо в этот объект кассеты — можно

            if (hit.collider.transform.IsChildOf(transform)) return true; // Если попали в дочерний объект этой кассеты — тоже можно
        }

        return false; // Во всех остальных случаях — нельзя
    }

    private void StartPickup() // Начало подбора кассеты
    {
        targetPosition = ejectPoint.position; // Запоминаем точку назначения
        isPickingUp = true; // Включаем движение
    }

    private void MoveToEjectPoint() // Движение кассеты
    {
        transform.position = Vector3.Lerp( // Плавно перемещаем объект
            transform.position, // Откуда
            targetPosition, // Куда
            Time.deltaTime * moveSpeed // Скорость
        );

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition); // Проверяем расстояние до цели

        if (distanceToTarget <= 0.02f) // Если почти дошли
        {
            CompletePickup(); // Завершаем подбор
        }
    }

    private void CompletePickup() // Завершение подбора
    {
        isPickingUp = false; // Останавливаем движение
        isCollected = true; // Помечаем как собранную

        if (inventoryUI != null) inventoryUI.AddCassette(); // Добавляем кассету в UI

        gameObject.SetActive(false); // Выключаем объект (исчезает из сцены)
    }

#if UNITY_EDITOR // Этот код работает только в редакторе Unity
    private void OnDrawGizmosSelected() // Рисует подсказки в Scene
    {
        Gizmos.color = Color.yellow; // Цвет — жёлтый

        if (player != null) // Если есть ссылка на игрока
        {
            Gizmos.DrawRay(player.position, player.forward * interactDistance); // Рисуем луч взаимодействия
        }

        if (ejectPoint != null) // Если есть точка
        {
            Gizmos.color = Color.cyan; // Цвет — голубой
            Gizmos.DrawLine(transform.position, ejectPoint.position); // Линия до точки
            Gizmos.DrawSphere(ejectPoint.position, 0.03f); // Маленький шарик в точке
        }
    }
#endif
}