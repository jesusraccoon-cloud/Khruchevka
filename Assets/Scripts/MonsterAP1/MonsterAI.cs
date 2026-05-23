using UnityEngine; // Подключаем основные функции Unity
using UnityEngine.AI; // Подключаем NavMeshAgent

public class MonsterAI : MonoBehaviour // Главный скрипт монстра
{
    public NavMeshAgent agent; // Ссылка на NavMeshAgent
    public Transform player; // Ссылка на игрока
    public PlayerHideController playerHide; // Ссылка на систему пряток
    public MonsterPatrol patrol; // Ссылка на патруль

    [Header("Activation")] // Блок активации
    public bool isActivated = false; // Активен ли монстр

    [Header("Vision")] // Блок зрения
    public float viewDistance = 8f; // Дистанция зрения
    public float viewAngle = 60f; // Угол зрения
    public LayerMask obstacleMask; // Слой препятствий

    [Header("Lose Player")] // Блок потери игрока
    public float loseTime = 3f; // Через сколько секунд монстр теряет игрока

    [Header("Door Opening")] // Блок дверей
    public float doorCheckDistance = 1.8f; // Дистанция проверки двери
    public LayerMask doorLayers = ~0; // Слой дверей

    [Header("Noise Investigation")] // Блок исследования шума
    public float noiseArriveDistance = 1.2f; // На каком расстоянии считать, что монстр дошёл до шума
    public float noiseWaitTime = 4f; // Сколько секунд монстр стоит на месте шума

    private Vector3 lastSeenPosition; // Последняя позиция игрока
    private Vector3 noisePosition; // Позиция последнего шума

    private float loseTimer = 0f; // Таймер потери игрока
    private float noiseWaitTimer = 0f; // Таймер ожидания на месте шума

    private bool isChasing = false; // Идёт ли погоня
    private bool isInvestigatingNoise = false; // Исследует ли монстр шум
    private bool isWaitingAtNoise = false; // Стоит ли монстр уже на месте шума

    void Update() // Каждый кадр
    {
        if (!isActivated) // Если монстр не активирован
        {
            if (patrol != null) patrol.isPatrolActive = false; // Выключаем патруль
            if (agent != null) agent.ResetPath(); // Останавливаем агента
            return; // Выходим, монстр ничего не делает
        }

        TryOpenDoorAhead(); // Проверяем дверь перед монстром

        if (CanSeePlayer()) // Если монстр видит игрока
        {
            StartChase(); // Начинаем погоню
        }
        else if (isChasing) // Если игрока не видно, но погоня была
        {
            StopChaseLogic(); // Пытаемся потерять игрока
        }
        else if (isInvestigatingNoise) // Если монстр сейчас идёт на шум
        {
            HandleNoiseInvestigation(); // Обрабатываем режим шума
        }

        if (isChasing) // Если идёт погоня
        {
            agent.SetDestination(player.position); // Идём за игроком
        }
    }

    bool CanSeePlayer() // Проверка видимости игрока
    {
        if (!isActivated) return false; // Спящий монстр не видит игрока
        if (player == null) return false; // Если игрок не назначен — не видим
        if (playerHide != null && playerHide.isHidden) return false; // Если игрок спрятан — не видим

        float distance = Vector3.Distance(transform.position, player.position); // Считаем дистанцию

        if (distance > viewDistance) return false; // Если далеко — не видим

        Vector3 directionToPlayer = (player.position - transform.position).normalized; // Направление к игроку
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer); // Угол до игрока

        if (angleToPlayer > viewAngle * 0.5f) return false; // Если игрок вне угла зрения — не видим

        Vector3 rayStart = transform.position + Vector3.up * 1.4f; // Точка начала луча

        if (Physics.Raycast(rayStart, directionToPlayer, out RaycastHit hit, viewDistance)) // Пускаем луч
        {
            if (!hit.transform.IsChildOf(player) && hit.transform != player) // Если луч попал не в игрока
                return false; // Между монстром и игроком препятствие
        }

        return true; // Игрок виден
    }

    void StartChase() // Начать погоню
    {
        isChasing = true; // Включаем погоню
        isInvestigatingNoise = false; // Выключаем исследование шума
        isWaitingAtNoise = false; // Выключаем ожидание на шуме

        loseTimer = 0f; // Сбрасываем таймер потери
        lastSeenPosition = player.position; // Запоминаем позицию игрока

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль
    }

    void StopChaseLogic() // Логика потери игрока
    {
        loseTimer += Time.deltaTime; // Увеличиваем таймер

        if (loseTimer < loseTime) // Пока монстр ещё помнит игрока
        {
            agent.SetDestination(lastSeenPosition); // Идём к последней позиции игрока
        }
        else // Если игрок потерян
        {
            isChasing = false; // Выключаем погоню
            if (patrol != null) patrol.StartPatrol(); // Возвращаем патруль
        }
    }

    public void HearNoise(Vector3 newNoisePosition) // Метод реакции на шум
    {
        if (!isActivated) return; // Если монстр спит — игнорирует шум
        if (isChasing) return; // Если монстр уже гонится за игроком — шум его не отвлекает

        noisePosition = newNoisePosition; // Запоминаем позицию шума
        isInvestigatingNoise = true; // Включаем режим исследования шума
        isWaitingAtNoise = false; // Пока ещё не ждём, сначала надо дойти
        noiseWaitTimer = 0f; // Сбрасываем таймер ожидания

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        agent.SetDestination(noisePosition); // Отправляем монстра к месту шума
    }

    void HandleNoiseInvestigation() // Логика исследования шума
    {
        if (agent.pathPending) return; // Если путь ещё строится — ждём

        if (!isWaitingAtNoise) // Если монстр ещё идёт к шуму
        {
            if (agent.remainingDistance <= agent.stoppingDistance + noiseArriveDistance) // Если дошёл
            {
                isWaitingAtNoise = true; // Включаем ожидание на месте
                noiseWaitTimer = 0f; // Сбрасываем таймер ожидания
                agent.ResetPath(); // Останавливаем монстра
            }
        }
        else // Если монстр уже стоит на месте шума
        {
            noiseWaitTimer += Time.deltaTime; // Считаем время ожидания

            if (noiseWaitTimer >= noiseWaitTime) // Если подождал достаточно
            {
                isInvestigatingNoise = false; // Выключаем режим шума
                isWaitingAtNoise = false; // Выключаем ожидание

                if (patrol != null) patrol.StartPatrol(); // Возвращаем патруль
            }
        }
    }

    void TryOpenDoorAhead() // Проверка двери перед монстром
    {
        if (!isActivated) return; // Если монстр спит — двери не открывает

        Vector3 checkCenter = transform.position + transform.forward * 1.2f + Vector3.up * 1.0f; // Центр проверки двери

        Debug.DrawRay(transform.position + Vector3.up * 1.0f, transform.forward * 1.5f, Color.red); // Красный луч для отладки

        Collider[] hits = Physics.OverlapSphere(checkCenter, 0.6f, doorLayers); // Ищем двери рядом

        foreach (Collider hit in hits) // Перебираем найденные коллайдеры
        {
            UniversalDoor door = hit.GetComponentInParent<UniversalDoor>(); // Ищем UniversalDoor

            if (door != null) // Если дверь найдена
            {
                door.OpenDoorForMonster(); // Открываем дверь
                return; // Выходим
            }
        }
    }

    public void ActivateMonster() // Активация монстра
{
    if (!gameObject.activeInHierarchy) return; // Если объект Monster выключен в Hierarchy — ничего не делаем

    if (agent == null) return; // Если NavMeshAgent не назначен — выходим

    if (!agent.isActiveAndEnabled) return; // Если NavMeshAgent выключен — выходим

    if (!agent.isOnNavMesh) return; // Если монстр не стоит на NavMesh — не запускаем патруль

    isActivated = true; // Включаем монстра

    if (patrol != null) // Если патруль назначен
    {
        patrol.StartPatrol(); // Запускаем патруль
    }
}
}