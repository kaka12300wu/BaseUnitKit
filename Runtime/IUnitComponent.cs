namespace Combat.Units.Components
{
    /// <summary>
    /// 单位组件接口
    /// 提供组件的生命周期管理
    /// </summary>
    public interface IUnitComponent
    {
        /// <summary>
        /// 组件的持有者单位
        /// </summary>
        BaseUnit Owner { get; }
        
        /// <summary>
        /// 组件是否已启用
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// 初始化组件，通过Owner获取所有关联的Attribute和Component
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 启用组件
        /// </summary>
        void Enable();
        
        /// <summary>
        /// 禁用组件
        /// </summary>
        void Disable();
        
        /// <summary>
        /// 添加组件到单位时调用，设置Owner并进行组件相关的特效或状态初始化
        /// </summary>
        /// <param name="owner">持有者单位</param>
        void OnAddToUnit(BaseUnit owner);
        
        /// <summary>
        /// 从单位移除组件，清理相关的特效或状态
        /// </summary>
        void OnRemoveFromUnit();
    }
}