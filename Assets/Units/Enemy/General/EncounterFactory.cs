using Battle.General;
using OptionMenu;
using UnityEngine;
using Utilities;
using Object = UnityEngine.Object;

namespace Units.Enemy.General
{
	public static class EncounterFactory
	{
		/// <summary>
		/// Load Encounter based on config.
		/// </summary>
		public static Encounter Build(BattleConfig config)
		{
            var retVal = new Encounter();
            
            // Linear sequence: 0: Hound, 1: Imp, 2: BrokenSeal
            string[] enemyNames = { "Hound", "Imp", "BrokenSeal" };
            int index = Mathf.Clamp(config.BattleCount, 0, enemyNames.Length - 1);
            string selectedEnemy = enemyNames[index];
            
            Debug.Log($"[EncounterFactory] Building encounter for Battle {config.BattleCount}: {selectedEnemy}");
            
            Enemy enemyPrefab = Resources.Load<Enemy>("Enemies/" + selectedEnemy);
            if (enemyPrefab == null)
            {
                Debug.LogError($"[EncounterFactory] Failed to load enemy prefab: Enemies/{selectedEnemy}");
                return retVal;
            }

            var newEnemy = Build(enemyPrefab);
            retVal.Enemies.Add(newEnemy);

			return retVal;
		}

		/// <summary>
		/// Create a new Enemy.
		/// </summary>
		public static Enemy Build(Enemy enemy)
		{
			if (enemy == null)
			{
				Debug.LogWarning("[EncounterFactory] Passed enemy is null! Falling back to 'Imp'.");
				enemy = Resources.Load<Enemy>("Enemies/Imp");
				if (enemy == null)
				{
					Debug.LogError("[EncounterFactory] Fallback 'Imp' failed to load!");
					return null;
				}
			}

			var newEnemy = Object.Instantiate(enemy);
			newEnemy.Pattern = LoadAttackPattern(enemy);
			newEnemy.name = enemy.name.Replace("(Clone)", "").Trim(); // Keep original name for pattern loading
			return newEnemy;
		}

		/// <summary>
		/// Load data from a json file, convert it to a Attack pattern.
		/// </summary>
		private static AttackPattern LoadAttackPattern(Object prefab)
		{
            string name = prefab.name.Replace("(Clone)", "").Trim();
			var data = Resources.Load<TextAsset>("AttackPattern/" + name);
            if (data == null)
            {
                Debug.LogError($"[EncounterFactory] Failed to load attack pattern for: {name}");
                return null;
            }
			return PersistentJson.Create<AttackPattern>(data.text);
		}
	}
}