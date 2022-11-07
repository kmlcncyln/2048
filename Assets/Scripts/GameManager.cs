using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> types;
    [SerializeField] private float travelTime = 0.2f;
    


    private List<Node> nodes;
    private List<Block> blocks;
    private GameState state;
    private int round;
    private BlockType GetBlockTypeByValue(int value) => types.First(t => t.Value == value);
    void Start()
    {
        ChangeState(GameState.GenerateLevel);
        //GenerateGrid();
    }
    public void ChangeState(GameState newState)
    {
        state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(round++ == 0 ? 2 : 1); // Creates ' in first round then 1 by 1
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    private void Update()
    {
        if (state != GameState.WaitingInput) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
    }
    void GenerateGrid()
    {
        round = 0;
        nodes = new List<Node>();
        blocks = new List<Block>(); 

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x,y), Quaternion.identity);
                nodes.Add(node);
            }
        }

        var center = new Vector2((float) width /2 - 0.5f, (float) height / 2 - 0.5f);

        var board = Instantiate(boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(width, height);

        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawningBlocks);
    }

    void SpawnBlocks(int amount)
    {
        var freeNodes = nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();
        
        foreach (var node in freeNodes.Take(amount))
        {
            var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeByValue(Random.value > 0.8f ? 4 : 2));
            block.SetBlock(node);
            blocks.Add(block);
        }

        if (freeNodes.Count() == 1)
        {
            //Lost Game
            return;
        }
        ChangeState(GameState.WaitingInput);
    }
    
    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);

        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach(var block in orderedBlocks)
        {
            var next = block.Node;
            do
            {
                block.SetBlock(next);

                var possibleNode = GetNodeAtPosition(next.Pos + dir);
                if(possibleNode != null)
                {
                    //If its possible merge set
                    if(possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
                        block.MergeBlock(possibleNode.OccupiedBlock);

                    }
                    //otherwise move spot?
                    else if (possibleNode.OccupiedBlock == null) next = possibleNode;
                    //None hit? End do while loop
                }
            } while (next != block.Node);

            block.transform.DOMove(block.Node.Pos, travelTime);
        }


        var sequence = DOTween.Sequence();
        foreach(var block in orderedBlocks)
        {
            var movePoint = block.Merging ? block.MergingBlock.Pos : block.Node.Pos;

            sequence.Insert(0, block.transform.DOMove(movePoint, travelTime)); 
        }

        sequence.onComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b=>b.MergingBlock != null))
            {
                MergeBlocks(block, block.MergingBlock);
            }
        });
    }

    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {

    }
    Node GetNodeAtPosition(Vector2 pos)
    {
        return nodes.FirstOrDefault(n => n.Pos == pos);
    }
}

[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
}
public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
} 