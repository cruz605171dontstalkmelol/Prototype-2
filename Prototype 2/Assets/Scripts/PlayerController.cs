using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerController : MonoBehaviour {

    [Header("Main References")]
    public PhotonView myView;
    private Rigidbody _myRigidbody;
    public Animator myAnimator;
    private PlayerInput playerInput;

    [Header("Type specific")]
    public bool isHuman;

    [Header("Walking/Running")]
    private float _speed = 8f;
    private Vector2 _movement;
    private Vector3 _updateMovementToVector3;
    private bool _isWalking;
    private bool _allowedToWalk = true;

    [Header("Abilities")]
    public GameObject thrownObject;
    public Transform spawnObjectReference;
    private bool _readyToDash;
    private bool _canShoot = true;
    public float reloadInterval = 3;
    private int currentDamage = 1;

    private int currentUpgrade = 0;
    public GameObject[] animalUpgrades;


    [Header("Upgrades")]
    public int speedUpgrade = 0;
    public int reloadUpgrade = 5;
    public int damageUpgrade = 0;
    private float _speedMultiplier = .8f;
    private float _reloadMultiplier = .9f;
    private float _damageMultiplier;

    [Header("Extra")]
    public GameObject hotbar;

    public bool canDamage = true;

    private void Awake() {
        if (myView.IsMine) {
            _myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
            playerInput = new PlayerInput();
        }

        Invoke("AddNewPlayer", .1f);
    }

    private void Start() {
        if (myView.IsMine) {
            hotbar.SetActive(true);
        }
    }
    private void AddNewPlayer () {
        if (!myView.IsMine) { return; }
        ServerGameManager.instance.serverView.RPC("SendNewPlayerToList", RpcTarget.AllBuffered, myView.ViewID, MainGameManager.instance.spotNumber);
    }

    private void Update() {
        if (ServerGameManager.instance.gameEnded) { return; }
        if (myView.IsMine) {
            if (!_allowedToWalk) { return; }
            RotateForward();
            UpdateAnimator();
        }
    }

    private void FixedUpdate() {
        if (ServerGameManager.instance.gameEnded) { return; }
        if (myView.IsMine) {
            if (!_allowedToWalk) { return; }
            MoveAround();
            if (_readyToDash) {
                Dash();
            }
        }
    }

    private void Dash() {
        _readyToDash = false;
        _myRigidbody.AddRelativeForce(Vector3.forward * 15500);
    }

    private void RotateForward() {
        
        if (_movement.sqrMagnitude > 0.1f) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_updateMovementToVector3), _speed);
        }

    }

    float AngleBetweenTwoPoints(Vector3 a, Vector3 b) {
        return Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
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
        _myRigidbody.MovePosition(_myRigidbody.position + _updateMovementToVector3 * (_speed + (_speedMultiplier * speedUpgrade)) * Time.fixedDeltaTime);
    }

    //inputs
    public void OnMove(InputAction.CallbackContext context) {
        if (myView.IsMine) {
            _movement = context.ReadValue<Vector2>();
            _updateMovementToVector3 = new Vector3(_movement.x, 0f, _movement.y);
        }
    }

    public void OnThrow(InputAction.CallbackContext context) {
        if (ServerGameManager.instance.gameEnded) { return; }
        if (context.started) {
            if (myView.IsMine) {
                if (isHuman) {
                    if (!_canShoot) { return; }
                    //throw object
                    StartCoroutine(ShootReload());
                    ServerGameManager.instance.serverView.RPC("PlayAnimation", RpcTarget.All, MainGameManager.instance.spotNumber, "GrenadeThrow", 0, reloadInterval);
                    GameObject instFood = PhotonNetwork.Instantiate(thrownObject.name, spawnObjectReference.position, spawnObjectReference.rotation);
                    ServerGameManager.instance.serverView.RPC("SetOwnerToFood", RpcTarget.All, instFood.GetComponent<PhotonView>().ViewID, MainGameManager.instance.spotNumber);
                    ServerGameManager.instance.serverView.RPC("PlayAnimation", RpcTarget.All, MainGameManager.instance.spotNumber, "SpawnFood_Throw", 1, reloadInterval);
                    ServerGameManager.instance.serverView.RPC("SetDamageAmount", RpcTarget.All, instFood.GetComponent<PhotonView>().ViewID, currentDamage);
                    _canShoot = false;
                }
                else if (!isHuman) {
                    _readyToDash = true;
                }
            }
        }
    }

    private IEnumerator ShootReload () {
        yield return new WaitForSeconds(reloadInterval - (reloadInterval * ((_reloadMultiplier * reloadUpgrade)/10)));
        _canShoot = true;
    }

    public void OnButton1 (InputAction.CallbackContext context) {
        if (context.started) {
            if (isHuman) {
                if (ClientGameManager.instance.currentDiamonds >= 1) { SpawnAnnoucncement(0); }
                ServerGameManager.instance.serverView.RPC("SpendDiamonds", RpcTarget.All, MainGameManager.instance.spotNumber, 1, 0, ClientGameManager.instance.currentDiamonds - 1);
            } else {
                if (ClientGameManager.instance.currentDiamonds >= 1) { SpawnAnnoucncement(3); }
                ServerGameManager.instance.serverView.RPC("SpendDiamonds", RpcTarget.All, MainGameManager.instance.spotNumber, 1, 0, ClientGameManager.instance.currentDiamonds - 1);
            }
        }
    }

    public void OnButton2(InputAction.CallbackContext context) {
        if (context.started) {
            if (isHuman) {
                if (ClientGameManager.instance.currentDiamonds >= 2) { SpawnAnnoucncement(1); reloadInterval = reloadInterval - (reloadInterval * ((_reloadMultiplier * reloadUpgrade) / 10)); }
                ServerGameManager.instance.serverView.RPC("SpendDiamonds", RpcTarget.All, MainGameManager.instance.spotNumber, 2, 1, ClientGameManager.instance.currentDiamonds - 2);
            }
        }
    }

    public void OnButton3(InputAction.CallbackContext context) {
        if (context.started) {
            if (isHuman) {
                if (ClientGameManager.instance.currentDiamonds >= 3) { SpawnAnnoucncement(2); }
                ServerGameManager.instance.serverView.RPC("SpendDiamonds", RpcTarget.All, MainGameManager.instance.spotNumber, 3, 2, ClientGameManager.instance.currentDiamonds - 3);
                currentDamage = Mathf.Min(currentDamage * damageUpgrade, 3);
            }
        }
    }

    public void SpawnAnnoucncement(int type) {
        if (ServerGameManager.instance.gameEnded) { return; }

        GameObject instAnnouncement = Instantiate(ClientGameManager.instance.announcements, transform.position, Quaternion.identity);
        instAnnouncement.transform.parent = ClientGameManager.instance.announcementsHolder;
        
        switch (type) {
            case 0:
                instAnnouncement.GetComponentInChildren<Text>().text = "+ SPEED UP";
                break;
            case 1:
                instAnnouncement.GetComponentInChildren<Text>().text = "+ RELOAD UP";
                break;
            case 2:
                instAnnouncement.GetComponentInChildren<Text>().text = "+ DAMAGE UP";
                break;
            case 3:
                instAnnouncement.GetComponentInChildren<Text>().text = "+ UPGRADED";
                currentUpgrade += 1;
                CheckAnimalUpgrade();
                break;
        }

        Destroy(instAnnouncement, .70f);

    }

    private void OnCollisionEnter(Collision collision) {
        if (!myView.IsMine) { return; }

        else {
            if (isHuman) {
                if (collision.gameObject.tag == "animal" && canDamage) {

                    Debug.Log("Animal doing damage");

                    ServerGameManager.instance.playerDiamondCounts[MainGameManager.instance.spotNumber] -= collision.gameObject.GetComponent<PlayerController>().currentDamage;
                    ServerGameManager.instance.serverView.RPC("GetHit", RpcTarget.All, MainGameManager.instance.spotNumber, ServerGameManager.instance.playerDiamondCounts[MainGameManager.instance.spotNumber]);
                    SendLoseDiamondsToClient(collision.gameObject.GetComponent<PlayerController>().currentDamage);

                    switch (collision.gameObject.GetComponent<PlayerController>().currentDamage) {
                        case 1:
                            GameObject instParticle = PhotonNetwork.Instantiate(ClientGameManager.instance.hitEffect1.name, transform.position, Quaternion.identity);
                            Destroy(instParticle, 1f);
                            break;
                        case 2:
                            GameObject instParticle1 = PhotonNetwork.Instantiate(ClientGameManager.instance.hitEffect2.name, transform.position, Quaternion.identity);
                            Destroy(instParticle1, 1f);
                            break;
                        default:
                            GameObject instParticle2 = PhotonNetwork.Instantiate(ClientGameManager.instance.hitEffect3.name, transform.position, Quaternion.identity);
                            Destroy(instParticle2, 1f);
                            break;
                    }
                    
                    GetComponent<Rigidbody>().AddForce(new Vector3(Random.insideUnitSphere.x, Mathf.Abs(Random.insideUnitSphere.y), Random.insideUnitSphere.z) * 200);

                    ServerGameManager.instance.serverView.RPC("DamageStun", RpcTarget.All, MainGameManager.instance.spotNumber);
                    Invoke("ReadyToBeDamaged", .3f);

                    if (ServerGameManager.instance.playerDiamondCounts[MainGameManager.instance.spotNumber] <= 0) {
                        ServerGameManager.instance.serverView.RPC("DestroyMe", RpcTarget.All, GetComponentInParent<PhotonView>().ViewID);
                    }
                }
            }
            else {
                if (collision.gameObject.tag == "food") {

                    ServerGameManager.instance.playerDiamondCounts[MainGameManager.instance.spotNumber] -= collision.gameObject.GetComponent<ThrowObject>().amountOfDamage;
                    ServerGameManager.instance.serverView.RPC("GetHit", RpcTarget.All, MainGameManager.instance.spotNumber, ServerGameManager.instance.playerDiamondCounts[MainGameManager.instance.spotNumber]);
                    SendLoseDiamondsToClient(collision.gameObject.GetComponent<ThrowObject>().amountOfDamage);

                    switch (collision.gameObject.GetComponent<ThrowObject>().amountOfDamage) {
                        case 1:
                            GameObject instParticle = PhotonNetwork.Instantiate(ClientGameManager.instance.hitEffect1.name, transform.position, Quaternion.identity);
                            Destroy(instParticle, 1f);
                            break;
                        case 2:
                            GameObject instParticle1 = PhotonNetwork.Instantiate(ClientGameManager.instance.hitEffect2.name, transform.position, Quaternion.identity);
                            Destroy(instParticle1, 1f);
                            break;
                        default:
                            GameObject instParticle2 = PhotonNetwork.Instantiate(ClientGameManager.instance.hitEffect3.name, transform.position, Quaternion.identity);
                            Destroy(instParticle2, 1f);
                            break;
                    }

                    GetComponent<Rigidbody>().AddForce(new Vector3(Random.insideUnitSphere.x, Mathf.Abs(Random.insideUnitSphere.y), Random.insideUnitSphere.z) * 200);

                    ServerGameManager.instance.serverView.RPC("DamageStun", RpcTarget.All, MainGameManager.instance.spotNumber);
                    Invoke("ReadyToBeDamaged", .3f);

                    if (ServerGameManager.instance.playerDiamondCounts[MainGameManager.instance.spotNumber] <= 0) {
                        ServerGameManager.instance.serverView.RPC("DestroyMe", RpcTarget.All, GetComponentInParent<PhotonView>().ViewID);
                    }
                }
            }
        }
    }

    private void ReadyToBeDamaged() {
        ServerGameManager.instance.serverView.RPC("DamageStunOver", RpcTarget.All, MainGameManager.instance.spotNumber);
    }

    public void SendLoseDiamondsToClient (int amountLost) {
        if (!myView.IsMine) { return; }
        ClientGameManager.instance.LoseDiamond(amountLost);
    }

    private void CheckAnimalUpgrade() {
        ServerGameManager.instance.serverView.RPC("SetUpgradeAnimal", RpcTarget.All, MainGameManager.instance.spotNumber, currentUpgrade);
    }

}
