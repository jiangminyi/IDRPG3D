using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	[System.Serializable]
	public abstract class Order
	{
		public Unit executor;

		public virtual void Execute() { End(); }
		public virtual void End() { executor.EndCurrentOrder(); }
		public abstract Order Clone();

		protected virtual Vector3 GetActualMovePosition() { return Vector3.zero; }
	}

	[System.Serializable]
	public class AttackOrder : Order
	{
		public Unit attackTarget;

		public override void Execute()
		{
			if (!executor.attackable || !attackTarget)
			{
				End();
				return;
			}

			if (executor.movable)
			{
				if (!executor.attackable.IsFireLineFree(attackTarget) ||
				    !executor.attackable.IsTargetInAttackRange(attackTarget))
					executor.movable.MoveToPosition(attackTarget.transform.position);
				else
					executor.movable.Stop();
			}

			// todo extend attackTarget
		}

		public override Order Clone()
		{
			AttackOrder order = new AttackOrder();
			order.attackTarget = attackTarget;

			return order;
		}

		protected override Vector3 GetActualMovePosition() { return attackTarget.transform.position; }
	}

	[System.Serializable]
	public class MovePositionOrder : Order
	{
		public Vector3 movePosition;

		public override void Execute()
		{
			if (!executor || !executor.movable)
			{
				if (!executor.movable)
					Debug.LogWarning("Movable order given to wrong unit.");

				End();
				return;
			}

			executor.movable.MoveToPosition(movePosition); // todo do it once at start

			var movePosWithSameY = movePosition;
			movePosWithSameY.y = executor.transform.position.y;
			
			if ((executor.transform.position - movePosWithSameY).sqrMagnitude <= executor.movable.sqrDistanceFineToStop)
				End();
		}

		public override Order Clone()
		{
			MovePositionOrder order = new MovePositionOrder();
			order.movePosition = movePosition;

			return order;
		}

		protected override Vector3 GetActualMovePosition() { return movePosition; }
	}

	[System.Serializable]
	public class FollowOrder : Order
	{
		public Transform followTarget;

		public override void Execute()
		{
			if (!followTarget)
			{
				End();
				return;
			}

			executor.movable.MoveToPosition(followTarget.position);

			// todo change it to make set movable target to follow target
		}

		public override Order Clone()
		{
			FollowOrder order = new FollowOrder();
			order.followTarget = followTarget;

			return order;
		}

		protected override Vector3 GetActualMovePosition() { return followTarget.transform.position; }
	}
}