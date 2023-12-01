using System;
using System.Reflection;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [HideReferenceObjectPicker, Serializable]
    [RefactoringOldNamespace("Rhinox.VOLT.Data", "com.rhinox.volt")]
    public class ObjectMemberData : IEquatable<ObjectMemberData>
    {
        [SerializeReference]
        public SerializableMemberInfo MemberInfo;
        [SerializeReference]
        public IMemberDataSource MemberData;
        
        public bool ResetValue()
        {
            if (MemberData != null)
                return MemberData.ResetValue();
            return false;
        }
        
        public bool Equals(ObjectMemberData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(MemberInfo, other.MemberInfo) && Equals(MemberData, other.MemberData);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObjectMemberData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((MemberInfo != null ? MemberInfo.GetHashCode() : 0) * 397) ^ (MemberData != null ? MemberData.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ObjectMemberData left, ObjectMemberData right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ObjectMemberData left, ObjectMemberData right)
        {
            return !Equals(left, right);
        }
    }
}