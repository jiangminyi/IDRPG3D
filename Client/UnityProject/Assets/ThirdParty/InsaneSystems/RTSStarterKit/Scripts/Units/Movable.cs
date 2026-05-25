using UnityEngine;
using UnityEngine.AI;

namespace InsaneSystems.RTSStarterKit
{
	public class Movable : Module
	{
		protected NavMeshAgent navMeshAgent;

		public event MoveAction startMoveEvent;
		public event MoveAction stopMoveEvent;

		public bool isMoving { get; protected set; }
		public float sqrDistanceFineToStop { get; protected set; }
		public float customSpeed { get; protected set; }
		public bool useCustomSpeed { get; protected set; }
		
		Wheels wheelsModule;

		Vector3 lastMovePosition, destination;

		public delegate void MoveAction();
		
		float airUnitMoveProblemTime;

		protected override void Awake()
		{
			base.Awake();

			navMeshAgent = gameObject.AddComponent<NavMeshAgent>();

			navMeshAgent.speed = selfUnit.data.moveSpeed;
			navMeshAgent.angularSpeed = selfUnit.data.rotationSpeed;

			var boxCollider = GetComponent<BoxCollider>();

			if (boxCollider)
			{
				navMeshAgent.radius = ((boxCollider.size.x + boxCollider.size.z) / 2f) / 2f;
			}
			else
			{
				var sphereCollider = GetComponent<SphereCollider>();

				if (sphereCollider)
					navMeshAgent.radius = sphereCollider.radius;
			}

			sqrDistanceFineToStop = 1.5f;

			Stop();
		}

		void Start()
		{
			if (!selfUnit.data.hasMoveModule)
				Debug.LogWarning("[Movable module] Unit " + name + " has disabled Has Move module toggle, but Movable module still added to prefab. Fix this.");

			wheelsModule = selfUnit.GetModule<Wheels>();

			if (selfUnit.data.moveType == UnitData.MoveType.Flying)
				navMeshAgent.enabled = false;
		}

		void Update()
		{
			if (destination != transform.position)
			{
				if (wheelsModule && isMoving)
					wheelsModule.RotateWheelsForward();

				// todo: проверять, находится ли цель правее или левее и поворачивать колёса в ту сторону.

				if (!IsAttackableAllowsMove())
					Stop();
				else
					MoveToPosition(lastMovePosition);

				if ((transform.position - destination).sqrMagnitude <= sqrDistanceFineToStop)// reached destination
					Stop();
			}
			else if (isMoving)
			{
				isMoving = false;

				if (stopMoveEvent != null)
					stopMoveEvent.Invoke();
			}

			if (selfUnit.data.moveType == UnitData.MoveType.Flying)
			{
				var selfPosition = transform.position;
				selfPosition.y = selfUnit.data.flyingFlyHeight;
				transform.position = selfPosition;

				if (!isMoving)
					airUnitMoveProblemTime = 0;
			}
		}

		public void SetCustomSpeed(float speed, bool useSpeed)
		{
			useCustomSpeed = useSpeed;
			customSpeed = speed;
			
			if (navMeshAgent)
				navMeshAgent.speed = useCustomSpeed ? customSpeed : selfUnit.data.moveSpeed;
		}

		public void MoveToPosition(Vector3 position)
		{
			if (destination == position || position == transform.position)
				return;

			if (selfUnit.isBeingCarried || selfUnit.isMovementLockedByHotkey)
				return;

			if (IsAttackableAllowsMove())
			{
				destination = position;

				if (navMeshAgent.enabled)
					navMeshAgent.destination = destination;
			}

			if (selfUnit.data.moveType == UnitData.MoveType.Flying && destination != transform.position)
			{
				destination.y = transform.position.y;

				var direction = (destination - transform.position).normalized;

				//var isObstacle = !Physics.Raycast(transform.position, direction, 1.5f);
				
				//if (true)
				//{
				transform.position += direction * ((useCustomSpeed ? customSpeed : selfUnit.data.moveSpeed) * Time.deltaTime);

				airUnitMoveProblemTime = Mathf.Clamp(airUnitMoveProblemTime - Time.deltaTime, 0f, 2f);
				//}
				/*
				else
				{
					transform.position += transform.right * Time.deltaTime * selfUnit.data.moveSpeed / 2f;

					airUnitMoveProblemTime += Time.deltaTime;
					
					if (Vector3.Distance(transform.position, destination) < 3f)
						destination += Vector3.right * Time.deltaTime; // if unit too close to destination and has problem to move to it, so we need little move side his destination.

					if (airUnitMoveProblemTime > 2f) // too long move problem so just stopping.
					{
						destination = transform.position;
						selfUnit.EndCurrentOrder();
					}
				}
				*/

				if (direction != Vector3.zero)
					transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * selfUnit.data.rotationSpeed / 360f);
			}

			lastMovePosition = position;

			isMoving = true;

			if (startMoveEvent != null)
				startMoveEvent.Invoke();
		}

		public void Stop()
		{
			destination = transform.position;

			if (navMeshAgent.enabled)
				navMeshAgent.destination = destination;

			isMoving = false;

			if (stopMoveEvent != null)
				stopMoveEvent.Invoke();
		}

		bool IsAttackableAllowsMove()
		{
			return true;

			// todo check what is wrong with this check below
			if (!selfUnit.attackable)
				return true;

			return selfUnit.attackable.CanMove();
		}

		void OnTriggerStay(Collider other)
		{
			PushUnitFromCollider(other);
		}

		void PushUnitFromCollider(Collider other)
		{
			if (selfUnit.data.moveType != UnitData.MoveType.Flying || isMoving)
				return;

			var otherUnit = other.GetComponent<Unit>();

			if (otherUnit)
			{
				var otherDirection = (other.transform.position - transform.position).normalized;

				transform.position -= otherDirection * Time.deltaTime;

				if (!isMoving)
					destination = transform.position;
			}
		}
	}
}