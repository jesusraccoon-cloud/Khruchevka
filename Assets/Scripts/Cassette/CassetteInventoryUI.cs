using UnityEngine; // Подключаем Unity
using TMPro; // Подключаем TextMeshPro

public class CassetteInventoryUI : MonoBehaviour // Скрипт счетчика кассет
{
    [Header("UI")]
    public TextMeshProUGUI cassetteCounterText; // Ссылка на текст счетчика

    [Header("Settings")]
    public int currentCassetteCount = 0; // Сколько кассет собрано
    public int maxCassetteCount = 6; // Сколько всего кассет нужно собрать

    void Start()
    {
        UpdateUI(); // Обновляем UI при старте
    }

    public void AddCassette()
    {
        currentCassetteCount++; // Увеличиваем счетчик

        if (currentCassetteCount > maxCassetteCount) // Не даем уйти выше максимума
        {
            currentCassetteCount = maxCassetteCount;
        }

        UpdateUI(); // Обновляем текст
    }

    void UpdateUI()
    {
        if (cassetteCounterText != null) // Если текст назначен
        {
            cassetteCounterText.text = currentCassetteCount + "/" + maxCassetteCount; // Показываем счетчик
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": cassetteCounterText НЕ назначен"); // Полезное предупреждение
        }
    }
}