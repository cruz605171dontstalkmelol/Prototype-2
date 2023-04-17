using UnityEngine;
using Photon.Pun;

public class Collectable : MonoBehaviour {

    public GameObject particleEffect;

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "human" || other.gameObject.tag == "animal") {
            if (other.gameObject.GetComponentInParent<PhotonView>().IsMine) {

                Destroy(PhotonNetwork.Instantiate(particleEffect.name, transform.position, Quaternion.identity), 1f);

                ClientGameManager.instance.CollectedDiamond();
                ServerGameManager.instance.serverView.RPC("UpdateDiamonds", RpcTarget.All, MainGameManager.instance.spotNumber, ClientGameManager.instance.currentDiamonds);
                
                ServerGameManager.instance.serverView.RPC("DestroyMe", RpcTarget.All, GetComponent<PhotonView>().ViewID);
                //Destroy(this.gameObject);

            }
        }
    }

}
