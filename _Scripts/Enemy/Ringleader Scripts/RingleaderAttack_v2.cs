﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inherits the enemy attack class.
/// Behaviour for the Ringleader's gambits.
/// </summary>
public class RingleaderAttack_v2 : EnemyAttack {

    /// <summary>
    /// Reference to its target's position.
    /// </summary>
	private Transform target;

	//to allow Ringleader control over the blocks
    /// <summary>
    /// Reference to the scene controller. Allows for the Ringleader to control the room's features such as ghost blocks.
    /// </summary>
	private SceneController sceneController;
    /// <summary>
    /// Reference to the Ringleader's animatable class.
    /// </summary>
	private RingleaderAnimatable ringleaderAnimatable;

    /// <summary>
    /// Reference to the Ringleader's health.
    /// </summary>
	private RingleaderHealth enemyHealth;
    /// <summary>
    /// Reference to the Ringleader's movement behaviour.
    /// </summary>
	private RingleaderMovement ringleaderMovement;
    /// <summary>
    /// Reference to the Ringleader's controller.
    /// </summary>
    private RingleaderController ringleaderController;
    /// <summary>
    /// Reference to the bullet hell game object. Part of the Ringleader's gambits.
    /// </summary>
    [SerializeField] private BulletHell bulletHell;
    /// <summary>
    /// Reference to the Ringleader's hazard. Serves as its melee attack.
    /// </summary>
	[SerializeField] private RingleaderNormalAttackHazard attackHazard;

    /// <summary>
    /// Cooldown in seconds before the Ringleader can start attacking.
    /// </summary>
	[SerializeField] private float atkCooldown = 2.5f;
    /// <summary>
    /// Duration in seconds of the bullet hell generated by the Ringleader.
    /// </summary>
	[SerializeField] private float bulletHellTime = 10.0f;

    /// <summary>
    /// Flag if the Ringleader is attacking.
    /// </summary>
	private bool isAttacking = false;
    /// <summary>
    /// Flag if the Ringleader is in cooldown from attacking.
    /// </summary>
	private bool cooldown = false;
    /// <summary>
    /// Ringleader's current HP.
    /// </summary>
	private int currentHp;

    /// <summary>
    /// Flag if the Ringleader is talking.
    /// </summary>
    private bool talkPause;

    /// <summary>
	/// Unity Function. Called once upon creation of the object.
	/// </summary>
    void Awake() {
		ringleaderMovement = GetComponent<RingleaderMovement> ();
		enemyHealth = GetComponent<RingleaderHealth> ();
		ringleaderAnimatable = GetComponent<RingleaderAnimatable> ();
        ringleaderController = GetComponent<RingleaderController>();
	}

    /// <summary>
	/// Standard Unity Function. Called once in the game object's lifetime to initiate the script once it has been enabled.
	/// </summary>
	void Start() {

        bulletHell.SetDamage (this.damage);
		this.currentHp = this.enemyHealth.GetHealth ();
        StartCoroutine(this.UpdateRoutine());

    }
   
    /// <summary>
    /// Coroutine that works like Unity's Update function.
    /// Runs the Ringleader's gambits while it is alive.
    /// </summary>
    /// <returns>None</returns>
    public IEnumerator UpdateRoutine() {
        while (this.enemyHealth.isAlive) {
            if (!talkPause) {
                if (this.currentHp != this.enemyHealth.GetHealth()) {
                    yield return StartCoroutine(this.ringleaderController.HitRoutine());
                    EventBroadcaster.Instance.PostEvent(EventNames.RETRY);
                    this.currentHp = this.enemyHealth.GetHealth();
                    
                    yield return StartCoroutine(this.ringleaderController.RetryRoutine());
                }
                    Random.Range(0, 2);

                    LookForPlayer();

                    ringleaderMovement.MoveTowards(target.position);
                    Attack();
                

                
            }
            yield return null;
        }
    }
    //void Update () {
    //       if (this.enemyHealth.isAlive) {
    //           if (!talkPause) {
    //               if (this.currentHp != this.enemyHealth.GetHealth()) {
    //                   EventBroadcaster.Instance.PostEvent(EventNames.RETRY);
    //                   this.currentHp = this.enemyHealth.GetHealth();
    //               }

    //               Random.Range(0, 2);

    //               LookForPlayer();

    //               ringleaderMovement.MoveTowards(target.position);

    //               Attack();
    //           }
    //       }
    //}

    /// <summary>
    /// Prompts the Ringleader will talk through a dialogue, pausing all action.
    /// </summary>
    public void TalkPause() {
        this.talkPause = true;
    }

    /// <summary>
    /// Resumes all action from the dialogue.
    /// </summary>
    public void Resume() {
        this.talkPause = false;
    }

    /// <summary>
    /// Look for the player avatar in the room to find its location.
    /// </summary>
    private void LookForPlayer() {
		if (this.target == null)
			this.target = FindObjectOfType<PlayerYuni> ().transform;
	}

    /// <summary>
    /// Preform the Ringleader's attack gambits.
    /// </summary>
	public override void Attack () {
		if (!isAttacking && !cooldown && !talkPause) {
			Debug.Log ("Attacking");
			int atk = 0;

			if (this.enemyHealth.GetHealth () < 3) {
				atk = Random.Range (0, 2);
			} else {
				atk = 0;
			}

			switch (atk) {
			case 0:
				Debug.Log ("Melee");
				StartCoroutine (MeleeAttacking ());
				break;
			case 1:
				Debug.Log ("Shoot");
				StartCoroutine (ShootAttack ());
				break;
			}
		}
	}

    /// <summary>
    /// Coroutine done when the Ringleader is performing a melee attack.
    /// </summary>
    /// <returns>None</returns>
	private IEnumerator MeleeAttacking() {
		isAttacking = true;
		ringleaderMovement.canMove = false;

		ringleaderMovement.TeleportToPlayer ();
		while (ringleaderMovement.IsTeleporting ())
			yield return null;

		this.ringleaderAnimatable.MeleeOpen();
		attackHazard.TriggerDamage (this.damage);
		while (ringleaderAnimatable.IsPlaying () && !talkPause) {
			yield return null;
		}

		isAttacking = false;
		ringleaderMovement.canMove = true;

		StartCoroutine (AttackCooldown ());
	}

    /// <summary>
    /// Coroutine done when the Ringleader is starting a bullet hell.
    /// </summary>
    /// <returns>None</returns>
	private IEnumerator ShootAttack() {
		isAttacking = true;
		ringleaderMovement.canMove = false;

		bulletHell.SetStyle (BulletHell.HellStyle.STRAIGHT_SHOTS);
		bulletHell.SetTarget (this.target);

		bulletHell.BeginHell ();

		ringleaderAnimatable.FlyOpen ();
		yield return new WaitForSeconds (this.bulletHellTime);

		bulletHell.EndHell ();
		ringleaderAnimatable.FlyClose ();
		while (ringleaderAnimatable.IsPlaying () && !talkPause) {
			yield return null;
		}

		ringleaderMovement.canMove = true;
		isAttacking = false;
		StartCoroutine (AttackCooldown ());
	}

    /// <summary>
    /// Coroutine done when the Ringleader has just finished an attack.
    /// </summary>
    /// <returns>None</returns>
	private IEnumerator AttackCooldown () {
		cooldown = true;
		yield return new WaitForSeconds (atkCooldown);
		cooldown = false;
	}
}