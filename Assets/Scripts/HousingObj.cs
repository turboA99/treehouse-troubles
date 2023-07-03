using UnityEngine;

[CreateAssetMenu(fileName = "Housing", menuName = "ScriptableObjects/Housing", order = 1)]
public class HousingObj : ScriptableObject
{
    public Texture2D picture;
    public string title = "title";
    public float price = 100;
    public string description = "descripton";
}
