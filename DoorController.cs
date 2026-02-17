using UnityEngine;

/// <summary>
/// Simple door controller that opens when the player enters a trigger zone.
/// Requires a Trigger collider on the same GameObject as this script.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Animator component on the door (can be on this object or a child)")]
    public Animator doorAnimator;

    [Header("Settings")]
    [Tooltip("Tag that identifies the player GameObject")]
    public string playerTag = "Player";

    [Header("Animator Parameters")]
    [Tooltip("Name of the bool parameter in the Animator for open/close state")]
    public string isOpenParam = "IsOpen";

    private void Start()
    {
        // Auto-assign animator if not set
        if (doorAnimator == null)
        {
            doorAnimator = GetComponentInChildren<Animator>();
        }

        // Ensure the collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"Collider on {gameObject.name} should be set to IsTrigger for door activation to work properly.");
        }

        // Validate
        if (doorAnimator == null)
        {
            Debug.LogError($"No Animator found on {gameObject.name}. Please assign one in the inspector.");
        }
    }

    /// <summary>
    /// Called when another collider enters this trigger
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag(playerTag))
        {
            OpenDoor();
        }
    }

    /// <summary>
    /// Called when another collider exits this trigger
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting object is the player
        if (other.CompareTag(playerTag))
        {
            CloseDoor();
        }
    }

    /// <summary>
    /// Opens the door by setting the Animator bool to true
    /// </summary>
    public void OpenDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(isOpenParam, true);
            Debug.Log("Door opened");
        }
    }

    /// <summary>
    /// Closes the door by setting the Animator bool to false
    /// </summary>
    public void CloseDoor()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(isOpenParam, false);
            Debug.Log("Door closed");
        }
    }
}

/// <summary>
/// ============================================
/// SETUP INSTRUCTIONS:
/// ============================================
/// 
/// 1. ANIMATOR SETUP:
///    - Create an Animator Controller (Assets > Create > Animator Controller)
///    - Add a Bool parameter named "IsOpen"
///    - Create two animation states: "Closed" and "Open"
///    - Create transitions: Closed -> Open (condition: IsOpen = true)
///                       Open -> Closed (condition: IsOpen = false)
///    - Assign your door open/close animations to the states
/// 
/// 2. GAMEOBJECT SETUP:
///    - Add this script to your door GameObject
///    - Assign the Animator component to the doorAnimator field
///    - OR: The script will auto-find an Animator in children
/// 
/// 3. TRIGGER SETUP:
///    - Add a Collider component (Box Collider recommended)
///    - Check "Is Trigger" on the collider
///    - Position/scale the collider to create your activation zone
///      (e.g., a trigger area in front of the door)
/// 
/// 4. PLAYER SETUP:
///    - Ensure your player GameObject has the tag "Player"
///    - Player needs a Collider component (doesn't need to be trigger)
///    - Player needs a Rigidbody for trigger events to fire
/// 
/// ============================================
/// </summary>
