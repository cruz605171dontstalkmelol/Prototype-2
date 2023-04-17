using UnityEngine;

public class RotateObject : MonoBehaviour {

    private void Update() {
        transform.RotateAround(transform.position, Vector3.up, 60 * Time.deltaTime);
    }
}
