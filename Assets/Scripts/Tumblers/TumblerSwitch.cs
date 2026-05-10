using UnityEngine; // Подключаем Unity-классы: MonoBehaviour, Renderer, Color, Material

public class TumblerSwitch : MonoBehaviour, IInteractable // Скрипт тумблера теперь является интерактивным объектом
{
    public bool isOn = false; // Хранит состояние тумблера: включен или выключен

    [Header("Renderer")] // Заголовок в Inspector

    public Renderer targetRenderer; // Renderer тумблера, у которого будем менять цвет

    [Header("Colors")] // Заголовок в Inspector

    public Color normalColor = Color.gray; // Цвет выключенного тумблера

    public Color highlightColor = Color.yellow; // Цвет при наведении, пока позже не используем

    public Color activeColor = Color.green; // Цвет включенного тумблера

    private Material runtimeMaterial; // Отдельный материал для этого тумблера, чтобы не менять цвет всем сразу

    private void Start() // Запускается один раз при старте сцены
    {
        if (targetRenderer == null) // Если Renderer не назначен вручную
        {
            targetRenderer = GetComponent<Renderer>(); // Пробуем найти Renderer на этом же объекте
        }

        if (targetRenderer == null) // Если Renderer всё ещё не найден
        {
            targetRenderer = GetComponentInChildren<Renderer>(); // Пробуем найти Renderer в дочерних объектах
        }

        if (targetRenderer != null) // Если Renderer найден
        {
            runtimeMaterial = new Material(targetRenderer.material); // Создаём копию материала только для этого тумблера
            targetRenderer.material = runtimeMaterial; // Назначаем копию материала этому Renderer
        }

        UpdateVisual(); // Обновляем цвет тумблера при старте
    }

    public void Interact() // Метод, который вызывает PlayerInteractor при нажатии E
    {
        Toggle(); // Переключаем тумблер
    }

    public void Toggle() // Метод переключения тумблера
    {
        isOn = !isOn; // Меняем состояние на противоположное

        UpdateVisual(); // Обновляем цвет после изменения состояния
    }

    public void SetHighlight(bool state) // Метод подсветки, оставляем для совместимости со старым TumblerInteractor
    {
        if (runtimeMaterial == null) return; // Если материала нет, выходим

        if (isOn) // Если тумблер включен
        {
            runtimeMaterial.color = activeColor; // Оставляем активный цвет
            return; // Выходим, чтобы подсветка не перебила зелёный цвет
        }

        runtimeMaterial.color = state ? highlightColor : normalColor; // Если наведены — жёлтый, если нет — обычный
    }

    public void UpdateVisual() // Метод обновления внешнего вида
    {
        if (runtimeMaterial == null) return; // Если материала нет, выходим

        runtimeMaterial.color = isOn ? activeColor : normalColor; // Если включен — зелёный, если выключен — серый
    }
}