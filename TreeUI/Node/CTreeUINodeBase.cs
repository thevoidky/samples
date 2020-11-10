using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


//--------------------------------------------------------------------------------------------------------------------
//	CTreeUINodeBase
//	
//--------------------------------------------------------------------------------------------------------------------
public abstract class CTreeUINodeBase : MonoBehaviour
{
	//--------------------------------------------------------------------------------------------------------------------
	//	변수
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [변수]

	[SerializeField] protected Selectable _defaultSelectable = null;

	[SerializeField] protected bool _isPopup = false;
	public bool isPopup
	{
		get { return _isPopup; }
		protected set { _isPopup = value; }
	}

	[SerializeField] protected bool _useEscape = true;
	public bool useEscape
	{
		get { return _useEscape; }
		set { _useEscape = value; }
	}

	private CTreeUIRoot _treeRoot = null;
	public CTreeUIRoot treeRoot
	{
		get { return _treeRoot; }
		set { _treeRoot = value; }
	}

	protected Selectable _lastSelectable = null;

	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	property
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [property]

	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	기본 함수
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [초기화]
	//--------------------------------------------------------------------------------------------------------------------
	protected virtual void Awake()
	{
		Debug.Log($"{GetType().ToString()} : {System.Reflection.MethodBase.GetCurrentMethod().ToString()}");
	}
	#endregion

	#region [소멸]
	//--------------------------------------------------------------------------------------------------------------------
	protected virtual void OnDestroy()
	{
		Debug.LogFormat("{0} : {1}", GetType().ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
	}
	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	추상 함수
	//
	//--------------------------------------------------------------------------------------------------------------------


	//--------------------------------------------------------------------------------------------------------------------
	//	메서드
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [UI 진입]
	//--------------------------------------------------------------------------------------------------------------------
	public virtual IEnumerator Enter()
	{
		Debug.Log($"{GetType().ToString()} : Enter()");

		gameObject.SetActive(true);

		yield break;
	}

	//--------------------------------------------------------------------------------------------------------------------
	public void Enter(CTreeUIRoot root)
	{
		Debug.Log($"{GetType().ToString()} : {System.Reflection.MethodBase.GetCurrentMethod().ToString()}");

		if (!root) return;

		root.Enter(this);
	}
	#endregion

	#region [UI 퇴장]
	//--------------------------------------------------------------------------------------------------------------------
	public virtual IEnumerator Exit(CTreeUIRoot rootOrNull = null)
	{
		Debug.Log($"{GetType().ToString()} : Exit(CTreeUIRoot)");

		gameObject.SetActive(false);

		yield break;
	}

	//--------------------------------------------------------------------------------------------------------------------
	public void Exit()
	{
		Debug.Log($"{GetType().ToString()} : {System.Reflection.MethodBase.GetCurrentMethod().ToString()}");

		treeRoot?.Exit();
	}
	#endregion

	#region [Selection 복원]
	//--------------------------------------------------------------------------------------------------------------------
	public void RestoreSelection()
	{
		EventSystem.current?.SetSelectedGameObject(_lastSelectable.gameObject);
	}
	#endregion
}
