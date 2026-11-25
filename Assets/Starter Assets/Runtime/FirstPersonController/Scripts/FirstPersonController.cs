using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 6.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// ================= [LD] DODATKI DO MECHANIK LEVEL DESIGNU =================
		// Ponizsze pola i metody zostaly dopisane na potrzeby zajec (hak, ruchome platformy,
		// zamiana cial, zmiana skali, teleportacja do punktu kontrolnego).
		[Header("LD: dodatkowy ped i platformy")]
		[Tooltip("Jak szybko wygasa dodatkowy ped (np. po puszczeniu haka), gdy postac stoi na ziemi. Wieksza wartosc = krotszy poslizg")]
		public float GroundFriction = 8.0f;
		[Tooltip("Jak szybko wygasa dodatkowy ped w powietrzu. 0 = ped w locie nie wygasa")]
		public float AirDrag = 0.0f;
		[Tooltip("Czy postac ma jezdzic razem z ruchomymi platformami, na ktorych stoi")]
		public bool FollowMovingPlatforms = true;
		[Tooltip("Czy ruchome obiekty (drzwi, platformy) maja wypychac postac, gdy na nia najada")]
		public bool PushedByMovingObjects = true;

		/// <summary>[LD] Gdy true, kontroler NIE porusza postacia (robi to np. hak). Kamera dalej dziala.</summary>
		public bool ExternalControl { get; set; }

		/// <summary>[LD] Aktualna predkosc postaci (m/s) - ruch z klawiszy + dodatkowy ped + pion.</summary>
		public Vector3 Velocity
		{
			get { return _inputVelocity + _externalVelocity + new Vector3(0.0f, _verticalVelocity, 0.0f); }
		}

		// [LD] dodatkowy ped poziomy i ruch z klawiszy z ostatniej klatki
		private Vector3 _externalVelocity;
		private Vector3 _inputVelocity;
		private float _noFrictionTimer;

		// [LD] ruchoma platforma pod stopami
		private Transform _platform;
		private Vector3 _platformLocalPoint;
		private Vector3 _platformWorldPoint;
		private float _platformYaw;
		private Vector3 _platformVelocity;
		private readonly Collider[] _overlapBuffer = new Collider[16];

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = -53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

	
#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			// [LD] wejscie moglo zostac juz podane z zewnatrz (mechanika zamiany cial)
			if (_input == null) _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			// [LD] najpierw przesuwamy postac razem z platforma, na ktorej stoi
			FollowPlatform();

			// [LD] gdy postacia steruje cos innego (np. hak), pomijamy zwykly ruch
			if (!ExternalControl)
			{
				JumpAndGravity();
			}
			GroundedCheck();
			if (!ExternalControl)
			{
				Move();
			}

			// [LD] zapamietujemy platforme pod stopami i wypychamy postac z ruchomych obiektow
			TrackPlatform();
			PushOutOfMovingObjects();
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = _input.lookFromTouch ? Time.deltaTime : 1.0f;

                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			// [LD] oryginalnie brana byla predkosc CharacterControllera, ale zawiera ona tez dodatkowy ped
			// (hak, platformy), wiec liczymy tylko predkosc pochodzaca z klawiszy
			float currentHorizontalSpeed = _speed;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			// [LD] do ruchu z klawiszy dodajemy dodatkowy ped (np. po puszczeniu haka)
			_inputVelocity = inputDirection.normalized * _speed;
			CollisionFlags flags = _controller.Move((_inputVelocity + _externalVelocity) * Time.deltaTime + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// [LD] uderzenie glowa w sufit konczy wznoszenie
			if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0.0f)
			{
				_verticalVelocity = 0.0f;
			}

			// [LD] wygaszanie dodatkowego pedu: szybko na ziemi (tarcie), wolno w powietrzu
			if (_noFrictionTimer > 0.0f) _noFrictionTimer -= Time.deltaTime;
			float decay = (Grounded && _noFrictionTimer <= 0.0f) ? GroundFriction : AirDrag;
			if ((flags & CollisionFlags.Sides) != 0) decay = Mathf.Max(decay, 6.0f); // uderzenie w sciane wytraca ped
			_externalVelocity *= Mathf.Exp(-decay * Time.deltaTime);
			if (_externalVelocity.sqrMagnitude < 0.01f) _externalVelocity = Vector3.zero;
		}

		// ================= [LD] METODY DLA MECHANIK =================

		/// <summary>[LD] Podmienia zrodlo sterowania (WSAD, mysz, skok). Uzywane przez zamiane cial:
		/// postac, ktora nikt nie steruje, dostaje "puste" wejscie i stoi w miejscu.</summary>
		public void SetInput(StarterAssetsInputs input)
		{
			if (input != null) _input = input;
		}

		/// <summary>[LD] Ustawia predkosc postaci (np. puszczenie haka). Y = predkosc pionowa.</summary>
		public void SetVelocity(Vector3 velocity)
		{
			_externalVelocity = new Vector3(velocity.x, 0.0f, velocity.z);
			_verticalVelocity = velocity.y;
			_speed = 0.0f;
			_inputVelocity = Vector3.zero;
			if (velocity.y > 0.0f)
			{
				_noFrictionTimer = 0.25f; // nie hamuj, zanim postac oderwie sie od ziemi
				// trzymana spacja nie moze nadpisac wybicia zwyklym skokiem
				_jumpTimeoutDelta = 0.3f;
				if (_input != null) _input.jump = false;
			}
		}

		/// <summary>[LD] Dodaje predkosc do aktualnej (np. podmuch, odrzut).</summary>
		public void AddVelocity(Vector3 velocity)
		{
			_externalVelocity += new Vector3(velocity.x, 0.0f, velocity.z);
			// stojac na ziemi postac ma stale "docisk" -2 m/s; zerujemy go, zeby podmuch w gore mogl ja oderwac
			if (velocity.y > 0.0f && Grounded && _verticalVelocity < 0.0f) _verticalVelocity = 0.0f;
			_verticalVelocity += velocity.y;
			if (_verticalVelocity > 0.0f && velocity.y > 0.0f) _noFrictionTimer = 0.1f;
		}

		/// <summary>[LD] Natychmiast przenosi postac w inne miejsce (punkt kontrolny, teleport).</summary>
		public void Teleport(Vector3 position, float yaw)
		{
			if (_controller == null) _controller = GetComponent<CharacterController>();
			// CharacterController trzeba na chwile wylaczyc, inaczej zignoruje zmiane pozycji
			_controller.enabled = false;
			transform.SetPositionAndRotation(position, Quaternion.Euler(0.0f, yaw, 0.0f));
			_controller.enabled = true;
			Physics.SyncTransforms();

			_externalVelocity = Vector3.zero;
			_inputVelocity = Vector3.zero;
			_verticalVelocity = 0.0f;
			_speed = 0.0f;
			_platform = null;
			_cinemachineTargetPitch = 0.0f;
			if (CinemachineCameraTarget != null) CinemachineCameraTarget.transform.localRotation = Quaternion.identity;
		}

		// [LD] Przesuwa postac o tyle, o ile od poprzedniej klatki przesunela sie platforma pod stopami.
		private void FollowPlatform()
		{
			if (!FollowMovingPlatforms || _platform == null) return;

			Vector3 newWorldPoint = _platform.TransformPoint(_platformLocalPoint);
			Vector3 delta = newWorldPoint - _platformWorldPoint;
			if (delta.sqrMagnitude > 0.0000001f)
			{
				_controller.Move(delta);
				_platformVelocity = delta / Mathf.Max(Time.deltaTime, 0.0001f);
			}
			else
			{
				_platformVelocity = Vector3.zero;
			}

			// obrot platformy obraca tez postac
			float yawDelta = Mathf.DeltaAngle(_platformYaw, _platform.eulerAngles.y);
			if (Mathf.Abs(yawDelta) > 0.0001f) transform.Rotate(0.0f, yawDelta, 0.0f);
		}

		// [LD] Sprawdza, na czym stoi postac i zapamietuje to do nastepnej klatki.
		private void TrackPlatform()
		{
			Transform ground = null;
			if (FollowMovingPlatforms && Grounded && _verticalVelocity <= 0.0f)
			{
				Vector3 origin = transform.position + Vector3.up * (GroundedRadius + 0.1f);
				if (Physics.SphereCast(origin, GroundedRadius * 0.9f, Vector3.down, out RaycastHit hit, 0.5f, GroundLayers, QueryTriggerInteraction.Ignore))
				{
					ground = hit.collider.attachedRigidbody != null ? hit.collider.attachedRigidbody.transform : hit.collider.transform;
				}
			}

			// zeskok z jadacej platformy: zabieramy ze soba jej predkosc
			if (ground == null && _platform != null && !Grounded)
			{
				_externalVelocity += new Vector3(_platformVelocity.x, 0.0f, _platformVelocity.z);
			}

			_platform = ground;
			if (_platform != null)
			{
				_platformWorldPoint = transform.position;
				_platformLocalPoint = _platform.InverseTransformPoint(transform.position);
				_platformYaw = _platform.eulerAngles.y;
			}
		}

		// [LD] CharacterController sam z siebie nie jest wypychany przez ruchome obiekty
		// (zamykajace sie drzwi, platforma najezdzajaca z boku) - robimy to recznie.
		private void PushOutOfMovingObjects()
		{
			if (!PushedByMovingObjects || !_controller.enabled) return;

			// wymiary kapsuly przeliczone przez skale postaci (mechanika zmiany skali)
			float scale = transform.lossyScale.y;
			float radius = _controller.radius * scale;
			Vector3 center = transform.TransformPoint(_controller.center);
			float half = Mathf.Max(_controller.height * scale * 0.5f - radius, 0.0f);
			int count = Physics.OverlapCapsuleNonAlloc(center + Vector3.up * half, center - Vector3.up * half, radius, _overlapBuffer, GroundLayers, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < count; i++)
			{
				Collider other = _overlapBuffer[i];
				if (other == _controller || other.transform.IsChildOf(transform)) continue;
				// wypychaja nas tylko obiekty kinematyczne (drzwi, platformy); skrzynie mozna po prostu odepchnac
				Rigidbody body = other.attachedRigidbody;
				if (body == null || !body.isKinematic) continue;

				if (Physics.ComputePenetration(_controller, transform.position, transform.rotation, other, other.transform.position, other.transform.rotation, out Vector3 direction, out float distance))
				{
					if (distance > 0.01f) _controller.Move(direction * (distance + 0.005f));
				}
			}
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if above terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity > _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}