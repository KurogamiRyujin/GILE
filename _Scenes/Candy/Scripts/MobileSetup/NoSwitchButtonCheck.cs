using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoSwitchButtonCheck : Cutscene {
	private GameObject player;

	private PlayerMovement playerController;
	private PlayerAttack playerAttack;
	private PickupObject pickupObject;

	void Start() {
		Init ();
	}


	protected override void Init() {
		this.name = "SwitchButtonCheck";
		base.Init ();
	}

	protected override void disablePlayerControls() {
		playerController.canMove (false);
		playerAttack.canAttack (false);
	}

	// Checks if YUNI has hammer
	private IEnumerator WeaponCheck() {

		if (!this.playerAttack.HasHammer ()) {
			GameController_v7.Instance.GetMobileUIManager().ToggleMobileControls(false);
			GameController_v7.Instance.GetMobileUIManager().ToggleBaseWithPickupControls(true);
		}
		yield return null;

	}
	void OnTriggerEnter2D(Collider2D other) {
		Debug.Log ("Tutorial Trigger : Tag is "+other.tag);
		if (other.gameObject.GetComponent<PlayerYuni>() != null) {
			player = other.gameObject;
			PlayerYuni playerStats = player.GetComponent<PlayerYuni> ();
			playerAttack = playerStats.GetPlayerAttack ();
			playerController = playerStats.GetPlayerMovement ();
			PlayScenes ();
		}
	}

	public override void PlayScenes() {
		if (!isPlaying && !isTriggered) {
			base.PlayScenes ();
			StartCoroutine (WeaponCheck ());
		}
	}
}
