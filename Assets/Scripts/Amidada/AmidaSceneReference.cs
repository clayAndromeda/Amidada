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

		private AmidaGameSystem gameSystem;
		
		private void Awake()
		{
			var stage = new AmidaStage(mainCamera, lineTemplate, canvas, amidaPlayerPrefab, amidaStarPrefab, amidaEnemyPrefab, tateLineParent, yokoLineParent);
			gameSystem = new AmidaGameSystem(stage, pathPencil);
			gameSystem.StartGameAsync().Forget();
		}

		private void OnDestroy()
		{
			gameSystem.Dispose();
		}

		private void OnGUI()
		{
			// 画面右上にポイント表示
			GUI.Label(new Rect(Screen.width - 100, 0, 100, 50), $"Point: {gameSystem.GamePoint}");
			// その下に、
		}
	}
}