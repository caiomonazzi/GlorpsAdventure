using UnityEngine;
using UnityEngine.UI;

public class InteractionCanvas : MonoBehaviour
{
    // Button
    public string interaction { get { return InputSettings.InteractionKey.ToString(); } }

    [Header("Components")]
    public Text InteractionText;

    [Header("Settings")]
    public bool useAssignedText = true; // Flag to toggle between input key and assigned text

    private void Update()
    {
        if (useAssignedText)
        {
            // Do nothing, use the assigned text
        }
        else
        {
            InteractionText.text = interaction; // Assign the text in the UI to our button from InputSettings
        }
    }
}
