using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResizableGridTesting : MonoBehaviour
{
    [SerializeField] private Vector2Int addPosition;
    [SerializeField] private string addItem;
    [SerializeField] private int cellLogWidth;
    [SerializeField] private TextMeshProUGUI gridDisplay;
    [SerializeField] private Vector2Int size;
    [SerializeField] private Vector2Int anchor;

    private ArrayGrid<string> grid;

    private void Awake()
    {
        grid = new(size, anchor);
        
        grid.AllPositions
            .Select((position, index) => (position, index))
            .ToList()
            .ForEach(item => grid.SetTile(item.position, item.index.ToString()));
    }

    public void AddItem()
    {
        grid.SetTile(addPosition, addItem);
    }

    private void Update()
    {
        gridDisplay.text = $"size: {grid.Size}\n";
        gridDisplay.text += string.Join("\n\n",
            Enumerable.Range(0, grid.Height).Select(y => string.Join("",
                Enumerable.Range(0, grid.Width).Select(x =>
                {
                    var pos = new Vector2Int(x, y) + grid.Anchor;
                    return $"({pos.x}, {pos.y}) {(grid.TryGetTile(pos, out var tile) ? tile : default)}".PadRight(cellLogWidth);
                }))));
    }
}
