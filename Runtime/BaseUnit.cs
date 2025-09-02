using System;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    /// <summary>
    /// 基础单位类
    /// 负责调用基础单位的生命周期函数
    /// 需要从一个单位配置文件进行初始化
    /// </summary>
    public abstract class BaseUnit : MonoBehaviour
    {
        /// <summary>
        /// 所有组件的列表
        /// </summary>
        protected List<IUnitComponent> components = new();
        
        /// <summary>
        /// 需要更新的组件列表
        /// </summary>
        protected List<IUpdatableComponent> updateableComponents = new();
        
        /// <summary>
        /// 单位是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Unity Awake生命周期
        /// </summary>
        protected virtual void Awake()
        {
            Initialize();
        }
        
        /// <summary>
        /// 初始化单位
        /// </summary>
        public virtual void Initialize()
        {
            if (IsInitialized) return;

            // 自动获取所有继承自MonoBehaviour的IUnitComponent组件
            AutoDiscoverComponents();
            
            OnInitialize();
            
            // 在OnInitialize后，统一调用所有组件的Initialize方法
            foreach (var component in components)
            {
                component.Initialize();
            }
            
            IsInitialized = true;
        }
        
        /// <summary>
        /// 自动发现并注册所有IUnitComponent组件
        /// </summary>
        protected virtual void AutoDiscoverComponents()
        {
            // 获取GameObject上所有的MonoBehaviour组件
            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
            
            // 用于跟踪已添加的组件类型
            var addedTypes = new HashSet<Type>();
            
            foreach (var monoBehaviour in monoBehaviours)
            {
                // 检查是否实现了IUnitComponent接口
                if (monoBehaviour is IUnitComponent unitComponent)
                {
                    // 验证规则：继承自MonoBehaviour的组件不能实现IUpdatableComponent接口
                    if (monoBehaviour is IUpdatableComponent)
                    {
                        Debug.LogError($"组件 {monoBehaviour.GetType().Name} 继承自MonoBehaviour但实现了IUpdatableComponent接口，这是不被允许的。" +
                                       "MonoBehaviour组件应该使用Unity的Update方法而不是IUpdatableComponent。");
                        continue;
                    }
                    
                    // 检查类型去重：同一种类型的组件只能添加一个
                    Type componentType = monoBehaviour.GetType();
                    if (addedTypes.Contains(componentType))
                    {
                        Debug.LogWarning($"组件类型 {componentType.Name} 已经存在，跳过重复添加。");
                        continue;
                    }
                    
                    // 避免重复添加实例
                    if (!components.Contains(unitComponent))
                    {
                        components.Add(unitComponent);
                        addedTypes.Add(componentType);
                        unitComponent.OnAddToUnit(this);
                    }
                }
            }
        }
        
        /// <summary>
        /// 子类重写的初始化方法
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// Unity Update生命周期
        /// </summary>
        protected virtual void Update()
        {
            if (!IsInitialized) return;
            
            float deltaTime = Time.deltaTime;
            
            // 更新所有需要更新的组件
            for (int i = 0; i < updateableComponents.Count; i++)
            {
                updateableComponents[i].OnUpdate(deltaTime);
            }
        }
        
        /// <summary>
        /// Unity OnDestroy生命周期
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 移除所有组件
            var componentsToRemove = new List<IUnitComponent>(components);
            foreach (var component in componentsToRemove)
            {
                RemoveComponent(component);
            }
            
            components.Clear();
            updateableComponents.Clear();
        }
        
        /// <summary>
        /// 添加组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">组件实例</param>
        public virtual void AddComponent<T>(T component) where T : IUnitComponent
        {
            if (component == null) return;
            
            // 检查类型去重：同一种类型的组件只能添加一个
            Type componentType = component.GetType();
            foreach (var existingComponent in components)
            {
                if (existingComponent.GetType() == componentType)
                {
                    Debug.LogWarning($"组件类型 {componentType.Name} 已经存在，跳过添加。");
                    return;
                }
            }
            
            // 避免重复添加实例
            if (components.Contains(component))
            {
                Debug.LogWarning($"组件实例 {component.GetType().Name} 已经存在，跳过添加。");
                return;
            }
            
            components.Add(component);
            component.OnAddToUnit(this);
            
            // 如果单位已经初始化，立即调用组件的Initialize方法
            if (IsInitialized)
            {
                component.Initialize();
            }
            
            // 如果组件实现了IUpdatableComponent接口，添加到更新列表（仅限非MonoBehaviour组件）
            if (component is IUpdatableComponent updateableComponent && !(component is MonoBehaviour))
            {
                updateableComponents.Add(updateableComponent);
            }
        }
        
        /// <summary>
        /// 移除组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="component">组件实例</param>
        public virtual void RemoveComponent<T>(T component) where T : IUnitComponent
        {
            if (component == null) return;
            
            component.OnRemoveFromUnit();
            components.Remove(component);
            
            // 如果组件实现了IUpdatableComponent接口，从更新列表移除（仅限非MonoBehaviour组件）
            if (component is IUpdatableComponent updateableComponent && !(component is MonoBehaviour))
            {
                updateableComponents.Remove(updateableComponent);
            }
        }
        
        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>组件实例，如果不存在则返回null</returns>
        public new virtual T GetComponent<T>() where T : class, IUnitComponent
        {
            foreach (var component in components)
            {
                if (component is T targetComponent)
                {
                    return targetComponent;
                }
            }
            return null;
        }
    }
}