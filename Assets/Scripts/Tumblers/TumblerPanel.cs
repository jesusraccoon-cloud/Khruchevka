using UnityEngine;

public class TumblerPanel : MonoBehaviour
{
    public TumblerSwitch[] tumblers;
    // Все тумблеры этой панели

    public TumblerSwitch GetClosestToScreenCenter(Camera cam)
    {
        // Если камера не задана — вернуть нечего
        if (cam == null) return null;

        // Если массив пустой — вернуть нечего
        if (tumblers == null || tumblers.Length == 0) return null;

        TumblerSwitch closest = null;
        // Здесь будем хранить лучший найденный тумблер

        float bestScore = Mathf.Infinity;
        // Чем меньше число — тем лучше кандидат

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        // Находим центр экрана

        for (int i = 0; i < tumblers.Length; i++)
        {
            TumblerSwitch current = tumblers[i];
            // Берем очередной тумблер

            if (current == null) continue;
            // Если элемент массива пустой — пропускаем

            Vector3 screenPos = cam.WorldToScreenPoint(current.transform.position);
            // Переводим мировую позицию тумблера в экранные координаты

            if (screenPos.z <= 0f) continue;
            // Если тумблер позади камеры — пропускаем

            float distanceToCenter = Vector2.Distance(
                new Vector2(screenPos.x, screenPos.y),
                screenCenter
            );
            // Считаем расстояние от тумблера до центра экрана

            if (distanceToCenter < bestScore)
            {
                bestScore = distanceToCenter;
                closest = current;
            }
            // Если этот тумблер ближе к центру — запоминаем его
        }

        return closest;
        // Возвращаем лучший вариант
    }
}