using Microsoft.EntityFrameworkCore;
using QM_AI.API.Models;
using QM_AI.API.Services;
using QM_AI.API.Models.M02_Inspection;
using QM_AI.API.Models.M03;
using QM_AI.API.Models.M04;
using QM_AI.API.Models.M05;
using QM_AI.API.Models.M06;
using QM_AI.API.Models.M09;
using QM_AI.API.Models.M11;

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

        #region InspectionStandards (35 条检验标准 DEMO 数据)
        // 覆盖全部 5 个产品 × 4 种检验类型（IQC/IPQC/FQC/OQC）
        var standards = new List<InspectionStandard>
        {
            // ═══════════════════════════════════════════════════════════
            // P001 精密转轴 A100（机加工件）— 10 条标准
            // ═══════════════════════════════════════════════════════════
            // --- IQC 来料检验 ---
            new() { Code = "STD-001", Name = "外径直径检验", Description = "精密转轴精车后外径尺寸检验，公差 ±0.05mm", InspectionType = "IPQC", ProductId = products[0].Id, ProcessId = processes[2].Id, ItemName = "外径", Usl = 50.05, Lsl = 49.95, Target = 50.0, Unit = "mm", InspectionMethod = "千分尺", SamplingFrequency = "每批次5件" },
            new() { Code = "STD-004", Name = "长度尺寸检验", Description = "精密转轴精车后长度尺寸检验，公差 ±0.1mm", InspectionType = "IPQC", ProductId = products[0].Id, ProcessId = processes[2].Id, ItemName = "长度", Usl = 100.10, Lsl = 99.90, Target = 100.0, Unit = "mm", InspectionMethod = "游标卡尺", SamplingFrequency = "每批次5件" },
            new() { Code = "STD-005", Name = "轴承座直径检验", Description = "精密转轴轴承安装位外径检验，配合公差 h6", InspectionType = "IPQC", ProductId = products[0].Id, ProcessId = processes[5].Id, ItemName = "轴承座外径", Usl = 24.98, Lsl = 24.95, Target = 24.96, Unit = "mm", InspectionMethod = "千分尺", SamplingFrequency = "每批次3件" },
            new() { Code = "STD-006", Name = "螺纹精度检验", Description = "转轴末端外螺纹 M10×1.25 精度检验", InspectionType = "IPQC", ProductId = products[0].Id, ProcessId = processes[3].Id, ItemName = "螺纹 M10×1.25", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "螺纹规（通止规）", SamplingFrequency = "每班次2件" },
            new() { Code = "STD-010", Name = "直线度检验", Description = "精密转轴全轴直线度检验，允许偏差 ≤0.05mm", InspectionType = "FQC", ProductId = products[0].Id, ProcessId = processes[7].Id, ItemName = "直线度", Usl = 0.05, Lsl = 0, Target = 0.02, Unit = "mm", InspectionMethod = "百分表 + V型铁", SamplingFrequency = "每批次3件" },
            new() { Code = "STD-011", Name = "圆度检验", Description = "精密转轴圆形截面圆度检验，允许偏差 ≤0.03mm", InspectionType = "FQC", ProductId = products[0].Id, ProcessId = processes[7].Id, ItemName = "圆度", Usl = 0.03, Lsl = 0, Target = 0.015, Unit = "mm", InspectionMethod = "圆度仪", SamplingFrequency = "每批次2件" },
            new() { Code = "STD-012", Name = "粗糙度 Ra 检验", Description = "精密转轴加工面表面粗糙度 Ra 值检验", InspectionType = "FQC", ProductId = products[0].Id, ProcessId = processes[7].Id, ItemName = "表面粗糙度 Ra", Usl = 1.6, Lsl = 0, Target = 0.8, Unit = "μm", InspectionMethod = "粗糙度仪", SamplingFrequency = "每批次3件" },
            new() { Code = "STD-013", Name = "外观综合检验", Description = "精密转轴出厂前外观检查：划伤、锈蚀、毛刺、磕碰", InspectionType = "FQC", ProductId = products[0].Id, ProcessId = processes[7].Id, ItemName = "外观", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查 + 标准样板", SamplingFrequency = "全检" },
            // --- OQC 出货检验 ---
            new() { Code = "STD-025", Name = "包装完整性检验", Description = "精密转轴出厂包装完整性及标识核对", InspectionType = "OQC", ProductId = products[0].Id, ProcessId = processes[9].Id, ItemName = "包装完整性", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查", SamplingFrequency = "全检" },
            new() { Code = "STD-026", Name = "标签标识核对", Description = "产品标签信息（批次号、品名、数量）与送货单一致性核对", InspectionType = "OQC", ProductId = products[0].Id, ProcessId = processes[9].Id, ItemName = "标签标识", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视核对", SamplingFrequency = "全检" },

            // ═══════════════════════════════════════════════════════════
            // P002 壳体 B200（压铸件）— 9 条标准
            // ═══════════════════════════════════════════════════════════
            // --- IQC 来料检验 ---
            new() { Code = "STD-007", Name = "外观质量检验", Description = "压铸件表面无砂眼、气孔、裂纹、冷隔等缺陷", InspectionType = "IQC", ProductId = products[1].Id, ProcessId = processes[0].Id, ItemName = "外观", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查 + 标准缺陷样板", SamplingFrequency = "每批次抽检10%" },
            new() { Code = "STD-008", Name = "外形尺寸检验", Description = "壳体关键外形尺寸：长×宽×高，公差 ±0.2mm", InspectionType = "IQC", ProductId = products[1].Id, ProcessId = processes[0].Id, ItemName = "外形尺寸 长×宽×高", Usl = 200.20, Lsl = 199.80, Target = 200.0, Unit = "mm", InspectionMethod = "游标卡尺", SamplingFrequency = "每批次5件" },
            new() { Code = "STD-009", Name = "重量检验", Description = "壳体单件重量检验，标准重量 2.5kg ±5%", InspectionType = "IQC", ProductId = products[1].Id, ProcessId = processes[0].Id, ItemName = "重量", Usl = 2.625, Lsl = 2.375, Target = 2.5, Unit = "kg", InspectionMethod = "电子秤", SamplingFrequency = "每批次3件" },
            // --- IPQC 过程检验 ---
            new() { Code = "STD-014", Name = "机加工孔径检验", Description = "壳体螺栓孔 Φ8H7 孔径检验", InspectionType = "IPQC", ProductId = products[1].Id, ProcessId = processes[3].Id, ItemName = "孔径 Φ8H7", Usl = 8.015, Lsl = 8.000, Target = 8.008, Unit = "mm", InspectionMethod = "内径千分尺", SamplingFrequency = "每班次3件" },
            new() { Code = "STD-015", Name = "壁厚检验", Description = "壳体关键部位壁厚检验，标准 3.0mm ±0.1mm", InspectionType = "IPQC", ProductId = products[1].Id, ProcessId = processes[7].Id, ItemName = "壁厚", Usl = 3.10, Lsl = 2.90, Target = 3.0, Unit = "mm", InspectionMethod = "超声波测厚仪", SamplingFrequency = "每批次3件" },
            new() { Code = "STD-016", Name = "螺纹孔检验", Description = "壳体 M6 螺纹孔通止规检验", InspectionType = "IPQC", ProductId = products[1].Id, ProcessId = processes[3].Id, ItemName = "螺纹 M6", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "螺纹规（通止规）", SamplingFrequency = "每班次2件" },
            // --- FQC 成品检验 ---
            new() { Code = "STD-017", Name = "表面处理质量检验", Description = "壳体阳极氧化/喷粉涂层质量检验：膜厚、附着力、颜色", InspectionType = "FQC", ProductId = products[1].Id, ProcessId = processes[7].Id, ItemName = "表面处理", Usl = 15.0, Lsl = 8.0, Target = 12.0, Unit = "μm", InspectionMethod = "涂层测厚仪 + 百格法", SamplingFrequency = "每批次3件" },
            new() { Code = "STD-018", Name = "壳体外观终检", Description = "壳体成品外观终检：飞边、变形、色泽一致性", InspectionType = "FQC", ProductId = products[1].Id, ProcessId = processes[7].Id, ItemName = "外观", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查 + 标准光源箱", SamplingFrequency = "全检" },
            // --- OQC 出货检验 ---
            new() { Code = "STD-027", Name = "壳体包装防护检验", Description = "壳体出厂包装防护：泡沫内衬到位、防撞措施", InspectionType = "OQC", ProductId = products[1].Id, ProcessId = processes[9].Id, ItemName = "包装防护", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查", SamplingFrequency = "全检" },

            // ═══════════════════════════════════════════════════════════
            // P003 PCB 主板 C300（电子件）— 8 条标准
            // ═══════════════════════════════════════════════════════════
            // --- IQC 来料检验 ---
            new() { Code = "STD-019", Name = "PCB 外观检验", Description = "PCB 基板外观检查：铜箔氧化、划痕、分层、字符清晰度", InspectionType = "IQC", ProductId = products[2].Id, ProcessId = processes[0].Id, ItemName = "外观", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查 + 10倍放大镜", SamplingFrequency = "每批次抽检20%" },
            new() { Code = "STD-020", Name = "板厚检验", Description = "PCB 基板厚度检验，标准 1.6mm ±0.15mm", InspectionType = "IQC", ProductId = products[2].Id, ProcessId = processes[0].Id, ItemName = "板厚", Usl = 1.75, Lsl = 1.45, Target = 1.6, Unit = "mm", InspectionMethod = "千分尺", SamplingFrequency = "每批次3片" },
            new() { Code = "STD-021", Name = "阻值检验", Description = "贴片电阻 10KΩ 阻值检验，偏差 ±1%", InspectionType = "IQC", ProductId = products[2].Id, ProcessId = processes[0].Id, ItemName = "电阻值 10KΩ", Usl = 10.10, Lsl = 9.90, Target = 10.0, Unit = "KΩ", InspectionMethod = "LCR 电桥", SamplingFrequency = "每批次5pcs" },
            // --- IPQC 过程检验 ---
            new() { Code = "STD-022", Name = "焊接质量检验", Description = "SMT 贴片焊接质量：虚焊、连锡、偏移、立碑", InspectionType = "IPQC", ProductId = products[2].Id, ProcessId = processes[6].Id, ItemName = "焊接质量", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "AOI 自动光学检测 + 目视复检", SamplingFrequency = "每班次抽检10%" },
            // --- FQC 成品检验 ---
            new() { Code = "STD-023", Name = "电气功能测试", Description = "PCB 主板通电功能测试：供电、通信、IO 输出", InspectionType = "FQC", ProductId = products[2].Id, ProcessId = processes[7].Id, ItemName = "电气功能", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "专用测试治具 + 固件烧录", SamplingFrequency = "全检" },
            new() { Code = "STD-024", Name = "绝缘电阻测试", Description = "PCB 主板绝缘电阻测试，标准 ≥100MΩ @500VDC", InspectionType = "FQC", ProductId = products[2].Id, ProcessId = processes[7].Id, ItemName = "绝缘电阻", Usl = null, Lsl = 100, Target = null, Unit = "MΩ", InspectionMethod = "绝缘电阻测试仪", SamplingFrequency = "全检" },
            // --- OQC 出货检验 ---
            new() { Code = "STD-028", Name = "PCB 防静电包装检验", Description = "PCB 主板防静电包装检查：防静电袋密封、干燥剂、标签", InspectionType = "OQC", ProductId = products[2].Id, ProcessId = processes[9].Id, ItemName = "防静电包装", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查", SamplingFrequency = "全检" },

            // ═══════════════════════════════════════════════════════════
            // P004 密封圈 D400（橡胶件）— 5 条标准
            // ═══════════════════════════════════════════════════════════
            // --- IQC 来料检验 ---
            new() { Code = "STD-029", Name = "密封圈外观检验", Description = "橡胶密封圈外观：气泡、杂质、飞边、变形", InspectionType = "IQC", ProductId = products[3].Id, ProcessId = processes[0].Id, ItemName = "外观", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查", SamplingFrequency = "每批次抽检10%" },
            new() { Code = "STD-030", Name = "外径与线径检验", Description = "密封圈外径和截面直径检验，配合公差", InspectionType = "IQC", ProductId = products[3].Id, ProcessId = processes[0].Id, ItemName = "外径/线径", Usl = 25.15, Lsl = 24.85, Target = 25.0, Unit = "mm", InspectionMethod = "游标卡尺", SamplingFrequency = "每批次5件" },
            // --- FQC 成品检验 ---
            new() { Code = "STD-031", Name = "硬度检验", Description = "橡胶密封圈邵氏硬度检验，标准 70A ±5", InspectionType = "FQC", ProductId = products[3].Id, ProcessId = processes[7].Id, ItemName = "硬度 Shore A", Usl = 75, Lsl = 65, Target = 70, Unit = "Shore A", InspectionMethod = "邵氏硬度计", SamplingFrequency = "每批次3件" },
            new() { Code = "STD-032", Name = "密封性测试", Description = "密封圈气密性测试：加压 0.3MPa 保持 30s 无泄漏", InspectionType = "FQC", ProductId = products[3].Id, ProcessId = processes[7].Id, ItemName = "密封性", Usl = null, Lsl = 0.3, Target = 0.3, Unit = "MPa", InspectionMethod = "气密测试仪", SamplingFrequency = "全检" },
            // --- OQC 出货检验 ---
            new() { Code = "STD-033", Name = "包装与标识检验", Description = "密封圈 PE 自封袋包装、干燥剂、标签信息核对", InspectionType = "OQC", ProductId = products[3].Id, ProcessId = processes[9].Id, ItemName = "包装标识", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查", SamplingFrequency = "全检" },

            // ═══════════════════════════════════════════════════════════
            // P005 连接线束 E500（标准件）— 6 条标准
            // ═══════════════════════════════════════════════════════════
            // --- IQC 来料检验 ---
            new() { Code = "STD-034", Name = "线材外观检验", Description = "PVC 护套线外观：绝缘层破损、色差、直径偏差", InspectionType = "IQC", ProductId = products[4].Id, ProcessId = processes[0].Id, ItemName = "线材外观", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查 + 卡尺", SamplingFrequency = "每批次抽检10%" },
            new() { Code = "STD-035", Name = "端子压接质量检验", Description = "端子 HT-05B 压接质量：拉拔力、外观、位移量", InspectionType = "IQC", ProductId = products[4].Id, ProcessId = processes[0].Id, ItemName = "端子压接力", Usl = null, Lsl = 80, Target = 120, Unit = "N", InspectionMethod = "拉拔力测试仪", SamplingFrequency = "每批次5pcs" },
            // --- FQC 成品检验 ---
            new() { Code = "STD-036", Name = "导通测试", Description = "线束各线路导通性测试，标准电阻 ≤0.1Ω", InspectionType = "FQC", ProductId = products[4].Id, ProcessId = processes[7].Id, ItemName = "导通电阻", Usl = 0.1, Lsl = 0, Target = 0.05, Unit = "Ω", InspectionMethod = "万用表/导通测试仪", SamplingFrequency = "全检" },
            new() { Code = "STD-037", Name = "绝缘电阻测试", Description = "线束线间及线对地绝缘电阻测试，标准 ≥100MΩ", InspectionType = "FQC", ProductId = products[4].Id, ProcessId = processes[7].Id, ItemName = "绝缘电阻", Usl = null, Lsl = 100, Target = null, Unit = "MΩ", InspectionMethod = "兆欧表 500VDC", SamplingFrequency = "全检" },
            new() { Code = "STD-038", Name = "外观与尺寸检验", Description = "线束成品外观：热缩管收缩到位、扎带固定、总长度", InspectionType = "FQC", ProductId = products[4].Id, ProcessId = processes[7].Id, ItemName = "外观/总长度", Usl = 1505, Lsl = 1495, Target = 1500, Unit = "mm", InspectionMethod = "卷尺 + 目视", SamplingFrequency = "全检" },
            // --- OQC 出货检验 ---
            new() { Code = "STD-039", Name = "包装完整性检验", Description = "线束 PE 包装袋密封性、产品标签、数量核对", InspectionType = "OQC", ProductId = products[4].Id, ProcessId = processes[9].Id, ItemName = "包装完整性", Usl = null, Lsl = null, Target = null, Unit = "-", InspectionMethod = "目视检查", SamplingFrequency = "全检" },
        };
        context.InspectionStandards.AddRange(standards);
        await context.SaveChangesAsync();
        #endregion

        #region DefectCodes
        var defectCodes = new List<DefectCode>
        {
            // --- 尺寸类 ---
            new() { Code = "D001", Name = "尺寸超差", DefectType = "尺寸", Severity = "MA", IsReworkable = true, Description = "零部件关键尺寸超出图纸公差范围" },
            new() { Code = "D006", Name = "螺纹不合格", DefectType = "尺寸", Severity = "MA", IsReworkable = true, Description = "螺纹牙型、中径或旋合长度不符合标准要求" },
            new() { Code = "D009", Name = "平面度超差", DefectType = "尺寸", Severity = "MI", IsReworkable = true, Description = "工件平面度超出允许偏差，影响装配贴合" },
            new() { Code = "D010", Name = "同心度超差", DefectType = "尺寸", Severity = "MA", IsReworkable = true, Description = "回转体各截面同心度超出公差，导致旋转不平衡" },
            new() { Code = "D011", Name = "孔径偏小", DefectType = "尺寸", Severity = "MA", IsReworkable = true, Description = "通孔或盲孔直径低于下限，影响配合件装配" },
            // --- 外观类 ---
            new() { Code = "D002", Name = "表面划伤", DefectType = "外观", Severity = "MI", IsReworkable = true, Description = "工件表面有线性划痕，深度未超过允许值" },
            new() { Code = "D003", Name = "裂纹", DefectType = "外观", Severity = "CR", IsReworkable = false, Description = "工件表面或内部存在裂纹，存在断裂风险" },
            new() { Code = "D007", Name = "氧化生锈", DefectType = "外观", Severity = "MI", IsReworkable = true, Description = "金属表面氧化变色或出现锈斑" },
            new() { Code = "D012", Name = "毛刺", DefectType = "外观", Severity = "MI", IsReworkable = true, Description = "工件边缘或孔口存在多余金属突起" },
            new() { Code = "D013", Name = "磕碰变形", DefectType = "外观", Severity = "MA", IsReworkable = false, Description = "工件在搬运或存储过程中遭受磕碰导致局部变形" },
            new() { Code = "D014", Name = "色差", DefectType = "外观", Severity = "MI", IsReworkable = false, Description = "涂覆或电镀层颜色与标准样板存在可见差异" },
            new() { Code = "D015", Name = "流挂", DefectType = "外观", Severity = "MI", IsReworkable = true, Description = "涂装后漆膜局部过厚，形成流挂痕迹" },
            // --- 功能类 ---
            new() { Code = "D004", Name = "硬度不足", DefectType = "功能", Severity = "MA", IsReworkable = true, Description = "热处理后硬度值低于图纸或标准要求" },
            new() { Code = "D008", Name = "装配不良", DefectType = "功能", Severity = "MA", IsReworkable = true, Description = "零部件装配后出现松动、卡滞或间隙不当" },
            new() { Code = "D016", Name = "密封泄漏", DefectType = "功能", Severity = "CR", IsReworkable = false, Description = "密封面存在泄漏，无法通过气密性或液密性测试" },
            new() { Code = "D017", Name = "电气短路", DefectType = "功能", Severity = "CR", IsReworkable = false, Description = "电路中存在异常导电路径，可能导致设备损坏" },
            new() { Code = "D018", Name = "绝缘不良", DefectType = "功能", Severity = "CR", IsReworkable = false, Description = "绝缘电阻或耐压测试不达标" },
            // --- 材料类 ---
            new() { Code = "D005", Name = "材料夹杂", DefectType = "材料", Severity = "CR", IsReworkable = false, Description = "金属基体中含有非金属夹杂物，影响力学性能" },
            new() { Code = "D019", Name = "材质不符", DefectType = "材料", Severity = "CR", IsReworkable = false, Description = "来料化学成分或金相组织与牌号要求不一致" },
            new() { Code = "D020", Name = "砂眼气孔", DefectType = "材料", Severity = "MA", IsReworkable = false, Description = "铸件表面或内部存在砂眼或气孔缺陷" },
            // --- 包装/标识类 ---
            new() { Code = "D021", Name = "包装破损", DefectType = "其他", Severity = "MI", IsReworkable = true, Description = "外包装箱体破损、变形或防潮层损坏" },
            new() { Code = "D022", Name = "标识错误", DefectType = "其他", Severity = "MA", IsReworkable = true, Description = "标签信息（批次号、日期、品名）与实物不符" },
            new() { Code = "D023", Name = "数量短缺", DefectType = "其他", Severity = "MA", IsReworkable = false, Description = "交付数量少于送货单或订单数量" },
        };
        context.DefectCodes.AddRange(defectCodes);
        await context.SaveChangesAsync();
        #endregion

        #region Equipment（22 台设备 DEMO 数据）
        // 查询组织ID，用于设备关联
        var workshopMachining = await context.Organizations.Where(o => o.Code == "WS_MACHINING").FirstOrDefaultAsync();
        var workshopHeatTreat = await context.Organizations.Where(o => o.Code == "WS_HEAT_TREAT").FirstOrDefaultAsync();
        var workshopAssembly = await context.Organizations.Where(o => o.Code == "WS_ASSEMBLY").FirstOrDefaultAsync();
        var workshopQuality = await context.Organizations.Where(o => o.Code == "WS_QUALITY").FirstOrDefaultAsync();
        var workshopStamping = await context.Organizations.Where(o => o.Code == "WS_STAMPING").FirstOrDefaultAsync();
        var workshopSurface = await context.Organizations.Where(o => o.Code == "WS_SURFACE").FirstOrDefaultAsync();
        var workshopSmt = await context.Organizations.Where(o => o.Code == "WS_SMT").FirstOrDefaultAsync();
        var lineA = await context.Organizations.Where(o => o.Code == "LINE_A").FirstOrDefaultAsync();
        var lineB = await context.Organizations.Where(o => o.Code == "LINE_B").FirstOrDefaultAsync();
        var lineC = await context.Organizations.Where(o => o.Code == "LINE_C").FirstOrDefaultAsync();
        var lineHeat = await context.Organizations.Where(o => o.Code == "LINE_HEAT").FirstOrDefaultAsync();
        var lineAssy1 = await context.Organizations.Where(o => o.Code == "LINE_ASSY_1").FirstOrDefaultAsync();
        var lineAssy2 = await context.Organizations.Where(o => o.Code == "LINE_ASSY_2").FirstOrDefaultAsync();
        var lineQc = await context.Organizations.Where(o => o.Code == "LINE_QC").FirstOrDefaultAsync();
        var lineStamp = await context.Organizations.Where(o => o.Code == "LINE_STAMP").FirstOrDefaultAsync();
        var lineSurface = await context.Organizations.Where(o => o.Code == "LINE_SURFACE").FirstOrDefaultAsync();
        var lineSmt = await context.Organizations.Where(o => o.Code == "LINE_SMT").FirstOrDefaultAsync();
        var orgId = await context.Organizations.Where(o => o.Code == "FACTORY_1").Select(o => o.Id).FirstOrDefaultAsync();

        var equipment = new List<Equipment>
        {
            // ═══════════════════════════════════════════════════════════
            // 机加车间 · CNC 设备（5 台）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "CNC-001", Name = "数控车床 #1", Model = "CK6140", ProductionLine = "A线", Workshop = "机加车间", OrgId = orgId, WorkshopId = workshopMachining?.Id, LineId = lineA?.Id, Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-a/cnc-001" },
            new() { Code = "CNC-002", Name = "数控车床 #2", Model = "CK6150", ProductionLine = "A线", Workshop = "机加车间", OrgId = orgId, WorkshopId = workshopMachining?.Id, LineId = lineA?.Id, Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-a/cnc-002" },
            new() { Code = "CNC-003", Name = "数控铣床 #1", Model = "XK714", ProductionLine = "B线", Workshop = "机加车间", OrgId = orgId, WorkshopId = workshopMachining?.Id, LineId = lineB?.Id, Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-b/cnc-003" },
            new() { Code = "CNC-004", Name = "数控车床 #3", Model = "CK6180", ProductionLine = "C线", Workshop = "机加车间", OrgId = orgId, WorkshopId = workshopMachining?.Id, LineId = lineC?.Id, Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-c/cnc-004" },
            new() { Code = "VMC-001", Name = "立式加工中心", Model = "VMC850", ProductionLine = "B线", Workshop = "机加车间", OrgId = orgId, WorkshopId = workshopMachining?.Id, LineId = lineB?.Id, Status = "idle", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-b/vmc-001" },

            // ═══════════════════════════════════════════════════════════
            // 热处理车间 · PLC 设备（2 台）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "HT-001", Name = "热处理炉", Model = "RX3-45-12", ProductionLine = "热处理线", Workshop = "热处理车间", OrgId = orgId, WorkshopId = workshopHeatTreat?.Id, LineId = lineHeat?.Id, Status = "running", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/heat/ht-001" },
            new() { Code = "HT-002", Name = "回火炉", Model = "RX3-90-15", ProductionLine = "热处理线", Workshop = "热处理车间", OrgId = orgId, WorkshopId = workshopHeatTreat?.Id, LineId = lineHeat?.Id, Status = "maintenance", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/heat/ht-002" },

            // ═══════════════════════════════════════════════════════════
            // 机加车间 · 磨削/检测（3 台）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "GR-001", Name = "磨床 #1", Model = "M7130", ProductionLine = "A线", Workshop = "机加车间", OrgId = orgId, WorkshopId = workshopMachining?.Id, LineId = lineA?.Id, Status = "idle", EquipmentType = "PLC", HasMqttConnection = false },
            new() { Code = "GR-002", Name = "平面磨床", Model = "M7520H", ProductionLine = "B线", Workshop = "机加车间", OrgId = orgId, WorkshopId = workshopMachining?.Id, LineId = lineB?.Id, Status = "running", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-b/gr-002" },
            new() { Code = "CMM-001", Name = "三坐标测量机", Model = "ZEISS CONTURA", ProductionLine = "质量检测线", Workshop = "质量中心", OrgId = orgId, WorkshopId = workshopQuality?.Id, LineId = lineQc?.Id, Status = "running", EquipmentType = "检测设备", HasMqttConnection = false },

            // ═══════════════════════════════════════════════════════════
            // 装配车间 · 机器人/自动化（3 台）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "ROBOT-WLD-01", Name = "焊接机器人 #1", Model = "IRB 2600-16/1.5", ProductionLine = "装配1线", Workshop = "装配车间", OrgId = orgId, WorkshopId = workshopAssembly?.Id, LineId = lineAssy1?.Id, Status = "running", EquipmentType = "机器人", HasMqttConnection = true, MqttTopicPrefix = "factory/assy-1/robot-wld-01" },
            new() { Code = "ROBOT-INS-01", Name = "装配机器人 #1", Model = "IRB 4600-60/2.05", ProductionLine = "装配1线", Workshop = "装配车间", OrgId = orgId, WorkshopId = workshopAssembly?.Id, LineId = lineAssy1?.Id, Status = "fault", EquipmentType = "机器人", HasMqttConnection = true, MqttTopicPrefix = "factory/assy-1/robot-ins-01" },
            new() { Code = "ROBOT-PCK-01", Name = "包装机器人", Model = "IRB 14000-12/1.6", ProductionLine = "装配2线", Workshop = "装配车间", OrgId = orgId, WorkshopId = workshopAssembly?.Id, LineId = lineAssy2?.Id, Status = "running", EquipmentType = "机器人", HasMqttConnection = true, MqttTopicPrefix = "factory/assy-2/robot-pck-01" },

            // ═══════════════════════════════════════════════════════════
            // 注塑/包装车间（2 台）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "INJ-001", Name = "注塑机 #1", Model = "MASTERY 180", ProductionLine = "C线", Workshop = "注塑车间", OrgId = orgId, Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/line-c/inj-001" },
            new() { Code = "PKG-001", Name = "自动包装机", Model = "PackPro-3000", ProductionLine = "包装线", Workshop = "包装车间", OrgId = orgId, Status = "idle", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/pack/pkg-001" },

            // ═══════════════════════════════════════════════════════════
            // 冲压车间 · 压力机（3 台）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "PRESS-001", Name = "冲压床 #1", Model = "SMD-60/1.5", ProductionLine = "冲压线", Workshop = "冲压车间", OrgId = orgId, WorkshopId = workshopStamping?.Id, LineId = lineStamp?.Id, Status = "running", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/stamp/press-001" },
            new() { Code = "PRESS-002", Name = "冲压床 #2", Model = "SMD-100/2.5", ProductionLine = "冲压线", Workshop = "冲压车间", OrgId = orgId, WorkshopId = workshopStamping?.Id, LineId = lineStamp?.Id, Status = "running", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/stamp/press-002" },
            new() { Code = "PRESS-003", Name = "精密冲床", Model = "FAX-200-3", ProductionLine = "冲压线", Workshop = "冲压车间", OrgId = orgId, WorkshopId = workshopStamping?.Id, LineId = lineStamp?.Id, Status = "maintenance", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/stamp/press-003" },

            // ═══════════════════════════════════════════════════════════
            // 表面处理车间（2 台）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "PLATE-001", Name = "电镀线", Model = "HD-6000", ProductionLine = "表面处理线", Workshop = "表面处理车间", OrgId = orgId, WorkshopId = workshopSurface?.Id, LineId = lineSurface?.Id, Status = "running", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/surface/plate-001" },
            new() { Code = "PAINT-001", Name = "自动喷枪", Model = "Gema 3920", ProductionLine = "涂装线", Workshop = "表面处理车间", OrgId = orgId, WorkshopId = workshopSurface?.Id, LineId = lineSurface?.Id, Status = "running", EquipmentType = "机器人", HasMqttConnection = true, MqttTopicPrefix = "factory/paint/paint-001" },

            // ═══════════════════════════════════════════════════════════
            // 电子车间 · SMT 贴片（2 台）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "SMT-001", Name = "SMT贴片机", Model = "YCM SA10N", ProductionLine = "SMT线", Workshop = "电子车间", OrgId = orgId, WorkshopId = workshopSmt?.Id, LineId = lineSmt?.Id, Status = "running", EquipmentType = "CNC", HasMqttConnection = true, MqttTopicPrefix = "factory/smt/smt-001" },
            new() { Code = "REFLOW-001", Name = "回流焊炉", Model = "HR-408N", ProductionLine = "SMT线", Workshop = "电子车间", OrgId = orgId, WorkshopId = workshopSmt?.Id, LineId = lineSmt?.Id, Status = "running", EquipmentType = "PLC", HasMqttConnection = true, MqttTopicPrefix = "factory/smt/reflow-001" },
        };
        context.Equipment.AddRange(equipment);
        await context.SaveChangesAsync();
        #endregion

        #region EquipmentParamMapping（设备-参数映射 DEMO 数据）
        var hasMappings = await context.Set<EquipmentParamMapping>().AnyAsync();
        if (!hasMappings)
        {
            // 获取已创建的设备ID
            var cnc001 = await context.Equipment.Where(e => e.Code == "CNC-001").Select(e => e.Id).FirstOrDefaultAsync();
            var cnc002 = await context.Equipment.Where(e => e.Code == "CNC-002").Select(e => e.Id).FirstOrDefaultAsync();
            var cnc003 = await context.Equipment.Where(e => e.Code == "CNC-003").Select(e => e.Id).FirstOrDefaultAsync();
            var cnc004 = await context.Equipment.Where(e => e.Code == "CNC-004").Select(e => e.Id).FirstOrDefaultAsync();
            var vmc001 = await context.Equipment.Where(e => e.Code == "VMC-001").Select(e => e.Id).FirstOrDefaultAsync();
            var ht001 = await context.Equipment.Where(e => e.Code == "HT-001").Select(e => e.Id).FirstOrDefaultAsync();
            var ht002 = await context.Equipment.Where(e => e.Code == "HT-002").Select(e => e.Id).FirstOrDefaultAsync();
            var gr002 = await context.Equipment.Where(e => e.Code == "GR-002").Select(e => e.Id).FirstOrDefaultAsync();
            var robotWld = await context.Equipment.Where(e => e.Code == "ROBOT-WLD-01").Select(e => e.Id).FirstOrDefaultAsync();
            var robotIns = await context.Equipment.Where(e => e.Code == "ROBOT-INS-01").Select(e => e.Id).FirstOrDefaultAsync();
            var robotPck = await context.Equipment.Where(e => e.Code == "ROBOT-PCK-01").Select(e => e.Id).FirstOrDefaultAsync();
            var inj001 = await context.Equipment.Where(e => e.Code == "INJ-001").Select(e => e.Id).FirstOrDefaultAsync();
            var pkg001 = await context.Equipment.Where(e => e.Code == "PKG-001").Select(e => e.Id).FirstOrDefaultAsync();
            var press001 = await context.Equipment.Where(e => e.Code == "PRESS-001").Select(e => e.Id).FirstOrDefaultAsync();
            var press002 = await context.Equipment.Where(e => e.Code == "PRESS-002").Select(e => e.Id).FirstOrDefaultAsync();
            var press003 = await context.Equipment.Where(e => e.Code == "PRESS-003").Select(e => e.Id).FirstOrDefaultAsync();
            var plate001 = await context.Equipment.Where(e => e.Code == "PLATE-001").Select(e => e.Id).FirstOrDefaultAsync();
            var paint001 = await context.Equipment.Where(e => e.Code == "PAINT-001").Select(e => e.Id).FirstOrDefaultAsync();
            var smt001 = await context.Equipment.Where(e => e.Code == "SMT-001").Select(e => e.Id).FirstOrDefaultAsync();
            var reflow001 = await context.Equipment.Where(e => e.Code == "REFLOW-001").Select(e => e.Id).FirstOrDefaultAsync();

            // 获取参数组ID
            var thermoGroup = await context.Set<Models.M02_5.ParamGroup>().Where(g => g.Code == "thermo_params").Select(g => g.Id).FirstOrDefaultAsync();
            var mechGroup = await context.Set<Models.M02_5.ParamGroup>().Where(g => g.Code == "mech_params").Select(g => g.Id).FirstOrDefaultAsync();
            var dimGroup = await context.Set<Models.M02_5.ParamGroup>().Where(g => g.Code == "dim_params").Select(g => g.Id).FirstOrDefaultAsync();

        var mappings = new List<EquipmentParamMapping>
        {
            // ── CNC-001 数控车床 #1（主轴转速、切削温度、振动、进给率） ──
            new() { EquipmentId = cnc001, MqttTopic = "factory/line-a/cnc-001/spindle_speed", SystemParamCode = "spindle_torque", ParamGroupId = mechGroup, DataType = "numeric", Unit = "rpm" },
            new() { EquipmentId = cnc001, MqttTopic = "factory/line-a/cnc-001/cutting_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = cnc001, MqttTopic = "factory/line-a/cnc-001/vibration", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "mm/s" },
            new() { EquipmentId = cnc001, MqttTopic = "factory/line-a/cnc-001/feed_rate", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm/rev" },
            new() { EquipmentId = cnc001, MqttTopic = "factory/line-a/cnc-001/power", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kW" },
            new() { EquipmentId = cnc001, MqttTopic = "factory/line-a/cnc-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── CNC-002 数控车床 #2（同型号，参数一致） ──
            new() { EquipmentId = cnc002, MqttTopic = "factory/line-a/cnc-002/spindle_speed", SystemParamCode = "spindle_torque", ParamGroupId = mechGroup, DataType = "numeric", Unit = "rpm" },
            new() { EquipmentId = cnc002, MqttTopic = "factory/line-a/cnc-002/cutting_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = cnc002, MqttTopic = "factory/line-a/cnc-002/vibration", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "mm/s" },
            new() { EquipmentId = cnc002, MqttTopic = "factory/line-a/cnc-002/feed_rate", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm/rev" },
            new() { EquipmentId = cnc002, MqttTopic = "factory/line-a/cnc-002/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── CNC-003 数控铣床 #1（铣削参数） ──
            new() { EquipmentId = cnc003, MqttTopic = "factory/line-b/cnc-003/spindle_speed", SystemParamCode = "spindle_torque", ParamGroupId = mechGroup, DataType = "numeric", Unit = "rpm" },
            new() { EquipmentId = cnc003, MqttTopic = "factory/line-b/cnc-003/spindle_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = cnc003, MqttTopic = "factory/line-b/cnc-003/coolant_flow", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "L/min" },
            new() { EquipmentId = cnc003, MqttTopic = "factory/line-b/cnc-003/tool_wear", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = cnc003, MqttTopic = "factory/line-b/cnc-003/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── CNC-004 数控车床 #3（C线，车削中心参数） ──
            new() { EquipmentId = cnc004, MqttTopic = "factory/line-c/cnc-004/spindle_speed", SystemParamCode = "spindle_torque", ParamGroupId = mechGroup, DataType = "numeric", Unit = "rpm" },
            new() { EquipmentId = cnc004, MqttTopic = "factory/line-c/cnc-004/cutting_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = cnc004, MqttTopic = "factory/line-c/cnc-004/vibration", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "mm/s" },
            new() { EquipmentId = cnc004, MqttTopic = "factory/line-c/cnc-004/feed_rate", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm/rev" },
            new() { EquipmentId = cnc004, MqttTopic = "factory/line-c/cnc-004/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── VMC-001 立式加工中心（铣削/钻削参数） ──
            new() { EquipmentId = vmc001, MqttTopic = "factory/line-b/vmc-001/spindle_speed", SystemParamCode = "spindle_torque", ParamGroupId = mechGroup, DataType = "numeric", Unit = "rpm" },
            new() { EquipmentId = vmc001, MqttTopic = "factory/line-b/vmc-001/spindle_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = vmc001, MqttTopic = "factory/line-b/vmc-001/coolant_flow", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "L/min" },
            new() { EquipmentId = vmc001, MqttTopic = "factory/line-b/vmc-001/tool_wear", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = vmc001, MqttTopic = "factory/line-b/vmc-001/axis_x_position", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = vmc001, MqttTopic = "factory/line-b/vmc-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── HT-001 热处理炉（温度、气氛、升温速率） ──
            new() { EquipmentId = ht001, MqttTopic = "factory/heat/ht-001/chamber_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = ht001, MqttTopic = "factory/heat/ht-001/atmosphere", SystemParamCode = "rust_prevention", ParamGroupId = null, DataType = "boolean" },
            new() { EquipmentId = ht001, MqttTopic = "factory/heat/ht-001/heating_rate", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃/min" },
            new() { EquipmentId = ht001, MqttTopic = "factory/heat/ht-001/furnace_pressure", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kPa" },
            new() { EquipmentId = ht001, MqttTopic = "factory/heat/ht-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── HT-002 回火炉（温度监控） ──
            new() { EquipmentId = ht002, MqttTopic = "factory/heat/ht-002/chamber_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = ht002, MqttTopic = "factory/heat/ht-002/cooling_rate", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃/min" },
            new() { EquipmentId = ht002, MqttTopic = "factory/heat/ht-002/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── GR-002 平面磨床（磨削参数） ──
            new() { EquipmentId = gr002, MqttTopic = "factory/line-b/gr-002/spindle_speed", SystemParamCode = "spindle_torque", ParamGroupId = mechGroup, DataType = "numeric", Unit = "rpm" },
            new() { EquipmentId = gr002, MqttTopic = "factory/line-b/gr-002/grinding_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = gr002, MqttTopic = "factory/line-b/gr-002/feed_rate", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm/pass" },
            new() { EquipmentId = gr002, MqttTopic = "factory/line-b/gr-002/surface_roughness", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "μm" },
            new() { EquipmentId = gr002, MqttTopic = "factory/line-b/gr-002/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── ROBOT-WLD-01 焊接机器人（焊接参数） ──
            new() { EquipmentId = robotWld, MqttTopic = "factory/assy-1/robot-wld-01/welding_current", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "A" },
            new() { EquipmentId = robotWld, MqttTopic = "factory/assy-1/robot-wld-01/welding_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = robotWld, MqttTopic = "factory/assy-1/robot-wld-01/welding_speed", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "cm/min" },
            new() { EquipmentId = robotWld, MqttTopic = "factory/assy-1/robot-wld-01/gas_flow", SystemParamCode = "rust_prevention", ParamGroupId = null, DataType = "numeric", Unit = "L/min" },
            new() { EquipmentId = robotWld, MqttTopic = "factory/assy-1/robot-wld-01/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── ROBOT-INS-01 装配机器人 ──
            new() { EquipmentId = robotIns, MqttTopic = "factory/assy-1/robot-ins-01/force_torque", SystemParamCode = "spindle_torque", ParamGroupId = mechGroup, DataType = "numeric", Unit = "N·m" },
            new() { EquipmentId = robotIns, MqttTopic = "factory/assy-1/robot-ins-01/position_x", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = robotIns, MqttTopic = "factory/assy-1/robot-ins-01/position_y", SystemParamCode = "id_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = robotIns, MqttTopic = "factory/assy-1/robot-ins-01/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── ROBOT-PCK-01 包装机器人（包装参数） ──
            new() { EquipmentId = robotPck, MqttTopic = "factory/assy-2/robot-pck-01/force_torque", SystemParamCode = "spindle_torque", ParamGroupId = mechGroup, DataType = "numeric", Unit = "N·m" },
            new() { EquipmentId = robotPck, MqttTopic = "factory/assy-2/robot-pck-01/position_z", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = robotPck, MqttTopic = "factory/assy-2/robot-pck-01/gripper_pressure", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kPa" },
            new() { EquipmentId = robotPck, MqttTopic = "factory/assy-2/robot-pck-01/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── INJ-001 注塑机（注塑参数） ──
            new() { EquipmentId = inj001, MqttTopic = "factory/line-c/inj-001/barrel_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = inj001, MqttTopic = "factory/line-c/inj-001/injection_pressure", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "MPa" },
            new() { EquipmentId = inj001, MqttTopic = "factory/line-c/inj-001/injection_speed", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "mm/s" },
            new() { EquipmentId = inj001, MqttTopic = "factory/line-c/inj-001/mold_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = inj001, MqttTopic = "factory/line-c/inj-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── PKG-001 自动包装机（包装参数） ──
            new() { EquipmentId = pkg001, MqttTopic = "factory/pack/pkg-001/seal_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = pkg001, MqttTopic = "factory/pack/pkg-001/seal_pressure", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kPa" },
            new() { EquipmentId = pkg001, MqttTopic = "factory/pack/pkg-001/conveyor_speed", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "m/min" },
            new() { EquipmentId = pkg001, MqttTopic = "factory/pack/pkg-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── PRESS-001 冲压床 #1（冲压力、行程、模具温度） ──
            new() { EquipmentId = press001, MqttTopic = "factory/stamp/press-001/pressure", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kN" },
            new() { EquipmentId = press001, MqttTopic = "factory/stamp/press-001/stroke", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = press001, MqttTopic = "factory/stamp/press-001/stroke_speed", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "mm/s" },
            new() { EquipmentId = press001, MqttTopic = "factory/stamp/press-001/die_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = press001, MqttTopic = "factory/stamp/press-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── PRESS-002 冲压床 #2（同型号，参数一致） ──
            new() { EquipmentId = press002, MqttTopic = "factory/stamp/press-002/pressure", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kN" },
            new() { EquipmentId = press002, MqttTopic = "factory/stamp/press-002/stroke", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = press002, MqttTopic = "factory/stamp/press-002/stroke_speed", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "mm/s" },
            new() { EquipmentId = press002, MqttTopic = "factory/stamp/press-002/die_temp", SystemParamCode = "temp_finishing", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = press002, MqttTopic = "factory/stamp/press-002/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── PRESS-003 精密冲床（高精度冲压力控制） ──
            new() { EquipmentId = press003, MqttTopic = "factory/stamp/press-003/pressure", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kN" },
            new() { EquipmentId = press003, MqttTopic = "factory/stamp/press-003/stroke", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = press003, MqttTopic = "factory/stamp/press-003/position_accuracy", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "μm" },
            new() { EquipmentId = press003, MqttTopic = "factory/stamp/press-003/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── PLATE-001 电镀线（电镀参数） ──
            new() { EquipmentId = plate001, MqttTopic = "factory/surface/plate-001/bath_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = plate001, MqttTopic = "factory/surface/plate-001/current_density", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "A/dm²" },
            new() { EquipmentId = plate001, MqttTopic = "factory/surface/plate-001/ph_value", SystemParamCode = "rust_prevention", ParamGroupId = null, DataType = "numeric", Unit = "-" },
            new() { EquipmentId = plate001, MqttTopic = "factory/surface/plate-001/plating_speed", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "μm/min" },
            new() { EquipmentId = plate001, MqttTopic = "factory/surface/plate-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── PAINT-001 自动喷枪（涂装参数） ──
            new() { EquipmentId = paint001, MqttTopic = "factory/paint/paint-001/atomizing_air", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kPa" },
            new() { EquipmentId = paint001, MqttTopic = "factory/paint/paint-001/flow_rate", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "mL/min" },
            new() { EquipmentId = paint001, MqttTopic = "factory/paint/paint-001/curing_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = paint001, MqttTopic = "factory/paint/paint-001/coating_thickness", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "μm" },
            new() { EquipmentId = paint001, MqttTopic = "factory/paint/paint-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── SMT-001 SMT贴片机（贴片参数） ──
            new() { EquipmentId = smt001, MqttTopic = "factory/smt/smt-001/nozzle_vacuum", SystemParamCode = "cutting_pressure", ParamGroupId = mechGroup, DataType = "numeric", Unit = "kPa" },
            new() { EquipmentId = smt001, MqttTopic = "factory/smt/smt-001/placement_speed", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "cpm" },
            new() { EquipmentId = smt001, MqttTopic = "factory/smt/smt-001/placement_accuracy", SystemParamCode = "od_tolerance", ParamGroupId = dimGroup, DataType = "numeric", Unit = "mm" },
            new() { EquipmentId = smt001, MqttTopic = "factory/smt/smt-001/reflow_zone_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = smt001, MqttTopic = "factory/smt/smt-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },

            // ── REFLOW-001 回流焊炉（回流焊接温区参数） ──
            new() { EquipmentId = reflow001, MqttTopic = "factory/smt/reflow-001/zone1_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = reflow001, MqttTopic = "factory/smt/reflow-001/zone2_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = reflow001, MqttTopic = "factory/smt/reflow-001/zone3_temp", SystemParamCode = "temp_heat_treat", ParamGroupId = thermoGroup, DataType = "numeric", Unit = "℃" },
            new() { EquipmentId = reflow001, MqttTopic = "factory/smt/reflow-001/conveyor_speed", SystemParamCode = "surface_roughness", ParamGroupId = null, DataType = "numeric", Unit = "m/min" },
            new() { EquipmentId = reflow001, MqttTopic = "factory/smt/reflow-001/status", SystemParamCode = "surface_defect", ParamGroupId = null, DataType = "status", Unit = "-" },
        };

            context.Set<EquipmentParamMapping>().AddRange(mappings);
            await context.SaveChangesAsync();
        }
        #endregion

        #region EquipmentStatusHistory（设备状态历史 DEMO 数据）
        var hasStatusHistory = await context.Set<EquipmentStatusHistory>().AnyAsync();
        if (!hasStatusHistory)
        {
            var cnc001Id = await context.Equipment.Where(e => e.Code == "CNC-001").Select(e => e.Id).FirstOrDefaultAsync();
            var cnc002Id = await context.Equipment.Where(e => e.Code == "CNC-002").Select(e => e.Id).FirstOrDefaultAsync();
            var cnc003Id = await context.Equipment.Where(e => e.Code == "CNC-003").Select(e => e.Id).FirstOrDefaultAsync();
            var cnc004Id = await context.Equipment.Where(e => e.Code == "CNC-004").Select(e => e.Id).FirstOrDefaultAsync();
            var vmc001Id = await context.Equipment.Where(e => e.Code == "VMC-001").Select(e => e.Id).FirstOrDefaultAsync();
            var ht001Id = await context.Equipment.Where(e => e.Code == "HT-001").Select(e => e.Id).FirstOrDefaultAsync();
            var ht002Id = await context.Equipment.Where(e => e.Code == "HT-002").Select(e => e.Id).FirstOrDefaultAsync();
            var robotWldId = await context.Equipment.Where(e => e.Code == "ROBOT-WLD-01").Select(e => e.Id).FirstOrDefaultAsync();
            var robotInsId = await context.Equipment.Where(e => e.Code == "ROBOT-INS-01").Select(e => e.Id).FirstOrDefaultAsync();
            var robotPckId = await context.Equipment.Where(e => e.Code == "ROBOT-PCK-01").Select(e => e.Id).FirstOrDefaultAsync();
            var inj001Id = await context.Equipment.Where(e => e.Code == "INJ-001").Select(e => e.Id).FirstOrDefaultAsync();
            var press001Id = await context.Equipment.Where(e => e.Code == "PRESS-001").Select(e => e.Id).FirstOrDefaultAsync();
            var press002Id = await context.Equipment.Where(e => e.Code == "PRESS-002").Select(e => e.Id).FirstOrDefaultAsync();
            var press003Id = await context.Equipment.Where(e => e.Code == "PRESS-003").Select(e => e.Id).FirstOrDefaultAsync();
            var smt001Id = await context.Equipment.Where(e => e.Code == "SMT-001").Select(e => e.Id).FirstOrDefaultAsync();
            var reflow001Id = await context.Equipment.Where(e => e.Code == "REFLOW-001").Select(e => e.Id).FirstOrDefaultAsync();

            var statusHistory = new List<EquipmentStatusHistory>
            {
                // ── CNC-001 一天内的状态变化 ──
                new() { EquipmentId = cnc001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = cnc001Id, Signal = "idle", SignalData = "{\"reason\":\"换刀\",\"tool\":\"T-001\"}", RecordedAt = new DateTime(2026, 7, 26, 8, 30, 0) },
                new() { EquipmentId = cnc001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 8, 45, 0) },
                new() { EquipmentId = cnc001Id, Signal = "fault", SignalData = "{\"code\":\"E201\",\"desc\":\"主轴过载\",\"error_code\":201}", RecordedAt = new DateTime(2026, 7, 26, 14, 20, 0) },
                new() { EquipmentId = cnc001Id, Signal = "idle", SignalData = "{\"reason\":\"维修\",\"technician\":\"王师傅\"}", RecordedAt = new DateTime(2026, 7, 26, 14, 25, 0) },
                new() { EquipmentId = cnc001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 16, 0, 0) },

                // ── CNC-002 一天内的状态变化 ──
                new() { EquipmentId = cnc002Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = cnc002Id, Signal = "idle", SignalData = "{\"reason\":\"班次切换\"}", RecordedAt = new DateTime(2026, 7, 26, 14, 0, 0) },
                new() { EquipmentId = cnc002Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 14, 15, 0) },

                // ── CNC-003 一天内的状态变化 ──
                new() { EquipmentId = cnc003Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = cnc003Id, Signal = "fault", SignalData = "{\"code\":\"E305\",\"desc\":\"冷却液不足\",\"error_code\":305}", RecordedAt = new DateTime(2026, 7, 26, 10, 15, 0) },
                new() { EquipmentId = cnc003Id, Signal = "idle", SignalData = "{\"reason\":\"添加冷却液\"}", RecordedAt = new DateTime(2026, 7, 26, 10, 30, 0) },
                new() { EquipmentId = cnc003Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 10, 45, 0) },

                // ── CNC-004 数控车床 #3（C线，新投用） ──
                new() { EquipmentId = cnc004Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 30, 0) },
                new() { EquipmentId = cnc004Id, Signal = "idle", SignalData = "{\"reason\":\"首件调试\"}", RecordedAt = new DateTime(2026, 7, 26, 8, 0, 0) },
                new() { EquipmentId = cnc004Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 8, 30, 0) },

                // ── VMC-001 立式加工中心（待料停机） ──
                new() { EquipmentId = vmc001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 25, 6, 0, 0) },
                new() { EquipmentId = vmc001Id, Signal = "idle", SignalData = "{\"reason\":\"等待上料\"}", RecordedAt = new DateTime(2026, 7, 26, 7, 0, 0) },

                // ── HT-001 热处理炉状态变化 ──
                new() { EquipmentId = ht001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 5, 30, 0) },
                new() { EquipmentId = ht001Id, Signal = "idle", SignalData = "{\"reason\":\"炉温达标，保温中\"}", RecordedAt = new DateTime(2026, 7, 26, 9, 0, 0) },
                new() { EquipmentId = ht001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 11, 30, 0) },

                // ── HT-002 回火炉（维护中） ──
                new() { EquipmentId = ht002Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 25, 6, 0, 0) },
                new() { EquipmentId = ht002Id, Signal = "fault", SignalData = "{\"code\":\"E102\",\"desc\":\"加热管故障\",\"error_code\":102}", RecordedAt = new DateTime(2026, 7, 26, 8, 0, 0) },
                new() { EquipmentId = ht002Id, Signal = "idle", SignalData = "{\"reason\":\"更换加热管\",\"technician\":\"李工\"}", RecordedAt = new DateTime(2026, 7, 26, 8, 10, 0) },

                // ── ROBOT-WLD-01 焊接机器人 ──
                new() { EquipmentId = robotWldId, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = robotWldId, Signal = "idle", SignalData = "{\"reason\":\"焊丝更换\"}", RecordedAt = new DateTime(2026, 7, 26, 10, 0, 0) },
                new() { EquipmentId = robotWldId, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 10, 20, 0) },

                // ── ROBOT-INS-01 装配机器人（故障状态） ──
                new() { EquipmentId = robotInsId, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 25, 6, 0, 0) },
                new() { EquipmentId = robotInsId, Signal = "fault", SignalData = "{\"code\":\"E500\",\"desc\":\"关节3编码器异常\",\"error_code\":500}", RecordedAt = new DateTime(2026, 7, 26, 9, 30, 0) },
                new() { EquipmentId = robotInsId, Signal = "idle", SignalData = "{\"reason\":\"等待备件\",\"priority\":\"high\"}", RecordedAt = new DateTime(2026, 7, 26, 9, 35, 0) },

                // ── ROBOT-PCK-01 包装机器人 ──
                new() { EquipmentId = robotPckId, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = robotPckId, Signal = "idle", SignalData = "{\"reason\":\"夹具校准\"}", RecordedAt = new DateTime(2026, 7, 26, 13, 0, 0) },
                new() { EquipmentId = robotPckId, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 13, 20, 0) },

                // ── INJ-001 注塑机 ──
                new() { EquipmentId = inj001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = inj001Id, Signal = "idle", SignalData = "{\"reason\":\"模具更换\"}", RecordedAt = new DateTime(2026, 7, 26, 12, 0, 0) },
                new() { EquipmentId = inj001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 12, 50, 0) },

                // ── PRESS-001 冲压床 #1 ──
                new() { EquipmentId = press001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = press001Id, Signal = "idle", SignalData = "{\"reason\":\"模具更换\"}", RecordedAt = new DateTime(2026, 7, 26, 9, 30, 0) },
                new() { EquipmentId = press001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 10, 15, 0) },
                new() { EquipmentId = press001Id, Signal = "fault", SignalData = "{\"code\":\"E401\",\"desc\":\"压力传感器异常\",\"error_code\":401}", RecordedAt = new DateTime(2026, 7, 26, 15, 0, 0) },
                new() { EquipmentId = press001Id, Signal = "idle", SignalData = "{\"reason\":\"传感器校准\",\"technician\":\"张工\"}", RecordedAt = new DateTime(2026, 7, 26, 15, 10, 0) },

                // ── PRESS-002 冲压床 #2 ──
                new() { EquipmentId = press002Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = press002Id, Signal = "idle", SignalData = "{\"reason\":\"午休停机\"}", RecordedAt = new DateTime(2026, 7, 26, 12, 0, 0) },
                new() { EquipmentId = press002Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 13, 0, 0) },

                // ── PRESS-003 精密冲床（维护中） ──
                new() { EquipmentId = press003Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 25, 6, 0, 0) },
                new() { EquipmentId = press003Id, Signal = "idle", SignalData = "{\"reason\":\"精度校准\"}", RecordedAt = new DateTime(2026, 7, 26, 7, 0, 0) },
                new() { EquipmentId = press003Id, Signal = "maintenance", SignalData = "{\"reason\":\"季度保养\",\"technician\":\"刘工\"}", RecordedAt = new DateTime(2026, 7, 26, 9, 0, 0) },

                // ── SMT-001 SMT贴片机 ──
                new() { EquipmentId = smt001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 6, 0, 0) },
                new() { EquipmentId = smt001Id, Signal = "idle", SignalData = "{\"reason\":\"换料台\"}", RecordedAt = new DateTime(2026, 7, 26, 11, 0, 0) },
                new() { EquipmentId = smt001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 11, 30, 0) },

                // ── REFLOW-001 回流焊炉 ──
                new() { EquipmentId = reflow001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 5, 30, 0) },
                new() { EquipmentId = reflow001Id, Signal = "idle", SignalData = "{\"reason\":\"锡炉清理\"}", RecordedAt = new DateTime(2026, 7, 26, 14, 0, 0) },
                new() { EquipmentId = reflow001Id, Signal = "running", SignalData = null, RecordedAt = new DateTime(2026, 7, 26, 14, 30, 0) },
            };

            context.Set<EquipmentStatusHistory>().AddRange(statusHistory);
            await context.SaveChangesAsync();
        }
        #endregion

        #region EquipmentQualityCorrelation（设备-质量关联分析 DEMO 数据）
        var hasCorrelation = await context.Set<EquipmentQualityCorrelation>().AnyAsync();
        if (!hasCorrelation)
        {
            var cnc001Id = await context.Equipment.Where(e => e.Code == "CNC-001").Select(e => e.Id).FirstOrDefaultAsync();
            var cnc002Id = await context.Equipment.Where(e => e.Code == "CNC-002").Select(e => e.Id).FirstOrDefaultAsync();
            var cnc003Id = await context.Equipment.Where(e => e.Code == "CNC-003").Select(e => e.Id).FirstOrDefaultAsync();
            var press001Id = await context.Equipment.Where(e => e.Code == "PRESS-001").Select(e => e.Id).FirstOrDefaultAsync();
            var smt001Id = await context.Equipment.Where(e => e.Code == "SMT-001").Select(e => e.Id).FirstOrDefaultAsync();
            var reflow001Id = await context.Equipment.Where(e => e.Code == "REFLOW-001").Select(e => e.Id).FirstOrDefaultAsync();
            var ht001Id = await context.Equipment.Where(e => e.Code == "HT-001").Select(e => e.Id).FirstOrDefaultAsync();
            var robotWldId = await context.Equipment.Where(e => e.Code == "ROBOT-WLD-01").Select(e => e.Id).FirstOrDefaultAsync();
            var inj001Id = await context.Equipment.Where(e => e.Code == "INJ-001").Select(e => e.Id).FirstOrDefaultAsync();

        var correlations = new List<EquipmentQualityCorrelation>
        {
            // ── CNC-001: 主轴转速与外径尺寸相关性分析 ──
            new() { EquipmentId = cnc001Id, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"主轴转速\",\"correlation\":0.82,\"impact\":\"high\",\"finding\":\"主轴转速>1500rpm时，外径尺寸超差率上升15%\"},{\"name\":\"切削温度\",\"correlation\":0.65,\"impact\":\"medium\",\"finding\":\"切削温度>460℃时，表面粗糙度Ra值增大\"}],\"summary\":\"建议将主轴转速控制在1200-1400rpm区间，切削温度控制在445℃以下\"}", CreatedAt = DateTime.UtcNow },

            // ── CNC-002: 振动与产品不良关联 ──
            new() { EquipmentId = cnc002Id, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"振动值\",\"correlation\":0.91,\"impact\":\"high\",\"finding\":\"振动>4.5mm/s时，尺寸Cpk下降0.3\"},{\"name\":\"进给率\",\"correlation\":0.45,\"impact\":\"low\",\"finding\":\"进给率对尺寸影响不显著\"}],\"summary\":\"设备导轨磨损严重，建议更换X轴导轨，当前振动值已接近预警阈值\"}", CreatedAt = DateTime.UtcNow },

            // ── CNC-003: 铣床刀具磨损与表面质量 ──
            new() { EquipmentId = cnc003Id, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"刀具磨损量\",\"correlation\":0.88,\"impact\":\"high\",\"finding\":\"刀具磨损>0.3mm时，表面粗糙度Ra超标\"},{\"name\":\"冷却液流量\",\"correlation\":0.55,\"impact\":\"medium\",\"finding\":\"冷却液流量不足时，刀具磨损加速\"}],\"summary\":\"建议刀具磨损量达到0.25mm时强制换刀，冷却液流量保持>8L/min\"}", CreatedAt = DateTime.UtcNow },

            // ── PRESS-001: 冲压力与冲压件尺寸关联 ──
            new() { EquipmentId = press001Id, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"冲压力\",\"correlation\":0.85,\"impact\":\"high\",\"finding\":\"压力>80kN时冲压件毛边风险增加\"},{\"name\":\"模具温度\",\"correlation\":0.60,\"impact\":\"medium\",\"finding\":\"模温>80℃时产品尺寸偏差增大\"}],\"summary\":\"冲压力控制在60-75kN，模具温度保持60-75℃\"}", CreatedAt = DateTime.UtcNow },

            // ── SMT-001: 贴装精度与焊接质量关联 ──
            new() { EquipmentId = smt001Id, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"贴装精度\",\"correlation\":0.93,\"impact\":\"high\",\"finding\":\"精度偏差>0.05mm时虚焊率上升30%\"},{\"name\":\"吸嘴真空度\",\"correlation\":0.70,\"impact\":\"medium\",\"finding\":\"真空度<8kPa时元件偏移率增加\"}],\"summary\":\"贴装精度需保持在±0.03mm以内，吸嘴真空度≥10kPa\"}", CreatedAt = DateTime.UtcNow },

            // ── REFLOW-001: 回流焊温区与焊接可靠性关联 ──
            new() { EquipmentId = reflow001Id, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"峰值温度\",\"correlation\":0.90,\"impact\":\"high\",\"finding\":\"峰值温度>245℃时焊点脆化\"},{\"name\":\"液相线以上时间\",\"correlation\":0.75,\"impact\":\"medium\",\"finding\":\"液相线以上时间<60s时润湿不良\"}],\"summary\":\"峰值温度控制在235-240℃，液相线以上时间保持60-90s\"}", CreatedAt = DateTime.UtcNow },

            // ── HT-001: 炉温与硬度关联 ──
            new() { EquipmentId = ht001Id, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"炉内温度\",\"correlation\":0.95,\"impact\":\"high\",\"finding\":\"炉温每偏离10℃，硬度HRC变化约1.5\"},{\"name\":\"升温速率\",\"correlation\":0.40,\"impact\":\"low\",\"finding\":\"升温速率对最终硬度影响较小\"},{\"name\":\"保温时间\",\"correlation\":0.70,\"impact\":\"medium\",\"finding\":\"保温时间<45min时硬度偏低\"}],\"summary\":\"炉温控制精度需保持在±5℃以内，保温时间不少于50min\"}", CreatedAt = DateTime.UtcNow },

            // ── ROBOT-WLD-01: 焊接电流与焊缝质量 ──
            new() { EquipmentId = robotWldId, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"焊接电流\",\"correlation\":0.87,\"impact\":\"high\",\"finding\":\"电流>180A时出现烧穿缺陷\"},{\"name\":\"焊接速度\",\"correlation\":0.72,\"impact\":\"medium\",\"finding\":\"速度<40cm/min时焊缝余高超标\"},{\"name\":\"保护气流量\",\"correlation\":0.60,\"impact\":\"medium\",\"finding\":\"气流量<15L/min时焊缝氧化\"}],\"summary\":\"焊接电流控制在150-175A，速度保持40-60cm/min，气流量≥18L/min\"}", CreatedAt = DateTime.UtcNow },

            // ── INJ-001: 注塑参数与产品外观关联 ──
            new() { EquipmentId = inj001Id, AnalysisDate = new DateOnly(2026, 7, 25), CorrelationData = "{\"analysisType\":\"parameter_quality_correlation\",\"parameters\":[{\"name\":\"料筒温度\",\"correlation\":0.75,\"impact\":\"high\",\"finding\":\"温度>230℃出现飞边\"},{\"name\":\"注射压力\",\"correlation\":0.80,\"impact\":\"high\",\"finding\":\"压力>90MPa时飞边风险显著增加\"},{\"name\":\"模具温度\",\"correlation\":0.50,\"impact\":\"medium\",\"finding\":\"模温<60℃时表面光泽度不足\"}],\"summary\":\"料筒温度控制在210-225℃，注射压力<85MPa，模温保持65-75℃\"}", CreatedAt = DateTime.UtcNow },
        };

            context.Set<EquipmentQualityCorrelation>().AddRange(correlations);
            await context.SaveChangesAsync();
        }
        #endregion

        #region Tools（刀具管理 DEMO 数据 25 条）
        var tools = new List<Tool>
        {
            // ═══════════════════════════════════════════════════════════
            // 车刀类（6 条）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "T-001", Name = "外圆车刀 90°", Model = "WNMG080408", ToolType = "车刀", DesignLife = 500, LifeUnit = "cycles", CurrentLife = 120, Supplier = "山特维克" },
            new() { Code = "T-006", Name = "内圆车刀 35°", Model = "VNGN160404", ToolType = "车刀", DesignLife = 450, LifeUnit = "cycles", CurrentLife = 85, Supplier = "山特维克" },
            new() { Code = "T-007", Name = "切断车刀 3mm", Model = "GGHN2525M12", ToolType = "车刀", DesignLife = 800, LifeUnit = "cycles", CurrentLife = 320, Supplier = "肯纳" },
            new() { Code = "T-008", Name = "精车刀 75°", Model = "DNGN160404", ToolType = "车刀", DesignLife = 600, LifeUnit = "cycles", CurrentLife = 550, Supplier = "OSG" },
            new() { Code = "T-009", Name = "滚花刀直纹 Φ30", Model = "RHR-30A", ToolType = "车刀", DesignLife = 2000, LifeUnit = "cycles", CurrentLife = 200, Supplier = "山特维克" },
            new() { Code = "T-010", Name = "圆弧车刀 R2", Model = "CNMG120408", ToolType = "车刀", DesignLife = 400, LifeUnit = "cycles", CurrentLife = 380, Supplier = "肯纳" },

            // ═══════════════════════════════════════════════════════════
            // 铣刀类（6 条）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "T-003", Name = "面铣刀 Φ50", Model = "F4042.BS.050", ToolType = "铣刀", DesignLife = 400, LifeUnit = "cycles", CurrentLife = 200, Supplier = "肯纳" },
            new() { Code = "T-011", Name = "球头铣刀 Φ10", Model = "C4-10R", ToolType = "铣刀", DesignLife = 200, LifeUnit = "cycles", CurrentLife = 50, Supplier = "三菱" },
            new() { Code = "T-012", Name = "立铣刀 Φ6 4刃", Model = "S46R-6.0-AL", ToolType = "铣刀", DesignLife = 300, LifeUnit = "cycles", CurrentLife = 150, Supplier = "OSG" },
            new() { Code = "T-013", Name = "键槽铣刀 Φ8", Model = "H8-64.408", ToolType = "铣刀", DesignLife = 250, LifeUnit = "cycles", CurrentLife = 30, Supplier = "OSG" },
            new() { Code = "T-014", Name = "圆鼻铣刀 R3", Model = "R3-4032F10R", ToolType = "铣刀", DesignLife = 350, LifeUnit = "cycles", CurrentLife = 100, Supplier = "三菱" },
            new() { Code = "T-015", Name = "倒角铣刀 60°", Model = "CR60-200", ToolType = "铣刀", DesignLife = 500, LifeUnit = "cycles", CurrentLife = 420, Supplier = "肯纳" },

            // ═══════════════════════════════════════════════════════════
            // 钻铰类（5 条）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "T-002", Name = "钻头 Φ8", Model = "D924-8.0", ToolType = "钻头", DesignLife = 300, LifeUnit = "cycles", CurrentLife = 45, Supplier = "OSG" },
            new() { Code = "T-004", Name = "铰刀 Φ10H7", Model = "HR500-10.0", ToolType = "钻头", DesignLife = 350, LifeUnit = "cycles", CurrentLife = 80, Supplier = "OSG" },
            new() { Code = "T-016", Name = "麻花钻 Φ12", Model = "D924-12.0", ToolType = "钻头", DesignLife = 280, LifeUnit = "cycles", CurrentLife = 160, Supplier = "OSG" },
            new() { Code = "T-017", Name = "中心钻 A2 Φ2", Model = "AC2.0", ToolType = "钻头", DesignLife = 1000, LifeUnit = "cycles", CurrentLife = 650, Supplier = "三菱" },
            new() { Code = "T-018", Name = "深孔钻 Φ16 喷液", Model = "BTA-16", ToolType = "钻头", DesignLife = 200, LifeUnit = "cycles", CurrentLife = 90, Supplier = "山特维克" },

            // ═══════════════════════════════════════════════════════════
            // 丝锥/板牙类（2 条）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "T-019", Name = "螺旋槽丝锥 M8×1.25", Model = "HSP-M8×1.25", ToolType = "丝锥", DesignLife = 800, LifeUnit = "cycles", CurrentLife = 200, Supplier = "OSG" },
            new() { Code = "T-020", Name = "板牙 SW12", Model = "SW12", ToolType = "丝锥", DesignLife = 1500, LifeUnit = "cycles", CurrentLife = 500, Supplier = "山特维克" },

            // ═══════════════════════════════════════════════════════════
            // 磨具/砂轮类（4 条）
            // ═══════════════════════════════════════════════════════════
            new() { Code = "T-005", Name = "砂轮 400×40", Model = "SA-40040", ToolType = "磨具", DesignLife = 200, LifeUnit = "hours", CurrentLife = 60, Supplier = "圣戈班" },
            new() { Code = "T-021", Name = "CBN 砂轮 Φ200×20", Model = "CBN-200×20", ToolType = "磨具", DesignLife = 100, LifeUnit = "hours", CurrentLife = 30, Supplier = "圣戈班" },
            new() { Code = "T-022", Name = "树脂砂轮 Φ100×6×32", Model = "RES-100×6×32", ToolType = "磨具", DesignLife = 80, LifeUnit = "hours", CurrentLife = 75, Supplier = "金意" },
            new() { Code = "T-023", Name = "油石 R5 100×25×25", Model = "STL-R5-100", ToolType = "磨具", DesignLife = 50, LifeUnit = "hours", CurrentLife = 10, Supplier = "金意" },
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
        // ═══════════════════════════════════════════════════════════
        // 客户种子数据 — 25 家跨行业制造企业场景
        // ═══════════════════════════════════════════════════════════
        var customers = new List<Customer>
        {
            // ── 第一组：经典制造业客户（CUST-001 ~ CUST-010） ──
            // CUST-001 精密机械
            new() { Code = "CUST-001", Name = "华东精密机械有限公司", Address = "江苏省苏州市工业园区星湖街328号", ContactPerson = "刘建国", ContactPhone = "0512-62881234", Email = "liu@huadong-precision.com", IsActive = true },
            // CUST-002 重工集团
            new() { Code = "CUST-002", Name = "北方重工集团", Address = "辽宁省沈阳市铁西区北三中路15号", ContactPerson = "陈志华", ContactPhone = "024-25872345", Email = "chen@northeast-heavy.com", IsActive = true },
            // CUST-003 汽车零部件
            new() { Code = "CUST-003", Name = "华南汽车零部件有限公司", Address = "广东省广州市黄埔区科学城揽月路3号", ContactPerson = "林志远", ContactPhone = "020-82093456", Email = "lin@huanan-parts.com", IsActive = true },
            // CUST-004 消费电子
            new() { Code = "CUST-004", Name = "深圳星辰电子科技股份有限公司", Address = "广东省深圳市宝安区福海街道新和社区宝安大道4088号", ContactPerson = "赵天明", ContactPhone = "0755-29084567", Email = "zhao@starelectronics.com", IsActive = true },
            // CUST-005 医疗器械
            new() { Code = "CUST-005", Name = "上海明瑞医疗器械有限公司", Address = "上海市嘉定区安亭镇墨玉南路888号", ContactPerson = "周雅琴", ContactPhone = "021-59185678", Email = "zhou@mingrui-medical.com", IsActive = true },
            // CUST-006 航空航天
            new() { Code = "CUST-006", Name = "中航航太精密制造股份有限公司", Address = "陕西省西安市阎良区机场大道1号", ContactPerson = "孙远航", ContactPhone = "029-86896789", Email = "sun@aerospace-tech.com", IsActive = true },
            // CUST-007 家电制造
            new() { Code = "CUST-007", Name = "青岛海尔集团精密零部件公司", Address = "山东省青岛市崂山区海尔路1号", ContactPerson = "吴晓峰", ContactPhone = "0532-88907890", Email = "wu@haier-precision.com", IsActive = true },
            // CUST-008 汽车 Tier-1
            new() { Code = "CUST-008", Name = "宁波博力德汽车系统有限公司", Address = "浙江省宁波市鄞州区工业园区民安路168号", ContactPerson = "黄伟强", ContactPhone = "0574-88018901", Email = "huang@bolid-auto.com", IsActive = true },
            // CUST-009 3C 连接器
            new() { Code = "CUST-009", Name = "东莞联创精密连接器有限公司", Address = "广东省东莞市松山湖高新技术产业开发区科技大道西3号", ContactPerson = "许丽娜", ContactPhone = "0769-89129012", Email = "xu@linkconnect.com", IsActive = true },
            // CUST-010 已停用客户
            new() { Code = "CUST-010", Name = "武汉华中模具有限公司（已终止合作）", Address = "湖北省武汉市东湖高新区关山大道132号", ContactPerson = "杨志华", ContactPhone = "027-59230123", Email = "yang@huazhong-mold.com", IsActive = false },

            // ── 第二组：新兴产业客户（CUST-011 ~ CUST-017） ──
            // CUST-011 半导体芯片封装
            new() { Code = "CUST-011", Name = "合肥长芯半导体封装测试有限公司", Address = "安徽省合肥市经济技术开发区始信路268号", ContactPerson = "马晓东", ContactPhone = "0551-62885678", Email = "ma@changxin-semi.com", IsActive = true },
            // CUST-012 新能源锂电池
            new() { Code = "CUST-012", Name = "厦门海辰新能源科技有限公司", Address = "福建省厦门市海沧区新景路88号", ContactPerson = "沈雪萍", ContactPhone = "0592-68182345", Email = "shen@hithium-energy.com", IsActive = true },
            // CUST-013 生物医药（GMP车间）
            new() { Code = "CUST-013", Name = "苏州百济神州生物制药股份有限公司", Address = "江苏省苏州市吴中区东安路555号", ContactPerson = "顾建平", ContactPhone = "0512-62899876", Email = "gu@bybayopharma.com", IsActive = true },
            // CUST-014 船舶海工
            new() { Code = "CUST-014", Name = "上海江南造船（集团）有限责任公司", Address = "上海市徐汇区龙腾路2555号", ContactPerson = "秦大勇", ContactPhone = "021-34336789", Email = "qin@jiangnanshipyard.com", IsActive = true },
            // CUST-015 食品包装机械
            new() { Code = "CUST-015", Name = "广州达意隆包装机械股份有限公司", Address = "广东省广州市花都区建设北路168号", ContactPerson = "何秀芳", ContactPhone = "020-86881234", Email = "he@delong-pkg.com", IsActive = true },
            // CUST-016 光学仪器
            new() { Code = "CUST-016", Name = "成都光机精密光学器件有限公司", Address = "四川省成都市成都高新区天府大道南段1688号", ContactPerson = "蒋明辉", ContactPhone = "028-85162345", Email = "jiang@optoprime.com", IsActive = true },
            // CUST-017 橡胶密封件
            new() { Code = "CUST-017", Name = "河北旭阳橡胶制品股份有限公司", Address = "河北省邢台市柏乡县旭阳工业新区", ContactPerson = "崔志远", ContactPhone = "0319-8776543", Email = "cui@xuyang-rubber.com", IsActive = true },

            // ── 第三组：细分行业客户（CUST-018 ~ CUST-023） ──
            // CUST-018 纺织服装
            new() { Code = "CUST-018", Name = "浙江报喜鸟服饰集团有限公司", Address = "浙江省温州市永嘉县瓯北街道报喜鸟工业园", ContactPerson = "蔡美惠", ContactPhone = "0577-67331234", Email = "cai@baoxiaoniao.com", IsActive = true },
            // CUST-019 工程机械
            new() { Code = "CUST-019", Name = "三一重工股份有限公司长沙工程机械基地", Address = "湖南省长沙市长沙县三一工业城", ContactPerson = "龙国强", ContactPhone = "0731-84085678", Email = "long@sanyheavy.com", IsActive = true },
            // CUST-020 陶瓷建材
            new() { Code = "CUST-020", Name = "广东东鹏控股股份有限公司", Address = "广东省佛山市禅城区南庄镇紫南工业区", ContactPerson = "谢碧虹", ContactPhone = "0757-82776543", Email = "xie@dongpeng.com", IsActive = true },
            // CUST-021 包装印刷
            new() { Code = "CUST-021", Name = "浙江合众印务集团有限公司", Address = "浙江省嘉兴市南湖区高桥工业园区", ContactPerson = "陶建新", ContactPhone = "0573-82886789", Email = "tao@hezong-print.com", IsActive = true },
            // CUST-022 IVD 体外诊断
            new() { Code = "CUST-022", Name = "深圳迈瑞生物医疗电子股份有限公司", Address = "广东省深圳市南山区高新区南区科技中二路迈瑞大楼", ContactPerson = "潘爱华", ContactPhone = "0755-26585678", Email = "pan@mindray.com", IsActive = true },
            // CUST-023 软件 IT 集成（非制造，纯采购方）
            new() { Code = "CUST-023", Name = "杭州海康威视数字技术股份有限公司", Address = "浙江省杭州市滨江区阡陌路555号", ContactPerson = "韩冰", ContactPhone = "0571-88075999", Email = "han@hikvision.com", IsActive = true },

            // ── 第四组：状态特殊客户（CUST-024 ~ CUST-025） ──
            // CUST-024 待审核新客户
            new() { Code = "CUST-024", Name = "重庆长安汽车股份有限公司（待审）", Address = "重庆市江北区桥北苑8号", ContactPerson = "魏来", ContactPhone = "023-67585678", Email = "wei@changan-auto.com", IsActive = false },
            // CUST-025 已停用客户
            new() { Code = "CUST-025", Name = "北京北方华创微电子设备（已终止合作）", Address = "北京市海淀区北京经济技术开发区宏达北路10号", ContactPerson = "孟繁华", ContactPhone = "010-67893456", Email = "meng@naura-tech.com", IsActive = false },
        };
        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // 获取客户ID（用于后续关联数据）
        var cust001Id = await context.Customers.Where(c => c.Code == "CUST-001").Select(c => c.Id).FirstOrDefaultAsync();
        var cust002Id = await context.Customers.Where(c => c.Code == "CUST-002").Select(c => c.Id).FirstOrDefaultAsync();
        var cust003Id = await context.Customers.Where(c => c.Code == "CUST-003").Select(c => c.Id).FirstOrDefaultAsync();
        var cust004Id = await context.Customers.Where(c => c.Code == "CUST-004").Select(c => c.Id).FirstOrDefaultAsync();
        var cust005Id = await context.Customers.Where(c => c.Code == "CUST-005").Select(c => c.Id).FirstOrDefaultAsync();
        var cust006Id = await context.Customers.Where(c => c.Code == "CUST-006").Select(c => c.Id).FirstOrDefaultAsync();
        var cust007Id = await context.Customers.Where(c => c.Code == "CUST-007").Select(c => c.Id).FirstOrDefaultAsync();
        var cust008Id = await context.Customers.Where(c => c.Code == "CUST-008").Select(c => c.Id).FirstOrDefaultAsync();
        var cust009Id = await context.Customers.Where(c => c.Code == "CUST-009").Select(c => c.Id).FirstOrDefaultAsync();

        #region Complaints（客诉记录 DEMO 数据）
        // ═══════════════════════════════════════════════════════════
        // 客诉种子数据 — 覆盖 8 个活跃客户
        // ═══════════════════════════════════════════════════════════
        var complaints = new List<Models.M09.Complaint>
        {
            // ── CUST-001 华东精密机械：客诉 #001（尺寸超差，已关闭） ──
            new()
            {
                ComplaintCode = "CMP-20260615-001",
                CustomerId = cust001Id,
                Severity = "major",
                Subject = "精密转轴A100批次外径尺寸超差投诉",
                Description = "2026年6月批次交付的精密转轴A100（批号LOT-20260601-001）在客户端装配时发现15件外径超差（实际测量50.06-50.08mm，图纸公差50±0.05mm）。客户反馈已影响其装配进度，要求我方在3个工作日内提供8D报告。",
                Status = "closed",
                FiveW2HJson = "{\"who\":\"华东精密机械-来料检验科\",\"what\":\"外径尺寸超差\",\"where\":\"客户装配线\",\"when\":\"2026-06-15\",\"why\":\"精车工序刀具磨损未及时更换\",\"how_many\":\"15pcs\",\"how\":\"启动8D报告流程\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 6, 18),
                AcknowledgedAt = new DateTime(2026, 6, 15, 10, 30, 0),
                ClosedAt = new DateTime(2026, 6, 18, 16, 0, 0),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 6, 15, 9, 0, 0)
            },
            // ── CUST-001 华东精密机械：客诉 #002（外观缺陷，处理中） ──
            new()
            {
                ComplaintCode = "CMP-20260701-002",
                CustomerId = cust001Id,
                Severity = "minor",
                Subject = "转轴产品包装破损及表面轻微磕碰",
                Description = "2026年7月1日交付批次发现3件产品包装箱破损，其中有2件产品表面存在轻微磕碰痕迹。客户不影响使用，但要求加强包装防护措施。",
                Status = "in_progress",
                FiveW2HJson = "{\"who\":\"华东精密机械-品质部\",\"what\":\"包装破损+表面磕碰\",\"where\":\"入库检验区\",\"when\":\"2026-07-01\",\"why\":\"运输过程中堆叠过高\",\"how_many\":\"3pcs/200pcs\",\"how\":\"调查包装方案并改进\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 7, 5),
                AcknowledgedAt = new DateTime(2026, 7, 1, 14, 0, 0),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 7, 1, 10, 0, 0)
            },

            // ── CUST-002 北方重工集团：客诉 #003（热处理硬度不足，紧急） ──
            new()
            {
                ComplaintCode = "CMP-20260620-003",
                CustomerId = cust002Id,
                Severity = "critical",
                Subject = "壳体B200热处理后硬度不达标批量退货",
                Description = "2026年6月20日批次壳体B200在客户端热处理后硬度测试值50-51HRC，低于图纸要求52-58HRC。客户已整批退货并要求重新生产。经我方调查，系热处理炉炉温偏差超出±10℃范围导致。",
                Status = "acknowledged",
                FiveW2HJson = "{\"who\":\"北方重工集团-来料检验科\",\"what\":\"热处理硬度不足\",\"where\":\"客户热处理车间\",\"when\":\"2026-06-20\",\"why\":\"热处理炉温校准失效\",\"how_many\":\"50pcs整批\",\"how\":\"紧急重新生产并加强炉温监控\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 6, 23),
                AcknowledgedAt = new DateTime(2026, 6, 20, 16, 0, 0),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 6, 20, 10, 0, 0)
            },

            // ── CUST-003 华南汽车零部件：客诉 #004（密封泄漏，主要） ──
            new()
            {
                ComplaintCode = "CMP-20260625-004",
                CustomerId = cust003Id,
                Severity = "major",
                Subject = "密封圈D400密封性测试不合格导致漏油",
                Description = "2026年6月25日批次密封圈D400在客户端装配后出现漏油问题，气密性测试压力0.3MPa保持30秒泄漏。经调查系橡胶原料批次硬度偏差导致弹性不足。",
                Status = "in_progress",
                FiveW2HJson = "{\"who\":\"华南汽车零部件-质量工程师\",\"what\":\"密封圈密封泄漏\",\"where\":\"客户装配线\",\"when\":\"2026-06-25\",\"why\":\"橡胶原料批次硬度偏低\",\"how_many\":\"8pcs/200pcs\",\"how\":\"更换原料批次并追溯已交付产品\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 6, 29),
                AcknowledgedAt = new DateTime(2026, 6, 25, 11, 0, 0),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 6, 25, 8, 0, 0)
            },

            // ── CUST-004 深圳星辰电子：客诉 #005（PCB焊接不良） ──
            new()
            {
                ComplaintCode = "CMP-20260705-005",
                CustomerId = cust004Id,
                Severity = "major",
                Subject = "PCB主板C300虚焊导致功能不良",
                Description = "2026年7月5日批次PCB主板C300在客户端功能测试中有5%的虚焊不良率（主要表现MCU焊点和USB接口虚焊），超出AQL 0.25标准。",
                Status = "new",
                FiveW2HJson = "{\"who\":\"深圳星辰电子-测试工程科\",\"what\":\"PCB虚焊\",\"where\":\"客户端功能测试站\",\"when\":\"2026-07-05\",\"why\":\"SMT贴片温度曲线偏差\",\"how_many\":\"5%不良率\",\"how\":\"调整回流焊温度曲线并全检\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 7, 10),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 7, 5, 9, 0, 0)
            },

            // ── CUST-005 上海明瑞医疗：客诉 #006（标识错误） ──
            new()
            {
                ComplaintCode = "CMP-20260710-006",
                CustomerId = cust005Id,
                Severity = "minor",
                Subject = "产品标签批次号打印错误",
                Description = "2026年7月10日交付批次产品标签上的批次号与实物不符（打印批次为LOT-20260705，实际为LOT-20260706），涉及50件产品。",
                Status = "new",
                FiveW2HJson = "{\"who\":\"上海明瑞医疗-来料检验科\",\"what\":\"标签批次号错误\",\"where\":\"客户仓库\",\"when\":\"2026-07-10\",\"why\":\"标签打印系统批次号配置错误\",\"how_many\":\"50pcs\",\"how\":\"重新打印标签并更换\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 7, 12),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 7, 10, 14, 0, 0)
            },

            // ── CUST-006 中航航太：客诉 #007（螺纹不合格，紧急） ──
            new()
            {
                ComplaintCode = "CMP-20260712-007",
                CustomerId = cust006Id,
                Severity = "critical",
                Subject = "航空用转轴螺纹M10×1.25通规不过",
                Description = "2026年7月12日批次航空用精密转轴螺纹M10×1.25在客户端使用通规检验时通规无法通过。此为航空安全关键部件，客户要求紧急处理。",
                Status = "acknowledged",
                FiveW2HJson = "{\"who\":\"中航航太-质量部\",\"what\":\"螺纹M10×1.25通规不过\",\"where\":\"客户装配线\",\"when\":\"2026-07-12\",\"why\":\"丝锥磨损超差\",\"how_many\":\"20pcs\",\"how\":\"紧急报废并重新加工，提交8D报告\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 7, 14),
                AcknowledgedAt = new DateTime(2026, 7, 12, 10, 0, 0),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 7, 12, 8, 0, 0)
            },

            // ── CUST-007 青岛海尔：客诉 #008（色差） ──
            new()
            {
                ComplaintCode = "CMP-20260715-008",
                CustomerId = cust007Id,
                Severity = "minor",
                Subject = "壳体阳极氧化后颜色与样板存在色差",
                Description = "2026年7月15日批次壳体阳极氧化后颜色与确认样板存在轻微色差，在标准光源下可辨识差异。客户可接受使用但不排除后续风险。",
                Status = "awaiting_verify",
                FiveW2HJson = "{\"who\":\"青岛海尔-外观检验科\",\"what\":\"阳极氧化色差\",\"where\":\"客户总装线\",\"when\":\"2026-07-15\",\"why\":\"阳极氧化槽液浓度波动\",\"how_many\":\"12pcs/300pcs\",\"how\":\"调整氧化工艺参数并确认样板\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 7, 18),
                AcknowledgedAt = new DateTime(2026, 7, 15, 13, 0, 0),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 7, 15, 9, 0, 0)
            },

            // ── CUST-008 宁波博力德：客诉 #009（毛刺） ──
            new()
            {
                ComplaintCode = "CMP-20260718-009",
                CustomerId = cust008Id,
                Severity = "minor",
                Subject = "壳体B200螺栓孔边缘毛刺导致装配困难",
                Description = "2026年7月18日批次壳体B200在客户端装配时发现8个螺栓孔边缘存在毛刺，影响螺栓顺畅插入。",
                Status = "new",
                FiveW2HJson = "{\"who\":\"宁波博力德-装配车间\",\"what\":\"螺栓孔毛刺\",\"where\":\"客户装配线\",\"when\":\"2026-07-18\",\"why\":\"钻孔后去毛刺工序遗漏\",\"how_many\":\"8pcs/200pcs\",\"how\":\"增加去毛刺工序并加强巡检\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 7, 22),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 7, 18, 10, 0, 0)
            },

            // ── CUST-009 东莞联创：客诉 #010（数量短缺） ──
            new()
            {
                ComplaintCode = "CMP-20260720-010",
                CustomerId = cust009Id,
                Severity = "major",
                Subject = "连接线束E500交付数量短缺20根",
                Description = "2026年7月20日交付的连接线束E500订单数量为500根，实收480根，短缺20根。仓库验收时发现包装箱数量与送货单不符。",
                Status = "new",
                FiveW2HJson = "{\"who\":\"东莞联创-仓库\",\"what\":\"交付数量短缺\",\"where\":\"客户仓库收货区\",\"when\":\"2026-07-20\",\"why\":\"包装环节分拣错误\",\"how_many\":\"20pcs短缺\",\"how\":\"补发短缺数量并调查包装流程\"}",
                AssignedTo = 3,
                DueDate = new DateOnly(2026, 7, 23),
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 7, 20, 15, 0, 0)
            },
        };
        context.Complaints.AddRange(complaints);
        await context.SaveChangesAsync();
        #endregion

        #region OqcReleases（OQC 出货放行 DEMO 数据）
        // ═══════════════════════════════════════════════════════════
        // OQC 出货放行种子数据 — 关联客户
        // ═══════════════════════════════════════════════════════════
        var oqcReleases = new List<Models.M05.OqcRelease>
        {
            // ── CUST-001 华东精密机械：出货放行（已完成） ──
            new()
            {
                CustomerId = cust001Id,
                BatchId = 1, // LOT-20260601-001
                ReleaseNumber = "OQC-REL-20260602-001",
                ReleaseDate = new DateTime(2026, 6, 2, 14, 0, 0),
                Quantity = 200,
                AuthorizedBy = 3,
                Status = "released",
                SignatureTime = new DateTime(2026, 6, 2, 15, 30, 0),
                CreatedAt = new DateTime(2026, 6, 2, 14, 0, 0)
            },

            // ── CUST-002 北方重工集团：出货放行（已完成） ──
            new()
            {
                CustomerId = cust002Id,
                BatchId = 2, // LOT-20260601-002
                ReleaseNumber = "OQC-REL-20260602-002",
                ReleaseDate = new DateTime(2026, 6, 2, 15, 0, 0),
                Quantity = 100,
                AuthorizedBy = 3,
                Status = "released",
                SignatureTime = new DateTime(2026, 6, 2, 16, 0, 0),
                CreatedAt = new DateTime(2026, 6, 2, 15, 0, 0)
            },

            // ── CUST-003 华南汽车零部件：出货放行（已授权，待签署） ──
            new()
            {
                CustomerId = cust003Id,
                BatchId = 1, // 复用批次
                ReleaseNumber = "OQC-REL-20260605-003",
                ReleaseDate = new DateTime(2026, 6, 5, 10, 0, 0),
                Quantity = 500,
                AuthorizedBy = 1,
                Status = "signed",
                SignatureTime = new DateTime(2026, 6, 5, 11, 0, 0),
                CreatedAt = new DateTime(2026, 6, 5, 10, 0, 0)
            },

            // ── CUST-004 深圳星辰电子：出货放行（待授权） ──
            new()
            {
                CustomerId = cust004Id,
                BatchId = 2, // 复用批次
                ReleaseNumber = "OQC-REL-20260606-004",
                ReleaseDate = new DateTime(2026, 6, 6, 9, 0, 0),
                Quantity = 1000,
                AuthorizedBy = null,
                Status = "pending",
                CreatedAt = new DateTime(2026, 6, 6, 9, 0, 0)
            },

            // ── CUST-006 中航航太：出货放行（已签署，待发布） ──
            new()
            {
                CustomerId = cust006Id,
                BatchId = 1, // 复用批次
                ReleaseNumber = "OQC-REL-20260710-005",
                ReleaseDate = new DateTime(2026, 7, 10, 11, 0, 0),
                Quantity = 80,
                AuthorizedBy = 3,
                Status = "signed",
                SignatureTime = new DateTime(2026, 7, 10, 12, 0, 0),
                CreatedAt = new DateTime(2026, 7, 10, 11, 0, 0)
            },
        };
        context.OqcReleases.AddRange(oqcReleases);
        await context.SaveChangesAsync();
        #endregion
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
            var ws5 = new Organization { Code = "WS_STAMPING", Name = "冲压车间", Level = "workshop", ParentId = f1.Id, SortOrder = 3 };
            var ws6 = new Organization { Code = "WS_SURFACE", Name = "表面处理车间", Level = "workshop", ParentId = f1.Id, SortOrder = 4 };
            var ws7 = new Organization { Code = "WS_SMT", Name = "电子车间", Level = "workshop", ParentId = f2.Id, SortOrder = 3 };
            context.Organizations.AddRange(ws1, ws2, ws3, ws4, ws5, ws6, ws7);
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
                new() { Code = "LINE_STAMP", Name = "冲压线", Level = "line", ParentId = ws5.Id, SortOrder = 1 },
                new() { Code = "LINE_SURFACE", Name = "表面处理线", Level = "line", ParentId = ws6.Id, SortOrder = 1 },
                new() { Code = "LINE_SMT", Name = "SMT线", Level = "line", ParentId = ws7.Id, SortOrder = 1 },
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
            new() { GroupId = paramGroups[0].Id, Name = "温度-精加工", Code = "temp_finishing", DataType = "numeric", Unit = "℃", TargetValue = 450m, Usl = 455m, Lsl = 445m, Precision = 1, AiStrategy = "{\"id\":\"normal_distribution\"}", SortOrder = 1, CreatedBy = 1 },
            new() { GroupId = paramGroups[0].Id, Name = "温度-热处理", Code = "temp_heat_treat", DataType = "numeric", Unit = "℃", TargetValue = 850m, Usl = 860m, Lsl = 840m, Precision = 1, AiStrategy = "{\"id\":\"trend_analysis\"}", SortOrder = 2, CreatedBy = 1 },
            new() { GroupId = paramGroups[1].Id, Name = "切削压力", Code = "cutting_pressure", DataType = "numeric", Unit = "MPa", TargetValue = 12.5m, Usl = 13.5m, Lsl = 11.5m, Precision = 1, AiStrategy = "{\"id\":\"outlier_detection\"}", SortOrder = 1, CreatedBy = 1 },
            new() { GroupId = paramGroups[1].Id, Name = "主轴扭矩", Code = "spindle_torque", DataType = "numeric", Unit = "N·m", TargetValue = 25m, Usl = 28m, Lsl = 22m, Precision = 1, SortOrder = 2, CreatedBy = 1 },
            new() { GroupId = paramGroups[2].Id, Name = "外径公差", Code = "od_tolerance", DataType = "numeric", Unit = "mm", TargetValue = 50m, Usl = 50.05m, Lsl = 49.95m, Precision = 2, AiStrategy = "{\"id\":\"cpk_monitoring\"}", SortOrder = 1, CreatedBy = 1 },
            new() { GroupId = paramGroups[2].Id, Name = "内径公差", Code = "id_tolerance", DataType = "numeric", Unit = "mm", TargetValue = 25m, Usl = 25.03m, Lsl = 24.97m, Precision = 2, SortOrder = 2, CreatedBy = 1 },
            new() { GroupId = paramGroups[3].Id, Name = "表面粗糙度", Code = "surface_roughness", DataType = "numeric", Unit = "μm", TargetValue = 0.8m, Usl = 1.6m, Lsl = 0m, Precision = 1, AiStrategy = "{\"id\":\"normal_distribution\"}", SortOrder = 1, CreatedBy = 1 },
            new() { GroupId = paramGroups[3].Id, Name = "表面缺陷", Code = "surface_defect", DataType = "categorical", Unit = "", Precision = 1, AiStrategy = "{\"id\":\"pareto_analysis\"}", SortOrder = 2, CreatedBy = 1 },
            new() { GroupId = paramGroups[3].Id, Name = "防锈处理", Code = "rust_prevention", DataType = "boolean", Precision = 1, SortOrder = 3, CreatedBy = 1 },
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

        #region IPQC 过程检验种子数据
        try
        {
            var ipqcProcess = await context.Processes.Where(p => p.Name == "过程检验").FirstOrDefaultAsync();
            var firstPieceProcess = await context.Processes.Where(p => p.Name == "来料检验").FirstOrDefaultAsync();
            var assemblyProcess = await context.Processes.Where(p => p.Name == "精车").FirstOrDefaultAsync();
            var weldingProcess = await context.Processes.Where(p => p.Name == "钻孔").FirstOrDefaultAsync();
            var packagingProcess = await context.Processes.Where(p => p.Name == "清洗包装").FirstOrDefaultAsync();

            var equipmentInj = await context.Equipment.Where(e => e.Code == "INJ-001").FirstOrDefaultAsync();
            var equipmentAssy = await context.Equipment.Where(e => e.Code == "ROBOT-INS-01").FirstOrDefaultAsync();
            var equipmentWeld = await context.Equipment.Where(e => e.Code == "ROBOT-WLD-01").FirstOrDefaultAsync();
            var equipmentPkg = await context.Equipment.Where(e => e.Code == "PKG-001").FirstOrDefaultAsync();

            if (ipqcProcess == null || firstPieceProcess == null || assemblyProcess == null)
            {
                Console.WriteLine("[DbInitializer] IPQC 种子数据跳过（工序缺失）");
            }
            else
            {
                var now = DateTime.UtcNow;

                // 如果已存在 IPQC 检验项目则跳过，否则创建
                var existingInspectionItems = await context.InspectionItems
                    .Where(i => i.ItemCode.StartsWith("IPQC-"))
                    .ToListAsync();
                List<InspectionItem>? ipqcInspectionItems;
                if (existingInspectionItems.Count > 0)
                {
                    Console.WriteLine("[DbInitializer] IPQC 检验项目已存在，跳过");
                    ipqcInspectionItems = existingInspectionItems;
                }
                else
                {
                    ipqcInspectionItems = new List<InspectionItem>
                    {
                        new() { ItemCode = "IPQC-FP-001", ItemName = "外观检查", Description = "首件检验外观检查", DataType = "visual", InspectionMethod = "目视检查", SampleSize = 5 },
                        new() { ItemCode = "IPQC-FP-002", ItemName = "尺寸测量", Description = "关键尺寸测量，公差 ±0.05mm", DataType = "numeric", Unit = "mm", Usl = 50.05m, Lsl = 49.95m, TargetValue = 50.00m, InspectionMethod = "千分尺", SampleSize = 5 },
                        new() { ItemCode = "IPQC-FP-003", ItemName = "功能测试", Description = "产品功能测试", DataType = "attribute", InspectionMethod = "专用测试治具", SampleSize = 3 },
                        new() { ItemCode = "IPQC-PL-001", ItemName = "设备点检", Description = "设备运行状态点检", DataType = "visual", InspectionMethod = "目视 + 听诊", SampleSize = 1 },
                        new() { ItemCode = "IPQC-PL-002", ItemName = "工艺参数核对", Description = "核对设备参数与工艺卡片一致性", DataType = "visual", InspectionMethod = "目视核对", SampleSize = 1 },
                        new() { ItemCode = "IPQC-PL-003", ItemName = "首件样品确认", Description = "确认首件样品与标准样品一致", DataType = "visual", InspectionMethod = "目视比对", SampleSize = 1 },
                    };
                    context.InspectionItems.AddRange(ipqcInspectionItems);
                    await context.SaveChangesAsync();
                }

                // 如果已存在首件检验数据则跳过，否则创建
                var existingFirstPieces = await context.IpqcFirstPieces.CountAsync();
                if (existingFirstPieces > 0)
                {
                    Console.WriteLine("[DbInitializer] IPQC 首件检验已存在，跳过");
                }
                else
                {
                    var firstPieces = new List<IpqcFirstPiece>
                    {
                        new() { FpNo = "FP-20260729-001", WorkOrderId = 1001, ProcessId = firstPieceProcess.Id, EquipmentId = equipmentInj?.Id ?? 0, OperatorId = 1, Shift = "早班", Reason = "开机首件", Conclusion = "qualified", AllowedToProduce = true, InspectorId = 3, CheckedAt = now - TimeSpan.FromHours(2), CreatedAt = now - TimeSpan.FromHours(24), UpdatedAt = now },
                        new() { FpNo = "FP-20260729-002", WorkOrderId = 1002, ProcessId = assemblyProcess.Id, EquipmentId = equipmentAssy?.Id ?? 0, OperatorId = 2, Shift = "早班", Reason = "班次切换", Conclusion = "pending", AllowedToProduce = false, InspectorId = null, CheckedAt = null, CreatedAt = now - TimeSpan.FromHours(1), UpdatedAt = now },
                        new() { FpNo = "FP-20260729-003", WorkOrderId = 1003, ProcessId = weldingProcess.Id, EquipmentId = equipmentWeld?.Id ?? 0, OperatorId = 3, Shift = "中班", Reason = "换模后首件", Conclusion = "unqualified", AllowedToProduce = false, InspectorId = 3, CheckedAt = now - TimeSpan.FromHours(4), CreatedAt = now - TimeSpan.FromHours(48), UpdatedAt = now },
                        new() { FpNo = "FP-20260728-004", WorkOrderId = 1004, ProcessId = firstPieceProcess.Id, EquipmentId = equipmentInj?.Id ?? 0, OperatorId = 1, Shift = "晚班", Reason = "开机首件", Conclusion = "qualified", AllowedToProduce = true, InspectorId = 2, CheckedAt = now - TimeSpan.FromHours(72), CreatedAt = now - TimeSpan.FromHours(96), UpdatedAt = now },
                        new() { FpNo = "FP-20260728-005", WorkOrderId = 1005, ProcessId = packagingProcess.Id, EquipmentId = equipmentPkg?.Id ?? 0, OperatorId = 4, Shift = "早班", Reason = "维修后首件", Conclusion = "qualified", AllowedToProduce = true, InspectorId = 3, CheckedAt = now - TimeSpan.FromHours(70), CreatedAt = now - TimeSpan.FromHours(94), UpdatedAt = now },
                    };
                    context.IpqcFirstPieces.AddRange(firstPieces);
                    await context.SaveChangesAsync();

                    var firstPieceItems = new List<IpqcFirstPieceItem>();
                    foreach (var fp in firstPieces)
                    {
                        if (fp.Conclusion == "qualified")
                        {
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[0].Id, ItemName = "外观检查", DataType = "visual", Result = "pass", ActualValue = null });
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[1].Id, ItemName = "尺寸测量", DataType = "numeric", Usl = 50.05m, Lsl = 49.95m, Result = "pass", ActualValue = 50.02m });
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[2].Id, ItemName = "功能测试", DataType = "attribute", Result = "pass", ActualValue = null });
                        }
                        else if (fp.Conclusion == "pending")
                        {
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[0].Id, ItemName = "外观检查", DataType = "visual", Result = "pending", ActualValue = null });
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[1].Id, ItemName = "尺寸测量", DataType = "numeric", Usl = 50.05m, Lsl = 49.95m, Result = "pending", ActualValue = null });
                        }
                        else
                        {
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[0].Id, ItemName = "外观检查", DataType = "visual", Result = "pass", ActualValue = null });
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[1].Id, ItemName = "尺寸测量", DataType = "numeric", Usl = 50.05m, Lsl = 49.95m, Result = "fail", ActualValue = 50.08m });
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[2].Id, ItemName = "功能测试", DataType = "attribute", Result = "pass", ActualValue = null });
                            firstPieceItems.Add(new IpqcFirstPieceItem { FirstPieceId = fp.Id, InspectionItemId = ipqcInspectionItems[3].Id, ItemName = "设备点检", DataType = "visual", Result = "pass", ActualValue = null });
                        }
                    }
                    context.IpqcFirstPieceItems.AddRange(firstPieceItems);
                    await context.SaveChangesAsync();
                }

                // 如果已存在巡检计划数据则跳过，否则创建
                var existingPatrolPlans = await context.IpqcPatrolPlans.CountAsync();
                if (existingPatrolPlans > 0)
                {
                    Console.WriteLine("[DbInitializer] IPQC 巡检计划已存在，跳过");
                }
                else
                {
                    var patrolPlans = new List<IpqcPatrolPlan>
                    {
                        new() { PlanNo = "PL-20260729-001", ProcessId = firstPieceProcess.Id, EquipmentId = equipmentInj?.Id ?? 0, PatrolIntervalMin = 120, AutoGenerate = true, Status = "active", Inspector = "李四", CreatedAt = now, UpdatedAt = now },
                        new() { PlanNo = "PL-20260729-002", ProcessId = assemblyProcess.Id, EquipmentId = equipmentAssy?.Id ?? 0, PatrolIntervalMin = 60, AutoGenerate = true, Status = "active", Inspector = "王五", CreatedAt = now, UpdatedAt = now },
                        new() { PlanNo = "PL-20260728-003", ProcessId = weldingProcess.Id, EquipmentId = equipmentWeld?.Id ?? 0, PatrolIntervalMin = 90, AutoGenerate = false, Status = "paused", Inspector = null, CreatedAt = now, UpdatedAt = now },
                        new() { PlanNo = "PL-20260728-004", ProcessId = packagingProcess.Id, EquipmentId = equipmentPkg?.Id ?? 0, PatrolIntervalMin = 180, AutoGenerate = true, Status = "active", Inspector = "李四", CreatedAt = now, UpdatedAt = now },
                    };
                    context.IpqcPatrolPlans.AddRange(patrolPlans);
                    await context.SaveChangesAsync();

                    var patrols = new List<IpqcPatrol>
                    {
                        new() { PatrolNo = "PTL-20260729-001", PatrolPlanId = patrolPlans[0].Id, ProcessId = firstPieceProcess.Id, EquipmentId = equipmentInj?.Id ?? 0, InspectorId = 3, ScheduledTime = now - TimeSpan.FromHours(1), ActualTime = now - TimeSpan.FromMinutes(50), TotalChecked = 3, TotalPass = 3, TotalFail = 0, Conclusion = "qualified", Status = "completed", Remarks = "", CreatedAt = now, UpdatedAt = now },
                        new() { PatrolNo = "PTL-20260729-002", PatrolPlanId = patrolPlans[1].Id, ProcessId = assemblyProcess.Id, EquipmentId = equipmentAssy?.Id ?? 0, InspectorId = 2, ScheduledTime = now - TimeSpan.FromMinutes(30), ActualTime = null, TotalChecked = 0, TotalPass = 0, TotalFail = 0, Conclusion = "pending", Status = "scheduled", Remarks = "", CreatedAt = now, UpdatedAt = now },
                        new() { PatrolNo = "PTL-20260729-003", PatrolPlanId = patrolPlans[0].Id, ProcessId = firstPieceProcess.Id, EquipmentId = equipmentInj?.Id ?? 0, InspectorId = 3, ScheduledTime = now - TimeSpan.FromHours(3), ActualTime = now - TimeSpan.FromHours(2.5f), TotalChecked = 3, TotalPass = 2, TotalFail = 1, Conclusion = "unqualified", Status = "completed", Remarks = "尺寸测量超差", CreatedAt = now, UpdatedAt = now },
                        new() { PatrolNo = "PTL-20260728-004", PatrolPlanId = patrolPlans[3].Id, ProcessId = packagingProcess.Id, EquipmentId = equipmentPkg?.Id ?? 0, InspectorId = 3, ScheduledTime = now - TimeSpan.FromHours(24), ActualTime = null, TotalChecked = 0, TotalPass = 0, TotalFail = 0, Conclusion = "pending", Status = "missed", Remarks = "检验员请假", CreatedAt = now, UpdatedAt = now },
                        new() { PatrolNo = "PTL-20260728-005", PatrolPlanId = patrolPlans[2].Id, ProcessId = weldingProcess.Id, EquipmentId = equipmentWeld?.Id ?? 0, InspectorId = 1, ScheduledTime = now - TimeSpan.FromHours(25), ActualTime = now - TimeSpan.FromHours(24.5f), TotalChecked = 2, TotalPass = 2, TotalFail = 0, Conclusion = "qualified", Status = "completed", Remarks = "", CreatedAt = now, UpdatedAt = now },
                    };
                    context.IpqcPatrols.AddRange(patrols);
                    await context.SaveChangesAsync();

                    var patrolItems = new List<IpqcPatrolItem>();
                    foreach (var patrol in patrols)
                    {
                        if (patrol.Conclusion == "qualified")
                        {
                            patrolItems.Add(new IpqcPatrolItem { PatrolId = patrol.Id, InspectionItemId = ipqcInspectionItems[0].Id, ItemName = "外观检查", DataType = "visual", Result = "pass", ActualValue = null });
                            patrolItems.Add(new IpqcPatrolItem { PatrolId = patrol.Id, InspectionItemId = ipqcInspectionItems[3].Id, ItemName = "设备点检", DataType = "visual", Result = "pass", ActualValue = null });
                            patrolItems.Add(new IpqcPatrolItem { PatrolId = patrol.Id, InspectionItemId = ipqcInspectionItems[1].Id, ItemName = "尺寸测量", DataType = "numeric", Usl = 50.05m, Lsl = 49.95m, Result = "pass", ActualValue = 50.01m });
                        }
                        else if (patrol.Conclusion == "unqualified")
                        {
                            patrolItems.Add(new IpqcPatrolItem { PatrolId = patrol.Id, InspectionItemId = ipqcInspectionItems[0].Id, ItemName = "外观检查", DataType = "visual", Result = "pass", ActualValue = null });
                            patrolItems.Add(new IpqcPatrolItem { PatrolId = patrol.Id, InspectionItemId = ipqcInspectionItems[1].Id, ItemName = "尺寸测量", DataType = "numeric", Usl = 50.05m, Lsl = 49.95m, Result = "fail", ActualValue = 50.08m });
                            patrolItems.Add(new IpqcPatrolItem { PatrolId = patrol.Id, InspectionItemId = ipqcInspectionItems[3].Id, ItemName = "设备点检", DataType = "visual", Result = "pass", ActualValue = null });
                        }
                    }
                    context.IpqcPatrolItems.AddRange(patrolItems);
                    await context.SaveChangesAsync();
                }

                // 如果已存在 AI 风险评分数据则跳过，否则创建
                var existingRiskScores = await context.IpqcAiRiskScores.CountAsync();
                if (existingRiskScores > 0)
                {
                    Console.WriteLine("[DbInitializer] IPQC AI 风险评分已存在，跳过");
                }
                else
                {
                    var riskScores = new List<IpqcAiRiskScore>
                    {
                        new() { EquipmentId = equipmentInj?.Id ?? 0, ProcessId = firstPieceProcess.Id, RiskScore = 45, RiskLevel = "normal", TrendDirection = "falling", FactorsJson = System.Text.Json.JsonSerializer.Serialize(new[] { new { Name = "尺寸偏差", Description = "关键尺寸超出控制限", CurrentValue = 0.15m, TargetValue = 0.10m, Impact = 12 }, new { Name = "温度波动", Description = "设备温度稳定性下降", CurrentValue = 2.5m, TargetValue = 1.0m, Impact = 8 }, new { Name = "设备参数", Description = "设备参数偏离标准范围", CurrentValue = 0.05m, TargetValue = 0.02m, Impact = 5 } }), CreatedAt = now },
                        new() { EquipmentId = equipmentAssy?.Id ?? 0, ProcessId = assemblyProcess.Id, RiskScore = 32, RiskLevel = "normal", TrendDirection = "stable", FactorsJson = System.Text.Json.JsonSerializer.Serialize(new[] { new { Name = "装配间隙", Description = "装配间隙在公差范围内", CurrentValue = 0.08m, TargetValue = 0.10m, Impact = 3 } }), CreatedAt = now },
                    };
                    context.IpqcAiRiskScores.AddRange(riskScores);
                    await context.SaveChangesAsync();
                    Console.WriteLine("[DbInitializer] IPQC AI 风险评分已创建");
                }

                Console.WriteLine("[DbInitializer] IPQC 种子数据初始化完成");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] IPQC 种子数据初始化失败（不阻断启动）: {ex.Message}");
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
