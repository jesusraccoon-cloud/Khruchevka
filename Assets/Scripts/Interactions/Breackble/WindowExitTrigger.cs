using UnityEngine; // Подключаем Unity-классы

public class WindowExitTrigger : MonoBehaviour, IInteractable // Скрипт выхода через разбитое окно
{
    public BreakableWindow breakableWindow; // Ссылка на разбиваемое окно

    public Transform exitPoint; // Точка, куда переносим игрока

    public CharacterController characterController; // CharacterController игрока

    [Header("Side Points")] // Блок точек сторон окна
    public Transform allowedPoint; // Точка со стороны, откуда можно использовать окно

    public Transform blockedPoint; // Точка со стороны, откуда нельзя использовать окно

    [Header("QTE")] // Блок QTE
    public FinalQTE finalQTE; // Ссылка на финальное QTE

    [HideInInspector] // Прячем из Inspector
    public bool playerStartedWindowExit = false; // Игрок начал перелезать через окно

    public void Interact() // Метод вызывается игроком через PlayerInteractor
    {
        if (breakableWindow == null) return; // Если окно не назначено — выходим

        if (!breakableWindow.IsBroken) return; // Если окно не разбито — выходим

        if (exitPoint == null) return; // Если точка выхода не назначена — выходим

        if (characterController == null) return; // Если CharacterController не назначен — выходим

        if (allowedPoint == null) return; // Если AllowedPoint не назначен — выходим

        if (blockedPoint == null) return; // Если BlockedPoint не назначен — выходим

        if (!IsPlayerOnAllowedSide()) return; // Если игрок не с разрешенной стороны — выходим

        TeleportPlayer(); // Переносим игрока
    }

    private bool IsPlayerOnAllowedSide() // Проверяем, находится ли игрок с разрешенной стороны
    {
        Vector3 playerPosition = characterController.transform.position; // Берем позицию игрока

        float distanceToAllowed = Vector3.Distance(playerPosition, allowedPoint.position); // Расстояние до разрешенной точки

        float distanceToBlocked = Vector3.Distance(playerPosition, blockedPoint.position); // Расстояние до запрещенной точки

        return distanceToAllowed < distanceToBlocked; // Разрешаем, если игрок ближе к AllowedPoint
    }

    private void TeleportPlayer() // Безопасный телепорт игрока
    {
        playerStartedWindowExit = true; // Сообщаем монстру, что игрок начал перелезать

        characterController.enabled = false; // Отключаем CharacterController перед телепортом

        characterController.transform.position = exitPoint.position; // Переносим игрока в ExitPoint

        characterController.transform.rotation = exitPoint.rotation; // Поворачиваем игрока как ExitPoint

        characterController.enabled = true; // Включаем CharacterController обратно

        if (finalQTE != null) // Если финальное QTE назначено
        {
            finalQTE.StartQTE(); // Запускаем QTE
        }
    }
}