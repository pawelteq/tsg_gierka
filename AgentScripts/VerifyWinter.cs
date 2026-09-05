using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class VerifyWinter
{
    public static async Task<string> Main()
    {
        var g=UnityEngine.Object.FindAnyObjectByType<WinterAdventure>();
        var body=g.GetComponent<Rigidbody2D>();
        var keyboard=Keyboard.current;
        if(keyboard==null)throw new Exception("No keyboard device");
        var background=InputSystem.settings.backgroundBehavior;
        var editor=InputSystem.settings.editorInputBehaviorInPlayMode;
        try {
        InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        g.Restart();
        await Task.Delay(350);
        float startY=g.transform.position.y;
        InputSystem.EnableDevice(keyboard);
        InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));
        InputSystem.Update();
        typeof(PlayerMovement).GetMethod("Update",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(g.GetComponent<PlayerMovement>(),null);
        await Task.Delay(140);
        InputSystem.QueueStateEvent(keyboard,new KeyboardState());
        float maxY=g.transform.position.y;
        for(int i=0;i<14;i++){await Task.Delay(65);maxY=Mathf.Max(maxY,g.transform.position.y);}
        bool jumped=maxY-startY>1.8f;
        bool landed=g.transform.position.y>startY+1.3f && Mathf.Abs(body.linearVelocity.y)<.3f;
        g.transform.position=g.lights[0].position;body.position=g.transform.position;body.linearVelocity=Vector2.zero;
        await Task.Delay(180);
        bool pickup=!g.lights[0].gameObject.activeSelf;
        g.transform.position=new Vector3(0,-40,0);body.position=g.transform.position;
        await Task.Delay(180);
        bool respawn=g.transform.position.y>-3 && g.lights[0].gameObject.activeSelf;
        g.Restart();
        return $"jump={jumped}; landed_on_first_platform={landed}; pickup={pickup}; fall_respawn={respawn}; jump_rise={maxY-startY:F2}";
        } finally {
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());
            InputSystem.settings.backgroundBehavior=background;
            InputSystem.settings.editorInputBehaviorInPlayMode=editor;
        }
    }
}
