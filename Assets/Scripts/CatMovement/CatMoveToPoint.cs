using UnityEngine; // Подключаем библиотеку Unity
using System.Collections.Generic; // Подключаем работу со списками

public class CatMoveToPoint : MonoBehaviour // Скрипт вешается на главный объект кота
{
    public Transform groundPoint; // Опорная точка кота на полу
    public List<Transform> routeToDoor = new List<Transform>(); // Маршрут кота до двери
    public List<Transform> routeInsideApartment = new List<Transform>(); // Маршрут кота внутри квартиры после открытия двери
    public UniversalDoor targetDoor; // Ссылка на дверь квартиры
    public float moveSpeed = 4f; // Скорость движения кота
    public float stopDistance = 0.1f; // Дистанция, при которой точка считается достигнутой

    private bool hasStarted = false; // Был ли запущен сценарий кота
    private bool isMoving = false; // Двигается ли кот сейчас
    private bool waitingForDoor = false; // Ждёт ли кот открытия двери
    private bool goingInside = false; // Побежал ли кот уже внутрь квартиры

    private int currentDoorRouteIndex = 0; // Индекс текущей точки маршрута до двери
    private int currentInsideRouteIndex = 0; // Индекс текущей точки маршрута внутри квартиры

    void Update() // Unity вызывает этот метод каждый кадр
    {
        if (!hasStarted) // Если сценарий кота ещё не запускался
            return; // Ничего не делаем

        if (groundPoint == null) // Если не назначена опорная точка кота
        {
            Debug.Log("Ground Point не назначен"); // Пишем ошибку
            return; // Выходим
        }

        if (targetDoor == null) // Если не назначена дверь
        {
            Debug.Log("Target Door не назначена"); // Пишем ошибку
            return; // Выходим
        }

        if (routeToDoor == null || routeToDoor.Count == 0) // Если маршрут до двери пустой
        {
            Debug.Log("Route To Door не назначен"); // Пишем ошибку
            return; // Выходим
        }

        if (routeInsideApartment == null || routeInsideApartment.Count == 0) // Если маршрут внутри квартиры пустой
        {
            Debug.Log("Route Inside Apartment не назначен"); // Пишем ошибку
            return; // Выходим
        }

        if (isMoving) // Если кот сейчас должен двигаться
        {
            if (!goingInside) // Если кот ещё идёт к двери
            {
                MoveAlongDoorRoute(); // Двигаем его по маршруту к двери
                return; // На этом кадре дальше ничего не делаем
            }
            else // Если кот уже начал движение внутрь квартиры
            {
                MoveAlongInsideRoute(); // Двигаем его по маршруту внутри квартиры
                return; // На этом кадре дальше ничего не делаем
            }
        }

        if (waitingForDoor && !goingInside) // Если кот уже дошёл до двери и ждёт
        {
            if (targetDoor.IsOpen) // Если дверь открылась
            {
                Debug.Log("Дверь открыта, кот бежит внутрь"); // Лог в консоль
                waitingForDoor = false; // Перестаём ждать дверь
                goingInside = true; // Переключаемся на маршрут внутри квартиры
                isMoving = true; // Снова включаем движение
                currentInsideRouteIndex = 0; // Начинаем внутренний маршрут с первой точки
            }
        }
    }

    public void StartMoving() // Метод запуска сценария кота
    {
        Debug.Log("Кот запущен"); // Лог в консоль
        hasStarted = true; // Помечаем, что сценарий начался
        isMoving = true; // Включаем движение
        waitingForDoor = false; // Сбрасываем ожидание двери
        goingInside = false; // Сбрасываем маршрут внутри квартиры
        currentDoorRouteIndex = 0; // Начинаем маршрут до двери с первой точки
        currentInsideRouteIndex = 0; // На всякий случай сбрасываем и внутренний маршрут
    }

    void MoveAlongDoorRoute() // Метод движения по маршруту до двери
    {
        if (currentDoorRouteIndex >= routeToDoor.Count) // Если маршрут до двери уже закончился
        {
            Debug.Log("Кот дошёл до двери и ждёт открытия"); // Лог
            isMoving = false; // Останавливаем движение
            waitingForDoor = true; // Начинаем ждать дверь
            return; // Выходим
        }

        Transform currentTarget = routeToDoor[currentDoorRouteIndex]; // Берём текущую точку маршрута до двери

        if (currentTarget == null) // Если одна из точек не назначена
        {
            Debug.Log("Одна из точек routeToDoor не назначена"); // Пишем ошибку
            return; // Выходим
        }

        MoveToTarget(currentTarget, true); // Двигаемся к текущей точке маршрута до двери
    }

    void MoveAlongInsideRoute() // Метод движения по маршруту внутри квартиры
    {
        if (currentInsideRouteIndex >= routeInsideApartment.Count) // Если маршрут внутри квартиры закончился
        {
            Debug.Log("Кот забежал в квартиру"); // Лог
            isMoving = false; // Останавливаем движение
            return; // Выходим
        }

        Transform currentTarget = routeInsideApartment[currentInsideRouteIndex]; // Берём текущую точку маршрута внутри квартиры

        if (currentTarget == null) // Если одна из внутренних точек не назначена
        {
            Debug.Log("Одна из точек routeInsideApartment не назначена"); // Пишем ошибку
            return; // Выходим
        }

        MoveToTarget(currentTarget, false); // Двигаемся к текущей внутренней точке
    }

    void MoveToTarget(Transform targetPoint, bool isDoorRoute) // Универсальный метод движения к точке
    {
        Vector3 currentGroundPosition = groundPoint.position; // Берём текущую позицию опорной точки кота
        Vector3 targetPosition = targetPoint.position; // Берём позицию текущей цели

        Vector3 direction = (targetPosition - currentGroundPosition).normalized; // Считаем направление к цели
        float distance = Vector3.Distance(currentGroundPosition, targetPosition); // Считаем расстояние до цели

        Debug.Log("Кот движется. Distance = " + distance); // Лог расстояния

        if (distance <= stopDistance) // Если кот дошёл до точки
        {
            if (isDoorRoute) // Если это маршрут до двери
            {
                Debug.Log("Кот дошёл до точки маршрута к двери: " + currentDoorRouteIndex); // Лог
                currentDoorRouteIndex++; // Переходим к следующей точке маршрута до двери
            }
            else // Если это маршрут внутри квартиры
            {
                Debug.Log("Кот дошёл до точки маршрута внутри квартиры: " + currentInsideRouteIndex); // Лог
                currentInsideRouteIndex++; // Переходим к следующей внутренней точке
            }

            return; // Выходим, чтобы на следующем кадре начать движение к новой точке
        }

        transform.position += direction * moveSpeed * Time.deltaTime; // Двигаем кота к текущей точке

        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z); // Оставляем только горизонтальное направление для поворота

        if (flatDirection != Vector3.zero) // Если горизонтальное направление не нулевое
        {
            transform.forward = flatDirection; // Поворачиваем кота в сторону движения
        }
    }
}