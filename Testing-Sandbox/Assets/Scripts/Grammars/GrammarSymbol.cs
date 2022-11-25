using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class GrammarSymbol
{
    private GrammarSymbol[] _subSymbols;

    public GrammarSymbol()
    {

    }
    public void SetSubSymbols(GrammarSymbol[] symbols)
    {
        _subSymbols = symbols;
    }
    public void Build()
    {
        for (int i = 0; i < _subSymbols.Length; i++)
        {
            _subSymbols[i].Build();
            BuildMesh();
        }
    }
    protected abstract void BuildMesh();

    public void Print()
    {
        UnityEngine.Debug.Log($"{GetID()}");
    }

    public virtual string GetID()
    {
        Symbol symbol = (Symbol)Attribute.GetCustomAttribute(this.GetType(), typeof(Symbol));
        string id = $"<{symbol.id}>";
        if (_subSymbols != null)
        {
            foreach (var sub in _subSymbols)
            {
                id += $"({sub.GetID()})";
            }
        }
        return id;
    }
}
