namespace QM_AI.API.Authorization;

/// <summary>
/// 权限点目录 — 27 个权限码，覆盖全部模块
/// </summary>
public static class PermissionCatalog
{
    public static readonly (string Code, string Name, string Module)[] All =
    [
        ("*:*",              "全部权限",     "System"),
        ("basic:data",       "基础数据管理", "M02"),
        ("iqc:inspect",      "IQC 检验",    "M03"),
        ("ipqc:inspect",     "IPQC 检验",   "M04"),
        ("fqc:inspect",      "FQC 检验",    "M05"),
        ("spc:view",         "SPC 查看",    "M06"),
        ("defect:manage",    "不良管理",    "M07"),
        ("trace:view",       "质量追溯",    "M08"),
        ("complaint:manage", "客诉管理",    "M09"),
        ("ai:analyze",       "AI 分析",     "M10"),
        ("equipment:manage", "设备管理",    "M11"),
        ("document:manage",  "文件管理",    "M12"),
        ("audit:manage",     "审核稽核",    "M13"),
        ("report:view",      "报表查看",    "M14"),
        ("report:export",    "报表导出",    "M14"),
        ("system:user",      "用户管理",    "M15"),
        ("system:role",      "角色管理",    "M15"),
        ("system:permission","权限管理",    "M15"),
        ("system:org",       "组织管理",    "M15"),
        ("system:dict",      "字典管理",    "M15"),
        ("system:log",       "日志管理",    "M15"),
        ("workflow:approve", "工作流审批",  "WF"),
        ("workflow:manage",  "工作流管理",  "WF"),
        ("form:view",        "表单查看",    "FORM"),
        ("form:fill",        "表单填报",    "FORM"),
        ("form:manage",      "表单管理",    "FORM"),
        ("export:manage",    "导出中心",    "EXP"),
    ];

    public static readonly string[] Codes = All.Select(p => p.Code).ToArray();
}
