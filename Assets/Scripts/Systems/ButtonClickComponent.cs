using UnityEngine;

public class ButtonClickComponent : MonoBehaviour
{
    private bool _isClicked;
    
    public bool IsClicked 
    {
        get 
        {
            if (_isClicked)
            {
                _isClicked = false;
                return true;
            }
            return false;
        }
    }

    public void OnClick()
    {
        _isClicked = true;
    }
}