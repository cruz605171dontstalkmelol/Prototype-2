using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks {

    [SerializeField] private string sceneName;
    public InputField codeInputField;
    public InputField nameInputField;

    public void CreateRoom() {
        PhotonNetwork.CreateRoom(codeInputField.text);
    }

    public void JoinRoom() {
        PhotonNetwork.JoinRoom(codeInputField.text);
    }

    public override void OnJoinedRoom() {
        //set multiplayer variables
        MainGameManager.instance.username = nameInputField.text;

        //load scene
        PhotonNetwork.LoadLevel(sceneName);
    }
}
