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

    public GameState gameState;
    public enum GameState {
        lobby,
        playing,
        finished
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

}
