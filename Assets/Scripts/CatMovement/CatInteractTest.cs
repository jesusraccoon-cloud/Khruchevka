using UnityEngine; // Подключаем библиотеку Unity

public class CatInteractTest : MonoBehaviour // Скрипт интеракции с котом через взгляд и кнопку E
{
    public CatMoveToPoint catMoveScript; // Ссылка на скрипт движения кота
    public Camera playerCamera; // Камера игрока
    public float interactDistance = 3f; // Максимальная дистанция интеракции

    void Update() // Unity вызывает этот метод каждый кадр
    {
        if (Input.GetKeyDown(KeyCode.E)) // Если игрок нажал клавишу E
        {
            TryInteract(); // Пытаемся выполнить интеракцию
        }
    }

    void TryInteract() // Метод проверки, смотрит ли игрок на кота
    {
        if (catMoveScript == null) // Если ссылка на скрипт движения кота не назначена
        {
            Debug.Log("CatMoveScript не назначен"); // Пишем ошибку в консоль
            return; // Выходим из метода
        }

        if (playerCamera == null) // Если камера игрока не назначена
        {
            Debug.Log("Player Camera не назначена"); // Пишем ошибку в консоль
            return; // Выходим из метода
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward); // Создаём луч из позиции камеры вперёд
        RaycastHit hit; // Переменная для хранения информации о попадании луча

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactDistance, Color.red, 1f); 
        // Рисуем красный луч в сцене для отладки

        if (Physics.Raycast(ray, out hit, interactDistance)) // Проверяем, попал ли луч в какой-то коллайдер в пределах дистанции
        {
            CatMoveToPoint hitCat = hit.collider.GetComponentInParent<CatMoveToPoint>(); 
            // Пытаемся найти скрипт кота на объекте, в который попали, или у его родителей

            if (hitCat != null && hitCat == catMoveScript) 
            // Если луч попал в объект, который относится именно к нашему коту
            {
                Debug.Log("Игрок взаимодействовал с котом"); // Пишем лог успешной интеракции
                catMoveScript.StartMoving(); // Запускаем сценарий кота
                return; // Выходим из метода
            }

            Debug.Log("Луч попал не в кота, а в: " + hit.collider.name); // Пишем лог, если попали в другой объект
            return; // Выходим
        }

        Debug.Log("Луч ни во что не попал"); // Пишем лог, если луч вообще ничего не задел
    }
}