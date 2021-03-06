﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DungeonArchitect;
using DungeonArchitect.Utils;
using DungeonArchitect.Navigation;

namespace DAShooter {
	public class GameController : MonoBehaviour {
		private static GameController instance;
		public Dungeon dungeon;
		public DungeonNavMesh navMesh;
		public GameObject minimap;

		public GameObject levelLoadingScreen;
		public Text textBuildingLayout;
		public Text textBuildingNavMesh;

		LevelNpcSpawner npcSpawner;
		string labelBuildingLayout = "Building Layout... ";
		string labelBuildingNavmesh = "Building Navmesh... ";


		public static GameController Instance {
			get {
				return instance;
			}
		}

		void Awake() {
			instance = this;
			npcSpawner = GetComponent<LevelNpcSpawner>();

			CreateNewLevel();
		}


		public void CreateNewLevel() {
			// Assing a different seed to create a new layout
			int seed = Mathf.FloorToInt(Random.value * int.MaxValue);
			dungeon.Config.Seed = (uint)seed;
			
			// Rebuild a new dungeon
			StartCoroutine(RebuildLevel(dungeon));
		}

		IEnumerator RebuildLevel(Dungeon dungeon) {
			textBuildingNavMesh.gameObject.SetActive(false);
			levelLoadingScreen.SetActive(true);
			minimap.SetActive(false);

			textBuildingLayout.text = labelBuildingLayout;
			textBuildingLayout.gameObject.SetActive(true);
			yield return 0;
            
			dungeon.DestroyDungeon();
			yield return 0;

			dungeon.Build();
			
			textBuildingLayout.text = labelBuildingLayout + "DONE!";

			textBuildingNavMesh.text = labelBuildingNavmesh;
			textBuildingNavMesh.gameObject.SetActive(true);
			yield return 0;
            

			RebuildNavigation();

			npcSpawner.OnPostDungeonBuild(dungeon, dungeon.ActiveModel);

			levelLoadingScreen.SetActive(false);
			minimap.SetActive(true);

			// reset player health
			var player = GameObject.FindGameObjectWithTag(GameTags.Player);
			if (player != null) {
				var health = player.GetComponent<PlayerHealth>();
				if (health != null) {
					health.currentHealth = health.startingHealth;
				}
			}

			// Destroy any npc too close to the player
			var enemies = GameObject.FindGameObjectsWithTag(GameTags.Enemy);
			var playerPosition = player.transform.position;
			foreach (var enemy in enemies) {
				var distance = (playerPosition - enemy.transform.position).magnitude;
				if (distance < 1) {
					Destroy (enemy);
				}
			}
	    }

		public void RebuildNavigation() {
	      navMesh.Build();
	    }
	}
}