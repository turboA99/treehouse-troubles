using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/Dialogue", order = 2)]
public class DialogueObj : ScriptableObject
{
    public string[] lines;
    public AudioClip dialogueNoise;
    public bool endGame;
}
