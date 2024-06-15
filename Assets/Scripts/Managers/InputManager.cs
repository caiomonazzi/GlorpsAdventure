using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class InputManager
    {
    //List of Controls 
    public static bool Attack { get { return Input.GetKeyDown(InputSettings.AttackKey); } } // Melee attack button
    public static bool RangedAttack { get { return Input.GetKeyDown(InputSettings.RangedAttackKey); } } // Ranged attack button
    public static bool MoveLeft { get { return Input.GetKeyDown(KeyCode.A); } } // Left
    public static bool MoveRight { get { return Input.GetKeyDown(KeyCode.D); } } // Right
    public static bool Jump { get { return Input.GetKeyDown(InputSettings.JumpKey); } } // Jump with W
    public static bool Crouch { get { return Input.GetKey(InputSettings.CrouchKey); } } // Crouch with S
    public static bool Pause { get { return Input.GetKeyDown(InputSettings.PauseKey); } set { } }

    // ****************************************************************************************************************************************************************************************************** //
    public static bool Interaction { get { return Input.GetKeyDown(InputSettings.InteractionKey); } set { } }

    } // ** THIS ONE AS WELL FOR DEBUGGING REASONS, IT WILL USE THE ATTACK --> Melee Attack Button.
    // ****************************************************************************************************************************************************************************************************** //

public class InputSettings
{
    // List of KeyCodes
    public static KeyCode AttackKey { get { return KeyCode.Space; } } // String for Unity Input system (Edit -> Project settings -> Input)
    public static KeyCode RangedAttackKey { get { return KeyCode.LeftControl; } } // String for Unity Input system (Edit -> Project settings -> Input)
    public static KeyCode PauseKey { get { return KeyCode.Escape; } }

    // [ ******* ] ** IT WILL BE INTERACTIN IN THE INVENTORY WITH THE ITEM
    public static KeyCode HealthKey { get { return KeyCode.Q; } }
    // [ ******* ]

    public static KeyCode JumpKey { get { return KeyCode.W; } }
    public static KeyCode CrouchKey { get { return KeyCode.S; } }

    // [ ******* ] ** WILL GET EXCHANGED FOR THE MELEEATTACK BUTTON
    public static KeyCode InteractionKey { get { return KeyCode.E; } } // Interaction Key
}   // [ ******* ]   