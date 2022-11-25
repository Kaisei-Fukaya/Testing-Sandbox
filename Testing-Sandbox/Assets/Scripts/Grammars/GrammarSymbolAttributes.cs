using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class Symbol : Attribute
{
    public string id;
    public Symbol(string id)
    {
        this.id = id;
    }
}
