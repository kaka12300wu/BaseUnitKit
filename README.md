# BaseUnitKit

基于组件模式的Unity单位系统框架，提供灵活的组件化架构，支持动态组件管理、生命周期控制和高效的更新机制。

## 📋 目录

- [特性](#特性)
- [安装](#安装)
- [快速开始](#快速开始)
- [核心概念](#核心概念)
- [使用指南](#使用指南)
- [API参考](#api参考)
- [最佳实践](#最佳实践)
- [示例代码](#示例代码)

## ✨ 特性

### 🏗️ 组件化架构
- **BaseUnit基类**: 提供统一的单位基础架构
- **IUnitComponent接口**: 标准化的组件生命周期管理
- **自动组件发现**: 自动识别和注册GameObject上的组件
- **类型安全**: 强类型的组件获取和管理

### ⚡ 生命周期管理
- **统一初始化**: 控制所有组件的初始化顺序
- **启用/禁用控制**: 动态控制组件的激活状态
- **清理机制**: 自动处理组件的资源清理

### 🔄 更新机制
- **IUpdatableComponent接口**: 为纯C#组件提供更新机制
- **MonoBehaviour分离**: MonoBehaviour组件使用Unity原生Update
- **性能优化**: 集中管理更新逻辑，减少Update调用开销

### 🔧 动态管理
- **运行时添加/移除**: 支持运行时动态操作组件
- **类型去重**: 防止同类型组件重复添加
- **实例检查**: 避免同一实例的重复注册

## 📦 安装

### 通过Package Manager安装

1. 打开Unity Editor
2. 进入 `Window > Package Manager`
3. 点击 `+` 按钮选择 `Add package from git URL`
4. 输入包的Git URL或选择本地路径
5. 点击 `Add` 完成安装

### 依赖项

```json
{
  "com.cysharp.unitask": "2.5.10"
}
```

## 🚀 快速开始

### 1. 创建基础单位类

```csharp
using Units;
using UnityEngine;

public class Player : BaseUnit
{
    [SerializeField] private int health = 100;
    [SerializeField] private float speed = 5f;
    
    protected override void OnInitialize()
    {
        Debug.Log("Player initialized with health: " + health);
    }
}
```

### 2. 创建MonoBehaviour组件

```csharp
using Units;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IUnitComponent
{
    public BaseUnit Owner { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    public void Initialize()
    {
        currentHealth = maxHealth;
        Debug.Log($"Health component initialized: {currentHealth}/{maxHealth}");
    }
    
    public void Enable()
    {
        IsEnabled = true;
        enabled = true;
    }
    
    public void Disable()
    {
        IsEnabled = false;
        enabled = false;
    }
    
    public void OnAddToUnit(BaseUnit owner)
    {
        Owner = owner;
    }
    
    public void OnRemoveFromUnit()
    {
        Owner = null;
    }
    
    public void TakeDamage(int damage)
    {
        if (!IsEnabled) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (currentHealth <= 0)
        {
            Debug.Log($"{Owner.name} has died!");
        }
    }
}
```

### 3. 创建纯C#组件

```csharp
using Units;

public class BuffSystem : IUpdatableComponent
{
    public BaseUnit Owner { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    
    private float buffTimer;
    private const float BUFF_DURATION = 10f;
    
    public void Initialize()
    {
        buffTimer = BUFF_DURATION;
    }
    
    public void Enable()
    {
        IsEnabled = true;
    }
    
    public void Disable()
    {
        IsEnabled = false;
    }
    
    public void OnAddToUnit(BaseUnit owner)
    {
        Owner = owner;
    }
    
    public void OnRemoveFromUnit()
    {
        Owner = null;
    }
    
    public void OnUpdate(float deltaTime)
    {
        if (!IsEnabled) return;
        
        buffTimer -= deltaTime;
        if (buffTimer <= 0f)
        {
            // Buff expired
            buffTimer = BUFF_DURATION;
        }
    }
}
```

### 4. 运行时动态添加组件

```csharp
public class GameManager : MonoBehaviour
{
    void Start()
    {
        var player = FindObjectOfType<Player>();
        
        // 添加纯C#组件
        var buffSystem = new BuffSystem();
        player.AddComponent(buffSystem);
        
        // 获取组件
        var healthComponent = player.GetComponent<HealthComponent>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(25);
        }
    }
}
```

## 💡 核心概念

### BaseUnit基类
- 继承自`MonoBehaviour`的抽象基类
- 管理所有`IUnitComponent`组件的生命周期
- 提供自动组件发现和手动组件管理功能
- 统一处理组件的初始化、更新和清理

### IUnitComponent接口
所有组件必须实现的基础接口：
- `Owner`: 持有该组件的BaseUnit实例
- `IsEnabled`: 组件启用状态
- `Initialize()`: 组件初始化方法
- `Enable()/Disable()`: 启用/禁用组件
- `OnAddToUnit()/OnRemoveFromUnit()`: 添加/移除生命周期方法

### IUpdatableComponent接口
继承自`IUnitComponent`，为纯C#组件提供更新机制：
- **重要规则**: MonoBehaviour组件禁止实现此接口
- `OnUpdate(float deltaTime)`: 每帧更新方法
- 由BaseUnit统一调度，避免过多的Unity Update调用

## 📖 使用指南

### 组件类型选择

#### MonoBehaviour组件
- 需要Unity生命周期方法（Start, Update等）
- 需要SerializeField序列化字段
- 需要在Inspector中配置参数
- 只能实现`IUnitComponent`接口

```csharp
public class MovementComponent : MonoBehaviour, IUnitComponent
{
    [SerializeField] private float speed = 5f;
    
    // 使用Unity的Update而非IUpdatableComponent
    void Update()
    {
        if (!IsEnabled) return;
        // 移动逻辑
    }
}
```

#### 纯C#组件
- 纯逻辑组件，不需要Unity特性
- 可以实现`IUpdatableComponent`接口
- 更轻量级，性能更好
- 适合数据驱动的系统

```csharp
public class StateMachine : IUpdatableComponent
{
    public void OnUpdate(float deltaTime)
    {
        // 状态机更新逻辑
    }
}
```

### 组件管理规则

#### 自动发现机制
BaseUnit会在Awake时自动发现GameObject上的所有IUnitComponent组件：

```csharp
// 这些组件会被自动发现和注册
public class Player : BaseUnit
{
    // HealthComponent, MovementComponent等会被自动添加
}
```

#### 手动添加组件
运行时可以动态添加纯C#组件：

```csharp
var player = GetComponent<Player>();

// 添加buff系统
player.AddComponent(new BuffSystem());

// 添加技能系统  
player.AddComponent(new SkillSystem());
```

#### 组件去重机制
- 同一类型的组件只能存在一个实例
- 尝试添加重复类型会被忽略并输出警告
- 确保组件系统的稳定性

### 生命周期控制

```csharp
public class CustomUnit : BaseUnit
{
    protected override void OnInitialize()
    {
        // 在所有组件Initialize之前调用
        // 适合设置单位的基础属性
    }
    
    protected override void Update()
    {
        base.Update(); // 确保调用基类Update
        
        // 自定义更新逻辑
    }
}
```

## 📚 API参考

### BaseUnit核心方法

| 方法 | 描述 |
|------|------|
| `Initialize()` | 初始化单位和所有组件 |
| `AddComponent<T>(T component)` | 添加组件到单位 |
| `RemoveComponent<T>(T component)` | 从单位移除组件 |
| `GetComponent<T>()` | 获取指定类型的组件 |

### BaseUnit重要属性

| 属性 | 类型 | 描述 |
|------|------|------|
| `IsInitialized` | bool | 单位是否已初始化 |
| `components` | List<IUnitComponent> | 所有组件列表 |
| `updateableComponents` | List<IUpdatableComponent> | 需要更新的组件列表 |

### IUnitComponent接口方法

| 方法 | 描述 |
|------|------|
| `Initialize()` | 组件初始化 |
| `Enable()` | 启用组件 |
| `Disable()` | 禁用组件 |
| `OnAddToUnit(BaseUnit owner)` | 添加到单位时调用 |
| `OnRemoveFromUnit()` | 从单位移除时调用 |

### IUpdatableComponent接口方法

| 方法 | 描述 |
|------|------|
| `OnUpdate(float deltaTime)` | 组件更新方法 |

## 💡 最佳实践

### 架构设计
1. **明确组件职责**: 每个组件专注单一功能
2. **合理选择组件类型**: MonoBehaviour vs 纯C#组件
3. **避免组件间直接依赖**: 通过Owner或事件系统通信
4. **遵循单一职责原则**: 保持组件的独立性和可复用性

### 性能优化
1. **优先使用纯C#组件**: 对于纯逻辑功能
2. **避免频繁添加/移除组件**: 在运行时
3. **合理使用Enable/Disable**: 而非频繁创建销毁
4. **批量处理**: 相关操作尽量集中处理

### 开发规范
1. **组件命名**: 使用清晰的Component后缀
2. **接口实现**: 完整实现所有接口方法
3. **异常处理**: 在关键方法中添加空值检查
4. **日志记录**: 适当添加调试信息

### 调试技巧
1. **使用IsInitialized检查**: 确保组件已正确初始化
2. **监控组件数量**: 避免组件过多影响性能
3. **检查组件状态**: 使用IsEnabled验证组件状态
4. **利用Unity Profiler**: 分析Update性能

## 🔧 示例代码

### 完整的单位示例

```csharp
// 基础单位类
public class Enemy : BaseUnit
{
    [SerializeField] private EnemyData enemyData;
    
    protected override void OnInitialize()
    {
        // 根据数据配置单位
        if (enemyData != null)
        {
            var health = GetComponent<HealthComponent>();
            if (health != null)
            {
                health.SetMaxHealth(enemyData.maxHealth);
            }
        }
    }
}

// AI组件（MonoBehaviour）
public class AIComponent : MonoBehaviour, IUnitComponent
{
    public BaseUnit Owner { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    
    [SerializeField] private float detectionRange = 10f;
    private Transform target;
    
    public void Initialize()
    {
        // 寻找玩家目标
        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            target = player.transform;
        }
    }
    
    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
    
    public void OnAddToUnit(BaseUnit owner) => Owner = owner;
    public void OnRemoveFromUnit() => Owner = null;
    
    void Update()
    {
        if (!IsEnabled || target == null) return;
        
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= detectionRange)
        {
            // 执行AI逻辑
            ChaseTarget();
        }
    }
    
    private void ChaseTarget()
    {
        var movement = Owner.GetComponent<MovementComponent>();
        movement?.MoveTowards(target.position);
    }
}

// 状态系统（纯C#组件）
public class StateSystem : IUpdatableComponent
{
    public BaseUnit Owner { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    
    private enum State { Idle, Moving, Attacking, Dead }
    private State currentState = State.Idle;
    
    public void Initialize()
    {
        ChangeState(State.Idle);
    }
    
    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
    
    public void OnAddToUnit(BaseUnit owner) => Owner = owner;
    public void OnRemoveFromUnit() => Owner = null;
    
    public void OnUpdate(float deltaTime)
    {
        if (!IsEnabled) return;
        
        // 状态机更新逻辑
        switch (currentState)
        {
            case State.Idle:
                UpdateIdleState();
                break;
            case State.Moving:
                UpdateMovingState();
                break;
            case State.Attacking:
                UpdateAttackingState();
                break;
        }
    }
    
    private void ChangeState(State newState)
    {
        currentState = newState;
        // 状态切换逻辑
    }
    
    private void UpdateIdleState() { /* 空闲状态逻辑 */ }
    private void UpdateMovingState() { /* 移动状态逻辑 */ }
    private void UpdateAttackingState() { /* 攻击状态逻辑 */ }
}
```

### 组件通信示例

```csharp
// 使用事件进行组件通信
public class HealthComponent : MonoBehaviour, IUnitComponent
{
    public System.Action<int> OnHealthChanged;
    public System.Action OnDeath;
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
}

public class UIHealthBar : MonoBehaviour, IUnitComponent
{
    public void Initialize()
    {
        var health = Owner.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.OnHealthChanged += UpdateHealthBar;
            health.OnDeath += ShowDeathEffect;
        }
    }
    
    private void UpdateHealthBar(int newHealth)
    {
        // 更新血条显示
    }
    
    private void ShowDeathEffect()
    {
        // 显示死亡效果
    }
}
```

---

**作者**: EricZhao  
**邮箱**: kaka12300wu@gmail.com  
**版本**: 1.0.0  
**Unity版本要求**: 2022.3.22f1+