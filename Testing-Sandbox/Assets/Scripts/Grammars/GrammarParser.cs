using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class GrammarParser : MonoBehaviour
{
    [SerializeField] string _baseString;
    [SerializeField] string[] _productionRuleStrings;

    private void Start()
    {
        Parse(_productionRuleStrings).Print();
    }

    private GrammarSymbol Parse(string[] productionRuleStrings)
    {
        //Build production rules dict
        Dictionary<string, string[]> productionRules = new Dictionary<string, string[]>();
        Dictionary<string, System.Type> allSymbols = GetAllSymbols();

        for (int i = 0; i < _productionRuleStrings.Length; i++)
        {
            string leftHand = productionRuleStrings[i].Split(':')[0];
            string rightHand = productionRuleStrings[i].Split(':')[1];
            string[] rightHandElements = rightHand.Split(',');

            //Validate rules
            if (!allSymbols.ContainsKey(leftHand)) { throw new System.Exception($"There is no defined class for {leftHand}"); }
            for (int j = 0; j < rightHandElements.Length; j++)
            {
                if (!allSymbols.ContainsKey(rightHandElements[j])) { throw new System.Exception($"There is no defined class for {rightHandElements[j]}"); }
            }

            productionRules.Add(leftHand, rightHandElements);
        }

        if (!productionRules.ContainsKey(_baseString))
            throw new System.Exception($"There is no non-terminal defined for {_baseString}");

        //return new SymbolSword();
        return Construct(_baseString, productionRules, allSymbols);
    }

    //Recursive
    private GrammarSymbol Construct(string leftHandSymbol, Dictionary<string, string[]> productionRules, Dictionary<string, System.Type> allSymbols)
    {
        GrammarSymbol baseObject = (GrammarSymbol)System.Activator.CreateInstance(allSymbols[leftHandSymbol]);
        string[] rightHand = null;
        if (productionRules.ContainsKey(leftHandSymbol)) { rightHand = productionRules[leftHandSymbol]; }
        if (rightHand != null)
        {
            //Debug.Log(rightHand.Length);
            GrammarSymbol[] subObjects = new GrammarSymbol[rightHand.Length];
            for (int i = 0; i < subObjects.Length; i++)
            {
                if (!allSymbols.ContainsKey(rightHand[i])) { throw new System.Exception($"The right hand symbol {rightHand[i]}, has not been defined"); }
                subObjects[i] = Construct(rightHand[i], productionRules, allSymbols);
            }
            baseObject.SetSubSymbols(subObjects);
        }
        return baseObject;
    }

    private Dictionary<string, System.Type> GetAllSymbols()
    {
        Dictionary<string, System.Type> grammarSymbols = new Dictionary<string, System.Type>();
        Assembly assembly = Assembly.GetCallingAssembly();

        foreach (System.Type type in assembly.GetTypes())
        {
            if(type.GetCustomAttributes(typeof(Symbol), true).Length > 0 && type.IsSubclassOf(typeof(GrammarSymbol)))
            {
                Symbol symbol = (Symbol)System.Attribute.GetCustomAttribute(type, typeof(Symbol));
                grammarSymbols.Add(symbol.id, type);
                //Debug.Log($"Added symbol '{symbol.id}'");
            }
        }

        return grammarSymbols;
    }
}
