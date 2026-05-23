using UnityEngine; // Подключаем Unity-классы

public class HallReturnDeathTrigger : MonoBehaviour // Триггер смерти при попытке вернуться в зал
{
    [Header("Death Settings")] // Блок настроек смерти
    public bool deathEnabled = true; // Включена ли смерть в этом триггере

    [Header("Direction Check")] // Блок проверки направления
    public Transform corridorSidePoint; // Точка со стороны коридора, откуда смерть разрешена

    public float corridorSideDistance = 2f; // Максимальная дистанция до точки коридора

    private bool triggered = false; // Защита от повторного срабатывания

    private void OnTriggerEnter(Collider other) // Срабатывает, когда объект входит в триггер
    {
        if (!deathEnabled) return; // Если смерть выключена — выходим

        if (triggered) return; // Если триггер уже сработал — выходим

        if (!other.CompareTag("Player")) return; // Если вошёл не игрок — выходим

        if (!EnteredFromCorridor(other.transform)) return; // Если игрок вошёл не со стороны коридора — не убиваем

        triggered = true; // Запоминаем, что триггер сработал

        Debug.Log("Игрок попытался вернуться в зал со стороны коридора. Смерть."); // Сообщение в Console

        KillPlayer(); // Запускаем смерть игрока
    }

    bool EnteredFromCorridor(Transform playerTransform) // Проверяем, пришёл ли игрок со стороны коридора
    {
        if (corridorSidePoint == null) return true; // Если точка не назначена — работаем по старой логике

        float distanceToCorridorPoint = Vector3.Distance(playerTransform.position, corridorSidePoint.position); // Считаем расстояние от игрока до точки коридора

        return distanceToCorridorPoint <= corridorSideDistance; // Если игрок близко к коридорной точке — считаем, что он пришёл из коридора
    }

    void KillPlayer() // Метод смерти игрока
    {
        Time.timeScale = 0f; // Останавливаем игру

        Debug.Log("GAME OVER"); // Временный вывод смерти
    }
}