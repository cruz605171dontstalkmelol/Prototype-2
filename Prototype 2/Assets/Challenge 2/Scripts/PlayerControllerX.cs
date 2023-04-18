using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerX : MonoBehaviour
{
    public GameObject dogPrefab;
    private PlayerInput _playerInput;

    private void Awake() {
        _playerInput = new PlayerInput();
    }

    public void OnDogSpawn (InputAction.CallbackContext context) {
        if (context.started) {
            Instantiate(dogPrefab, transform.position, dogPrefab.transform.rotation);
        }
    }
}
