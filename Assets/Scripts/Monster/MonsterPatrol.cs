using UnityEngine;                  // базовые классы Unity (Transform, MonoBehaviour и т.д.)
using UnityEngine.AI;               // доступ к NavMeshAgent

public class MonsterPatrol : MonoBehaviour   // наш скрипт, который вешается на монстра
{
    public NavMeshAgent agent;      // ссылка на NavMeshAgent (движение по NavMesh)
    public Transform[] patrolPoints; // массив точек патруля (куда идти по очереди)

    public float waitTime = 2f;     // сколько стоять на точке перед переходом к следующей

    private int currentPointIndex = 0; // индекс текущей точки в массиве
    private float waitTimer = 0f;      // таймер ожидания на точке

    void Start()                   // вызывается один раз при старте сцены
    {
        if (agent == null)                         // если ссылка на агент не задана
            agent = GetComponent<NavMeshAgent>();  // пытаемся найти NavMeshAgent на этом объекте

        if (patrolPoints.Length > 0)               // если есть хотя бы одна точка
        {
            agent.SetDestination(patrolPoints[0].position); // отправляем монстра к первой точке
        }
    }

    void Update()                  // вызывается каждый кадр
    {
        if (patrolPoints.Length == 0) return; // если точек нет — ничего не делаем

        if (agent.pathPending) return;        // если агент ещё строит путь — ждём

        // проверяем, дошёл ли агент до точки
        if (agent.remainingDistance <= agent.stoppingDistance) // если расстояние до цели маленькое
        {
            waitTimer += Time.deltaTime; // увеличиваем таймер ожидания

            if (waitTimer >= waitTime)   // если подождали достаточно
            {
                currentPointIndex++;     // переключаемся на следующую точку

                if (currentPointIndex >= patrolPoints.Length) // если дошли до конца массива
                    currentPointIndex = 0;                    // возвращаемся к первой точке

                agent.SetDestination(patrolPoints[currentPointIndex].position); // задаём новую цель

                waitTimer = 0f;        // сбрасываем таймер ожидания
            }
        }
    }
}