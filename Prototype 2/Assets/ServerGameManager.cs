using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ServerGameManager : MonoBehaviour {

    public static ServerGameManager instance;
    public PhotonView serverView;
    private void Awake() { if (instance == null) { instance = this; serverView = GetComponent<PhotonView>(); } else { Destroy(this.gameObject); } }

    public List<string> playerNames;
    public List<Image> playerIcons;
    public int currentSpotInList;
    private int totalReady;

    public GameObject[] playerReferences = new GameObject[10];
    public int[] playerDiamondCounts = new int[10];
    public int[,] playerUpgrades = new int[10,3];

    private int currentSpawnedDiamonds = 0;

    //game end
    public bool gameEnded;

    public GameState gameState;
    public enum GameState {
        lobby,
        playing,
        finished
    }

    [PunRPC]
    public void TextureGround(int textureID) {
        ClientGameManager.instance.ChangeGroundTexture(textureID);
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
        playerIcons.Add(ClientGameManager.instance.iconImages[currentSpotInList]);

        //update name ui
        ClientGameManager.instance.UpdateUsernames(currentSpotInList, name);
        ClientGameManager.instance.UpdateIcons(currentSpotInList, playerIcons[currentSpotInList]);
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
    public void PlayAnimation(int spotNumber, string animName, int type, float toWhat) {
        switch (type) {
            default:
                playerReferences[spotNumber].GetComponentInChildren<PlayerController>().myAnimator.Play(animName);
                playerReferences[spotNumber].GetComponentInChildren<Animator>().SetFloat("ReloadTime", 2.09f/toWhat);
                break;
            case 1:
                //special for human player to find spawned food object
                playerReferences[spotNumber].GetComponentInChildren<PlayerController>().spawnObjectReference.GetComponent<Animator>().Play(animName);
                playerReferences[spotNumber].GetComponentInChildren<PlayerController>().spawnObjectReference.GetComponent<Animator>().SetFloat("ReloadTime", 2.09f/toWhat);
                break;
        }
    }
    
    [PunRPC]
    public void PlayAnimationEnd(int viewID, string animName, int backdropID) {
        StartCoroutine(DoEndAnimation(viewID, animName, backdropID));
    }

    private IEnumerator DoEndAnimation (int viewID, string animName, int backdropID) {

        //set up player
        PhotonView.Find(viewID).gameObject.GetComponentInChildren<Animator>().SetFloat("Speed_f",0);
        PhotonView.Find(viewID).gameObject.GetComponentInChildren<Animator>().gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);

        //set up background
        ClientGameManager.instance.backdrop.sprite = ClientGameManager.instance.backdrops[backdropID];

        yield return new WaitForSeconds(3);
        PhotonView.Find(viewID).gameObject.GetComponentInChildren<Animator>().Play(animName);
    }

    [PunRPC]
    public void ChangeAnimationMultiplier(int spotNumber, float toWhat) {
        Debug.Log(toWhat);
        playerReferences[spotNumber].GetComponentInChildren<Animator>().SetFloat("ReloadTime", toWhat);
        Debug.Log(playerReferences[spotNumber].GetComponentInChildren<Animator>().GetFloat("ReloadTime"));
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
        if (currentSpawnedDiamonds >= 10) { return; }
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.Instantiate(ClientGameManager.instance.foodSpawn.name, new Vector3(x, y, z), Quaternion.identity);
            //Instantiate(ClientGameManager.instance.foodSpawn, new Vector3(x,y,z), Quaternion.identity);
            currentSpawnedDiamonds += 1;
        }
    }

    [PunRPC]
    public void DestroyMe (int viewID) {
        Destroy(PhotonView.Find(viewID).gameObject);
    }

    [PunRPC]
    public void UpdateDiamonds (int spotID, int theirDiamondAmount) {
        playerDiamondCounts[spotID] = theirDiamondAmount;
        currentSpawnedDiamonds -= 1;
    }

    [PunRPC]
    public void SpendDiamonds (int spotID, int diamondAmount, int upgradeType, int newDiamondCount) {
        if (playerDiamondCounts[spotID] >= diamondAmount) {

            playerReferences[spotID].GetComponentInChildren<PlayerController>().SendLoseDiamondsToClient(diamondAmount);
            playerDiamondCounts[spotID] = newDiamondCount;

            serverView.RPC("UpgradePlayer", RpcTarget.All, spotID, upgradeType);

        } else {
            Debug.Log("Not enough diamonds to upgrade!");
            return; 
        }
    }

    [PunRPC]
    public void UpgradePlayer (int spotID, int upgradeSpot) {

        playerUpgrades[spotID, upgradeSpot] += 1;

        switch (upgradeSpot) {
            case 0:
                playerReferences[spotID].GetComponentInChildren<PlayerController>().speedUpgrade += 1;
                break;
            case 1:
                playerReferences[spotID].GetComponentInChildren<PlayerController>().reloadUpgrade += 1;
                break;
            case 2:
                playerReferences[spotID].GetComponentInChildren<PlayerController>().damageUpgrade += 1;
                break;
        }

    }

    [PunRPC]
    public void WonGame(int spotID, string username) {
        ClientGameManager.instance.winnerName.text = username;
        ClientGameManager.instance.winnerName.gameObject.SetActive(true);

        ClientGameManager.instance.EndCutscene(playerReferences[spotID].GetComponent<PhotonView>().ViewID);

        gameEnded = true;
    }

    [PunRPC]
    public void SetDamageAmount (int viewID, int damage) {
        PhotonView.Find(viewID).gameObject.GetComponent<ThrowObject>().amountOfDamage = damage;
    }

    [PunRPC]
    public void GetHit (int spotID, int currentHealth) {
        //if (playerReferences[spotID].GetComponent<PhotonView>().ViewID == playerReferences[MainGameManager.instance.spotNumber].GetComponent<PhotonView>().ViewID) { return; }
        //playerReferences[spotID].GetComponentInChildren<PlayerController>();
        playerDiamondCounts[spotID] = currentHealth;
        //ClientGameManager.instance.scoreBar.fillAmount -= 0.055f;
    }

    [PunRPC]
    public void DamageStun (int spotID) {
        playerReferences[spotID].GetComponentInChildren<PlayerController>().canDamage = false;
    }

    [PunRPC]
    public void DamageStunOver (int spotID) {
        playerReferences[spotID].GetComponentInChildren<PlayerController>().canDamage = true;
    }

}
