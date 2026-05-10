using UnityEngine; // Подключаем Unity-классы
using System.Collections; // Подключаем корутины

public class PlayerHideController : MonoBehaviour, IHoldInteractable // Контроллер пряток игрока, теперь умеет реагировать на удержание E
{
    public bool isHidden = false; // Спрятан ли игрок сейчас

    public CharacterController characterController; // CharacterController игрока

    public Behaviour[] movementScriptsToDisable; // Скрипты движения, которые отключаются в шкафу

    public float exitInputDelay = 0.5f; // Задержка после входа, чтобы нельзя было сразу выйти

    public float holdTimeToExit = 2f; // Сколько секунд нужно держать E, чтобы выйти

    public float doorOpenBeforeExitDelay = 0.4f; // Пауза после открытия двери перед выходом

    public float doorCloseAfterExitDelay = 0.4f; // Пауза перед закрытием двери после выхода

    private Transform currentExitPoint; // Точка выхода из текущего шкафа

    private UniversalDoor currentDoor; // Дверь текущего шкафа

    private float hideEnterTime = 0f; // Время, когда игрок спрятался

    private bool isExiting = false; // Защита от повторного выхода

    private bool exitStarted = false; // Был ли уже запущен выход во время текущего удержания

    private void Reset() // Вызывается при добавлении скрипта на объект
    {
        characterController = GetComponent<CharacterController>(); // Автоматически ищем CharacterController
    }

    public void HoldInteract(float holdTime) // Вызывается PlayerInteractor каждый кадр, пока игрок держит E
    {
        if (!isHidden) return; // Если игрок не спрятан — выход не нужен

        if (isExiting) return; // Если уже выходим — ничего не делаем

        if (exitStarted) return; // Если выход уже запущен этим удержанием — повторно не запускаем

        if (Time.time < hideEnterTime + exitInputDelay) return; // Если задержка после входа ещё не прошла — выходим

        if (holdTime >= holdTimeToExit) // Если E удерживали достаточно долго
        {
            exitStarted = true; // Запоминаем, что выход уже запущен

            StartCoroutine(ExitHideSequence()); // Запускаем выход из шкафа
        }
    }

    public void HoldCancel(float holdTime) // Вызывается PlayerInteractor, когда игрок отпустил E после удержания
    {
        exitStarted = false; // Сбрасываем флаг выхода
    }

    public void Hide(Transform hidePoint, Transform exitPoint, UniversalDoor door) // Метод входа в шкаф
    {
        if (hidePoint == null || exitPoint == null) return; // Если точки не назначены — выходим

        isHidden = true; // Помечаем игрока как спрятанного

        hideEnterTime = Time.time; // Запоминаем момент входа

        currentExitPoint = exitPoint; // Запоминаем точку выхода

        currentDoor = door; // Запоминаем дверь шкафа

        exitStarted = false; // Сбрасываем состояние выхода

        SetMovement(false); // Отключаем движение игрока

        TeleportPlayer(hidePoint.position); // Переносим игрока внутрь шкафа
    }

    private IEnumerator ExitHideSequence() // Последовательность выхода из шкафа
    {
        isExiting = true; // Блокируем повторный выход

        if (currentDoor != null) // Если дверь шкафа назначена
        {
            currentDoor.OpenDoor(); // Открываем дверь шкафа
        }

        yield return new WaitForSeconds(doorOpenBeforeExitDelay); // Ждём открытия двери

        isHidden = false; // Игрок больше не спрятан

        if (currentExitPoint != null) // Если точка выхода есть
        {
            TeleportPlayer(currentExitPoint.position); // Переносим игрока наружу
        }

        SetMovement(true); // Включаем движение игрока

        yield return new WaitForSeconds(doorCloseAfterExitDelay); // Ждём после выхода

        if (currentDoor != null) // Если дверь шкафа назначена
        {
            currentDoor.CloseDoor(); // Закрываем дверь шкафа
        }

        currentDoor = null; // Очищаем ссылку на дверь

        currentExitPoint = null; // Очищаем точку выхода

        isExiting = false; // Разрешаем следующие выходы

        exitStarted = false; // Сбрасываем флаг выхода
    }

    private void TeleportPlayer(Vector3 targetPosition) // Безопасный перенос игрока
    {
        if (characterController != null) // Если CharacterController назначен
        {
            characterController.enabled = false; // Отключаем его перед переносом
        }

        transform.position = targetPosition; // Переносим игрока

        if (characterController != null) // Если CharacterController назначен
        {
            characterController.enabled = true; // Включаем обратно
        }
    }

    private void SetMovement(bool enabledState) // Включает или выключает движение
    {
        for (int i = 0; i < movementScriptsToDisable.Length; i++) // Перебираем все скрипты движения
        {
            if (movementScriptsToDisable[i] != null) // Если ссылка не пустая
            {
                movementScriptsToDisable[i].enabled = enabledState; // Включаем или выключаем скрипт
            }
        }
    }
}