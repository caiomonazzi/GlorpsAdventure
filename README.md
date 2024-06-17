# GlorpsAdventure
 KPU_2D_GameboyAssignment

Character Control
Character.cs
  The Character class manages the player's character, handling movement, states, attacking, and interactions with the game world.

Key Features
  Movement: Walking, running, jumping, and climbing with smooth transitions and animations.
  runSpeed (float): Speed at which the character runs.
  walkSpeed (float): Speed at which the character walks.
  climbSpeed (float): Speed at which the character climbs.
  jumpForce (float): Force applied when the character jumps.
  crouchSpeed (float): Speed multiplier when the character is crouching.
  movementSmoothing (float): Smoothing factor for movement transitions.
  airControl (bool): Determines if the character can move while in the air.
  groundCheck (Transform): Transform used to check if the character is grounded.
  whatIsGround (LayerMask): Layer mask to identify ground objects.
  crouchDisableCollider (Collider2D): Collider disabled when crouching.
Events
  OnLandEvent (UnityEvent): Event triggered when the character lands on the ground
Combat: Melee and ranged attacks with cooldowns and animations.
   Attack(): Handles character attacking.
   HandleRangedAttack(): Manages ranged attacks.
   HandleAttackRoutine(): Coroutine for handling attack animations and cooldowns.
   State Management: Efficient state changes based on character actions.
   UpdateState(): Updates the character's state based on current conditions.
   ChangeState(CharacterState newState): Changes the character's state.
Damage Handling: Fall damage calculation and application.
   Fall Damage
   fallDamageMultiplier (float): Multiplier for calculating fall damage.
   lastYPosition (float): Tracks the last Y position for fall detection.
   highestYPosition (float): Tracks the highest Y position during a jump or fall.
Input Handling: Handles various inputs to control the character.
   HandleInputs(): Handles various player inputs.
   HandleAttackInput(): Manages attack input and initiates attack.
   HandleRunning(): Manages running input and logic.
   HandleWalking(): Manages walking input and logic.
   HandleClimbingInput(): Manages climbing input and logic.
   
Dependencies
    The Character script depends on several other scripts:
   AttackController: Manages melee attack behavior.
   WeaponController: Manages ranged attack behavior and weapon handling.
   PlayerStats: Manages player's health, damage, and death.
   UIManager: Handles UI updates based on the character's state and actions.
   InputManager: Handles input detection for character control.

Utilities
   Flip(): Flips the character's direction.
   OnLanding(): Handles landing logic and resets jump state.
   CheckGroundedStatus(): Checks if the character is grounded.

AttackController.cs
   Handles melee attacks for the character.
 Key Features
   Melee attack logic
   Attack cooldown
   Damage application
   
WeaponController.cs
   Manages the character's weapons, including equipping, aiming, and shooting.
 Key Features
   Weapon equipping and removing
   Aiming at targets
   Performing ranged attacks
   
PlayerStats.cs
   Manages the player's stats, health, and inventory.
 Key Features
   Health management
   Damage application and healing
   Handling player death
   
PlayerCamera.cs
   Handles the camera follow behavior for the player's character.
 Key Features
   Smooth following of the player character
   Adjustable offset
   
InputManager.cs
   Handles the input settings and key bindings for the game.
 Key Features
   Input detection for various actions (movement, attack, jump, etc.)
