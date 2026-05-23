# 自我迭代系统使用指南

## 系统架构

### 四层架构
1. **数据层**: SQLite 持久化存储
2. **服务层**: SelfReflectionService/PatternMiner/AutoEvolver/ExperienceMemoryStore
3. **插件层**: SelfImprovementPlugin (SK Tool 集成)
4. **应用层**: ChatViewModel/HomeViewModel

### 核心服务
- **SelfReflectionService**: 任务反思记录，成功率计算，改进建议生成
- **PatternMiner**: 模式挖掘与推荐，基于历史数据提取最佳实践
- **AutoEvolver**: A/B 测试框架，策略优化与自动切换
- **ExperienceMemoryStore**: 经验记忆存储，置信度评分与主动检索

## 功能说明

### 1. 自动反馈记录
- 触发时机：每次对话任务完成后
- 记录内容：成功率、执行时间、使用工具、改进建议
- 存储位置：SQLite 数据库

### 2. 智能模式推荐
- 触发条件：用户请求模式推荐或新任务创建
- 推荐逻辑：基于成功率和相似度匹配
- 输出格式：Markdown 格式详细说明

### 3. 经验记忆检索
- 触发时机：新任务创建时
- 检索逻辑：基于任务相似度，置信度>0.7 才注入
- 注入方式：自动添加到 Prompt 上下文

### 4. A/B 测试优化
- 实验类型：模型参数/提示词/工具配置
- 采纳规则：实验评分>75% 自动采纳
- 回滚机制：失败实验自动恢复

## 工具函数

### SELF_IMPROVE-get_system_health
查看系统健康状态，包括模式库/经验库/实验状态。

### SELF_IMPROVE-recommend_pattern
请求推荐最佳实践模式，需要提供任务描述。

### SELF_IMPROVE-retrieve_experiences
检索历史经验记忆，需要提供关键词。

### SELF_IMPROVE-reflect_on_recent_tasks
查看最近任务反思总结。

### SELF_IMPROVE-mine_patterns
主动挖掘新模式，需要积累足够数据。

### SELF_IMPROVE-record_feedback
手动记录任务反馈，用于补充自动记录。

### SELF_IMPROVE-start_optimization_experiment
启动 A/B 测试实验，需要指定策略名称和测试类型。

### SELF_IMPROVE-record_experiment_metric
记录实验指标数据，用于统计分析。
