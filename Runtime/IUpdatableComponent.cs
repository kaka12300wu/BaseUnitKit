namespace Units
{
    /// <summary>
    /// 组件更新接口
    /// 需要周期性更新的组件可以实现此接口
    /// 重要规则：如果组件继承自MonoBehaviour，则不能实现该接口，只能实现IUnitComponent接口
    /// MonoBehaviour组件应该使用Unity的Update方法进行更新，而不是通过此接口
    /// 此接口仅适用于纯C#类组件
    /// </summary>
    public interface IUpdatableComponent : IUnitComponent
    {
        /// <summary>
        /// 组件更新方法
        /// 由BaseUnit在Update中调用
        /// </summary>
        /// <param name="deltaTime">距离上次更新的时间间隔</param>
        void OnUpdate(float deltaTime);
    }
}