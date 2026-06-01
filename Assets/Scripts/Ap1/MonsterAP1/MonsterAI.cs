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

    [Header("Attack")] // Блок атаки
    public float attackDistance = 1.2f; // На каком расстоянии монстр атакует игрока
    public float attackDelay = 1.2f; // Через сколько секунд после начала атаки показать Game Over
    public Animator animator; // Animator монстра
    public GameOverManager gameOverManager; // Ссылка на систему Game Over
    public StarterAssets.FirstPersonController playerController; // Ссылка на контроллер игрока

    [Header("Final Kitchen Chase")] // Блок финальной погони на кухне
    public GameObject kitchenBarricadeObject; // Объект баррикады на кухне
    public Transform kitchenAttackPoint; // Точка перед входом на кухню

    [Header("Final Window Threat")] // Блок угрозы у финального окна
    public WindowExitTrigger finalWindowExitTrigger; // Ссылка на выход через окно
    public Transform finalWindowAttackPoint; // Точка, куда монстр идёт после удара по окну

    private bool isAttacking = false; // Выполняет ли монстр атаку сейчас
    private bool isFinalKitchenChase = false; // Идёт ли финальная кухонная погоня
    private bool isFinalWindowThreat = false; // Идёт ли финальная угроза у окна

    private Vector3 lastSeenPosition; // Последняя позиция игрока
    private Vector3 noisePosition; // Позиция последнего шума

    private float loseTimer = 0f; // Таймер потери игрока
    private float noiseWaitTimer = 0f; // Таймер ожидания на месте шума

    private bool isChasing = false; // Идёт ли погоня
    private bool isInvestigatingNoise = false; // Исследует ли монстр шум
    private bool isWaitingAtNoise = false; // Стоит ли монстр уже на месте шума

    private bool isGoingToPoint = false; // Идёт ли монстр к специальной точке
    private bool isStandingAtSpecialPoint = false; // Стоит ли монстр на специальной точке
    private Transform specialTargetPoint; // Точка, куда монстр должен прийти и остановиться

    void Update() // Каждый кадр
    {
        if (!isActivated) // Если монстр не активирован
        {
            if (patrol != null) patrol.isPatrolActive = false; // Выключаем патруль

            if (agent != null) agent.ResetPath(); // Останавливаем агента

            return; // Выходим, монстр ничего не делает
        }

        if (isFinalKitchenChase) // Если включена финальная погоня на кухне
        {
            HandleFinalKitchenChase(); // Обрабатываем финальную погоню

            return; // Не выполняем обычный AI
        }

        if (isFinalWindowThreat) // Если включена угроза у финального окна
        {
            HandleFinalWindowThreat(); // Обрабатываем угрозу у окна

            return; // Не выполняем обычный AI
        }

        if (isStandingAtSpecialPoint) // Если монстр должен стоять на специальной точке
        {
            if (agent != null) agent.ResetPath(); // Не даём агенту снова идти

            return; // Обычный AI не выполняем
        }

        if (isGoingToPoint) // Если монстр идёт к специальной точке
        {
            TryOpenDoorAhead(); // Позволяем монстру открывать двери по пути

            if (!agent.pathPending) // Если путь уже построен
            {
                if (agent.remainingDistance <= agent.stoppingDistance + 0.4f) // Если монстр почти дошёл
                {
                    agent.ResetPath(); // Останавливаем монстра

                    isGoingToPoint = false; // Выключаем режим движения к точке

                    isStandingAtSpecialPoint = true; // Включаем режим постоянного стояния

                    Debug.Log("Монстр дошёл до специальной точки и остался стоять"); // Сообщение в Console
                }
            }

            return; // Пока идём к точке, обычный AI не выполняется
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
            float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Считаем расстояние до игрока

            if (distanceToPlayer <= attackDistance) // Если игрок подошёл слишком близко
            {
                StartAttack(); // Начинаем атаку

                return; // Выходим из Update
            }

            agent.SetDestination(player.position); // Продолжаем преследование
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
            {
                return false; // Между монстром и игроком препятствие
            }
        }

        return true; // Игрок виден
    }

    void StartChase() // Начать погоню
    {
        isChasing = true; // Включаем погоню
        isInvestigatingNoise = false; // Выключаем исследование шума
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isGoingToPoint = false; // Выключаем движение к специальной точке
        isStandingAtSpecialPoint = false; // Снимаем режим стояния, если началась обычная погоня

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
        if (isGoingToPoint) return; // Если монстр идёт к специальной точке — шум его не отвлекает
        if (isStandingAtSpecialPoint) return; // Если монстр стоит на специальной точке — шум его не отвлекает
        if (isFinalKitchenChase) return; // Если идёт финальная погоня на кухне — шум игнорируем
        if (isFinalWindowThreat) return; // Если идёт угроза у окна — шум игнорируем

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

    public void StartFinalKitchenChase() // Запустить финальную погоню к кухне
    {
        if (agent == null) return; // Если агент не назначен — выходим
        if (player == null) return; // Если игрок не назначен — выходим
        if (!agent.isActiveAndEnabled) return; // Если агент выключен — выходим
        if (!agent.isOnNavMesh) return; // Если монстр не стоит на NavMesh — выходим

        isActivated = true; // Включаем AI монстра
        isFinalKitchenChase = true; // Включаем режим финальной кухонной погони

        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isChasing = false; // Выключаем обычную погоню
        isInvestigatingNoise = false; // Выключаем расследование шума
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isGoingToPoint = false; // Выключаем обычное движение к точке
        isStandingAtSpecialPoint = false; // Убираем режим стояния у выхода

        if (patrol != null) // Если патруль назначен
        {
            patrol.isPatrolActive = false; // Отключаем патруль
        }

        agent.isStopped = false; // Разрешаем агенту двигаться

        Debug.Log("Монстр начал финальную погоню к кухне"); // Сообщение в Console
    }

    void HandleFinalKitchenChase() // Логика финальной кухонной погони
    {
        if (agent == null) return; // Если агент не назначен — выходим
        if (player == null) return; // Если игрок не назначен — выходим

        TryOpenDoorAhead(); // Монстр пробует открывать двери перед собой

        bool barricadeBlocksMonster = kitchenBarricadeObject != null && kitchenBarricadeObject.activeInHierarchy; // Проверяем активна ли баррикада

        if (barricadeBlocksMonster) // Если баррикада стоит и блокирует проход
        {
            if (kitchenAttackPoint != null) // Если точка перед кухней назначена
            {
                agent.SetDestination(kitchenAttackPoint.position); // Монстр идёт к точке перед кухней
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f) // Если монстр дошёл до баррикады
            {
                agent.ResetPath(); // Останавливаем монстра перед баррикадой
            }

            return; // Не даём монстру преследовать игрока через баррикаду
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Считаем расстояние до игрока

        if (distanceToPlayer <= attackDistance) // Если монстр достаточно близко к игроку
        {
            StartAttack(); // Запускаем атаку

            return; // Выходим
        }

        agent.SetDestination(player.position); // Если баррикады нет — монстр идёт за игроком
    }

    public void StartFinalWindowThreat(Transform targetPoint) // Запустить угрозу у финального окна
    {
        if (targetPoint == null) return; // Если точка не назначена — выходим
        if (agent == null) return; // Если агент не назначен — выходим
        if (!agent.isActiveAndEnabled) return; // Если агент выключен — выходим
        if (!agent.isOnNavMesh) return; // Если монстр не стоит на NavMesh — выходим

        isActivated = true; // Включаем AI монстра
        isFinalWindowThreat = true; // Включаем режим угрозы у окна

        finalWindowAttackPoint = targetPoint; // Запоминаем точку у окна

        isFinalKitchenChase = false; // Выключаем кухонную погоню
        isChasing = false; // Выключаем обычную погоню
        isInvestigatingNoise = false; // Выключаем расследование шума
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isGoingToPoint = false; // Выключаем движение к точке
        isStandingAtSpecialPoint = false; // Выключаем стояние на специальной точке

        if (patrol != null) // Если патруль назначен
        {
            patrol.isPatrolActive = false; // Выключаем патруль
        }

        agent.isStopped = false; // Разрешаем агенту двигаться

        agent.SetDestination(finalWindowAttackPoint.position); // Отправляем монстра к окну

        Debug.Log("Монстр начал угрозу у финального окна"); // Сообщение в Console
    }

    void HandleFinalWindowThreat() // Логика угрозы у финального окна
    {
        if (agent == null) return; // Если агент не назначен — выходим
        if (player == null) return; // Если игрок не назначен — выходим
        if (finalWindowAttackPoint == null) return; // Если точка окна не назначена — выходим

        TryOpenDoorAhead(); // Монстр пробует открыть двери перед собой

        bool playerStartedExit = false; // По умолчанию игрок не начал перелезать

        if (finalWindowExitTrigger != null) // Если ссылка на выход через окно назначена
        {
            playerStartedExit = finalWindowExitTrigger.playerStartedWindowExit; // Берём состояние выхода через окно
        }

        if (playerStartedExit) // Если игрок уже начал перелезать
        {
            agent.SetDestination(finalWindowAttackPoint.position); // Монстр просто идёт к точке окна

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f) // Если монстр дошёл до точки
            {
                agent.ResetPath(); // Останавливаем монстра

                isFinalWindowThreat = false; // Выключаем режим угрозы

                isStandingAtSpecialPoint = true; // Оставляем монстра стоять у окна

                Debug.Log("Игрок начал перелезать, монстр остановился у окна"); // Сообщение в Console
            }

            return; // Не преследуем игрока
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Считаем расстояние до игрока

        if (distanceToPlayer <= attackDistance) // Если монстр достаточно близко к игроку
        {
            StartAttack(); // Запускаем атаку

            return; // Выходим
        }

        agent.SetDestination(player.position); // Если игрок не перелезает — монстр идёт убивать игрока
    }

    public void GoToPointAndStop(Transform targetPoint) // Отправить монстра к точке и остановить там
    {
        if (targetPoint == null) return; // Если точка не назначена — выходим
        if (agent == null) return; // Если агент не назначен — выходим
        if (!agent.isActiveAndEnabled) return; // Если агент выключен — выходим
        if (!agent.isOnNavMesh) return; // Если монстр не стоит на NavMesh — выходим

        isActivated = true; // Включаем AI, чтобы Update начал работать

        isFinalKitchenChase = false; // Выключаем финальную кухонную погоню
        isFinalWindowThreat = false; // Выключаем угрозу у окна

        isGoingToPoint = true; // Включаем специальный режим движения
        isStandingAtSpecialPoint = false; // Сбрасываем режим стояния

        specialTargetPoint = targetPoint; // Запоминаем точку назначения

        isChasing = false; // Выключаем погоню
        isInvestigatingNoise = false; // Выключаем расследование шума
        isWaitingAtNoise = false; // Выключаем ожидание на шуме

        if (patrol != null) // Если патруль назначен
        {
            patrol.isPatrolActive = false; // Выключаем патруль
        }

        agent.isStopped = false; // Разрешаем агенту двигаться

        agent.SetDestination(specialTargetPoint.position); // Отправляем монстра к точке

        Debug.Log("Монстр пошёл к специальной точке"); // Сообщение в Console
    }

    public void ActivateMonster() // Активация монстра
    {
        if (!gameObject.activeInHierarchy) return; // Если объект Monster выключен в Hierarchy — ничего не делаем
        if (agent == null) return; // Если NavMeshAgent не назначен — выходим
        if (!agent.isActiveAndEnabled) return; // Если NavMeshAgent выключен — выходим
        if (!agent.isOnNavMesh) return; // Если монстр не стоит на NavMesh — не запускаем патруль

        isActivated = true; // Включаем монстра

        isFinalKitchenChase = false; // Выключаем финальную кухонную погоню
        isFinalWindowThreat = false; // Выключаем угрозу у окна

        agent.isStopped = false; // Разрешаем агенту двигаться

        if (patrol != null) // Если патруль назначен
        {
            patrol.StartPatrol(); // Запускаем патруль
        }
    }

    void StartAttack() // Начать атаку игрока
    {
        if (isAttacking) return; // Если атака уже идёт — ничего не делаем

        isAttacking = true; // Помечаем что атака началась

        isFinalKitchenChase = false; // Выключаем финальную погоню на кухне
        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isChasing = false; // Останавливаем погоню
        isInvestigatingNoise = false; // Отключаем расследование шума
        isWaitingAtNoise = false; // Отключаем ожидание на шуме

        if (patrol != null) // Если назначен патруль
        {
            patrol.isPatrolActive = false; // Выключаем патруль
        }

        if (agent != null) // Если агент существует
        {
            agent.ResetPath(); // Сбрасываем путь

            agent.isStopped = true; // Полностью останавливаем NavMeshAgent
        }

        if (playerController != null) // Если назначен контроллер игрока
        {
            playerController.canMove = false; // Запрещаем движение

            playerController.canLook = false; // Запрещаем вращение камеры
        }

        if (animator != null) // Если назначен Animator
        {
            animator.SetTrigger("Attack"); // Запускаем анимацию атаки
        }

        Invoke(nameof(FinishAttack), attackDelay); // Через несколько секунд завершаем атаку
    }

    void FinishAttack() // Завершение атаки
    {
        if (gameOverManager != null) // Если назначен GameOverManager
        {
            gameOverManager.ShowGameOver(); // Показываем экран Game Over
        }
    }

    public void StandAtFinalBlockPoint() // Поставить монстра в финальный режим блока выхода
    {
        isActivated = false; // Отключаем обычное зрение и преследование

        isFinalKitchenChase = false; // Выключаем финальную кухонную погоню
        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isChasing = false; // Выключаем погоню
        isInvestigatingNoise = false; // Выключаем реакцию на шум
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isGoingToPoint = false; // Выключаем движение к точке
        isStandingAtSpecialPoint = true; // Включаем режим стояния на точке

        if (patrol != null) // Если патруль назначен
        {
            patrol.isPatrolActive = false; // Выключаем патруль
        }

        if (agent != null) // Если агент назначен
        {
            agent.ResetPath(); // Сбрасываем путь

            agent.isStopped = true; // Останавливаем агента
        }
    }
}