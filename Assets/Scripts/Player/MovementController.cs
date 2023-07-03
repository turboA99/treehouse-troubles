using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    // Variables used to reference player input and character controller.
    [SerializeField] private CinemachineFreeLook freeLook;
    [SerializeField] private AudioClip audioGliding;
    [SerializeField] private AudioClip audioJump;
    [SerializeField] private AudioClip audioRunningWood;
    [SerializeField] private AudioClip audioRunningGrass;

    private CharacterController _characterController;
    private Animator _animator;
    private PlayerInput _playerInput;

    // Variables for movement. Should be edited from the inspector.
    public float RotationFactorPerFrame = 5.0f;
    public float MoveSpeed = 15.0f;
    public float Acceleration = 0.005f;

    // Private variables for movement.
    private Vector3 _velocity;

    // Variables for gravity on the ground and in the air.
    private float GroundedGravity = -0.05f;
    private float Gravity = -9.8f;
    public float GlideGravity = -5f;

    // Variables for movement input.
    private Vector2 _currentMovementInput;
    private Vector3 _currentMovement;
    private Vector3 _appliedMovement;
    private Vector3 _cameraRelativeMovement;
    private bool _isMovementPressed;

    // Variables for the properties of the player's jump. Should be edited in the editor.
    public float MaxJumpTime = 1f;
    public float MaxJumpHeight = 6.0f;
    public float jumpButtonGracePeriod;

    // Variables for jumping.
    private bool _isJumping;
    private bool _isJumpPressed = false;
    private float _initialJumpVelocity;
    private float? lastGroundedTime;

    // Variables for gliding.
    private bool _isGlidePressed;

    private int isRunningHash;
    private int isJumpingHash;
    private int isGlidingHash;

    private float ySensitivity;
    private float xSensitivity;

    public void SetSensitivity(float sensitivity)
    {
        freeLook.m_XAxis.m_MaxSpeed = xSensitivity * sensitivity;
        freeLook.m_YAxis.m_MaxSpeed = ySensitivity * sensitivity;
    }

    // Called when the object is first initialized into the scene.
    private void Start()
    {
        // Creating a new player input for the script and getting the player's character controller.
        _playerInput = GetComponent<PlayerInput>();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        // Initializes player input.

        _playerInput.actions.FindAction("Move").started += OnMovementInput;
        _playerInput.actions.FindAction("Move").canceled += OnMovementInput;
        _playerInput.actions.FindAction("Move").performed += OnMovementInput;
        _playerInput.actions.FindAction("Jump").started += OnJump;
        _playerInput.actions.FindAction("Jump").canceled += OnJump;
        _playerInput.actions.FindAction("Glide").started += OnGlide;
        _playerInput.actions.FindAction("Glide").canceled += OnGlide;
        _playerInput.actions.FindAction("Phone").started += OpenPhone;

        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        SetupJumpVariables();
        ySensitivity = freeLook.m_YAxis.m_MaxSpeed;
        xSensitivity = freeLook.m_XAxis.m_MaxSpeed;
        SetSensitivity(GameManager.instance.currentSettings.sensitivity);
    }

    // Called every frame.
    private void FixedUpdate()
    {
        HandleRotation();

        // Calculations for steering to create acceleration when the player starts moving.
        Vector3 desiredVelocity;
        desiredVelocity.x = _currentMovement.x;
        desiredVelocity.z = _currentMovement.z;
        Vector3 steeringVector;
        steeringVector.x = desiredVelocity.x - _velocity.x;
        steeringVector.z = desiredVelocity.z - _velocity.z;
        _velocity.x += steeringVector.x * Acceleration;
        _velocity.z += steeringVector.z * Acceleration;

        // Checks if movement keys are not pressed and if the player is on the ground. If yes, decreases acceleration significantly to make sure the
        // player slows down much faster than they start moving to give the player greater control.
        if (!_isMovementPressed && _characterController.isGrounded)
        {
            _velocity.x += steeringVector.x * Acceleration * 10;
            _velocity.z += steeringVector.z * Acceleration * 10;
        }

        if(_characterController.isGrounded && _isMovementPressed && !_isJumping)
        {
            AudioClip clip = audioRunningWood;
            var colliders = Physics.OverlapSphere(transform.position, .5f);
            foreach (Collider collider in colliders)
            {
                if (collider.tag == "Grass") clip = audioRunningGrass;
            }
            if (!GetComponent<AudioSource>().isPlaying || GetComponent<AudioSource>().clip != clip)
            {
                 GetComponent<AudioSource>().clip = clip;
                 GetComponent<AudioSource>().Play();
            }
            GetComponent<AudioSource>().UnPause();
        }

        _appliedMovement.x = _velocity.x;
        _appliedMovement.z = _velocity.z;

        _cameraRelativeMovement = ConvertToCameraSpace(_appliedMovement);

        _characterController.Move(_cameraRelativeMovement * Time.deltaTime);

        HandleGravity();
        HandleJump();
        HandleGlide();
        HandleAnimation();
    }

    private void HandleRotation()
    {
        // Sets the variable that defines the position to rotate the player towards.
        Vector3 positionToLookAt;
        positionToLookAt.x = _cameraRelativeMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = _cameraRelativeMovement.z;

        Quaternion currentRotation = transform.rotation;

        // If a movement key is pressed, look towards the direction the character is heading towards.
        // Multiplied by the RotationFactorPerFrame to make turning smooth.
        if (_isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, RotationFactorPerFrame * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        bool isFalling = _currentMovement.y <= 0.0f || !_isJumpPressed;
        // Multiplier to increase gravity if the player is falling.
        float fallMultiplier = 2.0f;

        // If the character is on the ground, set the gravity to GroundedGravity.
        // Otherwise, if the character is falling, set the gravity to gravity and multiply it by the fallMultiplier.
        // The resulting value is clamped to make sure that falling speed does not become ridiculously high if falling from a relatively high spot.
        // If the player is ascending, simply set the gravity to Gravity.
        if (_characterController.isGrounded)
        {
            _animator.SetBool(isJumpingHash, false);
            _currentMovement.y = GroundedGravity;
            _appliedMovement.y = GroundedGravity;
        }
        else if (isFalling)
        {
            // The old velocity and the new velocity are added together and then multiplied by .5 to get an average
            // then by deltatime. This makes sure that the jump does not change based on framerate and is always consistent.
            float previousYVelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (Gravity * fallMultiplier * Time.deltaTime);
            _appliedMovement.y = Mathf.Max((previousYVelocity + _currentMovement.y) * .5f, -20.0f);
        }
        else if (_isJumping)
        {
            float previousYVelocity = _currentMovement.y;
            _currentMovement.y = _currentMovement.y + (Gravity * Time.deltaTime);
            _appliedMovement.y = (previousYVelocity + _currentMovement.y) * .5f;
        }
    }

    private void HandleJump()
    {
        if(_characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        // Checks if the player is not already jumping, if they are on the ground, and if the jump button is pressed.
        // If yes, the player's Y movement is set to the _initialJumpVelocity variable as set in SetJumpVariables().
        // Otherwise, the _isJumping variable is reset back to false (if the player is on the ground but the jump button hasn't been pressed.)
        if (!_isJumping && Time.time - lastGroundedTime <= jumpButtonGracePeriod && _isJumpPressed)
        {
            _animator.SetBool(isJumpingHash, true);
            _isJumping = true;
            _currentMovement.y = _initialJumpVelocity;
            _appliedMovement.y = _initialJumpVelocity;
            lastGroundedTime = null;
        } else if (!_isJumpPressed && _isJumping && _characterController.isGrounded) {
            _isJumping = false;
        }
    }

    private void HandleGlide()
    {
        if (_isGlidePressed && _isMovementPressed)
        {
            _currentMovement.y = GlideGravity;
            _appliedMovement.y = GlideGravity;
        }
    }

    private void HandleAnimation()
    {
        bool isRunning = _animator.GetBool(isRunningHash);
        bool isJumping = _animator.GetBool(isJumpingHash);

        if (_isMovementPressed && !isRunning)
        {
            _animator.SetBool(isRunningHash, true);
        } else if (!_isMovementPressed && isRunning)
        {
            _animator.SetBool(isRunningHash, false);
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        // Returns true if the jump button is pressed.
        _isJumpPressed = context.ReadValueAsButton();
        if (context.started)
        {
            GetComponent<AudioSource>().Stop();
            GetComponent<AudioSource>().clip = audioJump;
            GetComponent<AudioSource>().Play();
        }
    }

    private void OnGlide(InputAction.CallbackContext context)
    {
        // Returns true if the glide button is pressed.
        _isGlidePressed = context.ReadValueAsButton();
        if (context.started)
        {
            GetComponent<AudioSource>().clip = audioGliding;
            GetComponent<AudioSource>().Play();

        } 
        else if(context.performed || context.canceled)
        {
            GetComponent<AudioSource>().Stop();
        }
    }

    private void OnMovementInput (InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x * MoveSpeed;
        _currentMovement.z = _currentMovementInput.y * MoveSpeed;
        _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
        if (context.performed || context.canceled)
        {
            GetComponent<AudioSource>().Pause();
        }
    }

    private void OpenPhone(InputAction.CallbackContext context)
    {
        UI.instance.SetPhoneOpened(true);
        _playerInput.SwitchCurrentActionMap("UI");
    }

    public void SwitchDialogue(bool active)
    {
        if (active)
        {
            _playerInput.SwitchCurrentActionMap("Dialogue");
        } else
        {
            _playerInput.SwitchCurrentActionMap("CharacterControls");
        }
    }

    // Sets up jumping variables when the player is initialized in the scene.
    private void SetupJumpVariables()
    {
        float timeToApex = MaxJumpTime / 2;
        Gravity = (-2 * MaxJumpHeight) / Mathf.Pow(timeToApex, 2);
        _initialJumpVelocity = (2 * MaxJumpHeight) / timeToApex;
    }

    // Takes a given Vector3 and returns a rotate vector relative to the camera. This is to make movement relative to the camera's orientation instead of the world's.
    private Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
    {
        // Holds current Y value for the vector to rotate to make sure jumping is not messed up.
        float currentYValue = vectorToRotate.y;

        // Gets the forward orientation and the right orientation of the camera.
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        // Sets the Y axis of both forward and right to 0.
        cameraForward.y = 0;
        cameraRight.y = 0;

        // Normalizes both axis to make sure diagonal movement is not faster.
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        // Multiplies the vector to rotate's Z and X axis by the cameraForward and cameraRight vectors.
        Vector3 cameraForwardZProduct = vectorToRotate.z * cameraForward;
        Vector3 cameraRightXProduct = vectorToRotate.x * cameraRight;

        // Adds both vectors together to get the rotated vector and sets the rotated vector's Y value to the original Y value.
        Vector3 vectorRotatedToCameraSpace = cameraForwardZProduct + cameraRightXProduct;
        vectorRotatedToCameraSpace.y = currentYValue;
        return vectorRotatedToCameraSpace;
    }
}
