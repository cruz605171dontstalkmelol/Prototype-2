using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerController : MonoBehaviour {

    [Header("Main References")]
    public PhotonView _myView;
    private Rigidbody _myRigidbody;
    private Animator _myAnimator;

    [Header("Type specific")]
    public bool isHuman;

    [Header("Walking/Running")]
    private float _speed = 8;
    private Vector2 _movement;
    private Vector3 _updateMovementToVector3;
    private bool isWalking;
    private bool allowedToWalk = true;

    [Header("Abilities")]
    public GameObject thrownObject;
    public Transform spawnObjectReference;

    private void Awake() {
        if (_myView.IsMine) {
            _myRigidbody = GetComponent<Rigidbody>();
            _myAnimator = GetComponent<Animator>();
        }
    }

    private void Update() {
        if (_myView.IsMine) {
            if (!allowedToWalk) { return; }
            RotateForward();
            UpdateAnimator();
        }
    }

    private void FixedUpdate() {
        if (_myView.IsMine) {
            if (!allowedToWalk) { return; }
            MoveAround();
        }
    }

    private void RotateForward() {
        if (_movement.sqrMagnitude > 0.1f) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_updateMovementToVector3), _speed);
        }
    }

    private void UpdateAnimator() {
        if (_movement.x != 0 || _movement.y != 0) {
            _myAnimator.SetFloat("Speed_f", _speed);
            _myAnimator.SetBool("Static_b", false);
        } else {
            _myAnimator.SetFloat("Speed_f", 0);
            _myAnimator.SetBool("Static_b", true);
        }
    }

    private void MoveAround() { 
        _myRigidbody.MovePosition(_myRigidbody.position + _updateMovementToVector3 * _speed * Time.fixedDeltaTime);
    }

    //inputs
    public void OnMove(InputAction.CallbackContext context) {
        if (_myView.IsMine) {
            _movement = context.ReadValue<Vector2>();
            _updateMovementToVector3 = new Vector3(_movement.x, 0f, _movement.y);
        }
    }

    public void OnThrow(InputAction.CallbackContext context) {
        if (context.started) {
            if (_myView.IsMine) {
                if (!isHuman) { return; }
                Debug.Log("Should be happening once");

                //throw object
                _myAnimator.Play("GrenadeThrow");
                GameObject instFood = PhotonNetwork.Instantiate(thrownObject.name, spawnObjectReference.position, spawnObjectReference.rotation);
                instFood.GetComponent<ThrowObject>().owner = this.transform;
                spawnObjectReference.GetComponent<Animator>().Play("SpawnFood_Throw");
                instFood.transform.parent = spawnObjectReference.transform;
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (isHuman) { return; }
        if (collision.gameObject.tag != "food") { return; }

        _myRigidbody.AddForce(Vector3.up * 1500);
    }

}
