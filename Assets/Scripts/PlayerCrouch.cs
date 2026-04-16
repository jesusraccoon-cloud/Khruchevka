using UnityEngine;

public class PlayerCrouch : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller; // CharacterController игрока
    public Transform cameraRoot; // PlayerCameraRoot

    [Header("Crouch Settings")]
    public KeyCode crouchKey = KeyCode.LeftControl; // Кнопка приседа
    public float crouchHeight = 1.0f; // Высота капсулы в приседе
    public float crouchCameraOffset = 0.45f; // Насколько опускать камеру вниз
    public float crouchSpeed = 10f; // Скорость перехода

    private float standingHeight; // Высота стоя
    private Vector3 standingCenter; // Центр CharacterController стоя
    private Vector3 crouchingCenter; // Центр CharacterController в приседе

    private Vector3 standingCameraLocalPos; // Позиция камеры стоя
    private Vector3 crouchingCameraLocalPos; // Позиция камеры в приседе

    private void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>(); // Берем CharacterController с этого же объекта
        }

        if (cameraRoot == null)
        {
            Debug.LogError("PlayerCrouch: не назначен cameraRoot.");
            return;
        }

        standingHeight = controller.height; // Запоминаем текущую высоту как высоту стоя
        standingCenter = controller.center; // Запоминаем текущий центр как центр стоя

        // Считаем новый центр капсулы для приседа
        crouchingCenter = new Vector3(
            standingCenter.x,
            crouchHeight * 0.5f,
            standingCenter.z
        );

        standingCameraLocalPos = cameraRoot.localPosition; // Запоминаем текущую позицию камеры как позицию стоя
        crouchingCameraLocalPos = standingCameraLocalPos - new Vector3(0f, crouchCameraOffset, 0f); // Опускаем камеру вниз для приседа
    }

    private void Update()
    {
        if (controller == null || cameraRoot == null) return;

        bool wantsToCrouch = Input.GetKey(crouchKey); // Пока держим Ctrl — сидим

        float targetHeight = wantsToCrouch ? crouchHeight : standingHeight; // Целевая высота
        Vector3 targetCenter = wantsToCrouch ? crouchingCenter : standingCenter; // Целевой центр
        Vector3 targetCameraPos = wantsToCrouch ? crouchingCameraLocalPos : standingCameraLocalPos; // Целевая позиция камеры

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchSpeed); // Плавно меняем высоту
        controller.center = Vector3.Lerp(controller.center, targetCenter, Time.deltaTime * crouchSpeed); // Плавно меняем центр
        cameraRoot.localPosition = Vector3.Lerp(cameraRoot.localPosition, targetCameraPos, Time.deltaTime * crouchSpeed); // Плавно двигаем камеру
    }
}