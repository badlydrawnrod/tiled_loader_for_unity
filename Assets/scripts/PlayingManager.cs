using UnityEngine;
using System.Collections;

public class PlayingManager : MonoBehaviour
{
    public int StartingLevel;

    private LevelLoader levelLoader;

    void Awake()
    {
        levelLoader = GetComponent<LevelLoader>();
    }

    void Start()
    {
        levelLoader.LoadLevel(StartingLevel);
    }
}
