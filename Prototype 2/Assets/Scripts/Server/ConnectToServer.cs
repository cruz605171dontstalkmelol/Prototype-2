using UnityEngine;
using Photon.Pun;

public class ConnectToServer : MonoBehaviourPunCallbacks {

    public GameObject toggleServerObjects;

    private void Start() {
        if (!PhotonNetwork.IsConnected) {

            PhotonNetwork.ConnectUsingSettings();

        }

    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() {
        toggleServerObjects.SetActive(true);
    }
}
