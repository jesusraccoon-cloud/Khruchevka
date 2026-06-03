using UnityEngine; // Подключаем Unity
using System.Collections; // Подключаем корутины

public class WardrobeFallInteract : MonoBehaviour, IInteractable // Скрипт QTE-падения шкафа
{
    [Header("QTE")] // Блок QTE
    public UniversalQTE universalQTE; // Ссылка на UniversalQTE

    public int requiredPresses = 10; // Сколько раз нажать E
    public float qteTime = 4f; // Время на QTE
    public string qteLabel = "ЖМИ E"; // Текст QTE

    [Header("Fall Target")] // Блок падения
    public Transform wardrobeTransform; // Transform шкафа
    public Transform fallenPoint; // Точка лежачего положения шкафа
    public float fallDuration = 1.2f; // Длительность падения

    [Header("Heavy Move")] // Блок движения
    public Collider heavyMoveCollider; // Коллайдер движения после падения

    [Header("Physics")] // Блок физики
    public Rigidbody wardrobeRigidbody; // Rigidbody шкафа

    private bool isQTEStarted = false; // QTE уже запущено
    private bool hasFallen = false; // Шкаф уже упал

    private void Start() // Старт сцены
    {
        if (heavyMoveCollider != null) heavyMoveCollider.enabled = false; // Выключаем движение шкафа на старте

        if (wardrobeRigidbody != null) wardrobeRigidbody.isKinematic = true; // Фиксируем шкаф до падения
    }

    public void Interact() // Игрок нажал E по шкафу
    {
        if (hasFallen) return; // Если шкаф уже упал — выходим

        if (isQTEStarted) return; // Если QTE уже идёт — выходим

        if (universalQTE == null) return; // Если QTE не назначен — выходим

        isQTEStarted = true; // Запоминаем запуск QTE

        universalQTE.onQTESuccess.RemoveListener(StartFall); // Убираем старую подписку

        universalQTE.onQTESuccess.AddListener(StartFall); // Подписываем падение на успех

        universalQTE.StartQTE(requiredPresses, qteTime, qteLabel); // Запускаем QTE
    }

    public void StartFall() // Запуск падения после QTE
    {
        if (hasFallen) return; // Если уже упал — выходим

        hasFallen = true; // Запоминаем падение

        StartCoroutine(FallRoutine()); // Запускаем плавное падение
    }

    private IEnumerator FallRoutine() // Корутина падения шкафа
    {
        if (wardrobeTransform == null) yield break; // Если шкаф не назначен — выходим

        if (fallenPoint == null) yield break; // Если точка падения не назначена — выходим

        Vector3 startPosition = wardrobeTransform.position; // Запоминаем стартовую позицию

        Quaternion startRotation = wardrobeTransform.rotation; // Запоминаем стартовый поворот

        Vector3 targetPosition = fallenPoint.position; // Берём позицию лежачей точки

        Quaternion targetRotation = fallenPoint.rotation; // Берём поворот лежачей точки

        float timer = 0f; // Таймер падения

        while (timer < fallDuration) // Пока падение не закончилось
        {
            timer += Time.deltaTime; // Увеличиваем таймер

            float t = timer / fallDuration; // Считаем прогресс

            wardrobeTransform.position = Vector3.Lerp(startPosition, targetPosition, t); // Плавно двигаем шкаф

            wardrobeTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t); // Плавно поворачиваем шкаф

            yield return null; // Ждём следующий кадр
        }

        wardrobeTransform.position = targetPosition; // Ставим точную финальную позицию

        wardrobeTransform.rotation = targetRotation; // Ставим точный финальный поворот

        if (wardrobeRigidbody != null) wardrobeRigidbody.isKinematic = false; // Включаем физику после падения

        if (heavyMoveCollider != null) heavyMoveCollider.enabled = true; // Включаем возможность двигать шкаф

        enabled = false; // Отключаем только этот скрипт
    }
}