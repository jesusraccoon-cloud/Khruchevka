using UnityEngine; // Подключаем Unity-классы

public class NoiseTestPlayerButton : MonoBehaviour // Тестовый скрипт шума от игрока
{
    [Header("References")] // Ссылки
    public NoiseManager noiseManager; // NoiseManager текущей квартиры
    public RoomTracker playerRoomTracker; // RoomTracker игрока

    [Header("Test Settings")] // Настройки теста
    public KeyCode testKey = KeyCode.N; // Кнопка тестового шума
    [Range(1, 10)] public int testNoisePower = 8; // Сила тестового шума

    [Header("Debug")] // Отладка
    public bool showDebugLogs = true; // Показывать ли сообщения

    private void Update() // Каждый кадр
    {
        if (Input.GetKeyDown(testKey)) // Если нажали тестовую кнопку
        {
            MakeTestNoise(); // Создаём тестовый шум
        }
    }

    private void MakeTestNoise() // Метод создания тестового шума
    {
        if (noiseManager == null) // Если NoiseManager не назначен
        {
            Debug.LogWarning("NoiseTestPlayerButton: не назначен NoiseManager"); // Предупреждаем
            return; // Выходим
        }

        RoomZone sourceRoom = null; // Создаём переменную комнаты

        if (playerRoomTracker != null) // Если RoomTracker игрока назначен
        {
            sourceRoom = playerRoomTracker.currentRoom; // Берём текущую комнату игрока
        }

        if (showDebugLogs) // Если отладка включена
        {
            string roomName = sourceRoom != null ? sourceRoom.roomId : "Unknown"; // Имя комнаты или Unknown

            Debug.Log("Тестовый шум игрока: " + testNoisePower + " | Комната: " + roomName); // Пишем в Console
        }

        noiseManager.MakeNoise(transform.position, testNoisePower, sourceRoom); // Отправляем шум в NoiseManager
    }
}