using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// This script should be attached to an AI agent in your Unity scene.
// Require Rigidbody and NavMeshAgent components
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class Basic_AiController : MonoBehaviour
{
    // AI agent components
    private NavMeshAgent _navMeshAgent; // The NavMeshAgent component attached to the AI agent
    private Transform _playerTransform; // The Transform component of the player

    // Waypoints variables
    public List<Transform> waypoints; // List of waypoints for the AI agent to patrol
    private int _currentWaypointIndex = 0; // The index of the current waypoint in the list
    public float waypointIdleTime = 2.0f; // Time in seconds for the AI agent to idle at each waypoint
    private float _currentIdleTime = 0f; // Counter to keep track of the time spent idling at a waypoint
    private bool patroling; // Boolean to keep track of whether the AI agent is patrolling or not

    // Sensory and attack variables
    public float sensoryRadius = 10.0f; // Radius within which the AI agent can sense the player
    public float attackRadius = 2.0f; // Radius within which the AI agent can attack the player
    public int damage = 10; // Damage dealt by the AI agent to the player on each attack
    public float stopSpeed = 0f; // Speed of the AI agent when it's not moving
    public float moveSpeed = 5f; // Speed of the AI agent when it's moving

    // Stopping distance variable
    public float stoppingDistance = 1.0f; // Distance at which the AI agent should stop from the player or waypoint

    // Attack cooldown variables
    public float attackCooldown = 1.0f; // Cooldown in seconds between each attack
    private float _timeSinceLastAttack = 0f; // Counter to keep track of the time since the last attack

    // Sensory and attack gizmos colors
    public Color sensoryRadiusColor = Color.yellow; // Color for the sensory radius gizmo
    public Color attackRadiusColor = Color.red; // Color for the attack radius gizmo

    // PlayerHealth script reference
    private PlayerHealth _playerHealth; // Reference to the PlayerHealth script attached to the player

    private void Start()
    {
        // Get required components
        _navMeshAgent = GetComponent<NavMeshAgent>(); // Get the NavMeshAgent component attached to the AI agent
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // Find the player object by its tag and get its Transform component
        _playerHealth = _playerTransform.GetComponent<PlayerHealth>(); // Get the PlayerHealth script attached to the player
        _navMeshAgent.stoppingDistance = stoppingDistance; // Set the stopping distance of the NavMeshAgent

        // Initialize the AI agent's destination
        MoveToNextWaypoint(); // Move the AI agent to the first waypoint in the list
    }

    private void Update()
    {
        // Check the distance between the AI agent and the player
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position); // Calculate the distance between the AI agent and the player

        // Check if the player is alive
        if (_playerHealth.IsAlive()) // If the player is alive
        {
            if (distanceToPlayer <= sensoryRadius) // If the player is within sensory radius
            {
                if (distanceToPlayer <= attackRadius) // If the player is within attack radius
                {
                    StopChasingPlayer(); // Stop chasing the player
                    AttackPlayer(); // Attack the player
                }
                else // If the player is outside attack radius but within sensory radius
                {
                    ChasePlayer(); // Chase the player
                }
            }
            else // If the player is outside sensory radius
            {
                Patrol(); // Patrol the waypoints
            }
        }
        else // If the player is dead
        {
            Patrol(); // Patrol the waypoints
        }

        _timeSinceLastAttack += Time.deltaTime; // Increment the attack cooldown counter
    }

    private void Patrol()
    {
        if (!patroling) // If the AI agent is not currently patrolling
        {
            MoveToClosestWaypoint(); // Move to the closest waypoint
        }

        // Check if the AI agent has reached the current waypoint
        if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
        {
            // Increment the idle timer
            _currentIdleTime += Time.deltaTime; // Increase the idle time counter

            if (_currentIdleTime >= waypointIdleTime) // If idle time reaches the defined idle duration
            {
                // Move to the next waypoint and reset the idle timer
                MoveToNextWaypoint(); // Move to the next waypoint in the list
                _currentIdleTime = 0f; // Reset the idle time counter
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        patroling = true; // Set the patrolling state to true
        _navMeshAgent.speed = moveSpeed; // Set the AI agent's movement speed

        // Set the AI agent's destination to the current waypoint position
        _navMeshAgent.SetDestination(waypoints[_currentWaypointIndex].position);

        // Update the waypoint index to the next one (looping back to the start if necessary)
        _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Count;
    }

    private void ChasePlayer()
    {
        patroling = false; // Set the patrolling state to false

        // Set the AI agent's destination to the player's position
        _navMeshAgent.SetDestination(_playerTransform.position);

        // Resume the AI agent's movement if stopped
        if (_navMeshAgent.isStopped)
        {
            _navMeshAgent.speed = moveSpeed; // Set the AI agent's movement speed
            _navMeshAgent.isStopped = false; // Resume the AI agent's movement
        }
    }

    private void AttackPlayer()
    {
        patroling = false; // Set the patrolling state to false

        // Check if the AI agent can attack based on the attack cooldown
        if (_timeSinceLastAttack >= attackCooldown) // If the time since the last attack is greater than or equal to the defined attack cooldown
        {
            // Deal damage to the player's health
            _playerHealth.TakeDamage(damage); // Apply damage to the player's health

            // Reset the attack timer
            _timeSinceLastAttack = 0f; // Reset the time since the last attack counter
        }
    }

    private void StopChasingPlayer()
    {
        _navMeshAgent.speed = stopSpeed; // Set the AI agent's movement speed to the stop speed

        // Stop the AI agent's movement
        _navMeshAgent.isStopped = true; // Stop the AI agent's movement
    }

    private void MoveToClosestWaypoint()
    {
        // Find the closest waypoint
        float closestDistance = float.MaxValue; // Initialize the closest distance as the maximum float value
        int closestWaypointIndex = 0; // Initialize the closest waypoint index

        for (int i = 0; i < waypoints.Count; i++) // Iterate through all waypoints
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].position); // Calculate the distance between the AI agent and the current waypoint
            if (distance < closestDistance) // If the calculated distance is less than the current closest distance
            {
                closestDistance = distance; // Update the closest distance value
                closestWaypointIndex = i; // Update the closest waypoint index
            }
        }

        // Set the AI agent's destination to the closest waypoint position
        _navMeshAgent.SetDestination(waypoints[closestWaypointIndex].position);

        // Resume the AI agent's movement if stopped
        if (_navMeshAgent.isStopped)
        {
            _navMeshAgent.speed = moveSpeed; // Set the AI agent's movement speed
            _navMeshAgent.isStopped = false; // Resume the AI agent's movement
        }
    }

    private void OnDrawGizmos()
    {
        // Draw sensory radius gizmo
        Gizmos.color = sensoryRadiusColor; // Set the gizmo color for the sensory radius
        Gizmos.DrawWireSphere(transform.position, sensoryRadius); // Draw a wire sphere to represent the sensory radius

        // Draw attack radius gizmo
        Gizmos.color = attackRadiusColor; // Set the gizmo color for the attack radius
        Gizmos.DrawWireSphere(transform.position, attackRadius); // Draw a wire sphere to represent the attack radius
    }
}