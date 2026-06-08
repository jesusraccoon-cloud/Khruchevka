using UnityEngine; // Подключаем основные классы Unity
using UnityEngine.AI; // Подключаем NavMeshAgent

public class MonsterAI : MonoBehaviour // Главный скрипт монстра
{
    public NavMeshAgent agent; // Ссылка на NavMeshAgent монстра
    public Transform player; // Ссылка на игрока
    public PlayerHideController playerHide; // Ссылка на систему пряток игрока
    public MonsterPatrol patrol; // Ссылка на патруль монстра

    [Header("Activation")] // Блок активации
    public bool isActivated = false; // Активен ли монстр

    [Header("Vision")] // Блок зрения
    public float viewDistance = 8f; // Дистанция зрения монстра
    public float viewAngle = 60f; // Угол зрения монстра
    public LayerMask obstacleMask; // Слой препятствий

    [Header("Lose Player")] // Блок потери игрока
    public float loseTime = 3f; // Через сколько секунд монстр теряет игрока

    [Header("Door Opening")] // Блок открытия дверей
    public float doorCheckDistance = 1.8f; // Дистанция проверки двери
    public LayerMask doorLayers = ~0; // Слои дверей

    [Header("Noise Reaction")] // Блок реакции на шум
    public float noiseArriveDistance = 1.2f; // Дистанция, на которой считаем, что монстр дошёл до шума
    public float noiseWaitTime = 4f; // Сколько секунд монстр стоит на месте шума
    public float suspiciousLookTime = 2f; // Сколько секунд монстр осматривается при шуме 4
    public float normalNoiseSpeed = 2.5f; // Скорость движения к шуму 5-6
    public float loudNoiseSpeed = 4.5f; // Скорость движения к шуму 7-10

    [Header("Attack")] // Блок атаки
    public float attackDistance = 1.2f; // Дистанция атаки
    public float attackDelay = 1.2f; // Задержка перед Game Over
    public Animator animator; // Animator монстра
    public GameOverManager gameOverManager; // Ссылка на GameOverManager
    public StarterAssets.FirstPersonController playerController; // Ссылка на контроллер игрока

    [Header("Final Kitchen Chase")] // Блок финальной погони на кухне
    public GameObject kitchenBarricadeObject; // Баррикада на кухне
    public Transform kitchenAttackPoint; // Точка перед кухней

    [Header("Final Window Threat")] // Блок угрозы у окна
    public WindowExitTrigger finalWindowExitTrigger; // Ссылка на финальный выход через окно
    public Transform finalWindowAttackPoint; // Точка атаки у окна

    private bool isAttacking = false; // Идёт ли атака сейчас
    private bool isFinalKitchenChase = false; // Идёт ли финальная кухонная погоня
    private bool isFinalWindowThreat = false; // Идёт ли финальная угроза у окна

    private Vector3 lastSeenPosition; // Последняя позиция, где монстр видел игрока
    private Vector3 noisePosition; // Последняя позиция шума

    private float loseTimer = 0f; // Таймер потери игрока
    private float noiseWaitTimer = 0f; // Таймер ожидания на месте шума
    private float lookAroundTimer = 0f; // Таймер осматривания после шума 4
    private float defaultAgentSpeed = 0f; // Стандартная скорость NavMeshAgent

    private bool isChasing = false; // Преследует ли монстр игрока
    private bool isInvestigatingNoise = false; // Идёт ли монстр к шуму
    private bool isWaitingAtNoise = false; // Стоит ли монстр на месте шума
    private bool isLookingAroundNoise = false; // Осматривается ли монстр после шума 4

    private bool isGoingToPoint = false; // Идёт ли монстр к специальной точке
    private bool isStandingAtSpecialPoint = false; // Стоит ли монстр на специальной точке
    private Transform specialTargetPoint; // Специальная точка назначения

    private void Start() // Вызывается один раз при запуске сцены
    {
        if (agent != null) // Если NavMeshAgent назначен
        {
            defaultAgentSpeed = agent.speed; // Запоминаем стандартную скорость монстра
        }
    }

    void Update() // Вызывается каждый кадр
    {
        if (!isActivated) // Если монстр не активирован
        {
            if (patrol != null) patrol.isPatrolActive = false; // Выключаем патруль

            if (agent != null) agent.ResetPath(); // Сбрасываем путь агента

            return; // Выходим из Update
        }

        if (isFinalKitchenChase) // Если идёт финальная кухонная погоня
        {
            HandleFinalKitchenChase(); // Обрабатываем финальную погоню

            return; // Не выполняем обычную логику
        }

        if (isFinalWindowThreat) // Если идёт финальная угроза у окна
        {
            HandleFinalWindowThreat(); // Обрабатываем угрозу у окна

            return; // Не выполняем обычную логику
        }

        if (isStandingAtSpecialPoint) // Если монстр должен стоять на специальной точке
        {
            if (agent != null) agent.ResetPath(); // Останавливаем NavMeshAgent

            return; // Не выполняем обычную логику
        }

        if (isGoingToPoint) // Если монстр идёт к специальной точке
        {
            HandleGoToSpecialPoint(); // Обрабатываем движение к специальной точке

            return; // Не выполняем обычную логику
        }

        TryOpenDoorAhead(); // Проверяем дверь перед монстром

        if (CanSeePlayer()) // Если монстр видит игрока
        {
            StartChase(); // Запускаем погоню
        }
        else if (isChasing) // Если монстр гнался, но потерял игрока
        {
            StopChaseLogic(); // Обрабатываем потерю игрока
        }
        else if (isLookingAroundNoise) // Если монстр осматривается после шума 4
        {
            HandleLookAroundNoise(); // Обрабатываем осматривание
        }
        else if (isInvestigatingNoise) // Если монстр идёт к шуму
        {
            HandleNoiseInvestigation(); // Обрабатываем движение к шуму
        }

        if (isChasing) // Если монстр сейчас преследует игрока
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Считаем расстояние до игрока

            if (distanceToPlayer <= attackDistance) // Если игрок достаточно близко
            {
                StartAttack(); // Запускаем атаку

                return; // Выходим из Update
            }

            agent.SetDestination(player.position); // Продолжаем идти к игроку
        }
    }

    private void HandleGoToSpecialPoint() // Обработка движения к специальной точке
    {
        TryOpenDoorAhead(); // Пробуем открыть дверь перед монстром

        if (agent == null) return; // Если агента нет — выходим

        if (agent.pathPending) return; // Если путь ещё строится — ждём

        if (agent.remainingDistance <= agent.stoppingDistance + 0.4f) // Если монстр почти дошёл
        {
            agent.ResetPath(); // Останавливаем монстра

            isGoingToPoint = false; // Выключаем режим движения к точке

            isStandingAtSpecialPoint = true; // Включаем режим стояния

            Debug.Log("Монстр дошёл до специальной точки и остался стоять"); // Пишем лог
        }
    }

    bool CanSeePlayer() // Проверка видимости игрока
    {
        if (!isActivated) return false; // Неактивный монстр не видит игрока
        if (player == null) return false; // Если игрок не назначен — не видим
        if (playerHide != null && playerHide.isHidden) return false; // Если игрок спрятан — не видим

        float distance = Vector3.Distance(transform.position, player.position); // Считаем расстояние до игрока

        if (distance > viewDistance) return false; // Если игрок дальше зрения — не видим

        Vector3 directionToPlayer = (player.position - transform.position).normalized; // Направление к игроку

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer); // Угол между взглядом монстра и игроком

        if (angleToPlayer > viewAngle * 0.5f) return false; // Если игрок вне угла зрения — не видим

        Vector3 rayStart = transform.position + Vector3.up * 1.4f; // Точка начала луча зрения

        if (Physics.Raycast(rayStart, directionToPlayer, out RaycastHit hit, viewDistance)) // Пускаем луч к игроку
        {
            if (!hit.transform.IsChildOf(player) && hit.transform != player) // Если луч попал не в игрока
            {
                return false; // Значит между монстром и игроком препятствие
            }
        }

        return true; // Игрок виден
    }

    void StartChase() // Начать погоню
    {
        isChasing = true; // Включаем погоню

        isInvestigatingNoise = false; // Выключаем движение к шуму
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isLookingAroundNoise = false; // Выключаем осматривание
        isGoingToPoint = false; // Выключаем движение к спецточке
        isStandingAtSpecialPoint = false; // Выключаем стояние на спецточке

        loseTimer = 0f; // Сбрасываем таймер потери игрока

        lastSeenPosition = player.position; // Запоминаем текущую позицию игрока

        RestoreDefaultSpeed(); // Возвращаем обычную скорость

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль
    }

    void StopChaseLogic() // Логика потери игрока
    {
        loseTimer += Time.deltaTime; // Увеличиваем таймер потери

        if (loseTimer < loseTime) // Если монстр ещё помнит игрока
        {
            agent.SetDestination(lastSeenPosition); // Идём к последней позиции игрока
        }
        else // Если время потери вышло
        {
            isChasing = false; // Выключаем погоню

            RestoreDefaultSpeed(); // Возвращаем обычную скорость

            if (patrol != null) patrol.StartPatrol(); // Возвращаем патруль
        }
    }

    public void ReactToNoise(Vector3 newNoisePosition, int noisePower) // Реакция на шум с силой от 1 до 10
    {
        if (!isActivated) return; // Если монстр не активен — игнорируем
        if (isChasing) return; // Если монстр уже гонится — шум не отвлекает
        if (isGoingToPoint) return; // Если монстр идёт к спецточке — игнорируем
        if (isStandingAtSpecialPoint) return; // Если монстр стоит на спецточке — игнорируем
        if (isFinalKitchenChase) return; // Если финальная кухонная погоня — игнорируем
        if (isFinalWindowThreat) return; // Если финальная угроза у окна — игнорируем

        noisePower = Mathf.Clamp(noisePower, 1, 10); // Ограничиваем силу шума от 1 до 10

        if (noisePower <= 3) // Если шум слабый
        {
            return; // Ничего не делаем
        }

        if (noisePower == 4) // Если шум средний
        {
            StartLookAroundNoise(newNoisePosition); // Монстр останавливается и осматривается

            return; // Выходим
        }

        if (noisePower >= 5 && noisePower <= 6) // Если шум 5-6
        {
            StartNoiseInvestigation(newNoisePosition, normalNoiseSpeed); // Идём к шуму обычной скоростью

            return; // Выходим
        }

        if (noisePower >= 7) // Если шум 7-10
        {
            StartNoiseInvestigation(newNoisePosition, loudNoiseSpeed); // Быстро идём к шуму

            return; // Выходим
        }
    }

    public void HearNoise(Vector3 newNoisePosition) // Старый метод реакции на шум для совместимости
    {
        ReactToNoise(newNoisePosition, 6); // Старый шум считаем как шум средней силы 6
    }

    private void StartLookAroundNoise(Vector3 newNoisePosition) // Запустить осматривание после шума 4
    {
        noisePosition = newNoisePosition; // Запоминаем позицию шума

        isLookingAroundNoise = true; // Включаем режим осматривания
        isInvestigatingNoise = false; // Выключаем движение к шуму
        isWaitingAtNoise = false; // Выключаем ожидание на шуме

        lookAroundTimer = 0f; // Сбрасываем таймер осматривания

        RestoreDefaultSpeed(); // Возвращаем обычную скорость

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        if (agent != null) agent.ResetPath(); // Останавливаем агента

        Vector3 directionToNoise = noisePosition - transform.position; // Считаем направление к шуму

        directionToNoise.y = 0f; // Убираем наклон по вертикали

        if (directionToNoise.sqrMagnitude > 0.01f) // Если направление не нулевое
        {
            transform.rotation = Quaternion.LookRotation(directionToNoise.normalized); // Поворачиваем монстра к шуму
        }

        Debug.Log("Монстр услышал шум 4 и осматривается"); // Пишем лог
    }

    private void HandleLookAroundNoise() // Обработка осматривания
    {
        lookAroundTimer += Time.deltaTime; // Увеличиваем таймер осматривания

        if (lookAroundTimer >= suspiciousLookTime) // Если время осматривания закончилось
        {
            isLookingAroundNoise = false; // Выключаем осматривание

            if (patrol != null) patrol.StartPatrol(); // Возвращаем патруль
        }
    }

    private void StartNoiseInvestigation(Vector3 newNoisePosition, float moveSpeed) // Запустить движение к шуму
    {
        noisePosition = newNoisePosition; // Запоминаем позицию шума

        isLookingAroundNoise = false; // Выключаем осматривание
        isInvestigatingNoise = true; // Включаем движение к шуму
        isWaitingAtNoise = false; // Пока не ждём, сначала идём

        noiseWaitTimer = 0f; // Сбрасываем таймер ожидания

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        if (agent != null) // Если агент назначен
        {
            agent.speed = moveSpeed; // Ставим скорость реакции на шум
            agent.isStopped = false; // Разрешаем движение
            agent.SetDestination(noisePosition); // Отправляем монстра к месту шума
        }

        Debug.Log("Монстр идёт на шум. Скорость: " + moveSpeed); // Пишем лог
    }

    void HandleNoiseInvestigation() // Логика движения к шуму
    {
        if (agent == null) return; // Если агента нет — выходим

        TryOpenDoorAhead(); // Пробуем открыть дверь по пути к шуму

        if (agent.pathPending) return; // Если путь ещё строится — ждём

        if (!isWaitingAtNoise) // Если монстр ещё идёт к шуму
        {
            if (agent.remainingDistance <= agent.stoppingDistance + noiseArriveDistance) // Если монстр дошёл
            {
                isWaitingAtNoise = true; // Включаем ожидание на месте шума

                noiseWaitTimer = 0f; // Сбрасываем таймер ожидания

                agent.ResetPath(); // Останавливаем агента
            }
        }
        else // Если монстр уже стоит на месте шума
        {
            noiseWaitTimer += Time.deltaTime; // Увеличиваем таймер ожидания

            if (noiseWaitTimer >= noiseWaitTime) // Если монстр подождал достаточно
            {
                isInvestigatingNoise = false; // Выключаем движение к шуму

                isWaitingAtNoise = false; // Выключаем ожидание

                RestoreDefaultSpeed(); // Возвращаем обычную скорость

                if (patrol != null) patrol.StartPatrol(); // Возвращаем патруль
            }
        }
    }

    void TryOpenDoorAhead() // Проверка двери перед монстром
    {
        if (!isActivated) return; // Если монстр не активен — двери не открывает

        Vector3 checkCenter = transform.position + transform.forward * 1.2f + Vector3.up * 1.0f; // Центр проверки двери

        Debug.DrawRay(transform.position + Vector3.up * 1.0f, transform.forward * 1.5f, Color.red); // Рисуем красный луч

        Collider[] hits = Physics.OverlapSphere(checkCenter, 0.6f, doorLayers); // Ищем коллайдеры двери рядом

        foreach (Collider hit in hits) // Перебираем найденные коллайдеры
        {
            UniversalDoor door = hit.GetComponentInParent<UniversalDoor>(); // Ищем UniversalDoor в родителях

            if (door != null) // Если дверь найдена
            {
                door.OpenDoorForMonster(); // Открываем дверь монстром

                return; // Выходим
            }
        }
    }

    public void StartFinalKitchenChase() // Запустить финальную погоню к кухне
    {
        if (agent == null) return; // Если агента нет — выходим
        if (player == null) return; // Если игрока нет — выходим
        if (!agent.isActiveAndEnabled) return; // Если агент выключен — выходим
        if (!agent.isOnNavMesh) return; // Если агент не на NavMesh — выходим

        isActivated = true; // Активируем монстра
        isFinalKitchenChase = true; // Включаем финальную кухонную погоню

        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isChasing = false; // Выключаем обычную погоню
        isInvestigatingNoise = false; // Выключаем движение к шуму
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isLookingAroundNoise = false; // Выключаем осматривание
        isGoingToPoint = false; // Выключаем движение к точке
        isStandingAtSpecialPoint = false; // Выключаем стояние на точке

        RestoreDefaultSpeed(); // Возвращаем обычную скорость

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        agent.isStopped = false; // Разрешаем движение агенту

        Debug.Log("Монстр начал финальную погоню к кухне"); // Пишем лог
    }

    void HandleFinalKitchenChase() // Логика финальной кухонной погони
    {
        if (agent == null) return; // Если агента нет — выходим
        if (player == null) return; // Если игрока нет — выходим

        TryOpenDoorAhead(); // Пробуем открыть дверь перед монстром

        bool barricadeBlocksMonster = kitchenBarricadeObject != null && kitchenBarricadeObject.activeInHierarchy; // Проверяем, стоит ли баррикада

        if (barricadeBlocksMonster) // Если баррикада блокирует монстра
        {
            if (kitchenAttackPoint != null) // Если точка атаки кухни назначена
            {
                agent.SetDestination(kitchenAttackPoint.position); // Идём к точке перед кухней
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f) // Если дошли до баррикады
            {
                agent.ResetPath(); // Останавливаемся перед баррикадой
            }

            return; // Не преследуем игрока через баррикаду
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Считаем расстояние до игрока

        if (distanceToPlayer <= attackDistance) // Если игрок рядом
        {
            StartAttack(); // Запускаем атаку

            return; // Выходим
        }

        agent.SetDestination(player.position); // Если баррикады нет — идём за игроком
    }

    public void StartFinalWindowThreat(Transform targetPoint) // Запустить угрозу у финального окна
    {
        if (targetPoint == null) return; // Если точка не назначена — выходим
        if (agent == null) return; // Если агента нет — выходим
        if (!agent.isActiveAndEnabled) return; // Если агент выключен — выходим
        if (!agent.isOnNavMesh) return; // Если агент не на NavMesh — выходим

        isActivated = true; // Активируем монстра
        isFinalWindowThreat = true; // Включаем угрозу у окна

        finalWindowAttackPoint = targetPoint; // Запоминаем точку окна

        isFinalKitchenChase = false; // Выключаем кухонную погоню
        isChasing = false; // Выключаем обычную погоню
        isInvestigatingNoise = false; // Выключаем движение к шуму
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isLookingAroundNoise = false; // Выключаем осматривание
        isGoingToPoint = false; // Выключаем движение к точке
        isStandingAtSpecialPoint = false; // Выключаем стояние на точке

        RestoreDefaultSpeed(); // Возвращаем обычную скорость

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        agent.isStopped = false; // Разрешаем движение

        agent.SetDestination(finalWindowAttackPoint.position); // Отправляем монстра к окну

        Debug.Log("Монстр начал угрозу у финального окна"); // Пишем лог
    }

    void HandleFinalWindowThreat() // Логика угрозы у финального окна
    {
        if (agent == null) return; // Если агента нет — выходим
        if (player == null) return; // Если игрока нет — выходим
        if (finalWindowAttackPoint == null) return; // Если точки окна нет — выходим

        TryOpenDoorAhead(); // Пробуем открыть дверь перед монстром

        bool playerStartedExit = false; // По умолчанию игрок ещё не начал выход

        if (finalWindowExitTrigger != null) // Если финальный триггер окна назначен
        {
            playerStartedExit = finalWindowExitTrigger.playerStartedWindowExit; // Берём состояние выхода через окно
        }

        if (playerStartedExit) // Если игрок начал перелезать
        {
            agent.SetDestination(finalWindowAttackPoint.position); // Монстр идёт к точке окна

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f) // Если монстр дошёл
            {
                agent.ResetPath(); // Останавливаем агента

                isFinalWindowThreat = false; // Выключаем угрозу у окна

                isStandingAtSpecialPoint = true; // Оставляем монстра стоять

                Debug.Log("Игрок начал перелезать, монстр остановился у окна"); // Пишем лог
            }

            return; // Не преследуем игрока
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Считаем расстояние до игрока

        if (distanceToPlayer <= attackDistance) // Если игрок рядом
        {
            StartAttack(); // Запускаем атаку

            return; // Выходим
        }

        agent.SetDestination(player.position); // Иначе монстр идёт за игроком
    }

    public void GoToPointAndStop(Transform targetPoint) // Отправить монстра к точке и остановить там
    {
        if (targetPoint == null) return; // Если точка не назначена — выходим
        if (agent == null) return; // Если агента нет — выходим
        if (!agent.isActiveAndEnabled) return; // Если агент выключен — выходим
        if (!agent.isOnNavMesh) return; // Если агент не на NavMesh — выходим

        isActivated = true; // Активируем монстра

        isFinalKitchenChase = false; // Выключаем кухонную погоню
        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isChasing = false; // Выключаем погоню
        isInvestigatingNoise = false; // Выключаем шум
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isLookingAroundNoise = false; // Выключаем осматривание

        isGoingToPoint = true; // Включаем движение к специальной точке

        isStandingAtSpecialPoint = false; // Выключаем стояние

        specialTargetPoint = targetPoint; // Запоминаем точку назначения

        RestoreDefaultSpeed(); // Возвращаем обычную скорость

        if (patrol != null) patrol.isPatrolActive = false; // Выключаем патруль

        agent.isStopped = false; // Разрешаем движение

        agent.SetDestination(specialTargetPoint.position); // Отправляем к точке

        Debug.Log("Монстр пошёл к специальной точке"); // Пишем лог
    }

    public void ActivateMonster() // Активация монстра
    {
        if (!gameObject.activeInHierarchy) return; // Если объект выключен — выходим
        if (agent == null) return; // Если агента нет — выходим
        if (!agent.isActiveAndEnabled) return; // Если агент выключен — выходим
        if (!agent.isOnNavMesh) return; // Если агент не на NavMesh — выходим

        isActivated = true; // Активируем монстра

        isFinalKitchenChase = false; // Выключаем кухонную погоню
        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isChasing = false; // Выключаем погоню
        isInvestigatingNoise = false; // Выключаем движение к шуму
        isWaitingAtNoise = false; // Выключаем ожидание
        isLookingAroundNoise = false; // Выключаем осматривание
        isGoingToPoint = false; // Выключаем движение к точке
        isStandingAtSpecialPoint = false; // Выключаем стояние

        RestoreDefaultSpeed(); // Возвращаем обычную скорость

        agent.isStopped = false; // Разрешаем движение агенту

        if (patrol != null) patrol.StartPatrol(); // Запускаем патруль
    }

    void StartAttack() // Начать атаку игрока
    {
        if (isAttacking) return; // Если атака уже идёт — выходим

        isAttacking = true; // Помечаем атаку активной

        isFinalKitchenChase = false; // Выключаем кухонную погоню
        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isChasing = false; // Выключаем погоню
        isInvestigatingNoise = false; // Выключаем движение к шуму
        isWaitingAtNoise = false; // Выключаем ожидание на шуме
        isLookingAroundNoise = false; // Выключаем осматривание

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        if (agent != null) // Если агент назначен
        {
            agent.ResetPath(); // Сбрасываем путь

            agent.isStopped = true; // Останавливаем агента
        }

        if (playerController != null) // Если контроллер игрока назначен
        {
            playerController.canMove = false; // Запрещаем движение

            playerController.canLook = false; // Запрещаем обзор
        }

        if (animator != null) // Если Animator назначен
        {
            animator.SetTrigger("Attack"); // Запускаем анимацию атаки
        }

        Invoke(nameof(FinishAttack), attackDelay); // Через задержку завершаем атаку
    }

    void FinishAttack() // Завершить атаку
    {
        if (gameOverManager != null) // Если GameOverManager назначен
        {
            gameOverManager.ShowGameOver(); // Показываем Game Over
        }
    }

    public void StandAtFinalBlockPoint() // Поставить монстра в режим блока выхода
    {
        isActivated = false; // Отключаем обычную активность

        isFinalKitchenChase = false; // Выключаем кухонную погоню
        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isChasing = false; // Выключаем погоню
        isInvestigatingNoise = false; // Выключаем движение к шуму
        isWaitingAtNoise = false; // Выключаем ожидание
        isLookingAroundNoise = false; // Выключаем осматривание
        isGoingToPoint = false; // Выключаем движение к точке
        isStandingAtSpecialPoint = true; // Включаем стояние на точке

        RestoreDefaultSpeed(); // Возвращаем обычную скорость

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        if (agent != null) // Если агент назначен
        {
            agent.ResetPath(); // Сбрасываем путь

            agent.isStopped = true; // Останавливаем агента
        }
    }

    public void ForceChasePlayer() // Принудительно заставить монстра преследовать игрока
    {
        if (agent == null) return; // Если агента нет — выходим
        if (player == null) return; // Если игрока нет — выходим
        if (!agent.isActiveAndEnabled) return; // Если агент выключен — выходим
        if (!agent.isOnNavMesh) return; // Если агент не на NavMesh — выходим

        isActivated = true; // Активируем монстра

        isFinalKitchenChase = false; // Выключаем кухонную погоню
        isFinalWindowThreat = false; // Выключаем угрозу у окна
        isGoingToPoint = false; // Выключаем движение к точке
        isStandingAtSpecialPoint = false; // Выключаем стояние на точке
        isInvestigatingNoise = false; // Выключаем движение к шуму
        isWaitingAtNoise = false; // Выключаем ожидание
        isLookingAroundNoise = false; // Выключаем осматривание

        isChasing = true; // Включаем погоню

        RestoreDefaultSpeed(); // Возвращаем обычную скорость

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        agent.isStopped = false; // Разрешаем движение

        agent.SetDestination(player.position); // Отправляем монстра к игроку

        Debug.Log("Монстр принудительно начал преследовать игрока"); // Пишем лог
    }

    private void RestoreDefaultSpeed() // Метод возврата стандартной скорости
    {
        if (agent == null) return; // Если агента нет — выходим

        if (defaultAgentSpeed <= 0f) // Если стандартная скорость не была записана
        {
            defaultAgentSpeed = agent.speed; // Записываем текущую скорость как стандартную
        }

        agent.speed = defaultAgentSpeed; // Возвращаем стандартную скорость
    }
}