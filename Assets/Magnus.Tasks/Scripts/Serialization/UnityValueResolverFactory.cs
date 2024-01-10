namespace Rhinox.Magnus.Tasks
{
    public class UnityEngine_Transform_Rhinox_Magnus_Tasks_UnityValueResolver_Generated : Rhinox.Magnus.Tasks.UnityValueResolver<UnityEngine.Transform> { }
    public class UnityEngine_GameObject_Rhinox_Magnus_Tasks_UnityValueResolver_Generated : Rhinox.Magnus.Tasks.UnityValueResolver<UnityEngine.GameObject> { }
    public class UnityEngine_UI_Button_Rhinox_Magnus_Tasks_UnityValueResolver_Generated : Rhinox.Magnus.Tasks.UnityValueResolver<UnityEngine.UI.Button> { }
    public class UnityEngine_Collider_Rhinox_Magnus_Tasks_UnityValueResolver_Generated : Rhinox.Magnus.Tasks.UnityValueResolver<UnityEngine.Collider> { }
    public class UnityEngine_Camera_Rhinox_Magnus_Tasks_UnityValueResolver_Generated : Rhinox.Magnus.Tasks.UnityValueResolver<UnityEngine.Camera> { }
    
    public class UnityValueResolverFactory : Rhinox.Magnus.BaseUnitySafeTypeFactory
    {
        public override System.Object BuildGenericType(System.Type t, System.Type genericTypeDefinition)
        {
            var type = genericTypeDefinition.MakeGenericType(t);
            if (type.IsAssignableFrom(typeof(UnityEngine_Transform_Rhinox_Magnus_Tasks_UnityValueResolver_Generated)))
            {
                return new UnityEngine_Transform_Rhinox_Magnus_Tasks_UnityValueResolver_Generated();
            }
            else if (type.IsAssignableFrom(typeof(UnityEngine_GameObject_Rhinox_Magnus_Tasks_UnityValueResolver_Generated)))
            {
                return new UnityEngine_GameObject_Rhinox_Magnus_Tasks_UnityValueResolver_Generated();
            }
            else if (type.IsAssignableFrom(typeof(UnityEngine_UI_Button_Rhinox_Magnus_Tasks_UnityValueResolver_Generated)))
            {
                return new UnityEngine_UI_Button_Rhinox_Magnus_Tasks_UnityValueResolver_Generated();
            }
            else if (type.IsAssignableFrom(typeof(UnityEngine_Collider_Rhinox_Magnus_Tasks_UnityValueResolver_Generated)))
            {
                return new UnityEngine_Collider_Rhinox_Magnus_Tasks_UnityValueResolver_Generated();
            }
            else if (type.IsAssignableFrom(typeof(UnityEngine_Camera_Rhinox_Magnus_Tasks_UnityValueResolver_Generated)))
            {
                return new UnityEngine_Camera_Rhinox_Magnus_Tasks_UnityValueResolver_Generated();
            }
            return null;
        }
    }
}
