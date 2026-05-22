using UnityEngine; // Подключаем основные классы Unity (Transform, MonoBehaviour, Debug и т.д.)

public class NoiseSource : MonoBehaviour // Создаём скрипт источника шума
{
    public float noiseRadius = 8f; 
    // Радиус шума (ПОКА не используется, но позже можно сделать:
    // если монстр дальше радиуса — он не услышит шум)

    public void MakeNoise() 
    // Метод создания шума
    // Его можно вызвать из любого другого скрипта:
    // например при падении предмета, ломании окна, движении шкафа
    {
        MonsterAI[] monsters = FindObjectsOfType<MonsterAI>();
        // Ищем ВСЕХ монстров на сцене,
        // у которых есть скрипт MonsterAI

        foreach (MonsterAI monster in monsters)
        // Запускаем цикл:
        // перебираем каждого найденного монстра по очереди
        {
            monster.HearNoise(transform.position);
            // Говорим монстру:
            // "услышь шум в позиции этого объекта"

            // transform.position =
            // позиция объекта, на котором висит NoiseSource
        }

        Debug.Log("Шум создан: " + gameObject.name);
        // Выводим сообщение в Console,
        // чтобы видеть что шум реально вызвался

        // gameObject.name =
        // имя объекта, который создал шум
    }
}