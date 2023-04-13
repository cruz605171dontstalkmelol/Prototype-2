using UnityEngine;

public class MainGameManager : MonoBehaviour {

    public static MainGameManager instance;

    private void Awake() { if (instance == null) { instance = this; DontDestroyOnLoad(this.gameObject); } else { Destroy(this.gameObject); } }

    public string username;
    public int spotNumber;
    public int teamID;
    public bool hasReadiedUp;

    public int theRandomNumber;
    public Vector3 theRandomPosition;

    public void ResetVariables () {
        username = "";
        spotNumber = 0;
        teamID = 0;
        hasReadiedUp = false;
    }

}
