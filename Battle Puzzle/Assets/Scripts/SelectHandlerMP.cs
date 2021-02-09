using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectHandlerMP : MonoBehaviour
{
    private GameObject[] selection;
    
    private bool p1Active;
    private bool p2Active;
    private int p1Ind;
    private int p2Ind;
    
    public GameObject barb;
    public GameObject arch;
    public GameObject rogue;
    public GameObject witch;
    public GameObject cleric;
    public GameObject monk;
    public GameObject druid;
    
    public GameObject p1PointPrefab;
    public GameObject p2PointPrefab;
    
    public GameObject p1Data;
    public GameObject p2Data;
    
    private GameObject p1Point;
    private GameObject p2Point;
    
    // Start is called before the first frame update
    void Start()
    {
        selection = new GameObject[]{barb, arch, rogue, witch, cleric, monk, druid};
        
        p1Point = Instantiate(p1PointPrefab, barb.transform.position, Quaternion.identity);
        p2Point = Instantiate(p2PointPrefab, druid.transform.position, Quaternion.identity);
        
        p1Active = true;
        p2Active = true;
        
        p1Ind = 0;
        p2Ind = 6;
        
        DisplayData(0);
        DisplayData(1);
    }

    // Update is called once per frame
    void Update()
    {
        if(p1Active)
        {
            if(Input.GetKeyDown("left"))
            {
                p1Ind = ShiftLeft(p1Ind);
                p1Point.transform.position = selection[p1Ind].transform.position;
                DisplayData(0);
            }
            
            else if(Input.GetKeyDown("right"))
            {
                p1Ind = ShiftRight(p1Ind);
                p1Point.transform.position = selection[p1Ind].transform.position;
                DisplayData(0);
            }
            
            else if(Input.GetKeyDown("enter"))
            {
                p1Active = false;
            }
        }
        
        if(p2Active)
        {
            if(Input.GetKeyDown("a"))
            {
                p2Ind = ShiftLeft(p2Ind);
                p2Point.transform.position = selection[p2Ind].transform.position;
                DisplayData(1);
            }
            
            else if(Input.GetKeyDown("d"))
            {
                p2Ind = ShiftRight(p2Ind);
                p2Point.transform.position = selection[p2Ind].transform.position;
                DisplayData(1);
            }
            
            else if(Input.GetKeyDown("space"))
            {
                p2Active = false;
            }
        }
        
        if(!p1Active && !p2Active)
        {
            MasterController.instance.SelectedMP(p1Ind, p2Ind);
        }
    }
    
    int ShiftLeft(int index)
    {
        index--;
        if(index < 0)
            index = selection.Length - 1;
        
        return index;
    }
    
    int ShiftRight(int index)
    {
        index++;
        if(index >= selection.Length)
            index = 0;
        
        return index;
    }
    
    private void DisplayData(int player)
    {
        int index;
        GameObject playerData;
        
        if(player == 0)
        {
            index = p1Ind;
            playerData = p1Data;
        }
        
        else
        {
            index = p2Ind;
            playerData = p2Data;
        }
        
        int[] currStats = CharacterData.GetStats(index);
        string[] statStrings = new string[]{"", "", "", "", ""};
        
        for(int i = 0; i < statStrings.Length; i++)
        {
            for(int k = 0; k < currStats[i]; k++)
            {
                statStrings[i] += "=";
            }
        }
        
        playerData.transform.Find("CharText").gameObject.GetComponent<Text>().text = CharacterData.GetName(index);
        
        if(currStats[5] == CharacterData.SupHeal)
            playerData.transform.Find("SupTypeText").gameObject.GetComponent<Text>().text = "Support Type : Heal";
            
        else if(currStats[5] == CharacterData.SupShield)
            playerData.transform.Find("SupTypeText").gameObject.GetComponent<Text>().text = "Support Type : Shield";
            
        else if(currStats[5] == CharacterData.SupLeech)
            playerData.transform.Find("SupTypeText").gameObject.GetComponent<Text>().text = "Support Type : Leech";
            
            
        if(currStats[6] == CharacterData.ConDrop)
            playerData.transform.Find("ConTypeText").gameObject.GetComponent<Text>().text = "Control Type  : Drop";
            
        else if(currStats[6] == CharacterData.ConWeak)
            playerData.transform.Find("ConTypeText").gameObject.GetComponent<Text>().text = "Control Type  : Weaken";
            
        else if(currStats[6] == CharacterData.ConSilence)
            playerData.transform.Find("ConTypeText").gameObject.GetComponent<Text>().text = "Control Type  : Silence";
            
        playerData.transform.Find("AttackText").gameObject.GetComponent<Text>().text = "Attack    : " + statStrings[0];
        
        playerData.transform.Find("MagicText").gameObject.GetComponent<Text>().text = "Magic    : " + statStrings[1];
        
        playerData.transform.Find("SupportText").gameObject.GetComponent<Text>().text = "Support : " + statStrings[2];
        
        playerData.transform.Find("ControlText").gameObject.GetComponent<Text>().text = "Control  : " + statStrings[3];
        
        playerData.transform.Find("VitalityText").gameObject.GetComponent<Text>().text = "Vitality   : " + statStrings[4];
    }
}
