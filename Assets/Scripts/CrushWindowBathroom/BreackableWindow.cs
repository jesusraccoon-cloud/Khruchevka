using UnityEngine; // Подключаем Unity

public class BreakableWindow : MonoBehaviour // Скрипт разбиваемого окна
{
    public GameObject windowIntact; 
    // Целое окно

    public GameObject windowBroken; 
    // Разбитое окно

    public float breakDistance = 5f; 
    // Дистанция увеличена для теста (чтобы точно доставало)

    public KeyCode breakKey = KeyCode.Mouse0; 
    // Кнопка удара (ЛКМ)

    public Camera playerCamera; 
    // Камера игрока

    private bool isBroken = false; 
    // Уже разбито или нет

    void Update() // Вызывается каждый кадр
    {
        if (isBroken) return; 
        // Если уже разбито — ничего не делаем

        if (Input.GetKeyDown(breakKey)) 
        {
            Debug.Log("🟡 Нажата кнопка удара"); // Проверяем ввод
            TryBreak();
        }
    }

    void TryBreak() // Попытка разбить окно
    {
        if (playerCamera == null)
        {
            Debug.LogError("❌ НЕ НАЗНАЧЕНА КАМЕРА!");
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); 
        // Луч от камеры вперед

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * breakDistance, Color.red, 2f);
        // Рисуем луч в сцене (видно в Scene View)

        RaycastHit hit; 

        if (Physics.Raycast(ray, out hit, breakDistance)) 
        {
            Debug.Log("🟢 Луч попал в: " + hit.collider.name);
            // Показывает в какой объект попали

            // 🔥 ВРЕМЕННО: убираем строгую проверку
            // чтобы убедиться, что сам Break() работает

            Break();
        }
        else
        {
            Debug.Log("🔴 Луч НИ ВО ЧТО НЕ ПОПАЛ");
        }
    }

    void Break() // Само разбитие окна
    {
        if (isBroken)
        {
            Debug.Log("⚠️ Окно уже разбито");
            return;
        }

        Debug.Log("💥 ОКНО РАЗБИТО!");

        isBroken = true;

        if (windowIntact != null)
        {
            windowIntact.SetActive(false);
            Debug.Log("👉 Выключили целое окно");
        }
        else
        {
            Debug.LogError("❌ windowIntact НЕ назначен");
        }

        if (windowBroken != null)
        {
            windowBroken.SetActive(true);
            Debug.Log("👉 Включили разбитое окно");
        }
        else
        {
            Debug.LogError("❌ windowBroken НЕ назначен");
        }
    }
}