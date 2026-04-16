using UnityEngine;

public class TumblerInteractor : MonoBehaviour
{
    public Camera playerCamera;
    // Камера игрока, из которой идет луч

    public float interactDistance = 3f;
    // Максимальная дистанция взаимодействия

    public KeyCode interactKey = KeyCode.E;
    // Кнопка взаимодействия

    public LayerMask interactLayer = ~0;
    // Слои, по которым работает Raycast

    private TumblerSwitch currentTumbler;
    // Тумблер, который сейчас выбран и подсвечен

    void Start()
    {
        // Если камера не назначена вручную — берем Main Camera
        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleSelection();
        // Каждый кадр обновляем выбор тумблера

        if (Input.GetKeyDown(interactKey))
        {
            // Если нажали кнопку взаимодействия
            if (currentTumbler != null)
            {
                currentTumbler.Toggle();
                // Переключаем выбранный тумблер
            }
        }
    }

    void HandleSelection()
    {
        // Если камера не задана — выходим
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        // Создаем луч от камеры вперед

        RaycastHit hit;
        // Сюда запишется информация о попадании луча

        TumblerSwitch newTumbler = null;
        // Новый выбранный тумблер в этом кадре

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.red);
        // Для отладки рисуем луч в Scene View

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            // Если луч попал во что-то на нужной дистанции

            TumblerPanel panel = hit.collider.GetComponentInParent<TumblerPanel>();
            // Пытаемся найти панель у объекта, в который попали

            if (panel != null)
            {
                newTumbler = panel.GetClosestToScreenCenter(playerCamera);
                // Если это панель — выбираем тумблер, который ближе к центру экрана
            }
        }

        if (currentTumbler != newTumbler)
        {
            // Если выбранный тумблер изменился

            if (currentTumbler != null)
            {
                currentTumbler.SetHighlight(false);
                // Убираем подсветку со старого тумблера
            }

            currentTumbler = newTumbler;
            // Запоминаем новый тумблер

            if (currentTumbler != null)
            {
                currentTumbler.SetHighlight(true);
                // Включаем подсветку на новом
            }
        }
    }
}