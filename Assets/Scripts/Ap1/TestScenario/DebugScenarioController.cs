using UnityEngine; // Подключаем Unity
using UnityEngine.SceneManagement; // Работа со сценами

public class DebugScenarioController : MonoBehaviour // Отдельный debug-контроллер сценария
{
    [Header("Debug Keys")] // Блок кнопок для тестирования
    public KeyCode phaseFourKey = KeyCode.F1; // Кнопка для фазы 4/6
    public KeyCode phaseSixKey = KeyCode.F2; // Кнопка для фазы 6/6
    public KeyCode teleportStartKey = KeyCode.F3; // Кнопка телепорта на старт
    public KeyCode teleportKitchenKey = KeyCode.F4; // Кнопка телепорта на кухню
    public KeyCode resetSceneKey = KeyCode.F5; // Кнопка полного сброса сцены

    [Header("Cassette System")] // Блок кассет
    public CassetteInventoryUI cassetteInventoryUI; // Ссылка на счетчик кассет

    [Header("Final Systems")] // Блок финальных систем
    public MonsterAI monsterAI; // Ссылка на AI монстра
    public ApartmentFinalSequence finalSequence; // Ссылка на финальный сценарий

    [Header("Player")] // Блок игрока
    public CharacterController playerController; // CharacterController игрока
    public Transform playerTransform; // Transform игрока
    public PlayerHideController playerHideController; // Скрипт пряток игрока

    [Header("Teleport Points")] // Блок точек телепорта
    public Transform startTeleportPoint; // Точка старта
    public Transform kitchenTeleportPoint; // Точка кухни

    void Update() // Проверяем кнопки каждый кадр
    {
        if (Input.GetKeyDown(phaseFourKey)) // Если нажата кнопка фазы 4/6
        {
            DebugSetPhaseFour(); // Включаем фазу 4/6
        }

        if (Input.GetKeyDown(phaseSixKey)) // Если нажата кнопка фазы 6/6
        {
            DebugSetPhaseSix(); // Включаем фазу 6/6
        }

        if (Input.GetKeyDown(teleportStartKey)) // Если нажата кнопка старта
        {
            TeleportPlayer(startTeleportPoint, "старт"); // Телепортируем игрока на старт
        }

        if (Input.GetKeyDown(teleportKitchenKey)) // Если нажата кнопка кухни
        {
            TeleportPlayer(kitchenTeleportPoint, "кухня"); // Телепортируем игрока на кухню
        }
        if (Input.GetKeyDown(resetSceneKey)) // Если нажали кнопку сброса
        {       
        ResetScene(); // Перезапускаем сцену
        }
    }

    void DebugSetPhaseFour() // Debug-фаза 4/6
    {
        if (cassetteInventoryUI != null) // Если счетчик назначен
        {
            cassetteInventoryUI.currentCassetteCount = 4; // Ставим 4/6

            cassetteInventoryUI.RefreshDebugUI(); // Обновляем UI
        }

        if (monsterAI != null) // Если монстр назначен
        {
            monsterAI.ActivateMonster(); // Активируем монстра
        }

        Debug.Log("DEBUG: включена фаза 4/6"); // Сообщение в Console
    }

    void DebugSetPhaseSix() // Debug-фаза 6/6
    {
        if (cassetteInventoryUI != null) // Если счетчик назначен
        {
            cassetteInventoryUI.currentCassetteCount = 6; // Ставим 6/6

            cassetteInventoryUI.RefreshDebugUI(); // Обновляем UI
        }

        if (monsterAI != null) // Если монстр назначен
        {
            monsterAI.ActivateMonster(); // Активируем монстра
        }

        if (finalSequence != null) // Если финальный сценарий назначен
        {
            finalSequence.StartFinalSequence(); // Запускаем финал
        }

        Debug.Log("DEBUG: включена фаза 6/6"); // Сообщение в Console
    }

    void TeleportPlayer(Transform targetPoint, string pointName) // Телепорт игрока
    {
        if (playerTransform == null || targetPoint == null) // Если игрок или точка не назначены
        {
            Debug.LogWarning("PlayerTransform или точка телепорта не назначены"); // Warning
            return; // Выходим
        }

        if (playerHideController != null) // Если скрипт пряток назначен
        {
            playerHideController.isHidden = false; // Сбрасываем состояние пряток
        }

        if (playerController != null) // Если CharacterController назначен
        {
            playerController.enabled = false; // Отключаем CharacterController
        }

        playerTransform.position = targetPoint.position; // Переносим игрока
        playerTransform.rotation = targetPoint.rotation; // Поворачиваем игрока

        if (playerController != null) // Если CharacterController назначен
        {
            playerController.enabled = true; // Включаем CharacterController
        }

        Debug.Log("DEBUG: игрок телепортирован: " + pointName); // Сообщение в Console
    }
    void ResetScene() // Полный сброс сцены
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Загружаем текущую сцену заново

    Debug.Log("DEBUG: сцена перезапущена"); // Сообщение в Console
}
}