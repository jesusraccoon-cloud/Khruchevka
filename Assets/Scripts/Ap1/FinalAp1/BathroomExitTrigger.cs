using UnityEngine; // Подключаем Unity

public class BathroomExitTrigger : MonoBehaviour // Триггер выхода из ванной
{
    public ApartmentFinalSequence finalSequence; // Ссылка на финальный сценарий

    private void OnTriggerEnter(Collider other) // Когда кто-то вошёл в триггер
    {
        if (!other.CompareTag("Player")) return; // Если это не игрок — выходим

        if (finalSequence != null) // Если финал назначен
        {
            finalSequence.OnBathroomExitTrigger(); // Сообщаем финалу, что игрок вышел из ванной
        }

        gameObject.SetActive(false); // Выключаем триггер после срабатывания
    }
}