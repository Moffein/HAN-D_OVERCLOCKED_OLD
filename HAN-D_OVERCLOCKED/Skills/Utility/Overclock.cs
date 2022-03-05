using HandPlugin.Components;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.HANDOverclocked
{
    public class BeginOverclock : BaseState
    {
		public override void OnEnter()
        {
			base.OnEnter();
			this.overclockController = base.gameObject.GetComponent<OverclockController>();
			if (base.isAuthority)
			{
				if (base.characterMotor && !base.characterMotor.isGrounded)
				{
					base.SmallHop(base.characterMotor, BeginOverclock.shortHopVelocity);
				}
				if (this.overclockController)
				{
					this.overclockController.BeginOverclock();
				}
			}

			this.skillSlot = (base.skillLocator ? base.skillLocator.utility : null);
			if (this.skillSlot)
            {
				startStocks = this.skillSlot.stock;
				this.skillSlot.SetSkillOverride(this, BeginOverclock.cancelSkillDef, GenericSkill.SkillOverridePriority.Contextual);
				this.skillSlot.stock = Mathf.Min(skillSlot.maxStock, startStocks + 1);
			}
		}

        public override void OnExit()
		{
			if (this.skillSlot)
			{
				this.skillSlot.UnsetSkillOverride(this, BeginOverclock.cancelSkillDef, GenericSkill.SkillOverridePriority.Contextual);
				this.skillSlot.stock = startStocks;
				//this.skillSlot.DeductStock(1);
			}
			base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
			if ((!this.skillSlot || this.skillSlot.stock == 0) || (!overclockController || !overclockController.ovcActive))
			{
				this.beginExit = true;
			}
			if (this.beginExit)
			{
				this.timerSinceComplete += Time.fixedDeltaTime;
				if (this.timerSinceComplete > BeginOverclock.baseExitDuration)
				{
					this.outer.SetNextStateToMain();
				}
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		private float timerSinceComplete = 0f;
		private bool beginExit;
		private int startStocks = 0;

		public static float baseExitDuration = 0.3f;
        public static float shortHopVelocity = 22f;
		private OverclockController overclockController;
		private GenericSkill skillSlot;
		public static SkillDef cancelSkillDef;
	}

    public class CancelOverclock : BaseState
    {
		public override void OnEnter()
		{
			base.OnEnter();
			overclockController = base.gameObject.GetComponent<OverclockController>();
			if (base.isAuthority)
			{
				if (base.characterMotor != null)	//Manually exiting will always trigger the shorthop regardless of grounded status.
				{
					base.SmallHop(base.characterMotor, BeginOverclock.shortHopVelocity);
				}
				if (overclockController)
				{
					overclockController.EndOverclock();
				}
				this.outer.SetNextStateToMain();
			}
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}

		private OverclockController overclockController;
	}
}
