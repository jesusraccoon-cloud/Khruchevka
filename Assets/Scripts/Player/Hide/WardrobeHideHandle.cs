using UnityEngine; // Подключаем Unity-классы
using System.Collections; // Подключаем корутины

public class WardrobeHideHandle : MonoBehaviour, IInteractable, IHoldInteractable // Ручка шкафа поддерживает короткий клик и удержание
{
    public PlayerHideController playerHideController; // Ссылка на систему пряток игрока

    public UniversalDoor wardrobeDoor; // Дверь шкафа, которую нужно открыть/закрыть

    public Transform hidePoint; // Точка внутри шкафа, куда переносится игрок

    public Transform exitPoint; // Точка выхода перед шкафом

    public float holdTimeToHide = 2f; // Сколько секунд нужно держать E, чтобы спрятаться

    public float doorOpenBeforeHideDelay = 0.4f; // Пауза после открытия двери перед прятаньем

    public float doorCloseAfterHideDelay = 0.4f; // Пауза перед закрытием двери после прятанья

    private bool isHiding = false; // Защита от повторного запуска прятанья

    private bool hideStarted = false; // Было ли уже запущено прятанье во время текущего удержания

    public void Interact() // Вызывается PlayerInteractor при коротком нажатии E
    {
        if (isHiding) return; // Если уже идёт процесс прятанья — ничего не делаем

        if (playerHideController != null && playerHideController.isHidden) return; // Если игрок уже спрятан — дверь снаружи не трогаем

        if (wardrobeDoor != null) // Если дверь шкафа назначена
        {
            wardrobeDoor.Interact(); // Короткое E открывает или закрывает дверь шкафа
        }
    }

    public void HoldInteract(float holdTime) // Вызывается каждый кадр, пока игрок держит E
    {
        if (isHiding) return; // Если уже идёт процесс прятанья — ничего не делаем

        if (hideStarted) return; // Если прятанье уже запущено этим удержанием — не запускаем второй раз

        if (playerHideController == null) return; // Если контроллер пряток не назначен — выходим

        if (playerHideController.isHidden) return; // Если игрок уже спрятан — повторно не прячем

        if (holdTime >= holdTimeToHide) // Если игрок удерживал E достаточно долго
        {
            hideStarted = true; // Запоминаем, что прятанье уже запущено

            StartCoroutine(HideSequence()); // Запускаем последовательность прятанья
        }
    }

    public void HoldCancel(float holdTime) // Вызывается, когда игрок отпустил E после долгого удержания
    {
        hideStarted = false; // Сбрасываем флаг удержания
    }

    IEnumerator HideSequence() // Последовательность залезания в шкаф
    {
        if (playerHideController == null) yield break; // Если контроллер пряток не назначен — выходим

        if (playerHideController.isHidden) yield break; // Если игрок уже спрятан — выходим

        if (hidePoint == null || exitPoint == null) yield break; // Если точка внутри шкафа или точка выхода не назначены — сразу останавливаем coroutine

        isHiding = true; // Блокируем повторный запуск

        if (wardrobeDoor != null) // Если дверь шкафа назначена
        {
            wardrobeDoor.OpenDoor(); // Открываем дверь шкафа
        }

        yield return new WaitForSeconds(doorOpenBeforeHideDelay); // Ждём, чтобы дверь успела открыться

        playerHideController.Hide(hidePoint, exitPoint, wardrobeDoor); // Прячем игрока внутрь шкафа

        yield return new WaitForSeconds(doorCloseAfterHideDelay); // Ждём немного после входа

        if (wardrobeDoor != null) // Если дверь шкафа назначена
        {
            wardrobeDoor.CloseDoor(); // Закрываем дверь шкафа
        }

        isHiding = false; // Разрешаем следующий запуск

        hideStarted = false; // Сбрасываем флаг удержания
    }
}