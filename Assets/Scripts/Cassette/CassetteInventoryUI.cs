using UnityEngine; // Подключаем Unity-классы: MonoBehaviour, Debug, Mathf и другие
using TMPro; // Подключаем TextMeshPro для работы с UI текстом

public class CassetteInventoryUI : MonoBehaviour // Скрипт счетчика кассет
{
    [Header("UI")] // Заголовок секции UI в Inspector
    public TextMeshProUGUI cassetteCounterText; // Ссылка на текст счетчика кассет

    [Header("Settings")] // Заголовок секции настроек
    public int currentCassetteCount = 0; // Сколько кассет уже собрано
    public int maxCassetteCount = 6; // Максимальное количество кассет

    [Header("Monster")] // Заголовок секции монстра
    public MonsterPatrol monsterPatrol; // Ссылка на скрипт монстра

    public int activateMonsterAt = 4; // При каком количестве кассет запускать монстра

    private bool monsterActivated = false; // Защита от повторного запуска монстра

    void Start() // Вызывается один раз при старте сцены
    {
        UpdateUI(); // Обновляем UI сразу после запуска сцены
    }

    public void AddCassette() // Метод добавления кассеты
    {
        // Увеличиваем количество кассет
        // Mathf.Clamp не дает значению выйти за пределы
        currentCassetteCount = Mathf.Clamp(currentCassetteCount + 1, 0, maxCassetteCount);

        // Если монстр еще не активирован
        // и количество кассет достигло нужного значения
        if (!monsterActivated && currentCassetteCount >= activateMonsterAt)
        {
            ActivateMonster(); // Запускаем монстра
        }

        UpdateUI(); // Обновляем отображение счетчика
    }

    void ActivateMonster() // Метод активации монстра
    {
        monsterActivated = true; // Помечаем что монстр уже был активирован

        if (monsterPatrol != null) // Если ссылка на монстра назначена
        {
            monsterPatrol.StartPatrol(); // Запускаем патрулирование монстра
        }
        else // Если ссылка отсутствует
        {
            Debug.LogWarning("MonsterPatrol не назначен"); // Выводим предупреждение в Console
        }
    }

    void UpdateUI() // Метод обновления UI
    {
        // Если текст не назначен — выходим
        // Это защищает от NullReference ошибки
        if (cassetteCounterText == null) return;

        // Обновляем текст счетчика
        // Например: 2/6
        cassetteCounterText.text = currentCassetteCount + "/" + maxCassetteCount;
    }
}