using UnityEngine; // Подключаем Unity-классы
using System.Collections; // Подключаем корутины

public class BreakableWindow : MonoBehaviour, IHitInteractable // Окно, которое можно разбить ударом
{
    public GameObject windowIntact; // Целая версия окна

    public GameObject windowBroken; // Разбитая версия окна

    public int hitsToBreak = 3; // Сколько ударов нужно для разбития

    public float hitDelay = 0.3f; // Задержка между ударами

    [Header("Noise")] // Заголовок шума
    public NoiseEmitter noiseEmitter; // Источник шума для окна

    [Range(1, 10)] public int hitNoisePower = 7; // Шум обычного удара молотком

    [Range(1, 10)] public int breakNoisePower = 8; // Шум полного разбития окна

    private int currentHits = 0; // Текущее количество ударов

    private bool isBroken = false; // Разбито ли окно

    private bool canHit = true; // Можно ли сейчас ударить

    public bool IsBroken // Публичное свойство состояния окна
    {
        get { return isBroken; } // Возвращаем true, если окно разбито
    }

    void Start() // Запуск сцены
    {
        if (windowIntact != null) // Если целое окно назначено
        {
            windowIntact.SetActive(true); // Показываем целое окно
        }

        if (windowBroken != null) // Если разбитое окно назначено
        {
            windowBroken.SetActive(false); // Прячем разбитое окно
        }

        if (noiseEmitter == null) // Если NoiseEmitter не назначили вручную
        {
            noiseEmitter = GetComponent<NoiseEmitter>(); // Пробуем найти NoiseEmitter на этом же объекте
        }
    }

    public void Hit() // Метод удара по окну
    {
        if (isBroken) return; // Если окно уже разбито — выходим

        if (!canHit) return; // Если задержка между ударами ещё не прошла — выходим

        currentHits++; // Добавляем один удар

        if (noiseEmitter != null) // Если источник шума назначен
        {
            noiseEmitter.EmitNoise(hitNoisePower); // Создаём шум удара молотком
        }

        if (currentHits >= hitsToBreak) // Если ударов достаточно
        {
            Break(); // Разбиваем окно
        }
        else // Если ударов пока мало
        {
            StartCoroutine(HitCooldown()); // Запускаем задержку между ударами
        }
    }

    IEnumerator HitCooldown() // Корутина задержки между ударами
    {
        canHit = false; // Запрещаем следующий удар

        yield return new WaitForSeconds(hitDelay); // Ждём указанное время

        canHit = true; // Снова разрешаем удар
    }

    void Break() // Метод полного разбития окна
    {
        if (isBroken) return; // Если уже разбито — выходим

        isBroken = true; // Помечаем окно разбитым

        if (windowIntact != null) // Если целое окно назначено
        {
            windowIntact.SetActive(false); // Выключаем целое окно
        }

        if (windowBroken != null) // Если разбитое окно назначено
        {
            windowBroken.SetActive(true); // Включаем разбитое окно
        }

        if (noiseEmitter != null) // Если источник шума назначен
        {
            noiseEmitter.EmitNoise(breakNoisePower); // Создаём сильный шум разбития
        }
    }
}