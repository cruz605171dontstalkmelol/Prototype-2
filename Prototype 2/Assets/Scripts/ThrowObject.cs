using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class ThrowObject : MonoBehaviour {

    private MeshFilter _myRendererFilter;
    private MeshRenderer _myRenderer;
    public Mesh[] possibleMeshes;
    public Material[] possibleMaterials;

    private Rigidbody _myRigidbody;
    private MeshCollider _myCollider;

    public Transform owner;

    [SerializeField] private int forcePower;

    private bool hasThrown = false;

    public bool canDamage = false;
    public int amountOfDamage;

    private void Start() {
        _myRendererFilter = GetComponent<MeshFilter>();
        _myRenderer = GetComponent<MeshRenderer>();
        _myRigidbody = GetComponent<Rigidbody>();
        _myCollider = GetComponent<MeshCollider>();

        if (GetComponent<PhotonView>().IsMine) {
            int minValue = owner.GetComponentInChildren<PlayerController>().damageUpgrade * 5;
            int maxValue = minValue+5;

            //get random value
            int rand = Random.Range(minValue, maxValue);
            ServerGameManager.instance.serverView.RPC("SetRandomInt", RpcTarget.All, rand);
        }

        _myRendererFilter.mesh = possibleMeshes[MainGameManager.instance.theRandomNumber];
        _myRenderer.material = possibleMaterials[MainGameManager.instance.theRandomNumber];
        _myCollider.sharedMesh = possibleMeshes[MainGameManager.instance.theRandomNumber];

        //throw
        Invoke("Throw", owner.GetComponentInChildren<PlayerController>().reloadInterval * .75f);
        //Invoke("Throw", .235f);
    }

    private void Update() {
        if (hasThrown) { return; }
        transform.localPosition = Vector3.zero;
    }

    private void Throw() {
        _myRigidbody.useGravity = true;
        transform.parent = null;
        _myRigidbody.AddForce(owner.forward * (forcePower * Mathf.Max(owner.GetComponentInChildren<PlayerController>().speedUpgrade * .2f, 1)));
        Invoke("ToggleCollision", .15f);
        hasThrown = true;
    }

    private void ToggleCollision() {
        _myCollider.enabled = true;
    }

    private void OnCollisionEnter(Collision collision) {
        StartCoroutine(TurnOffDamage());
    }

    private IEnumerator TurnOffDamage () {
        yield return new WaitForSeconds(.1f);
        canDamage = false;
    }
    

}
