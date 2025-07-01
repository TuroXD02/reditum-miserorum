using UnityEngine;

[CreateAssetMenu(fileName = "New Bestiary Entry", menuName = "Bestiary/Entry")]
public class BestiaryEntry : ScriptableObject
{
    public string enemyID;
    public string enemyName;
    public Sprite enemyIcon;
    [TextArea] public string description;

}