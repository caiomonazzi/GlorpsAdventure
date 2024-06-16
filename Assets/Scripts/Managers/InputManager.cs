using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    //List of Controls 
    public static bool Attack { get { return Input.GetKeyDown(InputSettings.AttackKey); } } // Melee attack button
    public static bool MoveLeft { get { return Input.GetKeyDown(KeyCode.A); } } // Left
    public static bool MoveRight { get { return Input.GetKeyDown(KeyCode.D); } } // Right
    public static bool Jump { get { return Input.GetKeyDown(InputSettings.JumpKey); } } // Jump with W
    public static bool Crouch { get { return Input.GetKey(InputSettings.CrouchKey); } } // Crouch with S
    public static bool Pause { get { return Input.GetKeyDown(InputSettings.PauseKey); } set { } }
    public static bool Interaction { get { return Input.GetKeyDown(InputSettings.InteractionKey); } set { } }
}
    

public class InputSettings
{
    // List of KeyCodes // (Edit -> Project settings -> Input)
    public static KeyCode AttackKey { get { return KeyCode.Space; } } 
    public static KeyCode PauseKey { get { return KeyCode.Escape; } }
    public static KeyCode JumpKey { get { return KeyCode.W; } }
    public static KeyCode CrouchKey { get { return KeyCode.S; } }
    public static KeyCode InteractionKey { get { return KeyCode.E; } } // Interaction Key
}  