using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    
*/

public abstract class FS
{
    public abstract void OnEnter();
    public abstract void OnEveryUpdate(float deltaTime);
    public abstract void OnExit();
}
