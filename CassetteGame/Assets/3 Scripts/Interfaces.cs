using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoldable
{
    public void TryHold(TouchManager.TouchData touch);
    public void Release();
}
