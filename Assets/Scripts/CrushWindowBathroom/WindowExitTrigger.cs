 using System.Collections; // Нужно для Coroutine и задержки
using UnityEngine; // Подключаем Unity

public class WindowExitTrigger : MonoBehaviour // Скрипт выхода через окно
{
    public Transform exitPoint; 
    // Точка, куда перенесём игрока

    public KeyCode interactKey = KeyCode.E; 
    // Кнопка взаимодействия

    public float exitDelay = 0.5f; 
    // Задержка перед переносом

    private Transform playerRoot; 
    // Найденный главный объект игрока

    private Transform playerColliderTransform; 
    // Объект, который вошёл в триггер

    private bool playerInside = false; 
    // Находится ли игрок в зоне окна

    private bool isExiting = false; 
    // Идёт ли уже процесс выхода
    public BreakableWindow breakableWindow; // Ссылка на окно

    private void Update() // Вызывается каждый кадр
    {
        if (!playerInside) return; 
        // Если игрок не в зоне — ничего не делаем

        if (isExiting) return; 
        // Если уже выходим — повторно не запускаем
        if (breakableWindow == null || !breakableWindow.IsBroken) return; // Если окно не разбито — нельзя выйти

        if (Input.GetKeyDown(interactKey)) 
        // Если нажали кнопку взаимодействия
        {
            StartCoroutine(ExitAfterDelay()); 
            // Запускаем выход с задержкой
        }
    }

    private void OnTriggerEnter(Collider other) // Когда объект вошёл в триггер
    {
        Debug.Log("1) Сработал trigger object: " + gameObject.name);
        Debug.Log("2) В триггер вошёл объект: " + other.name);

        Transform foundPlayer = FindActualPlayerRoot(other.transform); 
        // Ищем реального игрока

        if (foundPlayer == null) 
        // Если игрок не найден
        {
            Debug.Log("3) Игрок не найден");
            return; 
        }

        playerRoot = foundPlayer; 
        // Запоминаем объект игрока

        playerColliderTransform = other.transform; 
        // Запоминаем объект, который вошёл в триггер

        playerInside = true; 
        // Отмечаем, что игрок внутри зоны

        Debug.Log("4) Игрок вошёл в зону выхода: " + playerRoot.name);
    }

    private void OnTriggerExit(Collider other) // Когда объект вышел из триггера
    {
        Transform foundPlayer = FindActualPlayerRoot(other.transform); 
        // Проверяем, игрок ли вышел

        if (foundPlayer == playerRoot) 
        // Если вышел именно наш игрок
        {
            playerInside = false; 
            // Игрок больше не в зоне

            playerRoot = null; 
            // Очищаем ссылку на игрока

            playerColliderTransform = null; 
            // Очищаем ссылку на коллайдер игрока

            Debug.Log("5) Игрок вышел из зоны выхода");
        }
    }

    IEnumerator ExitAfterDelay() // Coroutine выхода с задержкой
    {
        isExiting = true; 
        // Блокируем повторный запуск

        Debug.Log("6) Нажата кнопка выхода через окно");

        yield return new WaitForSeconds(exitDelay); 
        // Ждём указанное время

        TeleportPlayer(); 
        // Переносим игрока

        isExiting = false; 
        // Разрешаем новый запуск
    }

    void TeleportPlayer() // Метод переноса игрока
    {
        if (exitPoint == null) 
        // Проверяем, назначен ли ExitPoint
        {
            Debug.LogError("7) ExitPoint не назначен на объекте: " + gameObject.name);
            return; 
        }

        if (playerRoot == null || playerColliderTransform == null) 
        // Проверяем, есть ли игрок
        {
            Debug.Log("8) Игрок не найден, перенос не выполняем");
            return; 
        }

        Debug.Log("9) Найден объект игрока: " + playerRoot.name);
        Debug.Log("10) ExitPoint position: " + exitPoint.position);

        CharacterController cc = playerRoot.GetComponentInChildren<CharacterController>(); 
        // Ищем CharacterController у игрока

        if (cc != null) 
        {
            cc.enabled = false; 
            // Выключаем CharacterController перед переносом

            Debug.Log("11) CharacterController выключен");
        }

        Vector3 offset = exitPoint.position - playerColliderTransform.position; 
        // Считаем смещение от текущей позиции коллайдера до ExitPoint

        Debug.Log("12) Смещение игрока: " + offset);

        playerRoot.position += offset; 
        // Переносим весь объект игрока

        playerRoot.rotation = exitPoint.rotation; 
        // Поворачиваем игрока как ExitPoint

        Debug.Log("13) Игрок перенесён на другую сторону окна");

        if (cc != null) 
        {
            cc.enabled = true; 
            // Включаем CharacterController обратно

            Debug.Log("14) CharacterController включен обратно");
        }

        playerInside = false; 
        // После переноса считаем, что игрок уже не в зоне

        playerRoot = null; 
        // Очищаем ссылку на игрока

        playerColliderTransform = null; 
        // Очищаем ссылку на коллайдер игрока
    }

    Transform FindActualPlayerRoot(Transform startTransform) // Поиск настоящего объекта игрока
    {
        Transform current = startTransform; 
        // Начинаем поиск с объекта, который вошёл в триггер

        Transform fallbackTagged = null; 
        // Запасной вариант, если найдём объект с тегом Player

        while (current != null) 
        // Идём вверх по родителям
        {
            if (current.name == "Player") 
            // Если нашли объект с именем Player
            {
                return current; 
                // Возвращаем его
            }

            if (fallbackTagged == null && current.CompareTag("Player")) 
            // Если нашли объект с тегом Player
            {
                fallbackTagged = current; 
                // Запоминаем его как запасной вариант
            }

            current = current.parent; 
            // Переходим к родителю
        }

        return fallbackTagged; 
        // Возвращаем объект с тегом Player, если нашли
    }
}