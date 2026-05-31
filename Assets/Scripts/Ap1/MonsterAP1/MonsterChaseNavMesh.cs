using UnityEngine; // Подключаем Unity
using UnityEngine.AI; // Подключаем NavMesh

public class MonsterChaseNavMesh : MonoBehaviour // Скрипт монстра
{
    [Header("References")] // Заголовок ссылок
    public Transform target; // Цель монстра, обычно игрок
    public NavMeshAgent agent; // Компонент NavMeshAgent

    [Header("Door Check")] // Заголовок проверки двери
    public float doorCheckDistance = 1.8f; // Дистанция проверки двери впереди
    public float doorOpenDistance = 2.2f; // Дистанция, на которой монстр может открыть дверь
    public LayerMask doorLayers = ~0; // Слои для проверки двери

    [Header("Debug")] // Заголовок для отладки
    public bool drawDebugRay = true; // Рисовать ли луч в Scene

    void Start() // Запуск
    {
        if (agent == null) // Если агент не назначен вручную
        {
            agent = GetComponent<NavMeshAgent>(); // Берём NavMeshAgent с этого же объекта
        }
    }

    void Update() // Каждый кадр
    {
        if (target == null) return; // Если цель не назначена — выходим
        if (agent == null) return; // Если нет агента — выходим

        agent.SetDestination(target.position); // Постоянно ведём монстра к игроку
        TryOpenDoorAhead(); // Проверяем дверь впереди
    }

    void TryOpenDoorAhead() // Проверка двери перед монстром
    {
        Vector3 rayStart = transform.position + Vector3.up * 1.2f; // Поднимаем старт луча немного вверх
        Vector3 rayDirection = transform.forward; // Луч идёт вперёд по направлению монстра

        if (drawDebugRay) // Если включена отладка
        {
            Debug.DrawRay(rayStart, rayDirection * doorCheckDistance, Color.red); // Рисуем луч в Scene
        }

        RaycastHit hit; // Данные попадания

        if (Physics.Raycast(rayStart, rayDirection, out hit, doorCheckDistance, doorLayers)) // Если луч попал во что-то впереди
        {
            UniversalDoor door = hit.collider.GetComponentInParent<UniversalDoor>(); // Ищем дверь у объекта или выше по иерархии

            if (door != null) // Если нашли дверь
            {
                float distanceToDoor = Vector3.Distance(transform.position, door.transform.position); // Считаем расстояние до двери

                if (distanceToDoor <= doorOpenDistance) // Если монстр уже достаточно близко
                {
                    door.OpenDoorForMonster(); // Открываем дверь монстром
                }
            }
        }
    }
}