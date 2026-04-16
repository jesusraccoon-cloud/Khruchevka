using UnityEngine;

public class TumblerSwitch : MonoBehaviour
{
    public bool isOn = false;
    // Включен ли тумблер сейчас

    [Header("Renderer")]
    public Renderer targetRenderer;
    // Renderer, которому будем менять цвет

    [Header("Colors")]
    public Color normalColor = Color.gray;
    // Обычный цвет

    public Color highlightColor = Color.yellow;
    // Цвет при наведении

    public Color activeColor = Color.green;
    // Цвет, когда тумблер уже включен

    private Material runtimeMaterial;
    // Отдельная копия материала именно для этого тумблера

    void Start()
    {
        // Если Renderer не задан вручную — пробуем взять его с этого объекта
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        // Если и тут не нашли — пробуем взять Renderer у дочерних объектов
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        // Если Renderer найден — создаем отдельную копию материала
        // чтобы цвет менялся только у этого тумблера
        if (targetRenderer != null)
        {
            runtimeMaterial = new Material(targetRenderer.material);
            targetRenderer.material = runtimeMaterial;
        }

        UpdateVisual();
        // Сразу ставим стартовый цвет
    }

    public void Toggle()
    {
        isOn = !isOn;
        // Меняем состояние на противоположное

        UpdateVisual();
        // Обновляем внешний вид
    }

    public void SetHighlight(bool state)
    {
        // Если материал не найден — выходим
        if (runtimeMaterial == null) return;

        // Если тумблер уже включен — он всегда остается зеленым
        if (isOn)
        {
            runtimeMaterial.color = activeColor;
            return;
        }

        // Если наводимся — желтый, если нет — обычный
        runtimeMaterial.color = state ? highlightColor : normalColor;
    }

    public void UpdateVisual()
    {
        // Если материал не найден — выходим
        if (runtimeMaterial == null) return;

        // Если тумблер включен — зеленый, иначе обычный
        runtimeMaterial.color = isOn ? activeColor : normalColor;
    }
}