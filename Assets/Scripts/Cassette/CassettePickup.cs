using UnityEngine; // Подключаем Unity-классы: MonoBehaviour, Transform, Vector3, Debug

public class CassettePickup : MonoBehaviour, IInteractable // Кассета теперь интерактивный объект
{
    [Header("Move Settings")] // Заголовок настроек движения в Inspector

    [SerializeField] private Transform ejectPoint; // Точка, куда кассета выезжает перед подбором

    [SerializeField] private float moveSpeed = 5f; // Скорость движения кассеты к точке EjectPoint

    [Header("Inventory")] // Заголовок настроек инвентаря

    [SerializeField] private CassetteInventoryUI inventoryUI; // Ссылка на UI счётчика кассет

    [Header("Optional Auto Find")] // Заголовок автопоиска ссылок

    [SerializeField] private bool autoFindInventoryUI = true; // Нужно ли автоматически искать UI счётчика

    [SerializeField] private bool autoFindEjectPoint = true; // Нужно ли автоматически искать EjectPoint

    [SerializeField] private string ejectPointName = "EjectPoint"; // Имя дочернего объекта-точки выезда

    private Vector3 targetPosition; // Позиция, куда должна ехать кассета

    private bool isPickingUp = false; // Двигается ли кассета сейчас

    private bool isCollected = false; // Собрана ли кассета уже

    private void Awake() // Вызывается при создании объекта
    {
        TryFindReferences(); // Пробуем автоматически найти нужные ссылки
    }

    private void Start() // Вызывается перед первым кадром
    {
        ValidateSetup(); // Проверяем, всё ли назначено
    }

    private void Update() // Вызывается каждый кадр
    {
        if (isPickingUp) // Если кассета сейчас выезжает
        {
            MoveToEjectPoint(); // Двигаем кассету к точке выезда
        }
    }

    public void Interact() // Метод, который вызывает PlayerInteractor при нажатии E
    {
        if (isCollected) return; // Если кассета уже собрана — ничего не делаем

        if (isPickingUp) return; // Если кассета уже движется — повторно не запускаем

        StartPickup(); // Запускаем подбор кассеты
    }

    private void TryFindReferences() // Метод автопоиска ссылок
    {
        if (autoFindInventoryUI && inventoryUI == null) // Если включён автопоиск UI и ссылка пустая
        {
            inventoryUI = FindFirstObjectByType<CassetteInventoryUI>(); // Ищем первый CassetteInventoryUI в сцене
        }

        if (autoFindEjectPoint && ejectPoint == null) // Если включён автопоиск EjectPoint и ссылка пустая
        {
            Transform foundPoint = transform.parent != null // Проверяем, есть ли родитель
                ? transform.parent.Find(ejectPointName) // Если родитель есть — ищем EjectPoint внутри родителя
                : null; // Если родителя нет — ничего не нашли

            if (foundPoint != null) // Если точку нашли
            {
                ejectPoint = foundPoint; // Запоминаем найденную точку
            }
        }
    }

    private void ValidateSetup() // Метод проверки настроек
    {
        if (inventoryUI == null) // Если UI не найден
        {
            Debug.LogWarning($"{gameObject.name}: CassetteInventoryUI не найден."); // Пишем предупреждение
        }

        if (ejectPoint == null) // Если EjectPoint не найден
        {
            Debug.LogWarning($"{gameObject.name}: EjectPoint не найден."); // Пишем предупреждение
        }
    }

    private void StartPickup() // Метод начала подбора кассеты
    {
        if (ejectPoint == null) return; // Если нет точки выезда — ничего не делаем

        targetPosition = ejectPoint.position; // Запоминаем позицию точки выезда

        isPickingUp = true; // Включаем режим движения кассеты
    }

    private void MoveToEjectPoint() // Метод движения кассеты
    {
        transform.position = Vector3.Lerp( // Плавно двигаем кассету
            transform.position, // От текущей позиции
            targetPosition, // К целевой позиции
            Time.deltaTime * moveSpeed // С учётом скорости и времени кадра
        );

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition); // Считаем расстояние до цели

        if (distanceToTarget <= 0.02f) // Если кассета почти дошла до точки
        {
            CompletePickup(); // Завершаем подбор
        }
    }

    private void CompletePickup() // Метод завершения подбора
    {
        isPickingUp = false; // Останавливаем движение

        isCollected = true; // Помечаем кассету как собранную

        if (inventoryUI != null) // Если UI найден
        {
            inventoryUI.AddCassette(); // Добавляем кассету в счётчик
        }

        gameObject.SetActive(false); // Выключаем объект кассеты
    }

#if UNITY_EDITOR // Код ниже работает только в редакторе Unity

    private void OnDrawGizmosSelected() // Рисуем подсказки, когда объект выбран
    {
        if (ejectPoint != null) // Если точка выезда назначена
        {
            Gizmos.color = Color.cyan; // Цвет линии — голубой

            Gizmos.DrawLine(transform.position, ejectPoint.position); // Рисуем линию от кассеты до EjectPoint

            Gizmos.DrawSphere(ejectPoint.position, 0.03f); // Рисуем маленький шарик на EjectPoint
        }
    }

#endif
}