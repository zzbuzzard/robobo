using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XY = UnityEngine.Vector2Int;

public class MakerSceneBlockScript : MonoBehaviour
{
    private BlockType currentBlock = BlockType.NONE;
    private int rotate = 0;
    public SpriteRenderer rend;

    public float baseSize = 2.0f;
    public Sprite emptySprite;

    // Must be set by creator
    public XY pos;
    public MakerScript maker;

    public void SetBlock(BlockType type)
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
    public void SetRotation(int r)
    {
        rotate = (r % 4 + 4) % 4;
        SetTransform();
    }
    public void ClearBlock()
    {
        currentBlock = BlockType.NONE;
        rotate = 0;
        SetTransform();
    }
    public void MarkReachable(bool reachable)
    {
        rend.color = (reachable ? Color.white : Color.red);
    }

    public bool IsEmpty()
    {
        return currentBlock == BlockType.NONE;
    }
    public BlockType GetBlock()
    {
        return currentBlock;
    }
    public int GetRotation()
    {
        return rotate;
    }

    private void SetTransform()
    {
        transform.rotation = Quaternion.Euler(0, 0, 90.0f * rotate);

        if (currentBlock == BlockType.NONE)
        {
            transform.localScale = Vector3.one * baseSize * 0.9f;
            rend.sprite = emptySprite;
            rend.sortingOrder = 0;
        }
        else
        {
            transform.localScale = Vector3.one * baseSize;
            rend.sprite = BlockInfo.blockInfos[(int)currentBlock].showSprite;
            rend.sortingOrder = 1;
        }
    }

    private void Awake()
    {
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
