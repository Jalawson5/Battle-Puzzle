using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectHandlerHelp : MonoBehaviour
{
    private GameObject[] selection;
    
    private bool p1Active;
    private int p1Ind;
    
    public GameObject barb;
    public GameObject arch;
    public GameObject rogue;
    public GameObject witch;
    public GameObject cleric;
    public GameObject monk;
    public GameObject druid;
    
    public GameObject p1PointPrefab;
    public GameObject p1Data;
    
    private GameObject p1Point;
    
    // Start is called before the first frame update
    void Start()
    {
        selection = new GameObject[]{barb, arch, rogue, witch, cleric, monk, druid};
        
        p1Point = Instantiate(p1PointPrefab, witch.transform.position, Quaternion.identity);
        
        p1Active = true;
        p1Ind = 3;
        
        DisplayData();
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
                DisplayData();
            }
            
            else if(Input.GetKeyDown("right"))
            {
                p1Ind = ShiftRight(p1Ind);
                p1Point.transform.position = selection[p1Ind].transform.position;
                DisplayData();
            }
            
            else if(Input.GetKeyDown("return"))
            {
                p1Active = false;
            }
        }
        
        if(!p1Active)
        {
            MasterController.instance.HelpButton();
        }
    }
    
    private int ShiftLeft(int index)
    {
        index--;
        if(index < 0)
            index = selection.Length - 1;
        
        return index;
    }
    
    private int ShiftRight(int index)
    {
        index++;
        if(index >= selection.Length)
            index = 0;
        
        return index;
    }
    
    private void DisplayData()
    {
        int[] currStats = CharacterData.GetStats(p1Ind);
        
        string[] statStrings = new string[]{"", "", "", "", ""};
        
        for(int i = 0; i < statStrings.Length; i++)
        {
            for(int k = 0; k < currStats[i]; k++)
            {
                statStrings[i] += "=";
            }
        }
        
        p1Data.transform.Find("CharText").gameObject.GetComponent<Text>().text = CharacterData.GetName(p1Ind);
        
        if(currStats[5] == CharacterData.SupHeal)
            p1Data.transform.Find("SupTypeText").gameObject.GetComponent<Text>().text = "Support Type : Heal";
            
        else if(currStats[5] == CharacterData.SupShield)
            p1Data.transform.Find("SupTypeText").gameObject.GetComponent<Text>().text = "Support Type : Shield";
            
        else if(currStats[5] == CharacterData.SupLeech)
            p1Data.transform.Find("SupTypeText").gameObject.GetComponent<Text>().text = "Support Type : Leech";
            
            
        if(currStats[6] == CharacterData.ConDrop)
            p1Data.transform.Find("ConTypeText").gameObject.GetComponent<Text>().text = "Control Type  : Drop";
            
        else if(currStats[6] == CharacterData.ConWeak)
            p1Data.transform.Find("ConTypeText").gameObject.GetComponent<Text>().text = "Control Type  : Weaken";
            
        else if(currStats[6] == CharacterData.ConSilence)
            p1Data.transform.Find("ConTypeText").gameObject.GetComponent<Text>().text = "Control Type  : Silence";
            
        p1Data.transform.Find("AttackText").gameObject.GetComponent<Text>().text = "Attack    : " + statStrings[0];
        
        p1Data.transform.Find("MagicText").gameObject.GetComponent<Text>().text = "Magic    : " + statStrings[1];
        
        p1Data.transform.Find("SupportText").gameObject.GetComponent<Text>().text = "Support : " + statStrings[2];
        
        p1Data.transform.Find("ControlText").gameObject.GetComponent<Text>().text = "Control  : " + statStrings[3];
        
        p1Data.transform.Find("VitalityText").gameObject.GetComponent<Text>().text = "Vitality   : " + statStrings[4];
        
        p1Data.transform.Find("CharDesc").gameObject.GetComponent<Text>().text = CharacterData.GetDesc(p1Ind);
    }
}
