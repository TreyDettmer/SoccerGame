using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MatchSettings : MonoBehaviour
{

    public static MatchSettings instance;
    public List<PlayerInput> team0PlayerInputs;
    public List<PlayerInput> team1PlayerInputs;
    public int numberOfAiOnTeam0 = 1;
    public int numberOfAiOnTeam1 = 1;
    public float gameLengthInMinutes = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
