using Microsoft.EntityFrameworkCore;
using QM_AI.API.Models;
using QM_AI.API.Services;

namespace QM_AI.API.Data;

/// <summary>
/// 数据库初始化器 — 启动时自动创建种子数据
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// EF Core 模型期望的所有表名
    /// </summary>
    private static readonly HashSet<string> ModelTableNames =
    [
        "Users", "Roles", "Permissions",
        "Products", "Boms", "Processes", "Routings",
        "InspectionStandards", "DefectCodes",
        "Equipment", "Tools", "Suppliers", "Customers",
        "ParamGroups", "DynamicParams", "ClosureRules", "ParamRealtimeValues",
        "IpqcFirstPieces", "IpqcFirstPieceItems",
        "IpqcPatrolPlans", "IpqcPatrols", "IpqcPatrolItems",
        "IpqcAiRiskScores", "IpqcClosureStatuses",
        "ProductBatches", "FqcInspections", "FqcInspectionItems",
        "OqcReleases", "PackagingConfirmations",
    ];

    public static async Task Initialize(AppDbContext context)
    {
        // 检查所有模型表是否存在，避免 EnsureCreatedAsync() 的"数据库已存在则跳过"问题
        var existingTables = await GetExistingTableNames(context);
        var missingTables = ModelTableNames.Except(existingTables).ToList();

        if (missingTables.Count > 0)
        {
            // 有表缺失 → 删库重建（开发阶段安全操作）
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            // 所有表已存在，仍需 EnsureCreatedAsync() 确保数据库存在（首次无操作）
            await context.Database.EnsureCreatedAsync();
        }

        // 已有数据则跳过
        if (await context.Users.AnyAsync())
            return;

        // ─── 角色 ────────────────────────────────────────────────
        var adminRole = new Role
        {
            Name = "Administrator",
            Description = "系统管理员，拥有全部权限"
        };
        var operatorRole = new Role
        {
            Name = "Operator",
            Description = "质检操作员"
        };
        var inspectorRole = new Role
        {
            Name = "Inspector",
            Description = "质量检验员"
        };
        var engineerRole = new Role
        {
            Name = "Engineer",
            Description = "质量工程师"
        };

        context.Roles.AddRange(adminRole, operatorRole, inspectorRole, engineerRole);
        await context.SaveChangesAsync();

        // ─── 权限 ────────────────────────────────────────────────
        var permissions = new List<Permission>
        {
            new() { Name = "全部权限", Code = "*:*", Module = "System" },
            new() { Name = "用户管理", Code = "system:user", Module = "M15" },
            new() { Name = "角色管理", Code = "system:role", Module = "M15" },
            new() { Name = "基础数据管理", Code = "basic:data", Module = "M02" },
            new() { Name = "IQC 检验", Code = "iqc:inspect", Module = "M03" },
            new() { Name = "IPQC 检验", Code = "ipqc:inspect", Module = "M04" },
            new() { Name = "FQC 检验", Code = "fqc:inspect", Module = "M05" },
            new() { Name = "SPC 查看", Code = "spc:view", Module = "M06" },
            new() { Name = "不良管理", Code = "defect:manage", Module = "M07" },
            new() { Name = "质量追溯", Code = "trace:view", Module = "M08" },
            new() { Name = "AI 分析", Code = "ai:analyze", Module = "M10" },
            new() { Name = "报表查看", Code = "report:view", Module = "M14" },
        };
        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();

        // ─── 管理员用户 ───────────────────────────────────────────
        var authService = new AuthService(context, new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "QM-AI-SuperSecretKey-2024-MustBeAtLeast32CharactersLong!",
                ["Jwt:Issuer"] = "QM-AI.API",
                ["Jwt:Audience"] = "QM-AI.Client",
                ["Jwt:ExpirationHours"] = "24"
            }!)
            .Build());

        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = authService.HashPassword("admin123"),
            DisplayName = "系统管理员",
            Avatar = "",
            Email = "admin@qm-ai.com",
            IsActive = true,
            RoleId = adminRole.Id
        };

        var operatorUser = new User
        {
            Username = "operator",
            PasswordHash = authService.HashPassword("operator123"),
            DisplayName = "质检操作员",
            Email = "operator@qm-ai.com",
            IsActive = true,
            RoleId = operatorRole.Id
        };

        var inspectorUser = new User
        {
            Username = "inspector",
            PasswordHash = authService.HashPassword("inspector123"),
            DisplayName = "质量检验员",
            Email = "inspector@qm-ai.com",
            IsActive = true,
            RoleId = inspectorRole.Id
        };

        context.Users.AddRange(adminUser, operatorUser, inspectorUser);
        await context.SaveChangesAsync();

        // ─── M02 基础数据种子 ─────────────────────────────────────
        #region Products
        var products = new List<Product>
        {
            new() { Code = "P001", Name = "精密转轴 A100", Category = "机加工件", Unit = "pcs", DefaultInspectionLevel = "II", DefaultAql = 1.0 },
            new() { Code = "P002", Name = "壳体 B200", Category = "压铸件", Unit = "pcs", DefaultInspectionLevel = "II", DefaultAql = 0.65 },
            new() { Code = "P003", Name = "PCB 主板 C300", Category = "电子件", Unit = "pcs", DefaultInspectionLevel = "S-3", DefaultAql = 0.25 },
            new() { Code = "P004", Name = "密封圈 D400", Category = "橡胶件", Unit = "pcs", DefaultInspectionLevel = "I", DefaultAql = 2.5 },
            new() { Code = "P005", Name = "连接线束 E500", Category = "标准件", Unit = "套", DefaultInspectionLevel = "II", DefaultAql = 1.5 },
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();
        #endregion

        #region Processes
        var processes = new List<Process>
        {
            new() { Code = "PRC-01", Name = "来料检验", ProcessType = "检验", Department = "品质部" },
            new() { Code = "PRC-02", Name = "粗车", ProcessType = "加工", Department = "机加车间" },
            new() { Code = "PRC-03", Name = "精车", ProcessType = "加工", Department = "机加车间" },
            new() { Code = "PRC-04", Name = "钻孔", ProcessType = "加工", Department = "机加车间" },
            new() { Code = "PRC-05", Name = "热处理", ProcessType = "加工", Department = "热处理车间" },
            new() { Code = "PRC-06", Name = "研磨", ProcessType = "加工", Department = "机加车间" },
            new() { Code = "PRC-07", Name = "过程检验", ProcessType = "检验", Department = "品质部" },
            new() { Code = "PRC-08", Name = "成品检验", ProcessType = "检验", Department = "品质部" },
            new() { Code = "PRC-09", Name = "清洗包装", ProcessType = "包装", Department = "包装车间" },
            new() { Code = "PRC-10", Name = "出货检验", ProcessType = "检验", Department = "品质部" },
        };
        context.Processes.AddRange(processes);
        await context.SaveChangesAsync();
        #endregion

        #region Routings (P001 精密转轴的工艺路线)
        var routings = new List<Routing>
        {
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 1, ProcessId = processes[0].Id, StandardTimeMinutes = 5 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 2, ProcessId = processes[1].Id, StandardTimeMinutes = 15 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 3, ProcessId = processes[2].Id, StandardTimeMinutes = 20 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 4, ProcessId = processes[3].Id, StandardTimeMinutes = 10 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 5, ProcessId = processes[4].Id, StandardTimeMinutes = 30 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 6, ProcessId = processes[5].Id, StandardTimeMinutes = 25 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 7, ProcessId = processes[6].Id, StandardTimeMinutes = 5 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 8, ProcessId = processes[7].Id, StandardTimeMinutes = 5 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 9, ProcessId = processes[8].Id, StandardTimeMinutes = 10 },
            new() { ProductId = products[0].Id, Code = "RT-P001", StepOrder = 10, ProcessId = processes[9].Id, StandardTimeMinutes = 5 },
        };
        context.Routings.AddRange(routings);
        await context.SaveChangesAsync();
        #endregion

        #region BOM (P001 精密转轴的物料清单)
        var boms = new List<Bom>
        {
            new() { ProductId = products[0].Id, MaterialCode = "MAT-001", MaterialName = "45# 圆钢 Φ50", Quantity = 1.2, Unit = "kg", Level = 1 },
            new() { ProductId = products[0].Id, MaterialCode = "MAT-002", MaterialName = "轴承 6205", Quantity = 2, Unit = "pcs", Level = 1 },
            new() { ProductId = products[0].Id, MaterialCode = "MAT-003", MaterialName = "润滑油", Quantity = 0.05, Unit = "L", Level = 1 },
        };
        context.Boms.AddRange(boms);
        await context.SaveChangesAsync();
        #endregion

        #region InspectionStandards
        var standards = new List<InspectionStandard>
        {
            new() { Code = "STD-001", Name = "直径公差检验", InspectionType = "IPQC", ProductId = products[0].Id, ProcessId = processes[2].Id, ItemName = "外径", Usl = 50.05, Lsl = 49.95, Target = 50.0, Unit = "mm", InspectionMethod = "千分尺", SamplingFrequency = "每批次5件" },
            new() { Code = "STD-002", Name = "粗糙度检验", InspectionType = "FQC", ProductId = products[0].Id, ProcessId = processes[7].Id, ItemName = "表面粗糙度 Ra", Usl = 1.6, Lsl = 0, Target = 0.8, Unit = "μm", InspectionMethod = "粗糙度仪", SamplingFrequency = "每批次3件" },
            new() { Code = "STD-003", Name = "硬度检验", InspectionType = "IPQC", ProductId = products[0].Id, ProcessId = processes[4].Id, ItemName = "硬度 HRC", Usl = 58, Lsl = 52, Target = 55, Unit = "HRC", InspectionMethod = "硬度计", SamplingFrequency = "每批次2件" },
        };
        context.InspectionStandards.AddRange(standards);
        await context.SaveChangesAsync();
        #endregion

        #region DefectCodes
        var defectCodes = new List<DefectCode>
        {
            new() { Code = "D001", Name = "尺寸超差", DefectType = "尺寸", Severity = "MA", IsReworkable = true },
            new() { Code = "D002", Name = "表面划伤", DefectType = "外观", Severity = "MI", IsReworkable = true },
            new() { Code = "D003", Name = "裂纹", DefectType = "外观", Severity = "CR", IsReworkable = false },
            new() { Code = "D004", Name = "硬度不足", DefectType = "功能", Severity = "MA", IsReworkable = true },
            new() { Code = "D005", Name = "材料夹杂", DefectType = "材料", Severity = "CR", IsReworkable = false },
            new() { Code = "D006", Name = "螺纹不合格", DefectType = "尺寸", Severity = "MA", IsReworkable = true },
            new() { Code = "D007", Name = "氧化生锈", DefectType = "外观", Severity = "MI", IsReworkable = true },
            new() { Code = "D008", Name = "装配不良", DefectType = "功能", Severity = "MA", IsReworkable = true },
        };
        context.DefectCodes.AddRange(defectCodes);
        await context.SaveChangesAsync();
        #endregion

        #region Equipment
        var equipment = new List<Equipment>
        {
            new() { Code = "CNC-001", Name = "数控车床 #1", Model = "CK6140", ProductionLine = "A线", Workshop = "机加车间", Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-a/cnc-001" },
            new() { Code = "CNC-002", Name = "数控车床 #2", Model = "CK6150", ProductionLine = "A线", Workshop = "机加车间", Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-a/cnc-002" },
            new() { Code = "CNC-003", Name = "数控铣床 #1", Model = "XK714", ProductionLine = "B线", Workshop = "机加车间", Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-b/cnc-003" },
            new() { Code = "HT-001", Name = "热处理炉", Model = "RX3-45-12", ProductionLine = "热处理线", Workshop = "热处理车间", Status = "running", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/heat/ht-001" },
            new() { Code = "GR-001", Name = "磨床 #1", Model = "M7130", ProductionLine = "A线", Workshop = "机加车间", Status = "idle", EquipmentType = "PLC", HasMqttConnection = false },
            new() { Code = "CMM-001", Name = "三坐标测量机", Model = "ZEISS CONTURA", ProductionLine = "质量中心", Workshop = "质量中心", Status = "running", EquipmentType = "检测设备", HasMqttConnection = false },
        };
        context.Equipment.AddRange(equipment);
        await context.SaveChangesAsync();
        #endregion

        #region Tools
        var tools = new List<Tool>
        {
            new() { Code = "T-001", Name = "外圆车刀 90°", Model = "WNMG080408", ToolType = "车刀", DesignLife = 500, LifeUnit = "cycles", CurrentLife = 120, Supplier = "山特维克" },
            new() { Code = "T-002", Name = "钻头 Φ8", Model = "D924-8.0", ToolType = "钻头", DesignLife = 300, LifeUnit = "cycles", CurrentLife = 45, Supplier = "OSG" },
            new() { Code = "T-003", Name = "面铣刀 Φ50", Model = "F4042.BS.050", ToolType = "铣刀", DesignLife = 400, LifeUnit = "cycles", CurrentLife = 200, Supplier = "肯纳" },
            new() { Code = "T-004", Name = "铰刀 Φ10H7", Model = "HR500-10.0", ToolType = "钻头", DesignLife = 350, LifeUnit = "cycles", CurrentLife = 80, Supplier = "OSG" },
            new() { Code = "T-005", Name = "砂轮 400×40", Model = "SA-40040", ToolType = "磨具", DesignLife = 200, LifeUnit = "hours", CurrentLife = 60, Supplier = "圣戈班" },
        };
        context.Tools.AddRange(tools);
        await context.SaveChangesAsync();
        #endregion

        #region Suppliers
        var suppliers = new List<Supplier>
        {
            new() { Code = "SUP-001", Name = "宝钢集团", Address = "上海市宝山区", ContactPerson = "张经理", ContactPhone = "021-55551234", Email = "zhang@baosteel.com", Grade = "A", SupplyCategory = "钢材", Score = 92 },
            new() { Code = "SUP-002", Name = "SKF 轴承", Address = "上海市嘉定区", ContactPerson = "李经理", ContactPhone = "021-55552345", Email = "li@skf.com", Grade = "A", SupplyCategory = "轴承", Score = 95 },
            new() { Code = "SUP-003", Name = "中石化润滑油", Address = "北京市朝阳区", ContactPerson = "王经理", ContactPhone = "010-55553456", Email = "wang@sinopec.com", Grade = "B", SupplyCategory = "润滑油", Score = 85 },
            new() { Code = "SUP-004", Name = "山特维克刀具", Address = "上海市浦东新区", ContactPerson = "赵经理", ContactPhone = "021-55554567", Email = "zhao@sandvik.com", Grade = "A", SupplyCategory = "刀具", Score = 93 },
        };
        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();
        #endregion

        #region Customers
        var customers = new List<Customer>
        {
            new() { Code = "CUST-001", Name = "华东精密机械有限公司", Address = "江苏省苏州市工业园区", ContactPerson = "刘经理", ContactPhone = "0512-55561234", Email = "liu@huadong.com" },
            new() { Code = "CUST-002", Name = "北方重工集团", Address = "辽宁省沈阳市铁西区", ContactPerson = "陈经理", ContactPhone = "024-55562345", Email = "chen@northheavy.com" },
            new() { Code = "CUST-003", Name = "华南汽车零部件有限公司", Address = "广东省广州市黄埔区", ContactPerson = "林经理", ContactPhone = "020-55563456", Email = "lin@huanan.com" },
        };
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
        #endregion

        #region M02.5 动态参数配置种子数据
        var paramGroups = new List<Models.M02_5.ParamGroup>
        {
            new() { Name = "热力学参数组", Code = "thermo_params", Description = "温度、热量相关工艺参数", SortOrder = 1, CreatedBy = 1 },
            new() { Name = "力学参数组", Code = "mech_params", Description = "压力、力值、扭矩等参数", SortOrder = 2, CreatedBy = 1 },
            new() { Name = "尺寸参数组", Code = "dim_params", Description = "长度、直径、公差等尺寸参数", SortOrder = 3, CreatedBy = 1 },
            new() { Name = "外观参数组", Code = "visual_params", Description = "外观、表面质量相关参数", SortOrder = 4, CreatedBy = 1 },
        };
        context.Set<Models.M02_5.ParamGroup>().AddRange(paramGroups);
        await context.SaveChangesAsync();

        var dynamicParams = new List<Models.M02_5.DynamicParam>
        {
            new() { GroupId = paramGroups[0].Id, Name = "温度-精加工", Code = "temp_finishing", DataType = "numeric", Unit = "℃", TargetValue = 450m, Usl = 455m, Lsl = 445m, Precision = 0.1m, AiStrategy = "{\"id\":\"normal_distribution\"}", SortOrder = 1, CreatedBy = 1 },
            new() { GroupId = paramGroups[0].Id, Name = "温度-热处理", Code = "temp_heat_treat", DataType = "numeric", Unit = "℃", TargetValue = 850m, Usl = 860m, Lsl = 840m, Precision = 1m, AiStrategy = "{\"id\":\"trend_analysis\"}", SortOrder = 2, CreatedBy = 1 },
            new() { GroupId = paramGroups[1].Id, Name = "切削压力", Code = "cutting_pressure", DataType = "numeric", Unit = "MPa", TargetValue = 12.5m, Usl = 13.5m, Lsl = 11.5m, Precision = 0.1m, AiStrategy = "{\"id\":\"outlier_detection\"}", SortOrder = 1, CreatedBy = 1 },
            new() { GroupId = paramGroups[1].Id, Name = "主轴扭矩", Code = "spindle_torque", DataType = "numeric", Unit = "N·m", TargetValue = 25m, Usl = 28m, Lsl = 22m, Precision = 0.5m, SortOrder = 2, CreatedBy = 1 },
            new() { GroupId = paramGroups[2].Id, Name = "外径公差", Code = "od_tolerance", DataType = "numeric", Unit = "mm", TargetValue = 50m, Usl = 50.05m, Lsl = 49.95m, Precision = 0.01m, AiStrategy = "{\"id\":\"cpk_monitoring\"}", SortOrder = 1, CreatedBy = 1 },
            new() { GroupId = paramGroups[2].Id, Name = "内径公差", Code = "id_tolerance", DataType = "numeric", Unit = "mm", TargetValue = 25m, Usl = 25.03m, Lsl = 24.97m, Precision = 0.01m, SortOrder = 2, CreatedBy = 1 },
            new() { GroupId = paramGroups[3].Id, Name = "表面粗糙度", Code = "surface_roughness", DataType = "numeric", Unit = "μm", TargetValue = 0.8m, Usl = 1.6m, Lsl = 0m, Precision = 0.1m, AiStrategy = "{\"id\":\"normal_distribution\"}", SortOrder = 1, CreatedBy = 1 },
            new() { GroupId = paramGroups[3].Id, Name = "表面缺陷", Code = "surface_defect", DataType = "categorical", Unit = "", Precision = 1m, AiStrategy = "{\"id\":\"pareto_analysis\"}", SortOrder = 2, CreatedBy = 1 },
            new() { GroupId = paramGroups[3].Id, Name = "防锈处理", Code = "rust_prevention", DataType = "boolean", Precision = 1m, SortOrder = 3, CreatedBy = 1 },
        };
        context.Set<Models.M02_5.DynamicParam>().AddRange(dynamicParams);
        await context.SaveChangesAsync();

        var closureRules = new List<Models.M02_5.ClosureRule>
        {
            new() { Name = "连续10件合格放行", Code = "consecutive_10_ok", ConditionJson = "[{\"type\":\"consecutive_ok\",\"paramCode\":\"\",\"threshold\":10.0,\"operator\":\">=\"}]", Logic = "AND", Description = "连续 10 件检验合格自动关单", CreatedBy = 1 },
            new() { Name = "Cpk+合格率双重验证", Code = "cpk_and_rate", ConditionJson = "[{\"type\":\"spk_cpk\",\"paramCode\":\"\",\"threshold\":1.33,\"operator\":\">\"},{\"type\":\"sampling_rate\",\"paramCode\":\"\",\"threshold\":98.0,\"operator\":\">=\"}]", Logic = "AND", Description = "Cpk > 1.33 且抽检合格率 ≥ 98%", CreatedBy = 1 },
            new() { Name = "低风险快速放行", Code = "low_risk_release", ConditionJson = "[{\"type\":\"ai_risk_score\",\"paramCode\":\"\",\"threshold\":40.0,\"operator\":\"<\"},{\"type\":\"sampling_rate\",\"paramCode\":\"\",\"threshold\":99.0,\"operator\":\">=\"}]", Logic = "AND", Description = "AI 风险评分 < 40 且合格率 ≥ 99%", CreatedBy = 1 },
        };
        context.Set<Models.M02_5.ClosureRule>().AddRange(closureRules);
        await context.SaveChangesAsync();
        #endregion
    }

    /// <summary>
    /// 获取数据库中现有的所有表名（含视图）
    /// </summary>
    private static async Task<HashSet<string>> GetExistingTableNames(AppDbContext context)
    {
        try
        {
            var tables = await context.Database
                .SqlQuery<string>($"""
                    SELECT TABLE_NAME
                    FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                      AND table_type = 'BASE TABLE'
                    """)
                .ToListAsync();
            return [.. tables];
        }
        catch
        {
            // 数据库还不存在则返回空集
            return [];
        }
    }
}
