using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerController : MonoBehaviour {

    [Header("Main References")]
    public PhotonView myView;
    private Rigidbody _myRigidbody;
    public Animator myAnimator;

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
        if (myView.IsMine) {
            _myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
        }

        Invoke("AddNewPlayer", .1f);
    }
    private void AddNewPlayer () {
        if (!myView.IsMine) { return; }
        ServerGameManager.instance.serverView.RPC("SendNewPlayerToList", RpcTarget.AllBuffered, myView.ViewID, MainGameManager.instance.spotNumber);
    }

    private void Update() {
        if (myView.IsMine) {
            if (!allowedToWalk) { return; }
            RotateForward();
            UpdateAnimator();
        }
    }

    private void FixedUpdate() {
        if (myView.IsMine) {
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
            myAnimator.SetFloat("Speed_f", _speed);
            myAnimator.SetBool("Static_b", false);
        } else {
            myAnimator.SetFloat("Speed_f", 0);
            myAnimator.SetBool("Static_b", true);
        }
    }

    private void MoveAround() { 
        _myRigidbody.MovePosition(_myRigidbody.position + _updateMovementToVector3 * _speed * Time.fixedDeltaTime);
    }

    //inputs
    public void OnMove(InputAction.CallbackContext context) {
        if (myView.IsMine) {
            _movement = context.ReadValue<Vector2>();
            _updateMovementToVector3 = new Vector3(_movement.x, 0f, _movement.y);
        }
    }

    public void OnThrow(InputAction.CallbackContext context) {
        if (context.started) {
            if (myView.IsMine) {
                if (isHuman) {
                    //throw object
                    ServerGameManager.instance.serverView.RPC("PlayAnimation", RpcTarget.All, MainGameManager.instance.spotNumber, "GrenadeThrow", 0);
                    GameObject instFood = PhotonNetwork.Instantiate(thrownObject.name, spawnObjectReference.position, spawnObjectReference.rotation);
                    ServerGameManager.instance.serverView.RPC("SetOwnerToFood", RpcTarget.All, instFood.GetComponent<PhotonView>().ViewID, MainGameManager.instance.spotNumber);
                    ServerGameManager.instance.serverView.RPC("PlayAnimation", RpcTarget.All, MainGameManager.instance.spotNumber, "SpawnFood_Throw", 1);
                }
                else if (!isHuman) {
                    _myRigidbody.AddForce(Vector3.forward * 5500);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (isHuman) { return; }
        if (collision.gameObject.tag != "food") { return; }

        _myRigidbody.AddForce(Vector3.up * 5500);
    }

}
