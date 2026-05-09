using UnityEngine; // подключаем базовые классы Unity
using System.Collections; // подключаем корутины

public class WardrobeHideHandle : MonoBehaviour // скрипт удержания E на ручке шкафа
{
    public Camera playerCamera; // камера игрока, из неё будет идти Raycast

    public PlayerHideController playerHideController; // ссылка на скрипт пряток игрока

    public UniversalDoor wardrobeDoor; // дверь шкафа, которую нужно открыть и закрыть

    public Transform hidePoint; // точка внутри шкафа

    public Transform exitPoint; // точка перед шкафом

    public float interactDistance = 2.5f; // дистанция взаимодействия с ручкой

    public float holdTimeToHide = 2f; // сколько секунд нужно держать E

    public float doorOpenBeforeHideDelay = 0.4f; // пауза после открытия двери перед прятаньем

    public float doorCloseAfterHideDelay = 0.4f; // пауза перед закрытием двери после прятанья

    public LayerMask interactLayer; // слой, по которому Raycast ищет ручку

    private float holdTimer = 0f; // таймер удержания E

    private bool isLookingAtThisHandle = false; // смотрит ли игрок сейчас на эту ручку

    private bool isHiding = false; // защита от повторного запуска прятанья

    void Update() // вызывается каждый кадр
    {
        if (isHiding) return; // если уже идет процесс прятанья — ничего не делаем

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
                holdTimer = 0f; // сбрасываем таймер

                StartCoroutine(HideSequence()); // запускаем последовательность прятанья
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

        RaycastHit hit; // переменная для результата Raycast

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer)) // если луч попал в объект нужного слоя
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)) // если попали именно в эту ручку
            {
                isLookingAtThisHandle = true; // игрок смотрит на эту ручку
            }
        }
    }

    IEnumerator HideSequence() // последовательность залезания в шкаф
    {
        if (playerHideController == null) yield break; // если игрок не назначен — выходим

        if (playerHideController.isHidden) yield break; // если игрок уже спрятан — выходим

        isHiding = true; // блокируем повторный запуск

        if (wardrobeDoor != null) // если дверь шкафа назначена
        {
            wardrobeDoor.OpenDoor(); // открываем дверь шкафа
        }

        yield return new WaitForSeconds(doorOpenBeforeHideDelay); // ждем, чтобы дверь успела открыться

        playerHideController.Hide(hidePoint, exitPoint, wardrobeDoor); // прячем игрока внутрь шкафа

        yield return new WaitForSeconds(doorCloseAfterHideDelay); // ждем немного после входа

        if (wardrobeDoor != null) // если дверь шкафа назначена
        {
            wardrobeDoor.CloseDoor(); // закрываем дверь шкафа
        }

        isHiding = false; // разрешаем следующий запуск
    }
}