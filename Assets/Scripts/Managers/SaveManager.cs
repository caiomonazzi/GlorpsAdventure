using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static void Save()
    {
        PlayerPrefs.SetInt("Saved_HP_Max", PlayerStats.Instance.HP.max);
        PlayerPrefs.SetInt("Saved_HP_Current", PlayerStats.Instance.HP.current);



        // Save keys as a string with a delimiter
        string keys = string.Join(",", PlayerStats.Instance.doorKeys.Keys);
        PlayerPrefs.SetString("Saved_DoorKeys", keys);

        PlayerPrefs.SetString("Saved_Level", SceneManager.GetActiveScene().name);

        Debug.Log("Game Saved");
        Debug.Log("HP: " + PlayerPrefs.GetInt("Saved_HP_Current") + "/" + PlayerPrefs.GetInt("Saved_HP_Max"));


        Debug.Log("Keys: " + PlayerPrefs.GetString("Saved_DoorKeys"));
        Debug.Log("Level: " + PlayerPrefs.GetString("Saved_Level"));
    }

    public static void Load()
    {
        PlayerStats.Instance.HP = new DoubleInt(PlayerPrefs.GetInt("Saved_HP_Max"), PlayerPrefs.GetInt("Saved_HP_Current"));

        // Load keys from the saved string
        string keys = PlayerPrefs.GetString("Saved_DoorKeys", "");
        if (!string.IsNullOrEmpty(keys))
        {
            PlayerStats.Instance.doorKeys.Clear();
            string[] keyValues = keys.Split(',');
            foreach (var key in keyValues)
            {
                int intKey = int.Parse(key);
                PlayerStats.Instance.doorKeys[intKey] = true;
            }
        }

        Debug.Log("Game Loaded");
        Debug.Log("HP: " + PlayerStats.Instance.HP.current + "/" + PlayerStats.Instance.HP.max);

        Debug.Log("Keys: " + string.Join(",", PlayerStats.Instance.doorKeys.Keys));
        Debug.Log("Level: " + PlayerPrefs.GetString("Saved_Level"));
    }
}