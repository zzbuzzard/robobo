using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XY = System.Tuple<int, int>;

public class MakerSceneBlockScript : MonoBehaviour
{
    private Robot.BlockType currentBlock = Robot.BlockType.NONE;
    private int rotate = 0;
    private SpriteRenderer rend;

    public Sprite emptySprite;

    // Must be set by creator
    public XY pos;
    public MakerScript maker;

    public void SetBlock(Robot.BlockType type)
    {
        currentBlock = type;
        rotate = 0;
        SetTransform();
    }
    public void Rotate()
    {
        rotate += 1;
        rotate %= 4;
        SetTransform();
    }
    public void ClearBlock()
    {
        currentBlock = Robot.BlockType.NONE;
        rotate = 0;
        SetTransform();
    }
    public void MarkReachable(bool reachable)
    {
        rend.color = (reachable ? Color.white : Color.red);
    }

    public bool IsEmpty()
    {
        return currentBlock == Robot.BlockType.NONE;
    }
    public Robot.BlockType GetBlock()
    {
        return currentBlock;
    }
    public int GetRotation()
    {
        return rotate;
    }

    private void SetTransform()
    {
        const float baseSize = 2.0f;

        transform.rotation = Quaternion.Euler(0, 0, 90.0f * rotate);
        transform.localScale = Vector3.one * (currentBlock == Robot.BlockType.NONE ? 0.9f : 1.0f) * baseSize;

        if (currentBlock == Robot.BlockType.NONE) 
            rend.sprite = emptySprite;
        else
            rend.sprite = Robot.blockTypePrefabs[(int)currentBlock].GetComponent<SpriteRenderer>().sprite;
    }

    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        ClearBlock();
        SetTransform();
    }
    private void OnMouseDown()
    {
        // Check GUI
        if (EventSystem.current.IsPointerOverGameObject()) return;
        maker.SpaceClicked(this);
    }
}
