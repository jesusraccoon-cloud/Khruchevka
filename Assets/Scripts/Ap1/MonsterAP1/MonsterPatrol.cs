using UnityEngine;                  // базовые классы Unity
using UnityEngine.AI;               // доступ к NavMeshAgent

public class MonsterPatrol : MonoBehaviour   // скрипт патруля монстра
{
    public bool isPatrolActive = false;      // включен ли патруль

    public NavMeshAgent agent;               // ссылка на NavMeshAgent
    public Transform[] patrolPoints;         // точки патруля

    public float waitTime = 2f;              // сколько ждать на точке

    private int currentPointIndex = 0;       // номер текущей точки
    private float waitTimer = 0f;            // таймер ожидания

    void Start()                             // вызывается при старте
    {
        if (agent == null)                   // если агент не назначен
            agent = GetComponent<NavMeshAgent>(); // ищем агент на этом объекте
    }

    void Update()                            // вызывается каждый кадр
    {
        if (!isPatrolActive) return;         // если патруль выключен — ничего не делаем

        if (patrolPoints.Length == 0) return; // если точек нет — выходим

        if (agent.pathPending) return;       // если путь ещё строится — ждём

        if (agent.remainingDistance <= agent.stoppingDistance) // если дошёл до точки
        {
            waitTimer += Time.deltaTime;     // считаем время ожидания

            if (waitTimer >= waitTime)       // если достаточно подождали
            {
                currentPointIndex++;         // переходим к следующей точке

                if (currentPointIndex >= patrolPoints.Length) // если дошли до конца списка
                    currentPointIndex = 0;   // возвращаемся к первой точке

                agent.SetDestination(patrolPoints[currentPointIndex].position); // идём к новой точке

                waitTimer = 0f;              // сбрасываем таймер
            }
        }
    }

    public void StartPatrol()                // метод для запуска патруля извне
    {
        isPatrolActive = true;               // включаем патруль

        waitTimer = 0f;                      // сбрасываем таймер ожидания

        currentPointIndex = 0;               // начинаем с первой точки

        if (agent == null)                   // если агент не назначен
            agent = GetComponent<NavMeshAgent>(); // ищем агент на монстре

        if (patrolPoints.Length > 0)         // если есть точки
            agent.SetDestination(patrolPoints[currentPointIndex].position); // отправляем к первой точке
    }
}