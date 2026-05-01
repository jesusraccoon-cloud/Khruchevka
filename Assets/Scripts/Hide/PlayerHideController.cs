using UnityEngine; // подключаем базовые классы Unity

public class PlayerHideController : MonoBehaviour // скрипт отвечает за состояние пряток игрока
{
    public bool isHidden = false; // спрятан ли игрок сейчас

    public CharacterController characterController; // ссылка на CharacterController игрока

    public Behaviour[] movementScriptsToDisable; // скрипты движения, которые нужно отключать в шкафу

    public float exitInputDelay = 0.5f; // задержка перед разрешением выхода, чтобы старое удержание E не выкинуло игрока сразу

    private Transform currentExitPoint; // точка выхода из текущего шкафа

    private float hideEnterTime = 0f; // время, когда игрок спрятался

    void Reset() // вызывается при добавлении скрипта на объект
    {
        characterController = GetComponent<CharacterController>(); // автоматически ищем CharacterController на игроке
    }

    void Update() // вызывается каждый кадр
    {
        if (isHidden && Time.time > hideEnterTime + exitInputDelay && Input.GetKeyDown(KeyCode.E)) // если игрок спрятан, задержка прошла и он заново нажал E
        {
            ExitHide(); // выходим из шкафа
        }
    }

    public void Hide(Transform hidePoint, Transform exitPoint) // метод прятанья в шкаф
    {
        if (hidePoint == null || exitPoint == null) return; // если точки не назначены — выходим

        isHidden = true; // помечаем игрока как спрятанного

        hideEnterTime = Time.time; // запоминаем момент входа в шкаф

        currentExitPoint = exitPoint; // запоминаем точку выхода

        SetMovement(false); // отключаем движение игрока

        TeleportPlayer(hidePoint.position); // переносим игрока внутрь шкафа
    }

    public void ExitHide() // метод выхода из шкафа
    {
        if (!isHidden) return; // если игрок не спрятан — ничего не делаем

        isHidden = false; // помечаем игрока как не спрятанного

        if (currentExitPoint != null) // если точка выхода назначена
        {
            TeleportPlayer(currentExitPoint.position); // переносим игрока наружу
        }

        SetMovement(true); // включаем движение игрока обратно
    }

    void TeleportPlayer(Vector3 targetPosition) // метод безопасного телепорта игрока
    {
        if (characterController != null) // если CharacterController назначен
        {
            characterController.enabled = false; // временно отключаем контроллер, чтобы телепорт не сломался
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