using UnityEngine; // Подключаем Unity-классы

public class NoiseEmitter : MonoBehaviour // Универсальный источник шума для двери, окна, предмета, шкафа
{
    [Header("References")] // Ссылки
    public NoiseManager noiseManager; // NoiseManager конкретной квартиры
    public RoomTracker sourceRoomTracker; // RoomTracker источника шума, если есть
    public RoomZone fixedRoom; // Фиксированная комната, если источник не двигается

    [Header("Noise Settings")] // Настройки шума
    [Range(1, 10)] public int noisePower = 5; // Сила шума от 1 до 10

    [Header("Debug")] // Отладка
    public bool showDebugLogs = false; // Показывать ли сообщения в Console

    public void EmitNoise() // Создать шум с обычной силой из Inspector
    {
        EmitNoise(noisePower); // Вызываем перегруженный метод с заданной силой
    }

    public void EmitNoise(int customPower) // Создать шум с кастомной силой
    {
        if (noiseManager == null) // Если NoiseManager не назначен
        {
            Debug.LogWarning(gameObject.name + ": не назначен NoiseManager"); // Предупреждаем
            return; // Выходим
        }

        customPower = Mathf.Clamp(customPower, 1, 10); // Ограничиваем шум от 1 до 10

        RoomZone sourceRoom = GetSourceRoom(); // Получаем комнату источника шума

        if (showDebugLogs) // Если отладка включена
        {
            string roomName = sourceRoom != null ? sourceRoom.roomId : "Unknown"; // Берём имя комнаты или Unknown

            Debug.Log(gameObject.name + " создал шум " + customPower + " в комнате: " + roomName); // Пишем в Console
        }

        noiseManager.MakeNoise(transform.position, customPower, sourceRoom); // Отправляем шум в NoiseManager
    }

    private RoomZone GetSourceRoom() // Получить комнату источника
    {
        if (sourceRoomTracker != null && sourceRoomTracker.currentRoom != null) // Если есть RoomTracker и он знает комнату
        {
            return sourceRoomTracker.currentRoom; // Возвращаем комнату из RoomTracker
        }

        if (fixedRoom != null) // Если назначена фиксированная комната
        {
            return fixedRoom; // Возвращаем фиксированную комнату
        }

        RoomZone parentRoom = GetComponentInParent<RoomZone>(); // Пробуем найти RoomZone в родителях объекта

        if (parentRoom != null) // Если нашли комнату
        {
            return parentRoom; // Возвращаем её
        }

        return null; // Если ничего не нашли — комнаты нет
    }
}