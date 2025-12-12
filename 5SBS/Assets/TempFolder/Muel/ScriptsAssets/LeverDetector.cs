using TMPro;
using UnityEngine;
public class LeverDetector : MonoBehaviour, IHistoryObject
{
    public int leverValue; // 0/1

    public Sprite turnleftSprite;
    public Sprite turnrightSprite;
    public TextMeshPro text;

    public GameObject doorObject1;
    public GameObject doorObject2;

    void Start()
    {
        updateDoors();
    }

    public HistoryData SaveState()
    {
        return new HistoryData { intValue = leverValue };
    }

    void Update()
    {
        if(Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, transform.position) < 2f)
        {
            text.gameObject.SetActive(true);
            if(Input.GetKeyDown(KeyCode.E))
            {
                leverPressed(leverValue == 0 ? 1 : 0);
            } 
        } else
        {
            text.gameObject.SetActive(false);
        }
    }

    public void LoadState(HistoryData data)
    {
        leverValue = data.intValue;
        updateDoors();
        // update animation lever di sini
    }

    public void leverPressed(int value)
    {
        leverValue = value;
        updateDoors();
        // update animation lever di sini
    }

    public void updateDoors()
    {
        if (doorObject1 != null && doorObject2 != null)
        {
            if (leverValue == 1)
            {
                doorObject1.SetActive(false);
                doorObject2.SetActive(true);
                GetComponent<SpriteRenderer>().sprite = turnrightSprite;
                
            }
            else
            {
                doorObject1.SetActive(true);
                doorObject2.SetActive(false);
                GetComponent<SpriteRenderer>().sprite = turnleftSprite;
            }
        }
    }
}
