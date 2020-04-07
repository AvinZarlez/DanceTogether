using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class View:MonoBehaviour
{
	#region VARS (PUBLIC)
    public enum ViewStyles {Dynamic, Static, LeftAsIs};						// declare type of view objects here dynamic for scaling to viewport and static for a set resolution.
    public ViewStyles viewStyle = ViewStyles.Static; 						// default to static for this case. 
	public bool useAnimator = false; 										// check true and view will need an animator - use animator to create complex animation
	public float fadeTime = 0.5f; 											// time fade will take when simple is checked. 0.0 for instant.
	public bool startOpen = false;
	#endregion

	#region VARS (PRIVATE)
	private Animator _animator; 											// animator reference
	private CanvasGroup _canvasGroup; 										// canvas group reference
    private bool _isOpen = false;                                           // internal bool to track view open state - used when no animator.
    private bool _openComplete = false;
    private bool _closeComplete = false;
	#endregion

	#region FUNCTIONS (PUBLIC)
	public bool isOpen
	{
		get
		{
            if (_animator != null && useAnimator)
            {
                return _animator.GetBool("isOpen");
            } else
            {
                return _isOpen;
            }
		}

		set
		{
            if (_animator != null && useAnimator)
            {
                _animator.SetBool("isOpen", value);
            }

            if (value)
            {
                OpenView();
            } else
            {
                CloseView();
            }

            // reset these conditions when isOpen is changed.
            openComplete = closeComplete = false;
		}
	}

    // Track the point when view is completely opened or closed.
    public bool openComplete
    {
        get { return _openComplete; }

        private set { _openComplete = value; }
    }

    public bool closeComplete
    {
        get { return _closeComplete; }

        private set { _closeComplete = value; }
    }

    public virtual void Init()
	{
		/* 
		 * SET FOR FUTURE REFACTOR - I'm pretty sure you can check 
		 * the game object to see if you are set to ancor or set 
		 * to offsets making a manual seting redundant 
		 */
		switch(viewStyle)
		{
		case ViewStyles.Dynamic:
			SetViewDynamic();
			
			break;
		case ViewStyles.Static:
			SetViewStatic();
			
			break;
		default:
			break;
		}
	}

	public void OpenView()
	{
		// call this to show current slide
		gameObject.SetActive(true);
		
		SetAnimator();
		
		SetCanvas();

        if (_canvasGroup.alpha == 1.0f && !isOpen) // special condition for simple open with start open + state object script.
            _canvasGroup.alpha = 0.0f;
		
		if(useAnimator)
		{
            _isOpen = true;
		}
		else
		{
            _isOpen = true;
            // solves error when trying to call close on an inactive view.
            if (gameObject.activeSelf)
            {
                StopCoroutine("SimpleClose");

                StartCoroutine("SimpleOpen");
            } else
            {
                openComplete = true;
            }
		}
	}
	
	public void CloseView()
	{
		if(useAnimator)
		{
            _isOpen = false;
		}
		else if(gameObject.activeInHierarchy)
		{
            _isOpen = false;
			// solves error when trying to call close on an inactive view.
			if(gameObject.activeSelf)
			{
				StopCoroutine("SimpleOpen");
				
				StartCoroutine("SimpleClose");
			} else
            {
                closeComplete = true;
            }
		} else
        {
            // if parent object is not active.
            _isOpen = false;
            ResetView();
            _canvasGroup.alpha = 0.0f;
            BlockRaycasts(false);
            Interactable(false);
            //viewClosed?.Invoke();
            closeComplete = true;
            gameObject.SetActive(false);

        }
	}

    public void ToggleView()
    {
        if(isOpen)
        {
            CloseView();
        } else
        {
            OpenView();
        }
    }
	
	public void SetInnactive()
	{
        if (_animator != null)
        {
            if (!_animator.GetBool("isOpen"))
            {
                gameObject.SetActive(false);
                viewClosed?.Invoke();

                closeComplete = true;
            }
        }
	}

	public void changeInteractionState()
	{
		/*
		 * this method is called at the begining 
		 * and end of animation transitions to 
		 * see if the slide is on or off. 
		 * // note: may no longer be needed as 
		 * the slide is deactivated not at the end.
		 */
		if(useAnimator)
		{
			bool _isActive = _animator.GetCurrentAnimatorStateInfo(0).IsName("Open");
			
			BlockRaycasts(_isActive);
			
			Interactable(_isActive);

            if(_isActive)
            {
                // Fire viewOpened when Animator marks the view as fully loaded.
                viewOpened?.Invoke();

                openComplete = true;
            } else
            {
                // Fire viewOpened when Animator marks the view as fully loaded.
                viewClosed?.Invoke();

                closeComplete = true;
            }
		}
	}
	
	public void BlockRaycasts(bool _value)
	{
		// for manual calls to canvas group to block raycasts or not
		_canvasGroup.blocksRaycasts = _value;
	}
	
	public void Interactable(bool _value)
	{
		// for manual calls to canvas group to be interactable or not
		_canvasGroup.interactable = _value;
	}
	
	public void ResetView()
	{
		// call this reset at begining of play animation to reset the view
		changeInteractionState();
		
		ResetScrollBars ();
	}
	
	public void LoadedView()
	{
		// call this reset at end of play animation to start the view
		changeInteractionState();
	}
	
	public void ResetScrollBars()
	{
		// trigger all scroll bars in children of this view to set to value indicated. 
		// reset scroll bars if any to 1
		Scrollbar[] _tempHelper = GetComponentsInChildren<Scrollbar>();
		
		foreach(Scrollbar _scrollbar in _tempHelper)
		{
			_scrollbar.value = 1;
		}
	}	
	#endregion

	#region FUNCTIONS (INTERNAL)
	void Awake()
	{
		if(!GetComponent<CanvasGroup>())
			gameObject.AddComponent<CanvasGroup>();

		SetAnimator();

		SetCanvas();

		Init();

		if(useAnimator)
		{
			changeInteractionState();

			// this is here because the animation affect parent canvas alpha. I should remove this method. -REFACTOR-
			//_canvasGroup.alpha = 0.0f;

			// checks default start state of animation
			if(!_animator.GetBool("isOpen"))
			{
				gameObject.SetActive(false);
			}
		}
		else if(!startOpen)
		{
			// this is needed for simple method.
			_canvasGroup.alpha = 0.0f;

			gameObject.SetActive(false);
		} else if (startOpen)
        {
            /* special start open condition to fix bug with stateObject script */
            // this is needed for simple method. 0.0f;

            
            _canvasGroup.alpha = 1.0f;
        }
    }

	void SetViewStatic()
	{
		// set registration points to 0,0 - note: only works if view is not using offset aka static
		RectTransform _rect = GetComponent<RectTransform>();
		
		_rect.localPosition = new Vector2(0, 0);
	}
	
	void SetViewDynamic()
	{
		// set offset limits to edge of parent container - note: only works if using offsets aka dynamic
		RectTransform _rect = GetComponent<RectTransform>();
		
		_rect.offsetMax = _rect.offsetMin = new Vector2(0, 0);
	}

	private IEnumerator SimpleOpen()
	{
        BlockRaycasts(true);

        for (float i = _canvasGroup.alpha; i < 1.0f; i = i + (Time.unscaledDeltaTime / fadeTime))
		{
			if(_canvasGroup != null)
			{
				_canvasGroup.alpha = i;
			}
			
			yield return null;
		}
		
		yield return null;
		
		_canvasGroup.alpha = 1.0f;
		
		Interactable (true);

        // Fire viewOpened when open is complete.
        viewOpened?.Invoke();

        openComplete = true;

        yield return null;
    }
	
	private IEnumerator SimpleClose()
	{
        if (_canvasGroup != null)
        {

            for (float i = _canvasGroup.alpha; i > 0.0f; i = i - (Time.unscaledDeltaTime / fadeTime))
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = i;
                }

                yield return null;
            }

            ResetView();

            yield return null;

            _canvasGroup.alpha = 0.0f;

            BlockRaycasts(false);

            Interactable(false);

            // Fire viewClosed when close is complete.
            viewClosed?.Invoke();

            closeComplete = true;

            // turns off game object
            gameObject.SetActive(false);

        }
	}
	#endregion

	#region FUNCTIONS (PRIVATE)
	private void SetCanvas()
	{
		if(GetComponent<CanvasGroup>())
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}
	}

	private void SetAnimator()
	{
		if(GetComponent<Animator>())
		{
			_animator = GetComponent<Animator>();
		}
	}
    #endregion

    #region Public Delegates
    public delegate void ViewClosedDelegate();      // fire this when View is closed.
    public ViewClosedDelegate viewClosed;
    public delegate void ViewOpenedDelegate();      // fire this when View is opened.
    public ViewOpenedDelegate viewOpened;
    #endregion

    private void OnDestroy()
    {
        // If view is destroyed remove delegate references.
        viewClosed = null;
        viewOpened = null;
    }

    private void OnDisable()
    {
        if(GetComponentsInChildren<View>() != null)
        {
            View[] childrenViews = GetComponentsInChildren<View>();
            foreach (View child in childrenViews)
            {
                child.CloseView();
            }
        }
    }
}
