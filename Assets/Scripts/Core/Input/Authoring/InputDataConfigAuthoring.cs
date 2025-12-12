using Unity.Entities;
using UnityEngine;

public class InputDataAuthoring : MonoBehaviour
{
    
}

public class InputDataAuthoringBaker : Baker<InputDataAuthoring>
{
    public override void Bake(InputDataAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent<InputDataConfig>(entity);
    }
}
