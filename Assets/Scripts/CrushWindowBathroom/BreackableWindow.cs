using UnityEngine; // Подключаем Unity

public class BreakableWindow : MonoBehaviour // Скрипт разбиваемого окна
{
    public GameObject windowIntact; 
    // Целое окно

    public GameObject windowBroken; 
    // Разбитое окно

    public float breakDistance = 2f; 
    // Дистанция, с которой можно ударить окно

    public KeyCode breakKey = KeyCode.Mouse0; 
    // Кнопка удара

    public Camera playerCamera; 
    // Камера игрока

    public int hitsToBreak = 3; 
    // Сколько ударов нужно, чтобы разбить окно

    public float hitDelay = 0.3f; 
    // Задержка между ударами, чтобы нельзя было спамить кликом

    private int currentHits = 0; 
    // Сколько ударов уже нанесено

    private bool isBroken = false; 
    // Разбито окно или нет

    private bool canHit = true; 
    // Можно ли сейчас ударить
    public bool IsBroken { get { return isBroken; } } // Даём доступ другим скриптам

    void Start() // Вызывается один раз при старте
    {
        if (windowIntact != null)
            windowIntact.SetActive(true);
        // Включаем целое окно

        if (windowBroken != null)
            windowBroken.SetActive(false);
        // Выключаем разбитое окно
    }

    void Update() // Вызывается каждый кадр
    {
        if (isBroken) return;
        // Если окно уже разбито — ничего не делаем

        if (Input.GetKeyDown(breakKey))
        // Если нажали кнопку удара
        {
            TryHitWindow();
            // Пытаемся ударить окно
        }
    }

    void TryHitWindow() // Попытка ударить окно
    {
        if (!canHit) return;
        // Если задержка еще не прошла — удар не засчитываем

        if (playerCamera == null)
        // Проверяем, назначена ли камера
        {
            Debug.LogError("НЕ НАЗНАЧЕНА КАМЕРА!");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        // Создаем луч из камеры вперед

        Debug.DrawRay(
            playerCamera.transform.position,
            playerCamera.transform.forward * breakDistance,
            Color.red,
            2f
        );
        // Рисуем луч в Scene View для проверки

        RaycastHit hit;
        // Информация о попадании луча

        if (Physics.Raycast(ray, out hit, breakDistance))
        // Если луч во что-то попал
        {
            Debug.Log("Луч попал в: " + hit.collider.name);

            BreakableWindow window = hit.collider.GetComponentInParent<BreakableWindow>();
            // Ищем BreakableWindow у объекта, в который попали, или у его родителя

            if (window == this)
            // Проверяем, что попали именно в это окно
            {
                HitWindow();
                // Засчитываем удар по окну
            }
            else
            {
                Debug.Log("Попали не в это окно");
            }
        }
        else
        {
            Debug.Log("Луч ни во что не попал");
        }
    }

    void HitWindow() // Удар по окну
    {
        if (isBroken) return;
        // Если окно уже разбито — ничего не делаем

        currentHits++;
        // Добавляем один удар

        Debug.Log("Удар по окну: " + currentHits + " / " + hitsToBreak);

        if (currentHits >= hitsToBreak)
        // Если ударов достаточно
        {
            Break();
            // Разбиваем окно
        }
        else
        {
            StartCoroutine(HitCooldown());
            // Запускаем задержку перед следующим ударом
        }
    }

    System.Collections.IEnumerator HitCooldown() // Задержка между ударами
    {
        canHit = false;
        // Запрещаем удар

        yield return new WaitForSeconds(hitDelay);
        // Ждем указанное количество секунд

        canHit = true;
        // Снова разрешаем удар
    }

    void Break() // Разбить окно
    {
        if (isBroken) return;
        // Если уже разбито — выходим

        Debug.Log("ОКНО РАЗБИТО!");

        isBroken = true;
        // Помечаем окно как разбитое

        if (windowIntact != null)
        {
            windowIntact.SetActive(false);
            // Выключаем целое окно
        }

        if (windowBroken != null)
        {
            windowBroken.SetActive(true);
            // Включаем разбитое окно
        }
    }
}