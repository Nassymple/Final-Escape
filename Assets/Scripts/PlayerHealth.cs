using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    [SerializeField] private int _currentHealth;

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        // Reduce the player's health by the damage amount
        _currentHealth -= damage;

        // Clamp the health value between 0 and maxHealth
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        // Print the player's health to the console
        Debug.Log($"Player health: {_currentHealth}");

        // Check if the player is dead
        if (_currentHealth <= 0)
        {
            // Handle player death (e.g., show game over screen, respawn, etc.)
            Debug.Log("Player is dead.");
            gameObject.SetActive(false);
        }
    }
    public bool IsAlive()
    {
        return _currentHealth > 0;
    }
}
