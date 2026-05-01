using UnityEngine; // подключаем базовые классы Unity

public class WardrobeHideHandle : MonoBehaviour // скрипт удержания E на ручке шкафа
{
    public Camera playerCamera; // камера игрока, из неё будет идти Raycast

    public PlayerHideController playerHideController; // ссылка на скрипт пряток игрока

    public Transform hidePoint; // точка внутри шкафа, куда переносим игрока

    public Transform exitPoint; // точка перед шкафом, куда игрок выходит

    public float interactDistance = 2.5f; // дистанция взаимодействия с ручкой

    public float holdTimeToHide = 2f; // сколько секунд нужно держать E

    public LayerMask interactLayer; // слой, по которому Raycast ищет ручку

    private float holdTimer = 0f; // таймер удержания E

    private bool isLookingAtThisHandle = false; // смотрит ли игрок сейчас на эту ручку

    void Update() // вызывается каждый кадр
    {
        CheckLookAtHandle(); // проверяем, смотрит ли игрок на эту ручку

        if (!isLookingAtThisHandle) // если игрок не смотрит на эту ручку
        {
            holdTimer = 0f; // сбрасываем таймер удержания
            return; // выходим из Update
        }

        if (Input.GetKey(KeyCode.E)) // если игрок удерживает E
        {
            holdTimer += Time.deltaTime; // увеличиваем таймер удержания

            if (holdTimer >= holdTimeToHide) // если E удерживали достаточно долго
            {
                TryHidePlayer(); // пробуем спрятать игрока

                holdTimer = 0f; // сбрасываем таймер после срабатывания
            }
        }

        if (Input.GetKeyUp(KeyCode.E)) // если игрок отпустил E
        {
            holdTimer = 0f; // сбрасываем таймер удержания
        }
    }

    void CheckLookAtHandle() // проверка наведения на ручку
    {
        isLookingAtThisHandle = false; // сначала считаем, что игрок не смотрит на ручку

        if (playerCamera == null) return; // если камера не назначена — выходим

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // создаём луч из камеры вперёд

        RaycastHit hit; // переменная, куда Unity запишет результат попадания луча

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer)) // если луч попал в объект нужного слоя
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)) // если попали именно в эту ручку или её дочерний объект
            {
                isLookingAtThisHandle = true; // значит игрок смотрит на эту ручку
            }
        }
    }

    void TryHidePlayer() // попытка спрятать игрока
    {
        if (playerHideController == null) return; // если игрок не назначен — выходим

        if (playerHideController.isHidden) return; // если игрок уже спрятан — не прячем повторно

        playerHideController.Hide(hidePoint, exitPoint); // отправляем игрока в шкаф
    }
}