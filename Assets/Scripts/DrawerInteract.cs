using UnityEngine; // Подключаем базовые функции Unity

public class DrawerInteract : MonoBehaviour, IInteractable // Скрипт для выдвижения ящика через PlayerInteractor
{
    public enum SlideDirection // Создаем список вариантов направления
    {
        Forward, // Вперед по локальной оси Z
        Back, // Назад по локальной оси Z
        Right, // Вправо по локальной оси X
        Left // Влево по локальной оси X
    }

    [Header("Drawer Movement Settings")] // Заголовок блока настроек движения
    public float slideDistance = 0.4f; // Насколько далеко выдвигать ящик

    public float moveSpeed = 3f; // Скорость движения

    public SlideDirection slideDirection = SlideDirection.Back; // Направление выезда

    private Vector3 closedLocalPosition; // Позиция закрытого ящика

    private Vector3 openLocalPosition; // Позиция открытого ящика

    private Vector3 targetLocalPosition; // Целевая позиция

    private bool isOpen = false; // Открыт ли ящик

    void Start() // Выполняется один раз при старте
    {
        closedLocalPosition = transform.localPosition; // Запоминаем текущую позицию как закрытую

        openLocalPosition = closedLocalPosition + GetSlideVector() * slideDistance; // Считаем позицию открытия

        targetLocalPosition = closedLocalPosition; // В начале цель = закрытая позиция
    }

    void Update() // Выполняется каждый кадр
    {
        transform.localPosition = Vector3.Lerp( // Плавно двигаем ящик
            transform.localPosition, // Из текущей позиции
            targetLocalPosition, // В нужную позицию
            Time.deltaTime * moveSpeed // С учетом времени кадра и скорости
        );
    }

    public void Interact() // Вызывается PlayerInteractor при нажатии E
    {
        ToggleDrawer(); // Открываем или закрываем ящик
    }

    void ToggleDrawer() // Метод переключения состояния ящика
    {
        isOpen = !isOpen; // Меняем состояние на противоположное

        targetLocalPosition = isOpen ? openLocalPosition : closedLocalPosition; // Выбираем нужную позицию
    }

    Vector3 GetSlideVector() // Метод возвращает направление движения
    {
        switch (slideDirection) // Проверяем выбранное направление
        {
            case SlideDirection.Forward: // Если выбрано Forward
                return Vector3.forward; // Возвращаем вперед

            case SlideDirection.Back: // Если выбрано Back
                return Vector3.back; // Возвращаем назад

            case SlideDirection.Right: // Если выбрано Right
                return Vector3.right; // Возвращаем вправо

            case SlideDirection.Left: // Если выбрано Left
                return Vector3.left; // Возвращаем влево

            default: // Если вдруг что-то пошло не так
                return Vector3.back; // По умолчанию назад
        }
    }
}