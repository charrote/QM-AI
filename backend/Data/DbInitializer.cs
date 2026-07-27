using Microsoft.EntityFrameworkCore;
using QM_AI.API.Models;
using QM_AI.API.Services;
using QM_AI.API.Models.M02_Inspection;
using QM_AI.API.Models.M03;
using QM_AI.API.Models.M04;
using QM_AI.API.Models.M05;
using QM_AI.API.Models.M06;

namespace QM_AI.API.Data;

/// <summary>
/// 数据库初始化器 — 启动时自动创建种子数据
/// </summary>
public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context)
    {
        Console.WriteLine("[DbInitializer] Initialize started...");
        
        // 跳过已有完整数据的初始化（检查 Roles 和 Organizations）
        var hasRoles = false;
        var hasOrgs = false;
        var hasDicts = false;
        try { hasRoles = await context.Roles.AnyAsync(); } catch { /* ignore */ }
        try { hasOrgs = await context.Organizations.AnyAsync(); } catch { /* ignore */ }
        try { hasDicts = await context.SysDictTypes.AnyAsync(); } catch { /* ignore */ }

        // ─── 角色（只在无角色时创建） ────────────────────────────
        if (!hasRoles) {
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
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 1, ProcessId = processes[0].Id, StandardTimeMinutes = 5 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 2, ProcessId = processes[1].Id, StandardTimeMinutes = 15 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 3, ProcessId = processes[2].Id, StandardTimeMinutes = 20 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 4, ProcessId = processes[3].Id, StandardTimeMinutes = 10 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 5, ProcessId = processes[4].Id, StandardTimeMinutes = 30 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 6, ProcessId = processes[5].Id, StandardTimeMinutes = 25 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 7, ProcessId = processes[6].Id, StandardTimeMinutes = 5 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 8, ProcessId = processes[7].Id, StandardTimeMinutes = 5 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 9, ProcessId = processes[8].Id, StandardTimeMinutes = 10 },
            new() { ProductId = products[0].Id, Code = "RT-P001", RoutingName = "精密转轴A100工艺路线", Description = "精密转轴A100工艺路线", StepOrder = 10, ProcessId = processes[9].Id, StandardTimeMinutes = 5 },
        };
        context.Routings.AddRange(routings);
        await context.SaveChangesAsync();
        #endregion

        #region BOM 物料清单
        var boms = new List<Bom>
        {
            // ── P001 精密转轴 A100（机加工件） ─────────────────────────
            new() { ProductId = products[0].Id, MaterialCode = "MAT-001", MaterialName = "45# 圆钢 Φ50", Quantity = 1.2, Unit = "kg", Level = 1, Remark = "宝钢供料，碳含量0.42-0.50%" },
            new() { ProductId = products[0].Id, MaterialCode = "MAT-002", MaterialName = "轴承 6205", Quantity = 2, Unit = "pcs", Level = 1, Remark = "SKF供料，深沟球轴承" },
            new() { ProductId = products[0].Id, MaterialCode = "MAT-003", MaterialName = "润滑油", Quantity = 0.05, Unit = "L", Level = 1, Remark = "中石化长城L-AN46" },
            new() { ProductId = products[0].Id, MaterialCode = "MAT-004", MaterialName = "防锈油", Quantity = 0.02, Unit = "L", Level = 1, Remark = "包装前防锈处理" },
            new() { ProductId = products[0].Id, MaterialCode = "MAT-005", MaterialName = "包装盒", Quantity = 1, Unit = "pcs", Level = 1, Remark = "瓦楞纸盒 200×80×80mm" },
            new() { ProductId = products[0].Id, MaterialCode = "MAT-006", MaterialName = "产品标签", Quantity = 1, Unit = "pcs", Level = 1, Remark = "含批次号、生产日期" },

            // ── P002 壳体 B200（压铸件） ─────────────────────────────
            new() { ProductId = products[1].Id, MaterialCode = "MAT-101", MaterialName = "ADC12 铝合金锭", Quantity = 2.5, Unit = "kg", Level = 1, Remark = "压铸件主体材料" },
            new() { ProductId = products[1].Id, MaterialCode = "MAT-102", MaterialName = "脱模剂", Quantity = 0.03, Unit = "L", Level = 1, Remark = "水性脱模剂" },
            new() { ProductId = products[1].Id, MaterialCode = "MAT-103", MaterialName = "密封圈 O型圈 25×3", Quantity = 2, Unit = "pcs", Level = 1, Remark = "NBR橡胶，耐油" },
            new() { ProductId = products[1].Id, MaterialCode = "MAT-104", MaterialName = "螺栓 M6×20", Quantity = 4, Unit = "pcs", Level = 1, Remark = "8.8级镀锌螺栓" },
            new() { ProductId = products[1].Id, MaterialCode = "MAT-105", MaterialName = "垫片 Φ6", Quantity = 4, Unit = "pcs", Level = 1, Remark = "弹簧垫片" },
            new() { ProductId = products[1].Id, MaterialCode = "MAT-106", MaterialName = "防尘罩", Quantity = 1, Unit = "pcs", Level = 1, Remark = "硅胶材质" },
            new() { ProductId = products[1].Id, MaterialCode = "MAT-107", MaterialName = "泡沫内衬", Quantity = 1, Unit = "pcs", Level = 1, Remark = "EPE珍珠棉定制" },

            // ── P003 PCB 主板 C300（电子件） ──────────────────────────
            new() { ProductId = products[2].Id, MaterialCode = "MAT-201", MaterialName = "FR-4 玻纤基板 150×100×1.6", Quantity = 1, Unit = "pcs", Level = 1, Remark = "4层板，阻焊绿油" },
            new() { ProductId = products[2].Id, MaterialCode = "MAT-202", MaterialName = "MCU STM32F103C8T6", Quantity = 1, Unit = "pcs", Level = 1, Remark = "主控芯片" },
            new() { ProductId = products[2].Id, MaterialCode = "MAT-203", MaterialName = "电容 100μF/16V", Quantity = 6, Unit = "pcs", Level = 1, Remark = "电解电容" },
            new() { ProductId = products[2].Id, MaterialCode = "MAT-204", MaterialName = "电容 0.1μF/50V", Quantity = 12, Unit = "pcs", Level = 1, Remark = "陶瓷电容" },
            new() { ProductId = products[2].Id, MaterialCode = "MAT-205", MaterialName = "电阻 10KΩ 1/4W", Quantity = 8, Unit = "pcs", Level = 1, Remark = "贴片电阻" },
            new() { ProductId = products[2].Id, MaterialCode = "MAT-206", MaterialName = "USB Type-C 接口", Quantity = 1, Unit = "pcs", Level = 1, Remark = "沉板焊接" },
            new() { ProductId = products[2].Id, MaterialCode = "MAT-207", MaterialName = "排针 2×10 Pin", Quantity = 2, Unit = "pcs", Level = 1, Remark = "2.54mm间距" },
            new() { ProductId = products[2].Id, MaterialCode = "MAT-208", MaterialName = "晶振 8MHz", Quantity = 1, Unit = "pcs", Level = 1, Remark = "无源晶振" },
            new() { ProductId = products[2].Id, MaterialCode = "MAT-209", MaterialName = "防静电袋", Quantity = 1, Unit = "pcs", Level = 1, Remark = "屏蔽包装袋" },

            // ── P004 密封圈 D400（橡胶件） ────────────────────────────
            new() { ProductId = products[3].Id, MaterialCode = "MAT-301", MaterialName = "NBR 橡胶原料", Quantity = 0.15, Unit = "kg", Level = 1, Remark = "丁腈橡胶，硬度70A" },
            new() { ProductId = products[3].Id, MaterialCode = "MAT-302", MaterialName = "润滑粉", Quantity = 0.001, Unit = "kg", Level = 1, Remark = "模具脱模用" },
            new() { ProductId = products[3].Id, MaterialCode = "MAT-303", MaterialName = "PE 自封袋", Quantity = 1, Unit = "pcs", Level = 1, Remark = "包装用" },
            new() { ProductId = products[3].Id, MaterialCode = "MAT-304", MaterialName = "干燥剂", Quantity = 1, Unit = "pcs", Level = 1, Remark = "硅胶干燥剂 5g" },

            // ── P005 连接线束 E500（标准件） ──────────────────────────
            new() { ProductId = products[4].Id, MaterialCode = "MAT-401", MaterialName = "PVC 护套线 2×0.75mm²", Quantity = 1.5, Unit = "m", Level = 1, Remark = "线束主线材" },
            new() { ProductId = products[4].Id, MaterialCode = "MAT-402", MaterialName = "端子 HT-05B", Quantity = 4, Unit = "pcs", Level = 1, Remark = "公母对插端子" },
            new() { ProductId = products[4].Id, MaterialCode = "MAT-403", MaterialName = "热缩管 Φ6 红", Quantity = 0.1, Unit = "m", Level = 1, Remark = "两端绝缘保护" },
            new() { ProductId = products[4].Id, MaterialCode = "MAT-404", MaterialName = "扎带 100mm 黑色", Quantity = 2, Unit = "pcs", Level = 1, Remark = "尼龙扎带固定" },
            new() { ProductId = products[4].Id, MaterialCode = "MAT-405", MaterialName = "缠绕管 Φ8 黑色", Quantity = 0.3, Unit = "m", Level = 1, Remark = "线束保护套管" },
            new() { ProductId = products[4].Id, MaterialCode = "MAT-406", MaterialName = "PE 包装袋", Quantity = 1, Unit = "pcs", Level = 1, Remark = "含产品标签" },
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
        } // end if (!hasRoles) — 以上为首次运行的完整种子数据

        #region M15 企业组织层级种子数据
        // 只有没有组织数据时才创建
        if (!hasOrgs)
        {
            var hq = new Organization { Code = "HQ", Name = "集团总部", Level = "group", SortOrder = 1 };
            context.Organizations.Add(hq);
            await context.SaveChangesAsync();

            var f1 = new Organization { Code = "FACTORY_1", Name = "第一工厂", Level = "company", ParentId = hq.Id, SortOrder = 1 };
            var f2 = new Organization { Code = "FACTORY_2", Name = "第二工厂", Level = "company", ParentId = hq.Id, SortOrder = 2 };
            context.Organizations.AddRange(f1, f2);
            await context.SaveChangesAsync();

            var ws1 = new Organization { Code = "WS_MACHINING", Name = "机加车间", Level = "workshop", ParentId = f1.Id, SortOrder = 1 };
            var ws2 = new Organization { Code = "WS_HEAT_TREAT", Name = "热处理车间", Level = "workshop", ParentId = f1.Id, SortOrder = 2 };
            var ws3 = new Organization { Code = "WS_ASSEMBLY", Name = "装配车间", Level = "workshop", ParentId = f2.Id, SortOrder = 1 };
            var ws4 = new Organization { Code = "WS_QUALITY", Name = "质量中心", Level = "workshop", ParentId = f2.Id, SortOrder = 2 };
            context.Organizations.AddRange(ws1, ws2, ws3, ws4);
            await context.SaveChangesAsync();

            var lines = new List<Organization>
            {
                new() { Code = "LINE_A", Name = "A线", Level = "line", ParentId = ws1.Id, SortOrder = 1 },
                new() { Code = "LINE_B", Name = "B线", Level = "line", ParentId = ws1.Id, SortOrder = 2 },
                new() { Code = "LINE_C", Name = "C线", Level = "line", ParentId = ws2.Id, SortOrder = 1 },
                new() { Code = "LINE_HEAT", Name = "热处理线", Level = "line", ParentId = ws2.Id, SortOrder = 2 },
                new() { Code = "LINE_ASSY_1", Name = "装配1线", Level = "line", ParentId = ws3.Id, SortOrder = 1 },
                new() { Code = "LINE_ASSY_2", Name = "装配2线", Level = "line", ParentId = ws3.Id, SortOrder = 2 },
                new() { Code = "LINE_QC", Name = "质量检测线", Level = "line", ParentId = ws4.Id, SortOrder = 1 },
            };
            context.Organizations.AddRange(lines);
            await context.SaveChangesAsync();
        }
        #endregion

        #region M15 系统字典种子数据
        if (!hasDicts)
        {
            var dictTypes = new List<SysDictType>
            {
                new() { TypeCode = "equipment_type", TypeName = "设备类型", IsSystem = true },
                new() { TypeCode = "process_type", TypeName = "工序类型", IsSystem = true },
                new() { TypeCode = "defect_category", TypeName = "不良分类", IsSystem = true },
                new() { TypeCode = "severity", TypeName = "严重等级", IsSystem = true },
                new() { TypeCode = "inspection_type", TypeName = "检验类型", IsSystem = true },
                new() { TypeCode = "product_category", TypeName = "产品类别", IsSystem = true },
                new() { TypeCode = "tool_type", TypeName = "刀具类型", IsSystem = true },
                new() { TypeCode = "supply_category", TypeName = "供应类别", IsSystem = true },
                new() { TypeCode = "material_unit", TypeName = "物料单位", IsSystem = true },
            };
            context.SysDictTypes.AddRange(dictTypes);
            await context.SaveChangesAsync();

            var dictItems = new List<SysDictItem>
            {
                new() { TypeCode = "equipment_type", ItemLabel = "CNC加工中心", ItemValue = "CNC", SortOrder = 1 },
                new() { TypeCode = "equipment_type", ItemLabel = "PLC设备", ItemValue = "PLC", SortOrder = 2 },
                new() { TypeCode = "equipment_type", ItemLabel = "检测设备", ItemValue = "检测设备", SortOrder = 3 },
                new() { TypeCode = "equipment_type", ItemLabel = "机器人", ItemValue = "机器人", SortOrder = 4 },
                new() { TypeCode = "equipment_type", ItemLabel = "其他", ItemValue = "其他", SortOrder = 99 },
                new() { TypeCode = "process_type", ItemLabel = "加工", ItemValue = "加工", SortOrder = 1 },
                new() { TypeCode = "process_type", ItemLabel = "检验", ItemValue = "检验", SortOrder = 2 },
                new() { TypeCode = "process_type", ItemLabel = "装配", ItemValue = "装配", SortOrder = 3 },
                new() { TypeCode = "process_type", ItemLabel = "包装", ItemValue = "包装", SortOrder = 4 },
                new() { TypeCode = "process_type", ItemLabel = "热处理", ItemValue = "热处理", SortOrder = 5 },
                new() { TypeCode = "process_type", ItemLabel = "表面处理", ItemValue = "表面处理", SortOrder = 6 },
                new() { TypeCode = "defect_category", ItemLabel = "外观", ItemValue = "外观", SortOrder = 1 },
                new() { TypeCode = "defect_category", ItemLabel = "尺寸", ItemValue = "尺寸", SortOrder = 2 },
                new() { TypeCode = "defect_category", ItemLabel = "功能", ItemValue = "功能", SortOrder = 3 },
                new() { TypeCode = "defect_category", ItemLabel = "材料", ItemValue = "材料", SortOrder = 4 },
                new() { TypeCode = "defect_category", ItemLabel = "性能", ItemValue = "性能", SortOrder = 5 },
                new() { TypeCode = "defect_category", ItemLabel = "其他", ItemValue = "其他", SortOrder = 99 },
                new() { TypeCode = "severity", ItemLabel = "CR - 严重", ItemValue = "CR", SortOrder = 1, Color = "#F56C6C" },
                new() { TypeCode = "severity", ItemLabel = "MA - 主要", ItemValue = "MA", SortOrder = 2, Color = "#E6A23C" },
                new() { TypeCode = "severity", ItemLabel = "MI - 次要", ItemValue = "MI", SortOrder = 3, Color = "#909399" },
                new() { TypeCode = "inspection_type", ItemLabel = "IQC来料检验", ItemValue = "IQC", SortOrder = 1 },
                new() { TypeCode = "inspection_type", ItemLabel = "IPQC过程检验", ItemValue = "IPQC", SortOrder = 2 },
                new() { TypeCode = "inspection_type", ItemLabel = "FQC成品检验", ItemValue = "FQC", SortOrder = 3 },
                new() { TypeCode = "inspection_type", ItemLabel = "OQC出货检验", ItemValue = "OQC", SortOrder = 4 },
                new() { TypeCode = "product_category", ItemLabel = "成品", ItemValue = "成品", SortOrder = 1 },
                new() { TypeCode = "product_category", ItemLabel = "半成品", ItemValue = "半成品", SortOrder = 2 },
                new() { TypeCode = "product_category", ItemLabel = "原材料", ItemValue = "原材料", SortOrder = 3 },
                new() { TypeCode = "product_category", ItemLabel = "辅料", ItemValue = "辅料", SortOrder = 4 },
                new() { TypeCode = "tool_type", ItemLabel = "车刀", ItemValue = "车刀", SortOrder = 1 },
                new() { TypeCode = "tool_type", ItemLabel = "铣刀", ItemValue = "铣刀", SortOrder = 2 },
                new() { TypeCode = "tool_type", ItemLabel = "钻头", ItemValue = "钻头", SortOrder = 3 },
                new() { TypeCode = "tool_type", ItemLabel = "磨具", ItemValue = "磨具", SortOrder = 4 },
                new() { TypeCode = "tool_type", ItemLabel = "丝锥", ItemValue = "丝锥", SortOrder = 5 },
                new() { TypeCode = "tool_type", ItemLabel = "其他", ItemValue = "其他", SortOrder = 99 },
                new() { TypeCode = "supply_category", ItemLabel = "原材料", ItemValue = "原材料", SortOrder = 1 },
                new() { TypeCode = "supply_category", ItemLabel = "零部件", ItemValue = "零部件", SortOrder = 2 },
                new() { TypeCode = "supply_category", ItemLabel = "包材", ItemValue = "包材", SortOrder = 3 },
                new() { TypeCode = "supply_category", ItemLabel = "设备", ItemValue = "设备", SortOrder = 4 },
                new() { TypeCode = "supply_category", ItemLabel = "服务", ItemValue = "服务", SortOrder = 5 },
                new() { TypeCode = "material_unit", ItemLabel = "个", ItemValue = "个", SortOrder = 1 },
                new() { TypeCode = "material_unit", ItemLabel = "件", ItemValue = "件", SortOrder = 2 },
                new() { TypeCode = "material_unit", ItemLabel = "套", ItemValue = "套", SortOrder = 3 },
                new() { TypeCode = "material_unit", ItemLabel = "kg", ItemValue = "kg", SortOrder = 4 },
                new() { TypeCode = "material_unit", ItemLabel = "g", ItemValue = "g", SortOrder = 5 },
                new() { TypeCode = "material_unit", ItemLabel = "m", ItemValue = "m", SortOrder = 6 },
                new() { TypeCode = "material_unit", ItemLabel = "L", ItemValue = "L", SortOrder = 7 },
                new() { TypeCode = "material_unit", ItemLabel = "pcs", ItemValue = "pcs", SortOrder = 8 },
            };
            context.SysDictItems.AddRange(dictItems);
            await context.SaveChangesAsync();
        }
        #endregion

        #region M02.5 动态参数配置种子数据
        var hasM02_5 = false;
        try { hasM02_5 = await context.Set<Models.M02_5.ParamGroup>().AnyAsync(); } catch { /* ignore */ }
        if (!hasM02_5)
        {
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
        } // end if (!hasM02_5)
        #endregion

        #region M02.1 检验项目主数据种子 (inspection_items)
        var hasInspectionItems = false;
        try { hasInspectionItems = await context.InspectionItems.AnyAsync(); } catch { /* ignore */ }
        if (!hasInspectionItems)
        {
            var now = DateTime.UtcNow;

            #region InspectionItems (12 个检验项目)
            var inspectionItems = new List<InspectionItem>
            {
                new() { ItemCode = "II-001", ItemName = "外径", Description = "轴类零件外径尺寸检验", DataType = "numeric", Unit = "mm", Usl = 50.05m, Lsl = 49.95m, TargetValue = 50.00m, ChartType = "Xbar_R", SubgroupSize = 5, InspectionMethod = "千分尺", SampleSize = 5, IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-002", ItemName = "内径", Description = "孔类零件内径尺寸检验", DataType = "numeric", Unit = "mm", Usl = 25.03m, Lsl = 24.97m, TargetValue = 25.00m, ChartType = "Xbar_R", SubgroupSize = 5, InspectionMethod = "内径千分尺", SampleSize = 5, IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-003", ItemName = "长度", Description = "零件长度尺寸检验", DataType = "numeric", Unit = "mm", Usl = 100.10m, Lsl = 99.90m, TargetValue = 100.00m, ChartType = "Xbar_R", SubgroupSize = 5, InspectionMethod = "游标卡尺", SampleSize = 5, IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-004", ItemName = "表面粗糙度 Ra", Description = "加工表面粗糙度检验", DataType = "numeric", Unit = "μm", Usl = 1.60m, Lsl = 0.00m, TargetValue = 0.80m, ChartType = "I_MR", SubgroupSize = 1, InspectionMethod = "粗糙度仪", SampleSize = 3, IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-005", ItemName = "硬度 HRC", Description = "热处理后硬度检验", DataType = "numeric", Unit = "HRC", Usl = 58.0m, Lsl = 52.0m, TargetValue = 55.0m, ChartType = "I_MR", SubgroupSize = 1, InspectionMethod = "硬度计", SampleSize = 2, IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-006", ItemName = "外观检查", Description = "产品外观质量检查（划伤、锈蚀、毛刺等）", DataType = "visual", Unit = "-", InspectionMethod = "目视检查", IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-007", ItemName = "螺纹精度", Description = "螺纹尺寸及精度检验", DataType = "attribute", Unit = "-", InspectionMethod = "螺纹规", IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-008", ItemName = "直线度", Description = "轴类零件直线度检验", DataType = "numeric", Unit = "mm", Usl = 0.05m, Lsl = 0.00m, TargetValue = 0.02m, ChartType = "I_MR", SubgroupSize = 1, InspectionMethod = "百分表", SampleSize = 1, IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-009", ItemName = "圆度", Description = "圆形截面圆度检验", DataType = "numeric", Unit = "mm", Usl = 0.03m, Lsl = 0.00m, TargetValue = 0.015m, ChartType = "I_MR", SubgroupSize = 1, InspectionMethod = "圆度仪", SampleSize = 1, IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-010", ItemName = "化学成分 C%", Description = "钢材碳含量检验", DataType = "numeric", Unit = "%", Usl = 0.45m, Lsl = 0.42m, TargetValue = 0.43m, ChartType = "I_MR", SubgroupSize = 1, InspectionMethod = "光谱分析仪", SampleSize = 1, IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-011", ItemName = "密封性测试", Description = "产品密封性能检验", DataType = "attribute", Unit = "-", InspectionMethod = "气密测试仪", IsActive = true, CreatedBy = 1 },
                new() { ItemCode = "II-012", ItemName = "包装完整性", Description = "出厂包装完整性检查", DataType = "visual", Unit = "-", InspectionMethod = "目视检查", IsActive = true, CreatedBy = 1 },
            };
            context.InspectionItems.AddRange(inspectionItems);
            await context.SaveChangesAsync();
            #endregion

            #region InspectionPlans (6 个检验计划)
            var plans = new List<InspectionPlan>
            {
                new() { PlanCode = "IP-IQC-001", PlanName = "精密转轴A100来料检验计划", InspectionType = "IQC", Description = "宝钢45#圆钢来料检验标准", ProductId = 1, SupplierId = 1, ProcessId = 1, IsActive = true, CreatedBy = 1 },
                new() { PlanCode = "IP-IQC-002", PlanName = "PCB主板C300来料检验计划", InspectionType = "IQC", Description = "PCB基板来料检验标准", ProductId = 3, SupplierId = 3, ProcessId = 1, IsActive = true, CreatedBy = 1 },
                new() { PlanCode = "IP-IPQC-001", PlanName = "精车工序首件检验计划", InspectionType = "IPQC", Description = "精密转轴精车工序首件检验项目", ProductId = 1, ProcessId = 3, EquipmentId = 1, IsActive = true, CreatedBy = 1 },
                new() { PlanCode = "IP-IPQC-002", PlanName = "热处理工序巡检计划", InspectionType = "IPQC", Description = "精密转轴热处理工序巡检项目", ProductId = 1, ProcessId = 5, EquipmentId = 4, IsActive = true, CreatedBy = 1 },
                new() { PlanCode = "IP-FQC-001", PlanName = "精密转轴A100成品检验计划", InspectionType = "FQC", Description = "精密转轴出厂成品检验标准", ProductId = 1, CustomerId = 1, ProcessId = 8, IsActive = true, CreatedBy = 1 },
                new() { PlanCode = "IP-FQC-002", PlanName = "壳体B200成品检验计划", InspectionType = "FQC", Description = "壳体成品出厂检验标准", ProductId = 2, CustomerId = 2, ProcessId = 8, IsActive = true, CreatedBy = 1 },
            };
            context.InspectionPlans.AddRange(plans);
            await context.SaveChangesAsync();
            #endregion

            #region InspectionPlanItems (18 个计划明细项)
            // plan[0]=IP-IQC-001: items 1(外径),2(内径),3(长度),10(化学成分)
            // plan[1]=IP-IQC-002: items 1(外径),3(长度),6(外观)
            // plan[2]=IP-IPQC-001: items 1(外径),3(长度),4(粗糙度),8(直线度)
            // plan[3]=IP-IPQC-002: items 5(硬度),9(圆度)
            // plan[4]=IP-FQC-001: items 1(外径),4(粗糙度),6(外观)
            // plan[5]=IP-FQC-002: items 3(长度),6(外观)

            var planItems = new List<InspectionPlanItem>
            {
                // IP-IQC-001 精密转轴A100来料检验
                new() { PlanId = plans[0].Id, InspectionItemId = inspectionItems[0].Id, SortOrder = 1, IsRequired = true },
                new() { PlanId = plans[0].Id, InspectionItemId = inspectionItems[1].Id, SortOrder = 2, IsRequired = true },
                new() { PlanId = plans[0].Id, InspectionItemId = inspectionItems[2].Id, SortOrder = 3, IsRequired = true },
                new() { PlanId = plans[0].Id, InspectionItemId = inspectionItems[9].Id, SortOrder = 4, IsRequired = true },
                // IP-IQC-002 PCB主板C300来料检验
                new() { PlanId = plans[1].Id, InspectionItemId = inspectionItems[0].Id, SortOrder = 1, IsRequired = true },
                new() { PlanId = plans[1].Id, InspectionItemId = inspectionItems[2].Id, SortOrder = 2, IsRequired = true },
                new() { PlanId = plans[1].Id, InspectionItemId = inspectionItems[5].Id, SortOrder = 3, IsRequired = true },
                // IP-IPQC-001 精车首件检验
                new() { PlanId = plans[2].Id, InspectionItemId = inspectionItems[0].Id, SortOrder = 1, IsRequired = true },
                new() { PlanId = plans[2].Id, InspectionItemId = inspectionItems[2].Id, SortOrder = 2, IsRequired = true },
                new() { PlanId = plans[2].Id, InspectionItemId = inspectionItems[3].Id, SortOrder = 3, IsRequired = true },
                new() { PlanId = plans[2].Id, InspectionItemId = inspectionItems[7].Id, SortOrder = 4, IsRequired = true },
                // IP-IPQC-002 热处理巡检
                new() { PlanId = plans[3].Id, InspectionItemId = inspectionItems[4].Id, SortOrder = 1, IsRequired = true },
                new() { PlanId = plans[3].Id, InspectionItemId = inspectionItems[8].Id, SortOrder = 2, IsRequired = true },
                // IP-FQC-001 精密转轴成品检验
                new() { PlanId = plans[4].Id, InspectionItemId = inspectionItems[0].Id, SortOrder = 1, IsRequired = true },
                new() { PlanId = plans[4].Id, InspectionItemId = inspectionItems[3].Id, SortOrder = 2, IsRequired = true },
                new() { PlanId = plans[4].Id, InspectionItemId = inspectionItems[5].Id, SortOrder = 3, IsRequired = true },
                // IP-FQC-002 壳体成品检验
                new() { PlanId = plans[5].Id, InspectionItemId = inspectionItems[2].Id, SortOrder = 1, IsRequired = true },
                new() { PlanId = plans[5].Id, InspectionItemId = inspectionItems[5].Id, SortOrder = 2, IsRequired = true },
            };
            context.InspectionPlanItems.AddRange(planItems);
            await context.SaveChangesAsync();
            #endregion

            #region S3: IQC 来料检验 DEMO 数据
            // 来料登记
            var receipts = new List<IqcReceipt>
            {
                new() { ReceiptNo = "REC-20260601-001", SupplierId = 1, ProductId = 1, BatchNo = "BATCH-S1-001", Quantity = 500, Unit = "pcs", ReceiptDate = new DateTime(2026, 6, 1), Inspector = "张明", Status = "completed" },
                new() { ReceiptNo = "REC-20260601-002", SupplierId = 3, ProductId = 3, BatchNo = "BATCH-S1-002", Quantity = 1000, Unit = "pcs", ReceiptDate = new DateTime(2026, 6, 1), Inspector = "张明", Status = "completed" },
            };
            context.IqcReceipts.AddRange(receipts);
            await context.SaveChangesAsync();

            // 检验单
            var iqcInspections = new List<IqcInspection>
            {
                new() { InspectionNo = "IQC-20260601-001", ReceiptId = receipts[0].Id, SampleSize = 50, Ac = 7, Re = 8, DefectQty = 0, SamplingLevel = "II", AqlValue = 1.0, Result = "pass", Inspector = "张明", InspectedAt = new DateTime(2026, 6, 1, 10, 30, 0) },
                new() { InspectionNo = "IQC-20260601-002", ReceiptId = receipts[1].Id, SampleSize = 80, Ac = 10, Re = 11, DefectQty = 1, SamplingLevel = "S-3", AqlValue = 0.25, Result = "pass", Inspector = "张明", InspectedAt = new DateTime(2026, 6, 1, 14, 0, 0) },
            };
            context.IqcInspections.AddRange(iqcInspections);
            await context.SaveChangesAsync();

            // 检验明细项（关联检验项目主数据）
            var iqcItems = new List<IqcInspectionItem>
            {
                // IQC-001（精密转轴A100来料）
                new() { InspectionId = iqcInspections[0].Id, InspectionItemId = inspectionItems[0].Id, ItemName = "外径", MeasuredValue = 50.02m, Usl = 50.05m, Lsl = 49.95m, Result = "pass" },
                new() { InspectionId = iqcInspections[0].Id, InspectionItemId = inspectionItems[1].Id, ItemName = "内径", MeasuredValue = 25.01m, Usl = 25.03m, Lsl = 24.97m, Result = "pass" },
                new() { InspectionId = iqcInspections[0].Id, InspectionItemId = inspectionItems[2].Id, ItemName = "长度", MeasuredValue = 100.03m, Usl = 100.10m, Lsl = 99.90m, Result = "pass" },
                new() { InspectionId = iqcInspections[0].Id, InspectionItemId = inspectionItems[9].Id, ItemName = "化学成分 C%", MeasuredValue = 0.43m, Usl = 0.45m, Lsl = 0.42m, Result = "pass" },
                // IQC-002（PCB主板C300来料）
                new() { InspectionId = iqcInspections[1].Id, InspectionItemId = inspectionItems[0].Id, ItemName = "外径", MeasuredValue = 50.01m, Usl = 50.05m, Lsl = 49.95m, Result = "pass" },
                new() { InspectionId = iqcInspections[1].Id, InspectionItemId = inspectionItems[2].Id, ItemName = "长度", MeasuredValue = 100.02m, Usl = 100.10m, Lsl = 99.90m, Result = "pass" },
                new() { InspectionId = iqcInspections[1].Id, InspectionItemId = inspectionItems[5].Id, ItemName = "外观检查", Result = "pass" },
            };
            context.IqcInspectionItems.AddRange(iqcItems);
            await context.SaveChangesAsync();
            #endregion

            #region S4: IPQC 过程检验 DEMO 数据
            // 首件检验
            var firstPieces = new List<IpqcFirstPiece>
            {
                new() { FpNo = "FP-20260601-001", WorkOrderId = 10001, ProcessId = 3, EquipmentId = 1, OperatorId = 2, Shift = "早班", Reason = "班次切换", Conclusion = "qualified", AllowedToProduce = true, InspectorId = 3, CheckedAt = new DateTime(2026, 6, 1, 8, 15, 0) },
            };
            context.IpqcFirstPieces.AddRange(firstPieces);
            await context.SaveChangesAsync();

            // 首件检验明细
            var fpItems = new List<IpqcFirstPieceItem>
            {
                new() { FirstPieceId = firstPieces[0].Id, InspectionItemId = inspectionItems[0].Id, ItemName = "外径", ItemCode = "II-001", Usl = 50.05m, Lsl = 49.95m, DataType = "numeric", ActualValue = 49.98m, Result = "pass" },
                new() { FirstPieceId = firstPieces[0].Id, InspectionItemId = inspectionItems[2].Id, ItemName = "长度", ItemCode = "II-003", Usl = 100.10m, Lsl = 99.90m, DataType = "numeric", ActualValue = 100.02m, Result = "pass" },
                new() { FirstPieceId = firstPieces[0].Id, InspectionItemId = inspectionItems[3].Id, ItemName = "表面粗糙度 Ra", ItemCode = "II-004", Usl = 1.60m, Lsl = 0.00m, DataType = "numeric", ActualValue = 0.75m, Result = "pass" },
                new() { FirstPieceId = firstPieces[0].Id, InspectionItemId = inspectionItems[7].Id, ItemName = "直线度", ItemCode = "II-008", Usl = 0.05m, Lsl = 0.00m, DataType = "numeric", ActualValue = 0.02m, Result = "pass" },
            };
            context.IpqcFirstPieceItems.AddRange(fpItems);
            await context.SaveChangesAsync();

            // 巡检计划
            var patrolPlans = new List<IpqcPatrolPlan>
            {
                new() { PlanNo = "PP-001", ProcessId = 5, EquipmentId = 4, PatrolIntervalMin = 60, AutoGenerate = true, Status = "active", Inspector = "李强" },
            };
            context.IpqcPatrolPlans.AddRange(patrolPlans);
            await context.SaveChangesAsync();

            // 巡检记录
            var patrols = new List<IpqcPatrol>
            {
                new() { PatrolNo = "PT-20260601-001", PatrolPlanId = patrolPlans[0].Id, WorkOrderId = 10001, ProcessId = 5, EquipmentId = 4, InspectorId = 3, ScheduledTime = new DateTime(2026, 6, 1, 9, 0, 0), ActualTime = new DateTime(2026, 6, 1, 9, 5, 0), TotalChecked = 2, TotalPass = 2, TotalFail = 0, Conclusion = "qualified", Status = "completed" },
            };
            context.IpqcPatrols.AddRange(patrols);
            await context.SaveChangesAsync();

            // 巡检明细
            var patrolItems = new List<IpqcPatrolItem>
            {
                new() { PatrolId = patrols[0].Id, InspectionItemId = inspectionItems[4].Id, ItemName = "硬度 HRC", ItemCode = "II-005", Usl = 58.0m, Lsl = 52.0m, DataType = "numeric", ActualValue = 55.5m, Result = "pass" },
                new() { PatrolId = patrols[0].Id, InspectionItemId = inspectionItems[8].Id, ItemName = "圆度", ItemCode = "II-009", Usl = 0.03m, Lsl = 0.00m, DataType = "numeric", ActualValue = 0.012m, Result = "pass" },
            };
            context.IpqcPatrolItems.AddRange(patrolItems);
            await context.SaveChangesAsync();
            #endregion

            #region S5: FQC/OQC 成品检验 DEMO 数据
            // 成品批次
            var batches = new List<ProductBatch>
            {
                new() { BatchCode = "LOT-20260601-001", Source = "ipqc-auto", ProductId = 1, WorkOrderId = 10001, Quantity = 200, Status = "inspected" },
                new() { BatchCode = "LOT-20260601-002", Source = "manual", ProductId = 2, Quantity = 100, Status = "inspected" },
            };
            context.ProductBatches.AddRange(batches);
            await context.SaveChangesAsync();

            // 成品检验单
            var fqcInspections = new List<FqcInspection>
            {
                new() { InspectionNo = "FQC-20260601-001", BatchId = batches[0].Id, WorkOrderId = 10001, InspectionType = "sampling", AqlLevel = 1.0m, SampleSize = 20, TotalChecked = 20, TotalPass = 20, TotalFail = 0, Ac = 3, Re = 4, Conclusion = "qualified", InspectorId = 3, CheckedAt = new DateTime(2026, 6, 2, 9, 0, 0) },
                new() { InspectionNo = "FQC-20260601-002", BatchId = batches[1].Id, InspectionType = "sampling", AqlLevel = 0.65m, SampleSize = 15, TotalChecked = 15, TotalPass = 15, TotalFail = 0, Ac = 2, Re = 3, Conclusion = "qualified", InspectorId = 3, CheckedAt = new DateTime(2026, 6, 2, 10, 0, 0) },
            };
            context.FqcInspections.AddRange(fqcInspections);
            await context.SaveChangesAsync();

            // 成品检验明细项
            var fqcItems = new List<FqcInspectionItem>
            {
                // FQC-001（精密转轴A100）
                new() { InspectionId = fqcInspections[0].Id, InspectionItemId = inspectionItems[0].Id, ItemName = "外径", ItemCode = "II-001", Usl = 50.05m, Lsl = 49.95m, DataType = "numeric", ActualValue = 50.01m, Result = "pass" },
                new() { InspectionId = fqcInspections[0].Id, InspectionItemId = inspectionItems[3].Id, ItemName = "表面粗糙度 Ra", ItemCode = "II-004", Usl = 1.60m, Lsl = 0.00m, DataType = "numeric", ActualValue = 0.82m, Result = "pass" },
                new() { InspectionId = fqcInspections[0].Id, InspectionItemId = inspectionItems[5].Id, ItemName = "外观检查", ItemCode = "II-006", DataType = "visual", Result = "pass" },
                // FQC-002（壳体B200）
                new() { InspectionId = fqcInspections[1].Id, InspectionItemId = inspectionItems[2].Id, ItemName = "长度", ItemCode = "II-003", Usl = 100.10m, Lsl = 99.90m, DataType = "numeric", ActualValue = 100.05m, Result = "pass" },
                new() { InspectionId = fqcInspections[1].Id, InspectionItemId = inspectionItems[5].Id, ItemName = "外观检查", ItemCode = "II-006", DataType = "visual", Result = "pass" },
            };
            context.FqcInspectionItems.AddRange(fqcItems);
            await context.SaveChangesAsync();
            #endregion

            #region S6: SPC 统计分析 DEMO 数据
            // 控制图1: 精密转轴外径 Xbar-R 控制图
            var chart1 = new SpcControlChart
            {
                Name = "精密转轴外径 Xbar-R 控制图",
                ProcessId = 3,
                ParameterCode = "od_tolerance",
                ChartType = "Xbar_R",
                SubgroupSize = 5,
                Usl = 50.05m,
                Lsl = 49.95m,
                TargetValue = 50.00m,
                Cl = 50.00m,
                Ucl = 50.035m,
                Lcl = 49.965m,
                CreatedBy = 1
            };
            // 控制图2: 精密转轴粗糙度 I-MR 控制图
            var chart2 = new SpcControlChart
            {
                Name = "精密转轴粗糙度 I-MR 控制图",
                ProcessId = 8,
                ParameterCode = "surface_roughness",
                ChartType = "I_MR",
                SubgroupSize = 1,
                Usl = 1.60m,
                Lsl = 0.00m,
                TargetValue = 0.80m,
                Cl = 0.80m,
                Ucl = 1.42m,
                Lcl = 0.18m,
                CreatedBy = 1
            };
            context.SpcControlCharts.AddRange(chart1, chart2);
            await context.SaveChangesAsync();

            // 数据点(外径 Xbar-R: 10 subgroups)
            var chart1Data = new (decimal[] values, decimal mean, decimal range)[]
            {
                (new[] { 49.98m, 50.01m, 50.02m, 50.00m, 49.99m }, 50.00m, 0.04m),
                (new[] { 50.02m, 50.00m, 49.97m, 50.01m, 50.00m }, 50.00m, 0.05m),
                (new[] { 49.99m, 50.03m, 50.01m, 49.98m, 50.00m }, 50.002m, 0.05m),
                (new[] { 50.01m, 49.99m, 50.00m, 50.02m, 49.98m }, 50.00m, 0.04m),
                (new[] { 49.97m, 50.00m, 50.01m, 50.02m, 49.99m }, 49.998m, 0.05m),
                (new[] { 50.00m, 49.98m, 50.01m, 50.00m, 50.02m }, 50.002m, 0.04m),
                (new[] { 49.99m, 50.02m, 49.98m, 50.00m, 50.01m }, 50.00m, 0.04m),
                (new[] { 50.03m, 50.01m, 49.99m, 50.00m, 49.97m }, 50.00m, 0.06m),
                (new[] { 50.00m, 49.99m, 50.02m, 49.98m, 50.01m }, 50.00m, 0.04m),
                (new[] { 49.98m, 50.00m, 50.01m, 49.99m, 50.02m }, 50.00m, 0.04m),
            };
            var chart1Points = chart1Data.Select((d, i) => new SpcDataPoint
            {
                ChartId = chart1.Id,
                SubgroupIndex = i + 1,
                IndividualValues = System.Text.Json.JsonSerializer.Serialize(d.values),
                SubgroupMean = d.mean,
                SubgroupRange = d.range,
                MeasuredAt = new DateTime(2026, 6, 1, 8, 0, 0).AddMinutes(i * 30)
            }).ToList();

            // 数据点(粗糙度 I-MR: 15 points)
            var roughnessValues = new[] { 0.75m, 0.82m, 0.78m, 0.85m, 0.72m, 0.79m, 0.88m, 0.76m, 0.81m, 0.74m, 0.83m, 0.77m, 0.80m, 0.86m, 0.73m };
            var chart2Points = roughnessValues.Select((v, i) => new SpcDataPoint
            {
                ChartId = chart2.Id,
                SubgroupIndex = i + 1,
                IndividualValues = $"[{v}]",
                SubgroupMean = v,
                MeasuredAt = new DateTime(2026, 6, 1, 8, 0, 0).AddMinutes(i * 20)
            }).ToList();

            context.SpcDataPoints.AddRange(chart1Points);
            context.SpcDataPoints.AddRange(chart2Points);
            await context.SaveChangesAsync();

            // 数据源配置（贯通S3/S4/S5 → S6）
            var dataSources = new List<SpcDataSource>
            {
                new() { ChartId = chart1.Id, SourceType = "IPQC", InspectionItemId = inspectionItems[0].Id, ProductId = 1, ProcessId = 3 },
                new() { ChartId = chart2.Id, SourceType = "FQC", InspectionItemId = inspectionItems[3].Id, ProductId = 1, ProcessId = 8 },
            };
            context.SpcDataSources.AddRange(dataSources);
            await context.SaveChangesAsync();

            // 分析结果
            var analysisResults = new List<SpcAnalysisResult>
            {
                new() { ChartId = chart1.Id, AnalysisType = "cpk", Cp = 1.33m, Cpk = 1.28m, Pp = 1.30m, Ppk = 1.25m, SigmaWithin = 0.0125m, EstimatedPpm = 180m, DataPointsUsed = 50, AnalysisPeriodStart = new DateTime(2026, 6, 1, 8, 0, 0), AnalysisPeriodEnd = new DateTime(2026, 6, 1, 12, 30, 0) },
                new() { ChartId = chart2.Id, AnalysisType = "cpk", Cp = 1.45m, Cpk = 1.40m, Pp = 1.42m, Ppk = 1.38m, SigmaWithin = 0.18m, EstimatedPpm = 85m, DataPointsUsed = 15, AnalysisPeriodStart = new DateTime(2026, 6, 1, 8, 0, 0), AnalysisPeriodEnd = new DateTime(2026, 6, 1, 12, 40, 0) },
            };
            context.SpcAnalysisResults.AddRange(analysisResults);
            await context.SaveChangesAsync();
            #endregion
        }
        #endregion

        #region SPC 判异规则 — EF Core 方式：为所有缺少规则的控制图补齐8大判异规则
        try
        {
            // 获取数据库中缺少全部8条规则的控制图（有规则则跳过）
            var chartsWithoutRules = await context.SpcControlCharts
                .Where(c => !context.SpcAlertRules.Any(r => r.ChartId == c.Id))
                .ToListAsync();

            if (chartsWithoutRules.Count > 0)
            {
                var now = DateTime.UtcNow;
                var rules = new List<SpcAlertRule>();
                foreach (var chart in chartsWithoutRules)
                {
                    rules.AddRange(GetDefaultAlertRules(chart.Id, now));
                }
                context.SpcAlertRules.AddRange(rules);
                await context.SaveChangesAsync();

                Console.WriteLine($"[DbInitializer] 已为 {chartsWithoutRules.Count} 个控制图添加 SPC 判异规则");
            }

            // 清理因之前 SQL INSERT 未指定 CreatedAt/UpdatedAt 导致的零日期脏数据
            var zeroDateRules = await context.SpcAlertRules
                .Where(r => r.CreatedAt == default || r.UpdatedAt == default)
                .ToListAsync();
            if (zeroDateRules.Count > 0)
            {
                context.SpcAlertRules.RemoveRange(zeroDateRules);
                await context.SaveChangesAsync();
                Console.WriteLine($"[DbInitializer] 已清理 {zeroDateRules.Count} 条零日期 SPC 判异规则");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] SPC 判异规则初始化失败（不阻断启动）: {ex.Message}");
        }
        #endregion
    }

private static List<SpcAlertRule> GetDefaultAlertRules(long chartId, DateTime now)
    {
        return new List<SpcAlertRule>
        {
            new() { ChartId = chartId, RuleNumber = 1, RuleName = "1 点超出 3σ 控制限", RuleDescription = "任何数据点超出 UCL 或 LCL", Enabled = true, TriggerThreshold = 1, SigmaThreshold = 3.0m, CreatedAt = now, UpdatedAt = now },
            new() { ChartId = chartId, RuleNumber = 2, RuleName = "连续 9 点在 CL 同侧", RuleDescription = "连续 9 个点位于中心线同一侧", Enabled = true, TriggerThreshold = 9, SigmaThreshold = 0m, CreatedAt = now, UpdatedAt = now },
            new() { ChartId = chartId, RuleNumber = 3, RuleName = "连续 6 点递增或递减", RuleDescription = "连续 6 个点单调上升或下降", Enabled = true, TriggerThreshold = 6, SigmaThreshold = 0m, CreatedAt = now, UpdatedAt = now },
            new() { ChartId = chartId, RuleNumber = 4, RuleName = "连续 14 点上下交替", RuleDescription = "连续 14 个点呈现上下交替模式", Enabled = true, TriggerThreshold = 14, SigmaThreshold = 0m, CreatedAt = now, UpdatedAt = now },
            new() { ChartId = chartId, RuleNumber = 5, RuleName = "连续 3 点中 2 点超出 2σ", RuleDescription = "连续 3 点中有 2 点落在 2σ和 3σ之间（同一侧）", Enabled = true, TriggerThreshold = 2, SigmaThreshold = 2.0m, CreatedAt = now, UpdatedAt = now },
            new() { ChartId = chartId, RuleNumber = 6, RuleName = "连续 5 点中 4 点超出 1σ", RuleDescription = "连续 5 点中有 4 点落在 1σ和 2σ之间（同一侧）", Enabled = true, TriggerThreshold = 4, SigmaThreshold = 1.0m, CreatedAt = now, UpdatedAt = now },
            new() { ChartId = chartId, RuleNumber = 7, RuleName = "连续 15 点在 1σ内", RuleDescription = "连续 15 个点落在中心线 1σ范围内（任一侧）", Enabled = true, TriggerThreshold = 15, SigmaThreshold = 1.0m, CreatedAt = now, UpdatedAt = now },
            new() { ChartId = chartId, RuleNumber = 8, RuleName = "连续 8 点超出 1σ", RuleDescription = "连续 8 个点落在 1σ范围外（双侧）", Enabled = true, TriggerThreshold = 8, SigmaThreshold = 1.0m, CreatedAt = now, UpdatedAt = now },
        };
    }

    }
