using UnityEngine;

public class ThrowObject : MonoBehaviour {

    private MeshFilter _myRendererFilter;
    private MeshRenderer _myRenderer;
    public Mesh[] possibleMeshes;
    public Material[] possibleMaterials;

    private Rigidbody _myRigidbody;

    private void Start() {
        _myRendererFilter = GetComponent<MeshFilter>();
        _myRenderer = GetComponent<MeshRenderer>();
        _myRigidbody = GetComponent<Rigidbody>();

        //get random value
        int rand = Random.Range(0, possibleMeshes.Length);

        _myRendererFilter.mesh = possibleMeshes[rand];
        _myRenderer.material = possibleMaterials[rand];

        //throw
        Invoke("Throw", .235f);
    }

    private void Throw() {
        _myRigidbody.useGravity = true;
        transform.parent = null;
        _myRigidbody.AddForce(Vector3.forward * 2500);
    }

}
