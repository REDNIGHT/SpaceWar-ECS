
namespace Unity.Entities
{
    [AutoClear]
    public struct OnCreateMessage : IComponentData { }

    /// <summary>
    /// OnDestroyMessage适合用在存在一帧以上的entity
    /// </summary>
    public struct OnDestroyMessage : IComponentData { }

    /// <summary>
    /// 很多system都会排除OnDestroyMessage
    /// 用这个就不会有这问题
    /// 适合用在只有一帧的entity
    /// </summary>
    public struct OnDestroyWithoutMessage : IComponentData { }
}
