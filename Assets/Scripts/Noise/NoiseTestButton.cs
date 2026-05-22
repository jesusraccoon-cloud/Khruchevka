using UnityEngine;
// Подключаем основные классы Unity:
// MonoBehaviour, Input, KeyCode и т.д.

public class NoiseTestButton : MonoBehaviour
// Создаём скрипт тестовой кнопки шума
// Его можно повесить на любой объект сцены
{
    public NoiseSource noiseSource;
    // Ссылка на объект/скрипт NoiseSource
    // Через него мы будем создавать шум

    void Update()
    // Метод Update вызывается каждый кадр
    {
        if (Input.GetKeyDown(KeyCode.N))
        // Проверяем:
        // нажал ли игрок кнопку N ИМЕННО в этот кадр

        // GetKeyDown =
        // срабатывает один раз в момент нажатия
        {
            noiseSource.MakeNoise();
            // Вызываем метод MakeNoise() у NoiseSource
            // То есть:
            // создаём шум в позиции объекта
        }
    }
}