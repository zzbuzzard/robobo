using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO: Move Awake() stuff to editor-time
public class BuildBlockButton : MonoBehaviour
{
    public BlockType blockType;

    // In the scene:
    public MakerScript maker;
    
    // In my own prefab:
    public Button myButton;
    public Image myImage;

    void Awake()
    {
        UnityEngine.Events.UnityAction onClick = delegate () { maker.ButtonClicked(blockType); };
        myButton.onClick.AddListener(onClick);

        BlockInfo info = BlockInfo.blockInfos[(int)blockType];
        myImage.sprite = info.showSprite;
    }
}
