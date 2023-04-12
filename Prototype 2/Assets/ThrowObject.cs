using UnityEngine;
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

    private void Start() {
        _myRendererFilter = GetComponent<MeshFilter>();
        _myRenderer = GetComponent<MeshRenderer>();
        _myRigidbody = GetComponent<Rigidbody>();
        _myCollider = GetComponent<MeshCollider>();

        if (GetComponent<PhotonView>().IsMine) {
            //get random value
            int rand = Random.Range(0, possibleMeshes.Length);
            ServerGameManager.instance.serverView.RPC("SetRandomInt", RpcTarget.All, rand);
        }

        _myRendererFilter.mesh = possibleMeshes[MainGameManager.instance.theRandomNumber];
        _myRenderer.material = possibleMaterials[MainGameManager.instance.theRandomNumber];
        _myCollider.sharedMesh = possibleMeshes[MainGameManager.instance.theRandomNumber];

        //throw
        Invoke("Throw", .75f);
        //Invoke("Throw", .235f);
    }

    private void Update() {
        if (hasThrown) { return; }
        transform.localPosition = Vector3.zero;
    }

    private void Throw() {
        _myRigidbody.useGravity = true;
        transform.parent = null;
        _myRigidbody.AddForce(owner.forward * forcePower);
        Invoke("ToggleCollision", .15f);
        hasThrown = true;
    }

    private void ToggleCollision() {
        _myCollider.enabled = true;
    }

}
