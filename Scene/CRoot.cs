using UnityEngine;
using UnityEngine.SceneManagement;
using System;


namespace CUtility.Scene
{
	//--------------------------------------------------------------------------------------------------------------------
	//	CRoot
	//
	//--------------------------------------------------------------------------------------------------------------------

	public class CRoot : CSingleton<CRoot>
	{
		//--------------------------------------------------------------------------------------------------------------------
		//	변수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [변수]

		public string _sceneHeader = "";
		public string _startScene = "";

		[SerializeField] private bool _runInBackground = false;
		public bool runInBackground { get => _runInBackground; set => _runInBackground = value; }

		[SerializeField] private Vector2Int _primaryScreenResolution = new Vector2Int(720, 1480);
		public Vector2Int primaryScreenResolution { get => _primaryScreenResolution; set => _primaryScreenResolution = value; }

		[SerializeField] private bool _sleepTimeout = false;
		public bool sleepTimeout { get => _sleepTimeout; set => _sleepTimeout = value; }

		[SerializeField] private int _primaryTargetFps;

		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	property
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [property]

		public float timeScale { get => Time.timeScale; set => Time.timeScale = value; }

		public int targetFps { get => Application.targetFrameRate; set => Application.targetFrameRate = value; }

		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	기본 함수
		//
		//--------------------------------------------------------------------------------------------------------------------

		#region [초기화]
		//--------------------------------------------------------------------------------------------------------------------
		protected override void init()
		{
		}
		#endregion

		#region [시작]
		//--------------------------------------------------------------------------------------------------------------------
		private void Start()
		{
			UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

			Application.runInBackground = runInBackground;
			Screen.sleepTimeout = sleepTimeout ? SleepTimeout.SystemSetting : SleepTimeout.NeverSleep;

			Application.targetFrameRate = _primaryTargetFps;

			var aspect = Screen.width / (float)Screen.height;

			if (_primaryScreenResolution.x > _primaryScreenResolution.y)
				Screen.SetResolution(Mathf.RoundToInt(_primaryScreenResolution.y * aspect), _primaryScreenResolution.y, Screen.fullScreenMode);
			else
				Screen.SetResolution(_primaryScreenResolution.x, Mathf.RoundToInt(_primaryScreenResolution.x / aspect), Screen.fullScreenMode);

			CSceneManager.LoadSceneAsync(_startScene, LoadSceneMode.Single, null, null);
		}
		#endregion


		//--------------------------------------------------------------------------------------------------------------------
		//	내부 함수
		//
		//--------------------------------------------------------------------------------------------------------------------
	}
}
