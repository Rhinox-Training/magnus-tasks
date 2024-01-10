using System;

namespace Rhinox.Magnus.Tasks
{
    public class RegisterApplicatorAttribute : Attribute
    {
        public Type DataType;

        public RegisterApplicatorAttribute(Type dataType)
        {
            DataType = dataType;
        }
    }
}