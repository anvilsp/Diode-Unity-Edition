using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelController : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject redBlockPrefab;
    [SerializeField] private GameObject redButtonPrefab;
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject ladderPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private TilemapRenderer tmr;

    public Vector2 initKeyPos;
    public GameObject keyObj;

    // Start is called before the first frame update
    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for(int x = 0; x < bounds.size.x; x++)
        {
            for(int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                if(tile != null)
                {
                    Vector3Int localPlace = (new Vector3Int(x, y, 2));
                    Vector3 place = tilemap.CellToWorld(localPlace);
                    Vector3 finalPosition = new Vector3((place.x + 2.75f) - (bounds.size.x / 3), (place.y + 3.25f) - (bounds.size.y / 2), 0);
                    string itemName = tile.name;
                    GameObject newItem;
                    ColorBlock block;
                    ColorButton button;
                    Key key;
                    print(itemName);
                    switch(itemName)
                    {
                        case "redBlock":
                            newItem = Instantiate(redBlockPrefab, finalPosition, Quaternion.identity);
                            block = newItem.GetComponent<ColorBlock>();
                            block.switchColor = ColorBlock.SwitchColor.Red;
                            continue;
                        case "blueBlock":
                            newItem = Instantiate(redBlockPrefab, finalPosition, Quaternion.identity);
                            block = newItem.GetComponent<ColorBlock>();
                            block.switchColor = ColorBlock.SwitchColor.Blue;
                            continue;
                        case "redButton":
                            newItem = Instantiate(redButtonPrefab, finalPosition, Quaternion.identity);
                            button = newItem.GetComponent<ColorButton>();
                            button.switchColor = ColorButton.SwitchColor.Red;
                            button.level = gameObject;
                            continue;
                        case "blueButton":
                            newItem = Instantiate(redButtonPrefab, finalPosition, Quaternion.identity);
                            button = newItem.GetComponent<ColorButton>();
                            button.switchColor = ColorButton.SwitchColor.Blue;
                            button.level = gameObject;
                            continue;
                        case "spike":
                            newItem = Instantiate(spikePrefab, finalPosition, Quaternion.identity);
                            continue;
                        case "ladder":
                            newItem = Instantiate(ladderPrefab, finalPosition, Quaternion.identity);
                            continue;
                        case "door":
                            newItem = Instantiate(doorPrefab, finalPosition, Quaternion.identity);
                            continue;
                        case "key":
                            newItem = Instantiate(keyPrefab, finalPosition, Quaternion.identity);
                            initKeyPos = newItem.transform.position;
                            key = newItem.GetComponent<Key>();
                            key.level = gameObject;
                            keyObj = newItem;
                            continue;
                    }
                }
            }
        }
        tmr.enabled = false;
        ColorSwitch("Blue");
        print("level loaded");
    }

    // Update is called once per frame
    void Update()
    {
        //print("current state: " + player.IsOnItem() + Time.deltaTime);
    }
    public void ColorSwitch(string col)
    {
        GameObject[] allObjs = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjs)
        {
            obj.SendMessage("Toggle", col);
        }
    }

}
