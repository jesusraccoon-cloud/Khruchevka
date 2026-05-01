using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public NavMeshAgent agent; // движение монстра

    public Transform player; // игрок
    public PlayerHideController playerHide; // чтобы знать спрятан ли игрок

    public MonsterPatrol patrol; // твой патруль

    public float viewDistance = 8f; // дистанция зрения
    public float viewAngle = 60f;  // угол зрения

    public LayerMask obstacleMask; // слой стен

    private Vector3 lastSeenPosition; // последняя позиция игрока

    private float loseTimer = 0f; // таймер потери игрока
    public float loseTime = 3f;   // через сколько забываем игрока

    private bool isChasing = false; // сейчас преследуем?

    void Update()
    {
        Debug.Log("MonsterAI работает");
        if (CanSeePlayer()) // если видим игрока
        {
            StartChase();
        }
        else
        {
            StopChaseLogic();
        }

        if (isChasing) // если в режиме погони
        {
            agent.SetDestination(player.position); // идём за игроком
        }
    }

    bool CanSeePlayer() // функция проверяет: видит ли монстр игрока
{
    if (playerHide != null && playerHide.isHidden) return false;
    // если у нас есть ссылка на систему пряток И игрок сейчас спрятан → сразу "не видим"

    float distance = Vector3.Distance(transform.position, player.position);
    // считаем расстояние от монстра до игрока

    if (distance > viewDistance) return false;
    // если игрок дальше, чем дистанция обзора → не видим

    Vector3 directionToPlayer = (player.position - transform.position).normalized;
    // считаем направление от монстра к игроку и нормализуем (получаем вектор длиной 1)

    float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
    // считаем угол между тем, куда смотрит монстр, и направлением на игрока

    if (angleToPlayer > viewAngle * 0.5f) return false;
    // если игрок вне угла обзора (например, сзади) → не видим

    if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, viewDistance))
    {
        // пускаем луч от "глаз" монстра (чуть выше центра), в сторону игрока

        if (!hit.transform.IsChildOf(player) && hit.transform != player)
        {
            // если луч упёрся НЕ в игрока (например, в стену)
            return false; // значит между вами есть препятствие → не видим
        }
    }

    return true;
    // если прошли все проверки → игрок виден
}

    void StartChase()
    {
        isChasing = true; // включаем погоню

        loseTimer = 0f; // сбрасываем таймер

        lastSeenPosition = player.position; // запоминаем позицию

        patrol.isPatrolActive = false; // выключаем патруль
    }

    void StopChaseLogic()
    {
        if (!isChasing) return;

        loseTimer += Time.deltaTime; // считаем время потери

        if (loseTimer < loseTime)
        {
            agent.SetDestination(lastSeenPosition); // идём к последнему месту
        }
        else
        {
            isChasing = false; // выключаем погоню

            patrol.isPatrolActive = true; // возвращаем патруль
        }
    }
}