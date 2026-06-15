using UnityEngine; // Подключаем Unity

public class MonsterHearing : MonoBehaviour // Отвечает только за реакцию монстра на шум
{
    public float noiseArriveDistance = 1.2f; // Дистанция прибытия к шуму

    public float noiseWaitTime = 4f; // Время ожидания на месте шума

    public float suspiciousLookTime = 2f; // Время осмотра при шуме 4

    public float normalNoiseSpeed = 2.5f; // Скорость реакции на шум 5-6

    public float loudNoiseSpeed = 4.5f; // Скорость реакции на шум 7-10

    private MonsterMovement movement; // Ссылка на движение

    private MonsterPatrol patrol; // Ссылка на патруль

    private Vector3 noisePosition; // Последняя позиция шума

    private float timer; // Универсальный таймер

    private bool isLookingAround; // Монстр сейчас осматривается

    private bool isInvestigating; // Монстр сейчас идёт к шуму

    private bool isWaitingAtNoise; // Монстр стоит на месте шума

    public bool IsBusy => isLookingAround || isInvestigating; // Занят ли слуховой системой

    private void Awake() // Вызывается при запуске объекта
    {
        movement = GetComponent<MonsterMovement>(); // Получаем MonsterMovement

        patrol = GetComponent<MonsterPatrol>(); // Получаем MonsterPatrol
    }

    public void ReactToNoise(Vector3 newNoisePosition, int noisePower) // Запустить реакцию на шум
    {
        noisePower = Mathf.Clamp(noisePower, 1, 10); // Ограничиваем силу шума от 1 до 10

        if (noisePower <= 3) return; // Шум 0-3 игнорируется

        if (noisePower == 4) // Если шум равен 4
        {
            StartLookAround(newNoisePosition); // Запускаем осмотр

            return; // Выходим
        }

        if (noisePower >= 5 && noisePower <= 6) // Если шум 5-6
        {
            StartInvestigation(newNoisePosition, normalNoiseSpeed); // Идём к шуму обычной скоростью

            return; // Выходим
        }

        if (noisePower >= 7) // Если шум 7-10
        {
            StartInvestigation(newNoisePosition, loudNoiseSpeed); // Идём к шуму быстро

            return; // Выходим
        }
    }

    public void Tick() // Обновление слуховой логики
    {
        if (isLookingAround) TickLookAround(); // Если осматриваемся — обновляем осмотр

        if (isInvestigating) TickInvestigation(); // Если идём к шуму — обновляем расследование
    }

    public void StopHearingLogic() // Остановить реакцию на шум
    {
        isLookingAround = false; // Выключаем осмотр

        isInvestigating = false; // Выключаем движение к шуму

        isWaitingAtNoise = false; // Выключаем ожидание

        timer = 0f; // Сбрасываем таймер

        if (movement != null) movement.RestoreDefaultSpeed(); // Возвращаем стандартную скорость
    }

    private void StartLookAround(Vector3 newNoisePosition) // Начать осмотр
    {
        noisePosition = newNoisePosition; // Запоминаем позицию шума

        isLookingAround = true; // Включаем осмотр

        isInvestigating = false; // Выключаем расследование

        isWaitingAtNoise = false; // Выключаем ожидание

        timer = 0f; // Сбрасываем таймер

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        if (movement != null) movement.Stop(); // Останавливаем монстра

        Vector3 directionToNoise = noisePosition - transform.position; // Считаем направление к шуму

        directionToNoise.y = 0f; // Убираем вертикаль

        if (directionToNoise.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(directionToNoise.normalized); // Поворачиваем монстра к шуму
    }

    private void StartInvestigation(Vector3 newNoisePosition, float speed) // Начать движение к шуму
    {
        noisePosition = newNoisePosition; // Запоминаем позицию шума

        isLookingAround = false; // Выключаем осмотр

        isInvestigating = true; // Включаем расследование

        isWaitingAtNoise = false; // Пока не ждём

        timer = 0f; // Сбрасываем таймер

        if (patrol != null) patrol.isPatrolActive = false; // Отключаем патруль

        if (movement != null) movement.SetSpeed(speed); // Ставим скорость реакции на шум

        if (movement != null) movement.MoveTo(noisePosition); // Отправляем монстра к шуму
    }

    private void TickLookAround() // Обновить осмотр
    {
        timer += Time.deltaTime; // Увеличиваем таймер

        if (timer < suspiciousLookTime) return; // Если время ещё не вышло — ждём

        isLookingAround = false; // Выключаем осмотр

        timer = 0f; // Сбрасываем таймер

        if (patrol != null) patrol.StartPatrol(); // Возвращаем патруль
    }

    private void TickInvestigation() // Обновить движение к шуму
    {
        if (movement == null) return; // Если движения нет — выходим

        if (!isWaitingAtNoise) // Если монстр ещё идёт
        {
            if (!movement.HasArrived(noiseArriveDistance)) return; // Если не дошёл — ждём

            isWaitingAtNoise = true; // Включаем ожидание

            timer = 0f; // Сбрасываем таймер

            movement.Stop(); // Останавливаем монстра

            return; // Выходим
        }

        timer += Time.deltaTime; // Увеличиваем таймер ожидания

        if (timer < noiseWaitTime) return; // Если ждать ещё рано — выходим

        StopHearingLogic(); // Останавливаем слуховую логику

        if (patrol != null) patrol.StartPatrol(); // Возвращаем патруль
    }
}