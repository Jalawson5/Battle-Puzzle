using System.Collections;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//This script contains data for characters, including stats and their corresponding integer value.                        //
//Also contains data for base values used during combat, including damage, healing, leech multipliers, and control blocks.//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
public class CharacterData
{
    public const int NumCharacters = 7;
    public const int BaseHealth = 500;
    public const int HealthInc = 150;
    
    public const int BaseDamageLow = 200;
    public const int BaseDamageMid = 300;
    public const int BaseDamageHigh = 400;
    
    public const int BaseHealLow = 100;
    public const int BaseHealMid = 200;
    public const int BaseHealHigh = 300;
    
    public const float BaseLeechLow = 0.2f;
    public const float BaseLeechMid = 0.35f;
    public const float BaseLeechHigh = 0.5f;
    
    public const int BaseContLow = 1;
    public const int BaseContMid = 2;
    public const int BaseContHigh = 3;
    
    public const int SupHeal = 0;
    public const int SupShield = 1;
    public const int SupLeech = 2;
    
    public const int ConDrop = 0;
    public const int ConWeak = 1;
    public const int ConSilence = 2;
    
    //////////////////////////////////////////////////
    //List of Characters:                           //
    //0 = Barbarian                                 //
    //1 = Archer                                    //
    //2 = Rogue                                     //
    //3 = Witch                                     //
    //4 = Cleric                                    //
    //5 = Monk                                      //
    //6 = Druid                                     //
    //                                              //
    //Possibly more to come...                      //
    //                                              //
    //Monster (AI Only) Characters:                 //
    //-1 = Orc                                      //
    //-2 = Dragon                                   //
    //-3 = Demon King                               //
    //These characters are fought in the last stages//
    //////////////////////////////////////////////////
    
    //stat[5] = support type (0 = shield, 1 = heal, 2 = leech)//
    //stat[6] = control type (0 = drop, 1 = weaken, 2 = silence)//
    public const int CharBarbarian = 0;
    public static readonly int[] StatBarbarian = new int[]{7, 2, 4, 4, 10, SupShield, ConDrop};
    
    public const int CharArcher = 1;
    public static readonly int[] StatArcher = new int[]{10, 3, 3, 5, 6, SupLeech, ConWeak};
    
    public const int CharRogue = 2;
    public static readonly int[] StatRogue = new int[]{6, 4, 3, 10, 4, SupLeech, ConDrop};
    
    public const int CharWitch = 3;
    public static readonly int[] StatWitch = new int[]{2, 10, 6, 6, 3, SupShield, ConWeak};
    
    public const int CharCleric = 4;
    public static readonly int[] StatCleric = new int[]{3, 5, 10, 3, 6, SupHeal, ConSilence};
    
    public const int CharMonk = 5;
    public static readonly int[] StatMonk = new int[]{7, 5, 4, 3, 8, SupHeal, ConWeak};
    
    public const int CharDruid = 6;
    public static readonly int[] StatDruid = new int[]{4, 5, 7, 7, 4, SupHeal, ConSilence};
    
    //Monster Characters (AI Only)//
    public const int CharOrc = -1;
    public static readonly int[] StatOrc = new int[]{8, 2, 5, 6, 9, SupShield, ConWeak};
    
    public const int CharDragon = -2;
    public static readonly int[] StatDragon = new int[]{7, 9, 7, 4, 10, SupShield, ConSilence};
    
    public const int CharKing = -3;
    public static readonly int[] StatKing = new int[]{10, 9, 7, 6, 10, SupHeal, ConDrop};
    
    //////////////////////////////////////////////////////////////////////
    //int InitializeHealth(int)                                         //
    //Returns the max health of a character with the input vitality stat//
    //////////////////////////////////////////////////////////////////////
    public static int InitializeHealth(int vitality)
    {
        return BaseHealth + HealthInc * vitality;
    }
    
    /////////////////////////////////////////////////////////////////////////
    //int[] GetStats(int)                                                  //
    //Returns the proper stats for the input character.                    //
    //See list above for the integer value corresponding to each character.//
    /////////////////////////////////////////////////////////////////////////
    public static int[] GetStats(int choice)
    {
        switch(choice)
        {
            case CharBarbarian:
                return StatBarbarian;
            case CharArcher:
                return StatArcher;
            case CharRogue:
                return StatRogue;
            case CharWitch:
                return StatWitch;
            case CharCleric:
                return StatCleric;
            case CharMonk:
                return StatMonk;
            case CharDruid:
                return StatDruid;
            case CharOrc:
                return StatOrc;
            case CharDragon:
                return StatDragon;
            case CharKing:
                return StatKing;
            default:
                Debug.Log("Invalid Character Value, default to Barbarian");
                return StatBarbarian;
        }
    }
    
    /////////////////////////////////////////////////////////////////////////
    //string GetName(int)                                                  //
    //Returns the name of the input character.                             //
    //See list above for the integer value corresponding to each character.//
    /////////////////////////////////////////////////////////////////////////
    public static string GetName(int choice)
    {
        switch(choice)
        {
            case CharBarbarian:
                return "Barbarian";
            case CharArcher:
                return "Archer";
            case CharRogue:
                return "Rogue";
            case CharWitch:
                return "Witch";
            case CharCleric:
                return "Cleric";
            case CharMonk:
                return "Monk";
            case CharDruid:
                return "Druid";
            case CharOrc:
                return "Orc";
            case CharDragon:
                return "Dragon";
            case CharKing:
                return "Demon King";
            default:
                Debug.Log("Invalid Character Value, default to Barbarian");
                return "INVALID";                
        }
    }
    
    /////////////////////////////////////////////////////////////////////////
    //string GetDesc(int)                                                  //
    //Returns a description of the chosen character.                       //
    //See list above for the integer value corresponding to each character.//
    /////////////////////////////////////////////////////////////////////////
    public static string GetDesc(int choice)
    {
        switch(choice)
        {
            case CharBarbarian:
                return "The Barbarian stands strong, shrugging off blow after blow. Using the shield, the barbarian can take massive amounts of damage without falling.";
            case CharArcher:
                return "The Archer boasts the highest attack, defeating most opponents with ease. Set up attack and support combos to deal massive damage and take advantage of leech.";
            case CharRogue:
                return "The Rogue excels in disrupting their foe with garbage blocks. Attack your opponent while they are unable to combo, but be careful, a skilled opponent can use these blocks to their advantage...";
            case CharWitch:
                return "With the highest magic stat, the Witch crushes opponents with powerful spells. Remember, with such low health, your shield is essential to surviving against strong foes.";
            case CharCleric:
                return "Though the Cleric cannot deal significant damage, their ability to heal is unmatched. With frequent heals, the Cleric can outlast any opponent.";
            case CharMonk:
                return "The Monk uses both fists and magic to rush opponents with a flurry of attacks. Use both attack and magic blocks to apply pressure to your opponents.";
            case CharDruid:
                return "The Druid uses both support and control to outlast their opponent. Destroy your opponent's support blocks to prevent them from blocking or healing your weaker attacks.";
            default:
                Debug.Log("Invalid Character Value");
                return "INVALID";
        }
    }
}
