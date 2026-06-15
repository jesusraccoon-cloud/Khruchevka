using UnityEngine; // Подключаем Unity

public class MonsterDoorOpener : MonoBehaviour // Отвечает только за открытие дверей монстром
{
    public float doorCheckDistance = 1.8f; // Дистанция проверки двери

    public LayerMask doorLayers = ~0; // Слои дверей

    public bool drawDebugRay = true; // Рисовать ли debug-луч

    public void TryOpenDoorAhead() // Попробовать открыть дверь перед монстром
    {
        Vector3 checkCenter = transform.position + transform.forward * 1.2f + Vector3.up * 1.0f; // Центр сферы проверки

        if (drawDebugRay) Debug.DrawRay(transform.position + Vector3.up * 1.0f, transform.forward * doorCheckDistance, Color.red); // Рисуем луч вперёд

        Collider[] hits = Physics.OverlapSphere(checkCenter, 0.6f, doorLayers); // Ищем коллайдеры дверей рядом

        foreach (Collider hit in hits) // Перебираем найденные коллайдеры
        {
            UniversalDoor door = hit.GetComponentInParent<UniversalDoor>(); // Ищем UniversalDoor в родителях

            if (door == null) continue; // Если двери нет — пропускаем

            door.OpenDoorForMonster(); // Открываем дверь через метод двери

            return; // Выходим после первой найденной двери
        }
    }
}