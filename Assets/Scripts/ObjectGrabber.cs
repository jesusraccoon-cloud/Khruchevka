using UnityEngine; 
// Подключаем библиотеку Unity, чтобы использовать все её функции (камера, физика, вектора и т.д.)

public class ObjectGrabber : MonoBehaviour
// Создаём класс ObjectGrabber, наследуемся от MonoBehaviour,
// чтобы Unity могла вызывать Update, FixedUpdate и другие методы
{
    public Camera playerCamera;
    // Ссылка на камеру игрока.
    // Через неё мы будем:
    // - пускать луч (Raycast)
    // - определять, куда смотрит игрок

    public Transform holdPoint;
    // Точка перед камерой, где будет удерживаться объект
    // (мы её сделали как отдельный объект внутри камеры)

    public float grabDistance = 3f;
    // Максимальная дистанция, с которой можно "схватить" объект

    public float moveSpeed = 10f;
    // Скорость, с которой объект притягивается к holdPoint

    public LayerMask grabbableLayer;
    // Слой объектов, которые можно брать (например Grabbable)
    // Это нужно, чтобы не хватать стены и пол

    private Rigidbody grabbedRigidbody;
    // Здесь хранится Rigidbody объекта, который сейчас держит игрок

    private bool oldUseGravity;
    // Запоминаем, была ли включена гравитация до захвата

    private RigidbodyConstraints oldConstraints;
    // Запоминаем ограничения Rigidbody (например FreezeRotation),
    // чтобы потом вернуть всё как было

    void Update()
    // Update вызывается каждый кадр (каждый FPS)
    {
        if (Input.GetKeyDown(KeyCode.E))
        // Проверяем: нажал ли игрок кнопку E (один раз, не удержание)
        {
            if (grabbedRigidbody == null)
            // Если сейчас ничего не держим
            {
                TryGrabObject();
                // Пытаемся взять объект
            }
            else
            {
                ReleaseObject();
                // Если уже держим — отпускаем
            }
        }
    }

    void FixedUpdate()
    // FixedUpdate вызывается с фиксированным шагом и используется для физики
    {
        if (grabbedRigidbody != null)
        // Если объект сейчас захвачен
        {
            HoldObject();
            // Держим его перед камерой
        }
    }

    void TryGrabObject()
    // Метод, который пытается взять объект
    {
        RaycastHit hit;
        // Переменная, куда запишется информация о попадании луча

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        // Создаём луч:
        // старт — позиция камеры
        // направление — куда смотрит камера (вперёд)

        if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
        // Пускаем луч:
        // - ray — сам луч
        // - out hit — куда сохранить результат
        // - grabDistance — максимальная длина луча
        // - grabbableLayer — фильтр по слоям
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            // Пытаемся получить Rigidbody у объекта, в который попали

            if (rb == null)
            // Если у объекта нет Rigidbody
            {
                return;
                // Выходим — брать его нельзя
            }

            grabbedRigidbody = rb;
            // Сохраняем Rigidbody — теперь этот объект "в руках"

            oldUseGravity = grabbedRigidbody.useGravity;
            // Запоминаем текущее состояние гравитации

            oldConstraints = grabbedRigidbody.constraints;
            // Запоминаем текущие ограничения Rigidbody

            grabbedRigidbody.useGravity = false;
            // Выключаем гравитацию, чтобы объект не падал вниз

            grabbedRigidbody.velocity = Vector3.zero;
            // Обнуляем скорость, чтобы убрать рывки

            grabbedRigidbody.angularVelocity = Vector3.zero;
            // Обнуляем вращение, чтобы объект не крутился

            grabbedRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            // Замораживаем вращение, чтобы объект не болтался
        }
    }

    void HoldObject()
    // Метод удержания объекта перед камерой
    {
        Vector3 newPosition = Vector3.Lerp(
            grabbedRigidbody.position,
            holdPoint.position,
            moveSpeed * Time.fixedDeltaTime
        );
        // Плавно двигаем объект к holdPoint:
        // - grabbedRigidbody.position — текущая позиция
        // - holdPoint.position — куда хотим переместить
        // - moveSpeed * deltaTime — скорость перемещения
        //
        // Lerp делает движение плавным, а не резким

        grabbedRigidbody.MovePosition(newPosition);
        // Перемещаем Rigidbody в новую позицию через физику
        // (важно: не transform.position, а MovePosition)
    }

    void ReleaseObject()
    // Метод отпускания объекта
    {
        if (grabbedRigidbody == null)
        // Если ничего не держим
        {
            return;
            // Выходим
        }

        grabbedRigidbody.useGravity = oldUseGravity;
        // Возвращаем гравитацию как было

        grabbedRigidbody.constraints = oldConstraints;
        // Возвращаем ограничения (например вращение)

        grabbedRigidbody.velocity = Vector3.zero;
        // Сбрасываем скорость, чтобы объект не улетел

        grabbedRigidbody.angularVelocity = Vector3.zero;
        // Сбрасываем вращение

        grabbedRigidbody = null;
        // Убираем ссылку — теперь объект не считается захваченным
    }
}