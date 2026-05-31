using UnityEngine; // Подключаем Unity-классы: MonoBehaviour, Renderer, Color, Material

public class TumblerSwitch : MonoBehaviour // Тумблер больше НЕ является отдельным интерактивным объектом
{
    public bool isOn = false; // Хранит состояние тумблера: включен или выключен

    [Header("Renderer")] // Заголовок в Inspector
    public Renderer targetRenderer; // Renderer тумблера, у которого будем менять цвет

    [Header("Colors")] // Заголовок в Inspector
    public Color normalColor = Color.gray; // Цвет выключенного тумблера
    public Color highlightColor = Color.yellow; // Цвет при наведении
    public Color activeColor = Color.green; // Цвет включенного тумблера

    private Material runtimeMaterial; // Отдельная копия материала только для этого тумблера

    private void Start() // Запускается один раз при старте сцены
    {
        if (targetRenderer == null) // Если Renderer не назначен вручную
        {
            targetRenderer = GetComponent<Renderer>(); // Ищем Renderer на этом объекте
        }

        if (targetRenderer == null) // Если всё ещё не найден
        {
            targetRenderer = GetComponentInChildren<Renderer>(); // Ищем Renderer у дочерних объектов
        }

        if (targetRenderer != null) // Если Renderer найден
        {
            runtimeMaterial = new Material(targetRenderer.material); // Создаем отдельную копию материала
            targetRenderer.material = runtimeMaterial; // Назначаем копию Renderer'у
        }

        UpdateVisual(); // Обновляем цвет тумблера при старте
    }

    public void Toggle() // Метод переключения тумблера
    {
        isOn = !isOn; // Меняем состояние на противоположное

        UpdateVisual(); // Обновляем внешний вид
    }

    public void SetHighlight(bool state) // Подсветка при наведении
    {
        if (runtimeMaterial == null) return; // Если материала нет — выходим

        if (isOn) // Если тумблер включен
        {
            runtimeMaterial.color = activeColor; // Оставляем зелёный цвет
            return; // Не даем подсветке перебить активный цвет
        }

        runtimeMaterial.color = state ? highlightColor : normalColor; // Жёлтый при наведении, серый без наведения
    }

    public void UpdateVisual() // Обновление цвета тумблера
    {
        if (runtimeMaterial == null) return; // Если материала нет — выходим

        runtimeMaterial.color = isOn ? activeColor : normalColor; // Зеленый если включен, серый если выключен
    }
}