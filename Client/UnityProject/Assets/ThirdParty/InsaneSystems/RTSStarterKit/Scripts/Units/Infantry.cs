using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class Infantry : Module
	{
		[SerializeField] Animator animator;

		private void Start()
		{
			if (!animator)
				animator = GetComponent<Animator>();

			if (!animator)
			{
				Debug.LogWarning("Infantry soldier " + name + " does not have Animator component! It will have NO animations, if you're not add it.");
				return;
			}

			if (!animator.runtimeAnimatorController && selfUnit.data.animatorController)
				animator.runtimeAnimatorController = selfUnit.data.animatorController;

			selfUnit.GetModule<Attackable>().startAttackEvent += OnStartAttack;
			selfUnit.GetModule<Attackable>().stopAttackEvent += OnStopAttack;
			selfUnit.GetModule<Movable>().startMoveEvent += OnStartMove;
			selfUnit.GetModule<Movable>().stopMoveEvent += OnStopMove;
			selfUnit.GetModule<Damageable>().damageableDiedEvent += OnDie;
		}

		public void OnStartAttack()
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool("Attack", true);
		}

		public void OnStartMove()
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool("Move", true);
		}

		public void OnStopMove()
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool("Move", false);
		}

		public void OnStopAttack()
		{
			if (animator.isActiveAndEnabled)
				animator.SetBool("Attack", false);
		}

		public void OnDie(Unit unit)
		{
			animator.transform.SetParent(null);
			if (animator.isActiveAndEnabled)
				animator.SetBool("Die", true);
		}
	}
}