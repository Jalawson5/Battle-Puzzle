using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DataEntry
{
    public int score;
    public bool barbClear;
    public bool archerClear;
    public bool rogueClear;
    public bool witchClear;
    public bool clericClear;
    public bool monkClear;
    public bool druidClear;
    
    ////////////////////////////
    //void LoadData()         //
    //Loads saved data        //
    //Currently includes:     //
    //-High Score             //
    //-Character Medals/clears//
    ////////////////////////////
    public bool LoadData()
    {
        try
        {
            string path = Application.persistentDataPath + "/savedata.dat";
        
            if(File.Exists(path))
            {
                using(BinaryReader rd = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    score = rd.ReadInt32();
                    barbClear = rd.ReadBoolean();
                    archerClear = rd.ReadBoolean();
                    rogueClear = rd.ReadBoolean();
                    witchClear = rd.ReadBoolean();
                    clericClear = rd.ReadBoolean();
                    monkClear = rd.ReadBoolean();
                    druidClear = rd.ReadBoolean();
                }
            }
            
            else
            {
                throw new IOException("Save Data Not Found");
            }
        }
        catch(IOException ex)
        {
            Debug.LogWarning("Failed to read data: " + ex.Message);
            return false;
        }
        
        Debug.Log("Data read successfully");
        Debug.Log("Data: " + score + " " + barbClear + "...");
        return true;
    }
    
    ////////////////////////////
    //void SaveData()         //
    //Saves data to a file    //
    //Currently includes:     //
    //-High Score             //
    //-Character Medals/Clears//
    ////////////////////////////
    public void SaveData()
    {
        string path = Application.persistentDataPath + "/savedata.dat";
        
        using(BinaryWriter wr = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            wr.Write(score);
            wr.Write(barbClear);
            wr.Write(archerClear);
            wr.Write(rogueClear);
            wr.Write(witchClear);
            wr.Write(clericClear);
            wr.Write(monkClear);
            wr.Write(druidClear);
        }
        
        Debug.Log("Data Saved");
    }
}
