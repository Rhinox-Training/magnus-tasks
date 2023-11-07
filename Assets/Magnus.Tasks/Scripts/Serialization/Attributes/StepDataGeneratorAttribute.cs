using System;

namespace Rhinox.Magnus.Tasks
{
    public class StepDataGeneratorAttribute : Attribute
    {
        public string ConvertMethodName;

        public StepDataGeneratorAttribute(string convertMethodName)
        {
            ConvertMethodName = convertMethodName;
        }
    }
}