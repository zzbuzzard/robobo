using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// TODO: Move Awake() stuff to editor-time
public class BuildBlockButton : MonoBehaviour
{
    public BlockType blockType;

    // In the scene:
    public MakerScript maker;
    
    // In my own prefab:
    public Button myButton;
    public Image myImage;
    public Image outlineImage;
    public TextMeshProUGUI costText;

    void Awake()
    {
        UnityEngine.Events.UnityAction onClick = delegate () { maker.ButtonClicked(this); };
        myButton.onClick.AddListener(onClick);

        BlockInfo info = BlockInfo.blockInfos[(int)blockType];
        myImage.sprite = info.showSprite;

        costText.text = "£" + info.cost;

        DeselectBlock();
    }

    public void SelectBlock()
    {
        outlineImage.enabled = true;
    }

    public void DeselectBlock()
    {
        outlineImage.enabled = false;
    }
}
