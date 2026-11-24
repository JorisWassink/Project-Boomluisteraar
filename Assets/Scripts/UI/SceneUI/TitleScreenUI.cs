using UnityEngine;

public class TitleScreenUI : UIManager
{
    public void GoToWorldMap() => SwitchScene(Scene.WorldMap);

    public new void Start()
    {
        base.Start();
        SetDialogueUI(DialogueEvent.FirstGameStart);
    }
    
    //Public Functions
    public override void SetDefaultUI()
    {
        SetCurrentUI(defaultUIScreen, true, true);
    }
}
