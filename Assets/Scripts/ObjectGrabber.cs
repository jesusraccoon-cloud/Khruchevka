using UnityEngine;
// Подключаем библиотеку Unity, чтобы использовать камеру, физику, Rigidbody, Vector3 и другие функции

public class ObjectGrabber : MonoBehaviour
// Создаём класс ObjectGrabber, который будет висеть на игроке
{
    public Camera playerCamera;
    // Камера игрока, из которой мы пускаем Raycast

    public Transform holdPoint;
    // Точка перед камерой, где будет держаться лёгкий предмет

    public float grabDistance = 3f;
    // Максимальная дистанция взаимодействия

    public float moveSpeed = 10f;
    // Скорость движения лёгкого предмета к holdPoint

    public float heavyMoveSpeed = 4f;
    // Скорость движения тяжёлого объекта

    public float heavyObjectDistance = 1.5f;
    // Дистанция, на которой тяжёлый объект будет держаться перед игроком

    public LayerMask grabbableLayer;
    // Слой лёгких объектов, которые можно поднимать

    public LayerMask movableHeavyLayer;
    // Слой тяжёлых объектов, которые можно двигать

    private Rigidbody grabbedRigidbody;
    // Rigidbody лёгкого объекта, который сейчас держит игрок

    private Rigidbody movingHeavyRigidbody;
    // Rigidbody тяжёлого объекта, который сейчас двигает игрок

    private bool oldUseGravity;
    // Запоминаем, была ли включена гравитация у лёгкого объекта

    private RigidbodyConstraints oldConstraints;
    // Запоминаем старые ограничения лёгкого объекта

    private RigidbodyConstraints oldHeavyConstraints;
    // Запоминаем старые ограничения тяжёлого объекта

    void Update()
    // Update вызывается каждый кадр
    {
        if (Input.GetKeyDown(KeyCode.E))
        // Проверяем, нажал ли игрок кнопку E
        {
            if (grabbedRigidbody == null && movingHeavyRigidbody == null)
            // Если игрок сейчас ничего не держит и ничего не двигает
            {
                TryInteract();
                // Пытаемся найти объект перед игроком
            }
            else
            {
                ReleaseLightObject();
                // Отпускаем лёгкий объект, если он был в руках

                StopMovingHeavyObject();
                // Перестаём двигать тяжёлый объект, если он двигался
            }
        }
    }

    void FixedUpdate()
    // FixedUpdate используется для физики
    {
        if (grabbedRigidbody != null)
        // Если игрок держит лёгкий объект
        {
            HoldLightObject();
            // Держим лёгкий объект перед камерой
        }

        if (movingHeavyRigidbody != null)
        // Если игрок двигает тяжёлый объект
        {
            MoveHeavyObject();
            // Двигаем тяжёлый объект перед игроком
        }
    }

    void TryInteract()
    // Метод, который проверяет, на что смотрит игрок
    {
        RaycastHit hit;
        // Переменная, куда попадёт информация о столкновении луча

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        // Создаём луч из камеры вперёд

        int interactMask = grabbableLayer.value | movableHeavyLayer.value;
        // Объединяем два слоя: лёгкие предметы и тяжёлые предметы

        if (Physics.Raycast(ray, out hit, grabDistance, interactMask))
        // Пускаем луч и проверяем, попал ли он в нужный слой
        {
            if (IsInLayerMask(hit.collider.gameObject.layer, movableHeavyLayer))
            // Если объект находится на слое тяжёлых двигаемых объектов
            {
                TryStartMovingHeavyObject(hit);
                // Пытаемся начать двигать тяжёлый объект

                return;
                // Выходим, чтобы не пытаться ещё и поднять его
            }

            if (IsInLayerMask(hit.collider.gameObject.layer, grabbableLayer))
            // Если объект находится на слое лёгких подбираемых объектов
            {
                TryGrabLightObject(hit);
                // Пытаемся поднять лёгкий объект

                return;
                // Выходим из метода
            }
        }
    }

    void TryGrabLightObject(RaycastHit hit)
    // Метод попытки поднять лёгкий объект
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        // Берём Rigidbody у объекта, в который попал луч

        if (rb == null)
        // Если Rigidbody нет
        {
            return;
            // Выходим, потому что объект нельзя физически поднять
        }

        grabbedRigidbody = rb;
        // Запоминаем Rigidbody как текущий поднятый объект

        oldUseGravity = grabbedRigidbody.useGravity;
        // Запоминаем старое состояние гравитации

        oldConstraints = grabbedRigidbody.constraints;
        // Запоминаем старые ограничения Rigidbody

        grabbedRigidbody.useGravity = false;
        // Отключаем гравитацию, чтобы предмет не падал

        grabbedRigidbody.velocity = Vector3.zero;
        // Обнуляем скорость, чтобы не было рывка

        grabbedRigidbody.angularVelocity = Vector3.zero;
        // Обнуляем вращение, чтобы предмет не крутился

        grabbedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        // Замораживаем вращение предмета
    }

    void HoldLightObject()
    // Метод удержания лёгкого объекта
    {
        Vector3 newPosition = Vector3.Lerp(
            grabbedRigidbody.position,
            holdPoint.position,
            moveSpeed * Time.fixedDeltaTime
        );
        // Плавно считаем новую позицию между текущей позицией объекта и holdPoint

        grabbedRigidbody.MovePosition(newPosition);
        // Двигаем Rigidbody через физику
    }

    void ReleaseLightObject()
    // Метод отпускания лёгкого объекта
    {
        if (grabbedRigidbody == null)
        // Если лёгкий объект не удерживается
        {
            return;
            // Выходим
        }

        grabbedRigidbody.useGravity = oldUseGravity;
        // Возвращаем гравитацию как было

        grabbedRigidbody.constraints = oldConstraints;
        // Возвращаем старые ограничения Rigidbody

        grabbedRigidbody.velocity = Vector3.zero;
        // Обнуляем скорость

        grabbedRigidbody.angularVelocity = Vector3.zero;
        // Обнуляем вращение

        grabbedRigidbody = null;
        // Очищаем ссылку на объект
    }

    void TryStartMovingHeavyObject(RaycastHit hit)
    // Метод попытки начать двигать тяжёлый объект
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        // Получаем Rigidbody тяжёлого объекта

        if (rb == null)
        // Если Rigidbody нет
        {
            return;
            // Выходим, потому что двигать через физику нечего
        }

        movingHeavyRigidbody = rb;
        // Запоминаем Rigidbody тяжёлого объекта

        oldHeavyConstraints = movingHeavyRigidbody.constraints;
        // Запоминаем старые ограничения тяжёлого объекта

        movingHeavyRigidbody.velocity = Vector3.zero;
        // Обнуляем скорость тяжёлого объекта

        movingHeavyRigidbody.angularVelocity = Vector3.zero;
        // Обнуляем вращение тяжёлого объекта

        movingHeavyRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        // Замораживаем вращение, чтобы шкаф/тумба не заваливались
    }

    void MoveHeavyObject()
// Метод движения тяжёлого объекта
{
    float input = Input.GetAxisRaw("Vertical");
    // Читаем движение вперёд/назад:
    // W = 1
    // S = -1
    // ничего не нажато = 0

    if (input == 0)
    // Если игрок не нажимает W или S
    {
        return;
        // Ничего не двигаем
    }

    Vector3 forward = playerCamera.transform.forward;
    // Берём направление взгляда камеры

    forward.y = 0f;
    // Убираем движение вверх/вниз

    forward.Normalize();
    // Нормализуем направление

    Vector3 newPosition = movingHeavyRigidbody.position + forward * input * heavyMoveSpeed * Time.fixedDeltaTime;
    // Считаем новую позицию:
    // W двигает вперёд
    // S двигает назад

    movingHeavyRigidbody.MovePosition(newPosition);
    // Двигаем Rigidbody через физику
}

    void StopMovingHeavyObject()
    // Метод остановки движения тяжёлого объекта
    {
        if (movingHeavyRigidbody == null)
        // Если тяжёлый объект не двигается
        {
            return;
            // Выходим
        }

        movingHeavyRigidbody.constraints = oldHeavyConstraints;
        // Возвращаем старые ограничения Rigidbody

        movingHeavyRigidbody.velocity = Vector3.zero;
        // Обнуляем скорость

        movingHeavyRigidbody.angularVelocity = Vector3.zero;
        // Обнуляем вращение

        movingHeavyRigidbody = null;
        // Очищаем ссылку на тяжёлый объект
    }

    bool IsInLayerMask(int layer, LayerMask layerMask)
    // Метод проверки: находится ли объект на нужном LayerMask
    {
        return (layerMask.value & (1 << layer)) != 0;
        // Возвращаем true, если слой объекта есть внутри LayerMask
    }
}