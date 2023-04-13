using UnityEngine;
using Photon.Pun;

public class Collectable : MonoBehaviour {

    public GameObject particleEffect;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "human" || other.gameObject.tag == "animal") {
            Instantiate(particleEffect, transform.position, Quaternion.identity);
            ServerGameManager.instance.serverView.RPC("DestroyMe", RpcTarget.All, GetComponent<PhotonView>().ViewID);
            ServerGameManager.instance.serverView.RPC("UpdateDiamonds", RpcTarget.All, MainGameManager.instance.spotNumber);
        }
    }

}
