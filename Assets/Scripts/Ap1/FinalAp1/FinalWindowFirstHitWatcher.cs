using UnityEngine; // Подключаем Unity

public class FinalWindowFirstHitWatcher : MonoBehaviour // Следит за первым ударом по финальному окну
{
    public BreakableWindow breakableWindow; // Финальное окно
    public ApartmentFinalSequence finalSequence; // Главный финальный режиссёр
    public float checkDistance = 3f; // Дистанция проверки удара

    private bool triggered = false; // Защита от повтора

    private void Update() // Каждый кадр
    {
        if (triggered) return; // Если уже сработало — выходим

        if (!Input.GetKeyDown(KeyCode.Mouse0)) return; // Ждём ЛКМ

        if (breakableWindow == null) return; // Если окно не назначено — выходим

        if (finalSequence == null) return; // Если финал не назначен — выходим

        Camera cam = Camera.main; // Берём главную камеру

        if (cam == null) return; // Если камеры нет — выходим

        Ray ray = new Ray(cam.transform.position, cam.transform.forward); // Луч из центра камеры

        if (Physics.Raycast(ray, out RaycastHit hit, checkDistance, ~0, QueryTriggerInteraction.Collide)) // Проверяем попадание
        {
            BreakableWindow hitWindow = hit.collider.GetComponentInParent<BreakableWindow>(); // Ищем окно

            if (hitWindow == breakableWindow) // Если ударили именно финальное окно
            {
                triggered = true; // Запоминаем

                finalSequence.OnFinalWindowFirstHit(); // Сообщаем ApartmentFinalSequence

                Debug.Log("Watcher: первый удар по финальному окну пойман"); // Проверка
            }
        }
    }
}