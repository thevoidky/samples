using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


//--------------------------------------------------------------------------------------------------------------------
//	CTreeUIRoot
//	
//--------------------------------------------------------------------------------------------------------------------
public class CTreeUIRoot : MonoBehaviour
{
	//--------------------------------------------------------------------------------------------------------------------
	//	변수
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [변수]

	[SerializeField] private Canvas _canvas = null;
	[SerializeField] private GraphicRaycaster _raycaster = null;

	private SortedDictionary<string, CTreeUINodeBase> _nodes = new SortedDictionary<string, CTreeUINodeBase>();
	private RectTransform _root = null;

	private Stack<CTreeUINodeBase> _nodeStack = new Stack<CTreeUINodeBase>();
	private Dictionary<Type, IList<CTreeUINodeBase>> _nodeMap = new Dictionary<Type, IList<CTreeUINodeBase>>();

	private bool _isTransiting = false;

	[SerializeField]
	private CTreeUINodeBase _rootNode = null;
	private CTreeUINodeBase _curNode = null;

	[SerializeField] private bool _usePointerMessage = true;
	public bool usePointerMessage
	{
		get { return _usePointerMessage; }
		set
		{
			_usePointerMessage = value; 
			_raycaster.enabled = value;
		}
	}

	[SerializeField] private UnityEvent _exitOnRoot = null;

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
	protected void Awake()
	{
		Debug.Log($"{GetType().ToString()} : {System.Reflection.MethodBase.GetCurrentMethod().ToString()}");

		if (null == _rootNode)
		{
			Debug.LogErrorFormat("{0} : Tree UI에는 1개 이상의 Node가 있어야 합니다.", GetType().ToString());
			return;
		}

		if (!_usePointerMessage)
			_raycaster.enabled = false;

		_root = _canvas.GetComponent<RectTransform>();
		foreach (var node in _root.GetComponentsInChildren<CTreeUINodeBase>(true))
		{
			if (node.transform == transform)
				continue;

			if (_nodes.ContainsKey(node.name))
			{
				Debug.LogWarningFormat("{0} : 같은 이름의 Tree Node가 이미 존재합니다. 해당 Node는 더 이상 저장되지 않습니다. ({1})", node.name);
				continue;
			}

			_nodes.Add(node.name, node);

			node.gameObject.SetActive(false);

			if (null == _rootNode)
				_rootNode = node;
		}
	}
	#endregion

	#region [시작]
	//--------------------------------------------------------------------------------------------------------------------
	protected void Start()
	{
		Enter(_rootNode);
	}
	#endregion

	#region [업데이트]
	//--------------------------------------------------------------------------------------------------------------------
	protected void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
			Exit();

		if (!_usePointerMessage && null == EventSystem.current?.currentSelectedGameObject)
			_curNode?.RestoreSelection();
	}
	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	내부 함수
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [UI 전환]
	//--------------------------------------------------------------------------------------------------------------------
	private IEnumerator transit(CTreeUINodeBase enterNode, bool isExiting = false, Action<bool> callback = null)
	{
		if (_isTransiting)
		{
			callback?.Invoke(false);
			yield break;
		}

		if (null == enterNode)
		{
			Debug.LogWarningFormat("{0} : Node is null to enter", GetType().ToString());

			callback?.Invoke(false);
			yield break;
		}

		//	전환 시작

		_isTransiting = true;
		if (_usePointerMessage)
			_raycaster.enabled = false;

		if (isExiting || !enterNode.isPopup && null != _curNode)
		{
			yield return StartCoroutine(_curNode.Exit(null));
			_curNode.gameObject.SetActive(false);
		}

		if (!(isExiting && enterNode.isPopup))
		{
			enterNode.gameObject.SetActive(true);
			yield return StartCoroutine(enterNode.Enter());
		}

		//	전환 끝

		_isTransiting = false;
		if (_usePointerMessage)
			_raycaster.enabled = true;

		callback?.Invoke(true);

		yield break;
	}
	#endregion


	//--------------------------------------------------------------------------------------------------------------------
	//	메서드
	//
	//--------------------------------------------------------------------------------------------------------------------

	#region [UI 목록 찾기]
	//--------------------------------------------------------------------------------------------------------------------
	public IList<CTreeUINodeBase> GetUINodeList(Type uiType)
	{
		Debug.Log($"{GetType().ToString()} : {System.Reflection.MethodBase.GetCurrentMethod().ToString()}");

		if (!_nodeMap.TryGetValue(uiType, out var list)) return null;
		return list;
	}
	#endregion

	#region [UI 해당 타입의 첫 번째 노트 찾기]
	//--------------------------------------------------------------------------------------------------------------------
	public CTreeUINodeBase GetUINodeFirst(Type uiType)
	{
		var list = GetUINodeList(uiType);
		if (null == list || 0 == list.Count) return null;

		return list[0];
	}
	#endregion

	#region [UI Depth 진입]
	//--------------------------------------------------------------------------------------------------------------------
	public void Enter(CTreeUINodeBase enterNode)
	{
		if (_isTransiting)
		{
			Debug.Log($"{GetType().ToString()} : UI 전환이 진행 중입니다.");
			return;
		}

		Debug.Log($"{GetType().ToString()} : {System.Reflection.MethodBase.GetCurrentMethod().ToString()}");

		if (null == enterNode)
		{
			Debug.LogErrorFormat("{0} : Node is null to enter", GetType().ToString());
			return;
		}

		enterNode.treeRoot = this;

		if (null == enterNode || _curNode == enterNode) return;

		var type = enterNode.GetType();
		if (!_nodeMap.ContainsKey(type)) _nodeMap.Add(type, new List<CTreeUINodeBase>());
		_nodeMap[type].Add(enterNode);

		StartCoroutine(transit(enterNode, false, isSucceeded =>
		{
			if (!isSucceeded)
			{
				_nodeMap[type].Remove(enterNode);
				return;
			}

			if (null != _curNode)
				_nodeStack.Push(_curNode);
			_curNode = enterNode;
		}));
	}

	//--------------------------------------------------------------------------------------------------------------------
	public void Enter(string enterNodeName)
	{
		Debug.Log($"{GetType().ToString()} : {System.Reflection.MethodBase.GetCurrentMethod().ToString()}");

		if (!_nodes.ContainsKey(enterNodeName))
		{
			Debug.LogErrorFormat("{0} : 해당 Node 정보가 없습니다. ({1})", GetType().ToString(), enterNodeName);
			return;
		}

		Enter(_nodes[enterNodeName]);
	}
	#endregion

	#region [UI Depth 회귀]
	//--------------------------------------------------------------------------------------------------------------------
	public bool Exit()
	{
		if (0 == _nodeStack.Count)
		{
			Debug.LogFormat("{0} : 최상위 Node입니다.", GetType().ToString());

			_exitOnRoot?.Invoke();

			return false;
		}

		if (_isTransiting)
		{
			Debug.Log($"{GetType().ToString()} : UI 전환이 진행 중입니다.");
			return false;
		}

		var returnNode = _nodeStack.Peek();
		StartCoroutine(transit(returnNode, true, isSucceeded =>
		{
			if (!isSucceeded) return;

			if (null != _curNode)
			{
				var type = _curNode.GetType();
				if (_nodeMap.ContainsKey(type))
					_nodeMap[type].Remove(_curNode);
			}

			_nodeStack.Pop();
			_curNode = returnNode;
		}));

		return true;
	}
	#endregion
}
