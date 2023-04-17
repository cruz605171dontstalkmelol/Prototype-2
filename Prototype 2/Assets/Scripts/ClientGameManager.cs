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

    public Sprite[] icons;
    public Image[] iconImages;

    public Text lobbyTitle;

    [Header("Mode:Playing (setup)")]
    public GameObject humanPrefab;
    public GameObject animalPrefab;
    public Collider spawningBounds;
    //public GameObject[] playerReferences;
    public MeshRenderer meshRenderer;
    public Material[] textures;

    [Header("Mode:Playing")]
    private float spawnFoodTimer = 1;
    public GameObject foodSpawn;
    public GameObject uiScoreHolder;

    public int currentDiamonds;
    public Image scoreBar;

    [Header("Mode:Win")]
    public Text winnerName;
    public Camera cutsceneCamera;
    public Text winnerTag;
    public Transform winnerPosition;
    public Image backdrop;
    public Sprite[] backdrops;

    public GameObject hitEffect1;
    public GameObject hitEffect2;
    public GameObject hitEffect3;

    [Header("Announcements")]
    public Transform announcementsHolder;
    public GameObject announcements;

    //rpc buffer delay
    private void Start() {
        HostOrPlayer();

        UpdateLobbyTitle();

        if (PhotonNetwork.IsMasterClient) {
            int textureID = Random.Range(0, textures.Length);

            ServerGameManager.instance.serverView.RPC("TextureGround", RpcTarget.AllBuffered, textureID);
        }

        Invoke("SendPlayerName", .1f);
    }

    public void ChangeGroundTexture(int textureID) {
        meshRenderer.material = textures[textureID];
    }

    private void UpdateLobbyTitle () {
        lobbyTitle.text = "Lobby [" + MainGameManager.instance.networkCode + "]";
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

    public void UpdateIcons(int updateSpotNumber, Image updateSpotIcon) {
        //update username
        iconImages[updateSpotNumber].sprite = icons[2];
        iconImages[updateSpotNumber].gameObject.SetActive(true);
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
            case 1:
                ServerGameManager.instance.playerIcons[spotNumber].sprite = icons[0];
                break;
            case 2:
                ServerGameManager.instance.playerIcons[spotNumber].sprite = icons[1];
                break;
            default:
                ServerGameManager.instance.playerIcons[spotNumber].sprite = icons[1];
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
        uiScoreHolder.SetActive(true);
        ServerGameManager.instance.serverView.RPC("ChangeGameState", RpcTarget.AllBuffered, 1);
        Invoke("SpawnFood", spawnFoodTimer);
    }

    //spawn food
    public void SpawnFood () {
        if (PhotonNetwork.IsMasterClient) {
            Bounds bounds = spawningBounds.bounds;
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = 1;
            float z = Random.Range(bounds.min.z, bounds.max.z);
            ServerGameManager.instance.serverView.RPC("GetRandomPosition", RpcTarget.All, x, y, z);
            Invoke("SpawnFood", spawnFoodTimer);
        }
    }

    public void StartedPlaying() {
        //get random position in our bounds and spawn player in there
        if (MainGameManager.instance.teamID == 0) { MainGameManager.instance.teamID = Random.Range(1, 3); }

        if (MainGameManager.instance.teamID == 1) { PhotonNetwork.Instantiate(humanPrefab.name, SpawnPosition(spawningBounds.bounds), Quaternion.identity); }
        else if (MainGameManager.instance.teamID == 2) { PhotonNetwork.Instantiate(animalPrefab.name, SpawnPosition(spawningBounds.bounds), Quaternion.identity); }
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

    public void UpgradeType(int upgradeType) {
        ServerGameManager.instance.serverView.RPC("UpgradePlayer", RpcTarget.All, MainGameManager.instance.spotNumber, upgradeType);
    }

    public void CollectedDiamond() {
        currentDiamonds += 1;
        scoreBar.fillAmount += 0.055f;

        if (currentDiamonds >= 18) {
            //win the game
            ServerGameManager.instance.serverView.RPC("WonGame", RpcTarget.All, MainGameManager.instance.spotNumber, MainGameManager.instance.username);
        }
    }


    public void LoseDiamond(int amountLost) {
        for (int i = 0; i < amountLost; i++) {
            currentDiamonds -= 1;
            scoreBar.fillAmount -= 0.055f;
        }
    }

    public void EndCutscene(int winnerViewID) {

        //stall time for 3 seconds saying that its finished
        //minecraft background and the winner in the middle
        //username over them
        //winner
        //camera back and fourth side to side

        PhotonView.Find(winnerViewID).GetComponentInChildren<PlayerController>().gameObject.transform.position = winnerPosition.position;

        if (PhotonNetwork.IsMasterClient) {
            string animationName;

            switch (Random.Range(0,8)) {
                case 0:
                    animationName = "Idle_SexyDance";
                    break;
                case 1:
                    animationName = "Idle_LeaningAgaintWall";
                    break;
                case 2:
                    animationName = "Idle_HandOnHips";
                    break;
                case 3:
                    animationName = "Idle_SittingOnGround";
                    break;
                case 4:
                    animationName = "Idle_Smoking";
                    break;
                case 5:
                    animationName = "Idle_WipeMouth";
                    break;
                case 6:
                    animationName = "Idle_CheckWatch";
                    break;
                case 7:
                    animationName = "Idle_CrossedArms";
                    break;
                default:
                    animationName = "Idle_SexyDance";
                    break;
            }

            int backdropID = Random.Range(0, backdrops.Length);

            ServerGameManager.instance.serverView.RPC("PlayAnimationEnd", RpcTarget.All, winnerViewID, animationName, backdropID);
        }

        StartCoroutine(FinishAnnoucements());

    }

    public IEnumerator FinishAnnoucements() {
        uiScoreHolder.SetActive(false);
        yield return new WaitForSeconds(3);
        cutsceneCamera.gameObject.SetActive(true);
        cutsceneCamera.depth = 0;

        winnerTag.text = winnerName.text;
    }

    public void DisconnectPlayer () {
        PhotonNetwork.LeaveRoom();
    }

}
