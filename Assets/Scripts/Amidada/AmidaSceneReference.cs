using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Amidada
{
	public class AmidaSceneReference : MonoBehaviour
	{
		[SerializeField] private AmidaPathPencil pathPencil;
		[SerializeField] private LineRenderer lineTemplate;

		[Header("Prefab参照")] [SerializeField] private AmidaPlayerObject amidaPlayerPrefab;
		[SerializeField] private AmidaTarget amidaStarPrefab;
		[SerializeField] private AmidaTarget amidaEnemyPrefab;

		[SerializeField] private Camera mainCamera;


		[Header("オブジェクトのルート"), SerializeField]
		private Canvas canvas;

		[SerializeField] private Transform tateLineParent;
		[SerializeField] private Transform yokoLineParent;
		
		[SerializeField] GameSoundManager soundManager;
		[SerializeField] GameUIManager uiManager;

		private AmidaGameSystem gameSystem;
		
		private void Awake()
		{
			var stage = new AmidaStage(mainCamera, lineTemplate, canvas, amidaPlayerPrefab, amidaStarPrefab, amidaEnemyPrefab, tateLineParent, yokoLineParent);
			gameSystem = new AmidaGameSystem(stage, pathPencil);
			gameSystem.StartGameAsync().Forget();
			
			soundManager.RegisterSoundEvents(gameSystem);
			uiManager.RegisterGameEvents(gameSystem);
		}

		private void OnDestroy()
		{
			gameSystem.Dispose();
		}
	}
}