using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ServerGameManager : MonoBehaviour {

    public static ServerGameManager instance;
    public PhotonView serverView;
    private void Awake() { if (instance == null) { instance = this; serverView = GetComponent<PhotonView>(); } else { Destroy(this.gameObject); } }

    public List<string> playerNames;
    public int currentSpotInList;
    private int totalReady;

    public GameObject[] playerReferences = new GameObject[15];
    public int[] playerDiamondCounts = new int[15];

    public GameState gameState;
    public enum GameState {
        lobby,
        playing,
        finished
    }

    [PunRPC]
    public void SendNewPlayerToList(int viewID, int mySpot) {
        //playerReferences.Add(PhotonView.Find(viewID).gameObject);
        //playerReferences.Insert(MainGameManager.instance.spotNumber, PhotonView.Find(viewID).gameObject);
        playerReferences[mySpot] = PhotonView.Find(viewID).gameObject;
    }

    [PunRPC]
    public void ChangeGameState(int changedState) {
        switch (changedState) {
            case 0:
                gameState = GameState.lobby;
                break;
            case 1:
                if (gameState != GameState.playing) {
                    gameState = GameState.playing;
                    GameObject tempObj = new GameObject();

                    Debug.Log("Happens once");


                    ClientGameManager.instance.StartedPlaying();
                }
                break;
            case 2:
                gameState = GameState.finished;
                break;
        }
    }

    [PunRPC]
    public void AddPlayerToList(string name) {
        //retrive name
        playerNames.Add(name);

        //update name ui
        ClientGameManager.instance.UpdateUsernames(currentSpotInList, name);
        ClientGameManager.instance.UpdateReadyList(totalReady, playerNames.Count);
        currentSpotInList+=1;
    }

    [PunRPC]
    public void ChangePlayerTeam(int teamID, int spotNumber) {
        ClientGameManager.instance.UpdateTeam(teamID, spotNumber);
    }

    [PunRPC]
    public void ReadyPlayerUp () {
        totalReady += 1;
        ClientGameManager.instance.UpdateReadyList(totalReady, playerNames.Count);
    }

    [PunRPC]
    public void StartGame() {
        if (totalReady == playerNames.Count) {
            ClientGameManager.instance.StartGame();
        }
    }

    [PunRPC]
    public void PlayAnimation(int spotNumber, string animName, int type) {
        switch (type) {
            default:
                playerReferences[spotNumber].GetComponentInChildren<PlayerController>().myAnimator.Play(animName);
                break;
            case 1:
                //special for human player to find spawned food object
                playerReferences[spotNumber].GetComponentInChildren<PlayerController>().spawnObjectReference.GetComponent<Animator>().Play(animName);
                break;
        }
    }

    [PunRPC]
    public void ChangeName(int viewID, string newName) {
        PhotonView.Find(viewID).gameObject.name = newName;
    }

    [PunRPC]
    public void SetOwnerToFood(int viewID, int playerSpot) {
        PhotonView.Find(viewID).GetComponent<ThrowObject>().owner = playerReferences[playerSpot].GetComponentInChildren<PlayerController>().transform;
        PhotonView.Find(viewID).GetComponent<ThrowObject>().transform.parent = playerReferences[playerSpot].GetComponentInChildren<PlayerController>().spawnObjectReference.transform;

    }

    [PunRPC]
    public void SetRandomInt (int value) {
        MainGameManager.instance.theRandomNumber = value;
    }

    [PunRPC]
    public void GetRandomPosition (float x, float y, float z) {
        Debug.Log("yes");
        PhotonNetwork.Instantiate(ClientGameManager.instance.foodSpawn.name, new Vector3(x,y,z), Quaternion.identity);
        Debug.Log("yesyes");
    }

    [PunRPC]
    public void DestroyMe (int viewID) {
        Destroy(PhotonView.Find(viewID).gameObject);
    }

    [PunRPC]
    public void UpdateDiamonds(int spotID) {
        playerDiamondCounts[spotID] += 1;
    }

    [PunRPC]
    public void SpendDiamonds(int spotID, int diamondAmount) {
        if (playerDiamondCounts[spotID] >= diamondAmount) {
            playerDiamondCounts[spotID] -= diamondAmount;
        } else { return; }
    }

}
