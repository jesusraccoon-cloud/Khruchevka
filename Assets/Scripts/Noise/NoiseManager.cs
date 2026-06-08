using UnityEngine; // Подключаем Unity-классы

public class NoiseManager : MonoBehaviour // Главный менеджер шума для одной квартиры
{
    [System.Serializable] // Позволяет показывать этот класс в Inspector
    public class RoomConnection // Одна связь слышимости между двумя комнатами
    {
        public string fromRoom; // Комната, откуда идёт шум
        public string toRoom; // Комната, где находится монстр
        [Range(0f, 1f)] public float volumeMultiplier = 1f; // Насколько звук ослабляется между комнатами
    }

    [Header("Main References")] // Заголовок ссылок
    public MonsterAI monster; // Ссылка на MonsterAI этой квартиры
    public RoomTracker monsterRoomTracker; // Ссылка на RoomTracker монстра

    [Header("Noise Reaction")] // Настройки реакции
    [Range(1, 10)] public int investigateThreshold = 6; // С какого итогового шума монстр идёт проверять
    [Range(1, 10)] public int strongNoiseThreshold = 8; // С какого шума считаем звук очень сильным

    [Header("Room Connections")] // Настройки связей комнат
    public RoomConnection[] roomConnections; // Массив связей между комнатами

    [Header("Debug")] // Отладка
    public bool showDebugLogs = true; // Показывать ли сообщения в Console

    public void MakeNoise(Vector3 noisePosition, int noisePower, RoomZone sourceRoom) // Создать шум в конкретной позиции
    {
        noisePower = Mathf.Clamp(noisePower, 1, 10); // Ограничиваем силу шума от 1 до 10

        if (monster == null) // Если монстр не назначен
        {
            Debug.LogWarning("NoiseManager: не назначен MonsterAI"); // Предупреждаем
            return; // Выходим
        }

        if (monsterRoomTracker == null) // Если RoomTracker монстра не назначен
        {
            Debug.LogWarning("NoiseManager: не назначен RoomTracker монстра"); // Предупреждаем
            return; // Выходим
        }

        if (sourceRoom == null) // Если комната источника шума неизвестна
        {
            if (showDebugLogs) // Если отладка включена
            {
                Debug.Log("NoiseManager: шум без комнаты, используем прямую силу: " + noisePower); // Пишем в Console
            }

            TrySendNoiseToMonster(noisePosition, noisePower); // Отправляем шум без ослабления

            return; // Выходим
        }

        RoomZone monsterRoom = monsterRoomTracker.currentRoom; // Берём текущую комнату монстра

        if (monsterRoom == null) // Если монстр сейчас не в комнате
        {
            if (showDebugLogs) // Если отладка включена
            {
                Debug.Log("NoiseManager: монстр сейчас не находится в RoomZone"); // Пишем в Console
            }

            return; // Выходим
        }

        float multiplier = GetVolumeMultiplier(sourceRoom.roomId, monsterRoom.roomId); // Получаем множитель слышимости

        float finalNoiseFloat = noisePower * multiplier; // Считаем итоговую силу шума после стен/комнат

        int finalNoise = Mathf.RoundToInt(finalNoiseFloat); // Округляем итоговый шум до целого числа

        if (showDebugLogs) // Если отладка включена
        {
            Debug.Log(
                "Шум: " + noisePower +
                " | из: " + sourceRoom.roomId +
                " | монстр в: " + monsterRoom.roomId +
                " | множитель: " + multiplier +
                " | итог: " + finalNoise
            ); // Подробный лог
        }

        TrySendNoiseToMonster(noisePosition, finalNoise); // Проверяем и отправляем шум монстру
    }

    public void MakeNoiseFromTracker(Vector3 noisePosition, int noisePower, RoomTracker sourceTracker) // Создать шум от объекта с RoomTracker
    {
        RoomZone sourceRoom = null; // Создаём переменную комнаты

        if (sourceTracker != null) // Если трекер назначен
        {
            sourceRoom = sourceTracker.currentRoom; // Берём комнату из трекера
        }

        MakeNoise(noisePosition, noisePower, sourceRoom); // Вызываем основной метод шума
    }

    private void TrySendNoiseToMonster(Vector3 noisePosition, int finalNoise) // Проверка реакции монстра на итоговый шум
    {
    finalNoise = Mathf.Clamp(finalNoise, 0, 10); // Ограничиваем итоговый шум от 0 до 10

    if (finalNoise <= 3) // Если шум 0-3
    {
        if (showDebugLogs) // Если отладка включена
        {
            Debug.Log("Монстр игнорирует шум. Итоговый шум: " + finalNoise); // Лог
        }

        return; // Ничего не делаем
    }

        if (showDebugLogs) // Если отладка включена
    {
        Debug.Log("Монстр реагирует на шум. Итоговый шум: " + finalNoise); // Лог
    }

        monster.ReactToNoise(noisePosition, finalNoise); // Передаём монстру позицию и силу шума
    }

    private float GetVolumeMultiplier(string fromRoom, string toRoom) // Получить множитель между комнатами
    {
        if (fromRoom == toRoom) return 1f; // Если шум и монстр в одной комнате — звук полный

        foreach (RoomConnection connection in roomConnections) // Перебираем все связи
        {
            if (connection == null) continue; // Если связь пустая — пропускаем

            bool directMatch = connection.fromRoom == fromRoom && connection.toRoom == toRoom; // Проверяем прямое направление

            bool reverseMatch = connection.fromRoom == toRoom && connection.toRoom == fromRoom; // Проверяем обратное направление

            if (directMatch || reverseMatch) // Если нашли связь в любую сторону
            {
                return connection.volumeMultiplier; // Возвращаем множитель
            }
        }

        return 0f; // Если связи нет — звук не проходит
    }
}