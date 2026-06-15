using UnityEngine; // Подключаем Unity

public class MonsterAttack : MonoBehaviour // Отвечает только за атаку монстра
{
    public Transform player; // Ссылка на игрока

    public float attackDistance = 1.2f; // Дистанция атаки

    public float attackDelay = 1.2f; // Задержка перед Game Over

    public Animator animator; // Animator монстра

    public GameOverManager gameOverManager; // Менеджер Game Over

    public StarterAssets.FirstPersonController playerController; // Контроллер игрока

    private bool isAttacking = false; // Идёт ли атака сейчас

    public bool IsAttacking => isAttacking; // Публичное чтение состояния атаки

    private void Reset() // Автозаполнение при добавлении компонента
    {
        animator = GetComponent<Animator>(); // Ищем Animator на объекте монстра
    }

    public bool IsPlayerInAttackDistance() // Проверить, можно ли атаковать игрока
    {
        if (player == null) return false; // Если игрока нет — атаковать нельзя

        float distance = Vector3.Distance(transform.position, player.position); // Считаем дистанцию до игрока

        return distance <= attackDistance; // Возвращаем true, если игрок достаточно близко
    }

    public void StartAttack() // Запустить атаку
    {
        if (isAttacking) return; // Если атака уже идёт — выходим

        isAttacking = true; // Помечаем атаку активной

        if (playerController != null) playerController.canMove = false; // Запрещаем игроку двигаться

        if (playerController != null) playerController.canLook = false; // Запрещаем игроку смотреть

        if (animator != null) animator.SetTrigger("Attack"); // Запускаем trigger Attack в Animator

        Invoke(nameof(FinishAttack), attackDelay); // Через задержку показываем Game Over
    }

    private void FinishAttack() // Завершить атаку
    {
        if (gameOverManager != null) gameOverManager.ShowGameOver(); // Показываем Game Over
    }
}