using UnityEngine; // Подключаем Unity-классы

public class TumblerPanel : MonoBehaviour, IInteractable, ILookInteractable // Панель можно активировать и она реагирует на наведение
{
    public Camera playerCamera; // Камера игрока
    public TumblerSwitch[] tumblers; // Все тумблеры панели

    private TumblerSwitch currentTumbler; // Тумблер, который сейчас выбран

    private void Start() // Запускается один раз
    {
        if (playerCamera == null) // Если камера не назначена
        {
            playerCamera = Camera.main; // Берём MainCamera
        }

        if (tumblers == null || tumblers.Length == 0) // Если массив пустой
        {
            tumblers = GetComponentsInChildren<TumblerSwitch>(); // Ищем тумблеры внутри панели
        }
    }

    public void LookUpdate() // Вызывается, пока игрок смотрит на InteractZone панели
    {
        TumblerSwitch newTumbler = GetClosestToScreenCenter(playerCamera); // Ищем ближайший тумблер к центру экрана

        if (newTumbler == currentTumbler) return; // Если выбран тот же самый — ничего не меняем

        if (currentTumbler != null) // Если раньше был выбранный тумблер
        {
            currentTumbler.SetHighlight(false); // Убираем подсветку
        }

        currentTumbler = newTumbler; // Запоминаем новый выбранный тумблер

        if (currentTumbler != null) // Если новый тумблер найден
        {
            currentTumbler.SetHighlight(true); // Подсвечиваем его
        }
    }

    public void LookExit() // Вызывается, когда игрок перестал смотреть на панель
    {
        if (currentTumbler != null) // Если был выбранный тумблер
        {
            currentTumbler.SetHighlight(false); // Убираем подсветку
        }

        currentTumbler = null; // Очищаем выбранный тумблер
    }

    public void Interact() // Вызывается при нажатии E
    {
        if (currentTumbler == null) // Если тумблер ещё не выбран
        {
            currentTumbler = GetClosestToScreenCenter(playerCamera); // Пробуем выбрать ближайший
        }

        if (currentTumbler != null) // Если тумблер найден
        {
            currentTumbler.Toggle(); // Переключаем именно выбранный тумблер
        }
    }

    private TumblerSwitch GetClosestToScreenCenter(Camera camera) // Поиск ближайшего тумблера к центру экрана
    {
        if (camera == null) return null; // Если камеры нет — возвращаем пусто
        if (tumblers == null || tumblers.Length == 0) return null; // Если тумблеров нет — возвращаем пусто

        TumblerSwitch closestTumbler = null; // Лучший найденный тумблер
        float closestDistance = float.MaxValue; // Самая маленькая дистанция до центра
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f); // Центр экрана

        for (int i = 0; i < tumblers.Length; i++) // Перебираем все тумблеры
        {
            if (tumblers[i] == null) continue; // Если ячейка пустая — пропускаем

            Vector3 screenPoint = camera.WorldToScreenPoint(tumblers[i].transform.position); // Переводим позицию тумблера в экран

            if (screenPoint.z < 0f) continue; // Если тумблер за камерой — пропускаем

            Vector2 tumblerScreenPosition = new Vector2(screenPoint.x, screenPoint.y); // Экранная позиция тумблера

            float distance = Vector2.Distance(screenCenter, tumblerScreenPosition); // Расстояние от тумблера до центра экрана

            if (distance < closestDistance) // Если этот тумблер ближе
            {
                closestDistance = distance; // Запоминаем дистанцию
                closestTumbler = tumblers[i]; // Запоминаем тумблер
            }
        }

        return closestTumbler; // Возвращаем выбранный тумблер
    }
}