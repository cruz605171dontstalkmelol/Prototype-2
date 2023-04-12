using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ClientGameManager : MonoBehaviour {

    public static ClientGameManager instance;
    private void Awake() { if (instance == null) { instance = this; } else { Destroy(this.gameObject); } }

    [Header("Mode:Lobby")]
    public GameObject lobbyHolder;
    public Text[] usernameSpots;
    [SerializeField] private Color32 teamHumanColor, teamAnimalColor;
    public Text readyText;
    public GameObject hostButton;

    [Header("Mode:Playing (setup)")]
    public GameObject humanPrefab;
    public GameObject animalPrefab;
    public Collider spawningBounds;
    //public GameObject[] playerReferences;

    //rpc buffer delay
    private void Start() {
        HostOrPlayer();

        Invoke("SendPlayerName", .1f);
    }

    //change button depending on host or player
    private void HostOrPlayer() {
        if (!PhotonNetwork.IsMasterClient) {
            hostButton.SetActive(false);
        }
    }

    //send player name data to rpc
    private void SendPlayerName() {
        ServerGameManager.instance.serverView.RPC("AddPlayerToList", RpcTarget.AllBuffered, MainGameManager.instance.username);
        MainGameManager.instance.spotNumber = ServerGameManager.instance.currentSpotInList-1;
    }

    //update usernames
    public void UpdateUsernames (int updateSpotNumber, string updateSpotName) {
        //update username
        usernameSpots[updateSpotNumber].text = updateSpotName;
        usernameSpots[updateSpotNumber].gameObject.SetActive(true);
    }

    //update the ready text
    public void UpdateReadyList (int totalReadyCount, int totalPlayerCount) {
        readyText.text = "Ready (" + totalReadyCount + "/" + totalPlayerCount + ")";
    }

    //send change team data to rpc
    public void SendChangeTeam (int teamID) {
        if (MainGameManager.instance.hasReadiedUp) { return; }
        MainGameManager.instance.teamID = teamID;
        ServerGameManager.instance.serverView.RPC("ChangePlayerTeam", RpcTarget.AllBuffered, teamID, MainGameManager.instance.spotNumber);
    }

    //update the players team id
    public void UpdateTeam(int teamID, int spotNumber) {
        //teamID 0 = human; 1 = animal
        switch (teamID) {
            case 0:
                usernameSpots[spotNumber].color = teamHumanColor;
                break;
            case 1:
                usernameSpots[spotNumber].color = teamAnimalColor;
                break;
            default:
                usernameSpots[spotNumber].color = teamAnimalColor;
            break;
        }
    }

    //send ready up data to rpc
    public void SendReadyUp() {
        if (MainGameManager.instance.hasReadiedUp) { return; }
        ServerGameManager.instance.serverView.RPC("ReadyPlayerUp", RpcTarget.AllBuffered);
        MainGameManager.instance.hasReadiedUp = true;
    }

    //send start game data to rpc
    public void SendStartGame() {
        ServerGameManager.instance.serverView.RPC("StartGame", RpcTarget.AllBuffered);
    }

    //start the game
    public void StartGame() {
        lobbyHolder.SetActive(false);
        ServerGameManager.instance.serverView.RPC("ChangeGameState", RpcTarget.AllBuffered, 1);
    }

    public void StartedPlaying() {
        //get random position in our bounds and spawn player in there
        if (MainGameManager.instance.teamID == 0) { PhotonNetwork.Instantiate(humanPrefab.name, SpawnPosition(spawningBounds.bounds), Quaternion.identity); }
        else { PhotonNetwork.Instantiate(animalPrefab.name, SpawnPosition(spawningBounds.bounds), Quaternion.identity); }
    }

    //get a random position
    Vector3 SpawnPosition(Bounds bounds) {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            //Random.Range(bounds.min.y, bounds.max.y),
            1,
            Random.Range(bounds.min.z, bounds.max.z)
       );
    }
}
