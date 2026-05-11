using UnityEngine; // Подключаем Unity-классы

public class WindowExitInteract : MonoBehaviour, IInteractable // Скрипт выхода через разбитое окно
{
    public BreakableWindow breakableWindow; // Ссылка на окно, которое должно быть разбито

    public Transform exitPoint; // Точка, куда нужно перенести игрока

    public CharacterController characterController; // CharacterController игрока, обычно PlayerCapsule

    public void Interact() // Метод вызывается PlayerInteractor при коротком нажатии E
    {
        if (breakableWindow == null) return; // Если окно не назначено — выходим

        if (!breakableWindow.IsBroken) return; // Если окно ещё не разбито — выход запрещён

        if (exitPoint == null) return; // Если точка выхода не назначена — выходим

        if (characterController == null) return; // Если CharacterController не назначен — выходим

        TeleportPlayer(); // Переносим игрока к ExitPoint
    }

    private void TeleportPlayer() // Метод безопасного переноса игрока
    {
        characterController.enabled = false; // Отключаем CharacterController, чтобы он не мешал телепорту

        characterController.transform.position = exitPoint.position; // Переносим PlayerCapsule точно в позицию ExitPoint

        characterController.transform.rotation = exitPoint.rotation; // Поворачиваем PlayerCapsule как ExitPoint

        characterController.enabled = true; // Включаем CharacterController обратно

    }
}