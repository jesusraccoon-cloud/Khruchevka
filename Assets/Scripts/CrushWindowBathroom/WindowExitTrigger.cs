using UnityEngine;

public class WindowExitTrigger : MonoBehaviour
{
    public Transform exitPoint;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("1) Сработал trigger object: " + gameObject.name);
        Debug.Log("2) В триггер вошёл объект: " + other.name);

        if (exitPoint == null)
        {
            Debug.LogError("3) ExitPoint не назначен на объекте: " + gameObject.name);
            return;
        }

        Transform playerRoot = FindActualPlayerRoot(other.transform);

        if (playerRoot == null)
        {
            Debug.Log("4) Игрок не найден, перенос не выполняем");
            return;
        }

        Debug.Log("5) Найден объект игрока: " + playerRoot.name);
        Debug.Log("6) ExitPoint position: " + exitPoint.position);

        CharacterController cc = playerRoot.GetComponentInChildren<CharacterController>();

        if (cc != null)
        {
            cc.enabled = false;
            Debug.Log("7) CharacterController выключен");
        }

        Vector3 offset = exitPoint.position - other.transform.position;
        Debug.Log("8) Смещение игрока: " + offset);

        playerRoot.position += offset;
        playerRoot.rotation = exitPoint.rotation;

        Debug.Log("9) Игрок перенесён на другую сторону окна");

        if (cc != null)
        {
            cc.enabled = true;
            Debug.Log("10) CharacterController включен обратно");
        }
    }

    Transform FindActualPlayerRoot(Transform startTransform)
    {
        Transform current = startTransform;
        Transform fallbackTagged = null;

        while (current != null)
        {
            if (current.name == "Player")
            {
                return current;
            }

            if (fallbackTagged == null && current.CompareTag("Player"))
            {
                fallbackTagged = current;
            }

            current = current.parent;
        }

        return fallbackTagged;
    }
}