using System;
using UnityEngine;

namespace Rhinox
{
    public class TabAttribute : Attribute
    {
        public string Name;
        
        public TabAttribute(string name) { Name = name;}
    }
}