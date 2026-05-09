using UnityEngine; // подключаем базовые классы Unity
using System.Collections; // подключаем корутины

public class PlayerHideController : MonoBehaviour // скрипт отвечает за состояние пряток игрока
{
    public bool isHidden = false; // спрятан ли игрок сейчас

    public CharacterController characterController; // ссылка на CharacterController игрока

    public Behaviour[] movementScriptsToDisable; // скрипты движения, которые нужно отключать в шкафу

    public float exitInputDelay = 0.5f; // задержка перед разрешением выхода

    public float holdTimeToExit = 2f; // сколько секунд нужно держать E, чтобы выйти

    public float doorOpenBeforeExitDelay = 0.4f; // пауза после открытия двери перед выходом

    public float doorCloseAfterExitDelay = 0.4f; // пауза перед закрытием двери после выхода

    private Transform currentExitPoint; // точка выхода из текущего шкафа

    private UniversalDoor currentDoor; // дверь текущего шкафа

    private float hideEnterTime = 0f; // время, когда игрок спрятался

    private float exitHoldTimer = 0f; // таймер удержания E для выхода

    private bool isExiting = false; // защита от повторного выхода

    void Reset() // вызывается при добавлении скрипта на объект
    {
        characterController = GetComponent<CharacterController>(); // автоматически ищем CharacterController на игроке
    }

    void Update() // вызывается каждый кадр
    {
        if (!isHidden) return; // если игрок не спрятан — ничего не делаем

        if (isExiting) return; // если уже идет выход — ничего не делаем

        if (Time.time < hideEnterTime + exitInputDelay) return; // если задержка после входа еще не прошла — выходим

        if (Input.GetKey(KeyCode.E)) // если игрок держит E
        {
            exitHoldTimer += Time.deltaTime; // увеличиваем таймер удержания

            if (exitHoldTimer >= holdTimeToExit) // если E держали достаточно долго
            {
                exitHoldTimer = 0f; // сбрасываем таймер

                StartCoroutine(ExitHideSequence()); // запускаем выход из шкафа
            }
        }

        if (Input.GetKeyUp(KeyCode.E)) // если игрок отпустил E
        {
            exitHoldTimer = 0f; // сбрасываем таймер выхода
        }
    }

    public void Hide(Transform hidePoint, Transform exitPoint, UniversalDoor door) // метод прятанья в шкаф
    {
        if (hidePoint == null || exitPoint == null) return; // если точки не назначены — выходим

        isHidden = true; // помечаем игрока как спрятанного

        hideEnterTime = Time.time; // запоминаем момент входа в шкаф

        currentExitPoint = exitPoint; // запоминаем точку выхода

        currentDoor = door; // запоминаем дверь текущего шкафа

        exitHoldTimer = 0f; // сбрасываем таймер выхода

        SetMovement(false); // отключаем движение игрока

        TeleportPlayer(hidePoint.position); // переносим игрока внутрь шкафа
    }

    IEnumerator ExitHideSequence() // последовательность выхода из шкафа
    {
        isExiting = true; // блокируем повторный выход

        if (currentDoor != null) // если дверь шкафа назначена
        {
            currentDoor.OpenDoor(); // открываем дверь шкафа
        }

        yield return new WaitForSeconds(doorOpenBeforeExitDelay); // ждем, чтобы дверь успела открыться

        isHidden = false; // помечаем игрока как не спрятанного

        if (currentExitPoint != null) // если точка выхода назначена
        {
            TeleportPlayer(currentExitPoint.position); // переносим игрока наружу
        }

        SetMovement(true); // включаем движение игрока обратно

        yield return new WaitForSeconds(doorCloseAfterExitDelay); // ждем немного после выхода

        if (currentDoor != null) // если дверь шкафа назначена
        {
            currentDoor.CloseDoor(); // закрываем дверь шкафа
        }

        currentDoor = null; // очищаем ссылку на дверь

        currentExitPoint = null; // очищаем ссылку на точку выхода

        isExiting = false; // разрешаем следующие действия
    }

    void TeleportPlayer(Vector3 targetPosition) // метод безопасного телепорта игрока
    {
        if (characterController != null) // если CharacterController назначен
        {
            characterController.enabled = false; // временно отключаем контроллер
        }

        transform.position = targetPosition; // переносим игрока в нужную позицию

        if (characterController != null) // если CharacterController назначен
        {
            characterController.enabled = true; // включаем контроллер обратно
        }
    }

    void SetMovement(bool enabledState) // включает или выключает скрипты движения
    {
        for (int i = 0; i < movementScriptsToDisable.Length; i++) // проходим по всем скриптам движения
        {
            if (movementScriptsToDisable[i] != null) // если элемент массива не пустой
            {
                movementScriptsToDisable[i].enabled = enabledState; // включаем или выключаем скрипт
            }
        }
    }
}