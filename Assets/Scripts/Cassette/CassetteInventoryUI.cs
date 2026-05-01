using UnityEngine; // Подключаем Unity
using TMPro; // Подключаем TextMeshPro

public class CassetteInventoryUI : MonoBehaviour // Скрипт счетчика кассет
{
    [Header("UI")]
    public TextMeshProUGUI cassetteCounterText; // Ссылка на текст счетчика

    [Header("Settings")]
    public int currentCassetteCount = 0; // Сколько кассет собрано
    public int maxCassetteCount = 6; // Сколько всего кассет нужно собрать

    [Header("Monster")]
    public MonsterPatrol monsterPatrol; // ссылка на монстра

    public int activateMonsterAt = 4; // при каком количестве кассет активируется монстр

    private bool monsterActivated = false; // чтобы не активировать несколько раз

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

        // 🔥 ВАЖНО: проверка активации монстра
        if (!monsterActivated && currentCassetteCount >= activateMonsterAt) // если еще не активировали и достигли нужного количества
        {
            ActivateMonster(); // запускаем монстра
        }

        UpdateUI(); // Обновляем текст
    }

    void ActivateMonster()
    {
        monsterActivated = true; // помечаем, что уже активировали

        if (monsterPatrol != null) // если ссылка есть
        {
            monsterPatrol.StartPatrol(); // включаем патруль
        }
        else
        {
            Debug.LogWarning("MonsterPatrol не назначен"); // если забыли назначить
        }
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