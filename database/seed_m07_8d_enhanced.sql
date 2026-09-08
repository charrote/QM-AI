-- ============================================================================
-- M07 – 不良与异常管理 补充模拟数据（8D 报告增强版）
-- 运行方式：docker exec -i qm-ai-mysql mysql -u root -p'qmai_root_2024' qmai < database/seed_m07_8d_enhanced.sql
-- 说明：基于现有 seed_m07_sample_data.sql，补充更多缺陷、CAPA 及子表数据
-- ============================================================================

-- ============================================================
-- Part 1: 补充产品数据（新增）
-- ============================================================

INSERT IGNORE INTO products (id, product_code, product_name, description, product_type) VALUES
(211, 'RES-H800',  '精密电阻 H800',  '0.1% 精度薄膜电阻', '元器件'),
(213, 'CAP-I900',  '陶瓷电容 I900',  'X7R 高稳定性陶瓷电容 10uF', '元器件'),
(214, 'SWT-J000',  '继电器 J000',    '小型信号继电器 5VDC', '电子元器件'),
(215, 'CONN-K100', 'SMA 连接器 K100', 'SMA 射频同轴连接器', '连接器');

-- ============================================================
-- Part 2: 补充缺陷记录（新增 15 条）
-- ============================================================

INSERT IGNORE INTO defects (id, defect_no, severity, source_type, source_ref_id, product_id, batch_id, equipment_id, quantity, description, image_urls, discovered_by, discovered_at, status) VALUES
(9,  'DEF-2026-009',  'major',   'IQC',            402, 214, 308, 0, 20, '继电器触点镀银层厚度不足（实测 0.8μm，标准要求 ≥2μm），可能导致接触电阻增大', NULL, 2, '2026-07-20 09:15:00', 'investigating'),
(10, 'DEF-2026-010', 'minor',   'IPQC-PATROL',    304, 201, 309, 11, 2, '波峰焊锡炉温度波动±5°C（设定 255°C，实测 250-260°C），在工艺窗口内但接近上限', NULL, 4, '2026-07-19 11:30:00', 'closed'),
(11, 'DEF-2026-011', 'critical','FQC',            502, 207, 310, 0, 15, '温湿度传感器成品功能测试 Fail Rate 7.5%（标准 ≤1%），主要表现为温度漂移超标（±3°C vs 要求 ±1°C）', NULL, 3, '2026-07-18 15:00:00', 'open'),
(12, 'DEF-2026-012', 'critical','CUSTOMER',        0, 207,   0, 0, 30, '客户反馈 30 pcs 传感器在 2 周内出现温度漂移，批量性问题，涉及 3 个不同生产批次', NULL, 5, '2026-07-17 08:00:00', 'open'),
(13, 'DEF-2026-013', 'major',   'PRODUCTION',      0, 201, 311, 13, 5, '贴片机 Feeder 供料器 #7 频繁报警掉料，已停机 2 小时，影响当班产能约 200 pcs', NULL, 6, '2026-07-16 14:30:00', 'in_progress'),
(14, 'DEF-2026-014', 'minor',   'IQC',            403, 211, 312, 0, 500, '精密电阻包装纸箱受潮变形，部分卷带边缘有轻微水渍，需评估是否影响产品性能', NULL, 2, '2026-07-15 10:20:00', 'resolved'),
(15, 'DEF-2026-015', 'critical','IPQC-FIRSTPIECE', 602, 205, 313, 0, 1, '首件检测发现电解电容极性方向贴反（负极朝上，应为朝下），已立即停机', NULL, 4, '2026-07-14 07:45:00', 'closed'),
(16, 'DEF-2026-016', 'minor',   'FQC',            503, 210, 314, 0, 6, '成品连接器外观有指纹油污残留，占比约 3%，需增加清洗工序或改善操作规范', NULL, 3, '2026-07-13 16:10:00', 'in_progress'),
(17, 'DEF-2026-017', 'major',   'PRODUCTION',      0, 207, 315, 14, 8, '老化测试（85°C/85%RH/48h）中 8 pcs 传感器输出信号衰减超过 5%，超出规格要求', NULL, 6, '2026-07-12 11:00:00', 'investigating'),
(18, 'DEF-2026-018', 'major',   'IQC',            404, 215, 316, 0, 10, 'SMA 连接器外径尺寸超差（实测 3.98mm，标准 4.00±0.02mm），影响与面板安装的配合', NULL, 2, '2026-07-11 09:30:00', 'open'),
(19, 'DEF-2026-019', 'minor',   'IPQC-PATROL',    305, 201, 317, 10, 1, '锡膏印刷后检测（SPI）显示 2 个焊盘锡量偏低（85% vs 标准 100%±10%），暂时在允收范围内', NULL, 4, '2026-07-10 13:20:00', 'closed'),
(20, 'DEF-2026-020', 'major',   'CUSTOMER',        0, 210,   0, 0, 3, '客户收到 3 pcs 连接器缺少合格证和包装清单，虽不影响产品功能但违反出货规范', NULL, 5, '2026-07-09 10:00:00', 'in_progress'),
(21, 'DEF-2026-021', 'minor',   'FQC',            504, 212, 318, 0, 100, 'LED 指示灯批次标签上生产日期印刷错误（实际 20260708，标签印 20260709），需追溯已出货批次', NULL, 3, '2026-07-08 15:45:00', 'in_progress'),
(22, 'DEF-2026-022', 'critical','IQC',            405, 209, 319, 0, 100, '到货物料与采购订单不一致，订购 E500（LDO 5V），实际收到 E501（LDO 3.3V），可能影响后续装配', NULL, 2, '2026-07-07 08:15:00', 'investigating'),
(23, 'DEF-2026-023', 'minor',   'PRODUCTION',      0, 201, 320, 15, 0, 'AOI 自动光学检测报警率升至 12%（正常 <5%），误报率约 60%，需调整检测参数', NULL, 6, '2026-07-06 10:30:00', 'closed'),
(24, 'DEF-2026-024', 'major',   'IPQC-PATROL',    306, 210, 321, 10, 4, '防水连接器组装线 4 个工位扭矩数据离散度大（CV=18%），标准要求 CV<10%，需排查治具和人员操作', NULL, 4, '2026-07-05 14:00:00', 'in_progress');

-- ============================================================
-- Part 3: 补充 CAPA 记录（新增 8 条）
-- ============================================================

INSERT IGNORE INTO capa (id, capa_no, defect_id, severity, title, description, current_phase, status, created_by, assigned_to, due_date, created_at) VALUES
(106, 'CAPA-2026-006', 12, 'critical','传感器温度漂移批量问题 8D 改善', '客户反馈 30 pcs 传感器 2 周内温度漂移超标，需追溯传感器核心元件和封装工艺', 4, 'in_progress', 5, 10, '2026-08-12', '2026-07-18 09:00:00'),
(107, 'CAPA-2026-007', 9,  'major',   '继电器触点镀银层厚度不足改善', '供应商来料继电器触点镀银层厚度不达标，需推动供应商改进电镀工艺', 3, 'in_progress', 2, 12, '2026-08-08', '2026-07-21 10:00:00'),
(108, 'CAPA-2026-008', 17, 'major',   '传感器高温高湿老化失效根因分析', '85°C/85%RH 老化测试中信号衰减超标，怀疑封装胶材耐湿热性能不足', 2, 'in_progress', 6, 8, '2026-08-18', '2026-07-13 09:00:00'),
(109, 'CAPA-2026-009', 20, 'major',   '出货包装漏发合格证及清单改善', '客户反馈多批次缺少合格证和包装清单，需建立出货前核对机制', 1, 'open', 5, 10, '2026-08-03', '2026-07-10 11:00:00'),
(110, 'CAPA-2026-010', 21, 'minor',   '批次标签生产日期印刷错误改善', '标签生产日期印刷与实际不符，需建立标签打印双重确认流程', 3, 'in_progress', 3, 4, '2026-08-01', '2026-07-09 08:30:00'),
(111, 'CAPA-2026-011', 22, 'critical','IQC 来料与订单不符物料混料处理', '供应商发错物料（E501 替代 E500），需紧急处置并确保追溯性', 4, 'in_progress', 2, 12, '2026-08-06', '2026-07-08 09:30:00'),
(112, 'CAPA-2026-012', 16, 'minor',   '成品连接器外观油污改善', 'FQC 检出连接器表面指纹油污，占比 3%，需改善操作人员卫生管理', 5, 'in_progress', 3, 9, '2026-07-28', '2026-07-14 10:00:00'),
(113, 'CAPA-2026-013', 24, 'major',   '防水连接器组装线扭矩离散度改善', '4 个工位扭矩 CV=18% 远超标准 10%，需系统排查治具、工具和操作方法', 2, 'open', 4, 7, '2026-08-22', '2026-07-06 09:00:00');

-- ============================================================
-- Part 4: 补充临时措施数据
-- ============================================================

INSERT IGNORE INTO capa_temporary_measures (id, capa_id, description, executed_by, executed_at) VALUES
(6,  106, '立即冻结库存批次 310、315，禁止出货；对已出货 30 pcs 启动召回评估', 10, '2026-07-18 10:00:00'),
(7,  106, '通知客户暂停使用该批次产品，提供备用样品', 5, '2026-07-18 11:00:00'),
(8,  107, '对已入库 20 pcs 继电器全检镀银层厚度（XRF 检测）', 12, '2026-07-21 11:00:00'),
(9,  108, '批次 315 老化测试延长至 72h，追加 10 pcs 样品测试', 8, '2026-07-13 10:00:00'),
(10, 109, '对最近 3 个月出货记录进行全面核查，补发缺失文件', 10, '2026-07-10 12:00:00'),
(11, 111, '隔离批次 319 全部物料，通知采购联系供应商换货', 12, '2026-07-08 10:00:00'),
(12, 111, '紧急调用安全库存 E500 保障生产线不停线', 2, '2026-07-08 10:30:00'),
(13, 112, '增加 1 道异丙醇擦拭清洁工序，每 2 小时抽检', 9, '2026-07-14 11:00:00'),
(14, 113, '对已组装成品加严扭矩复检（100% 全检）', 7, '2026-07-06 10:00:00');

-- ============================================================
-- Part 5: 补充根本原因分析数据
-- ============================================================

-- CAPA 002 纠正措施（之前缺失）
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(6,  102, '重新标定喷涂设备压力参数，建立参数锁定机制', 8, '2026-08-05', 'completed', '2026-08-04 16:00:00'),
(7,  102, '油漆批次更换时必须执行首件确认和小批量试喷', 3, '2026-08-08', 'completed', '2026-08-07 17:00:00');

-- CAPA 002 预防措施（之前缺失）
INSERT IGNORE INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(4,  102, '引入色差自动在线检测系统，每 50 pcs 自动检测', 8, '2026-09-01', 'pending', NULL);

-- CAPA 002 效果验证（之前缺失）
INSERT IGNORE INTO capa_verifications (id, capa_id, verifier_id, verification_date, conclusion, evidence) VALUES
(2,  102, 3, '2026-08-10 00:00:00', 'effective', '连续 3 批喷涂色差 ΔE<1.5，全部合格（检验报告 #FQC-2026-088）');

-- CAPA 004 纠正措施（之前缺失）
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(8,  104, '排查密封结构，增加 O 型圈截面尺寸（从 2.0mm 改为 2.4mm）', 9, '2026-08-12', 'in_progress', NULL);

-- CAPA 006 根本原因分析
INSERT IGNORE INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(5,  106, 'five_whys', '["1.温度漂移","2.传感元件封装胶材吸湿","3.封装胶材更换为低成本型号","4.变更未经充分可靠性验证","5.工程变更管理流程不完善，缺少可靠性验证要求"]', '封装胶材变更未经充分可靠性验证，低成本胶材耐湿热性能不足', 10);

-- CAPA 006 纠正措施
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(9,  106, '恢复使用原型号封装胶材（A 型号），暂停 B 型号使用', 10, '2026-08-02', 'completed', '2026-08-01 16:00:00'),
(10, 106, '对在用 B 型号胶材进行全面可靠性评估（含 HTS/LTHS 测试）', 8, '2026-08-15', 'in_progress', NULL);

-- CAPA 006 预防措施
INSERT IGNORE INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(5,  106, '修订工程变更管理程序，将可靠性验证列为关键物料变更的强制步骤', 10, '2026-08-25', 'pending', NULL),
(6,  106, '建立关键物料替代料清单和验证数据库', 8, '2026-09-10', 'pending', NULL);

-- CAPA 007 根本原因分析
INSERT IGNORE INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(7,  107, 'five_whys', '["1.镀银层厚度不足","2.供应商电镀时间缩短","3.供应商为降低成本自行调整工艺","4.供应商变更管理缺失，未通知客户","5.供应商评审未覆盖电镀工艺过程"]', '供应商为降低成本自行缩短电镀时间，变更管理缺失', 12);

-- CAPA 007 纠正措施
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(11, 107, '要求供应商恢复标准电镀工艺参数，电镀时间 ≥45min', 12, '2026-08-05', 'in_progress', NULL),
(12, 107, '对供应商电镀过程进行专项审核', 2, '2026-08-10', 'pending', NULL);

-- CAPA 008 根本原因分析
INSERT IGNORE INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(8,  108, 'fishbone', '{"人":"老化测试参数设置正确","机":"老化箱温度均匀性合格（±1.5°C）","料":"封装胶材批次间性能波动，玻璃化转变温度偏低","法":"来料验证仅做外观和尺寸检查，未做胶材可靠性抽检"}', '封装胶材批次间性能波动，且 IQC 未对胶材做可靠性抽样验证', 8);

-- CAPA 008 纠正措施
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(13, 108, '筛选在役胶材批次，TDS 中玻璃化转变温度 Tg>150°C 方可入库', 8, '2026-08-12', 'in_progress', NULL);

-- CAPA 008 预防措施
INSERT IGNORE INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(7,  108, '修订 IQC 检验标准，增加胶材 Tg 值抽检（DSC 法，每批次 1 pcs）', 8, '2026-08-20', 'pending', NULL),
(8,  108, '每季度对封装胶材供应商做 1 次过程审核', 2, '2026-09-30', 'pending', NULL);

-- CAPA 009 根本原因分析
INSERT IGNORE INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(9,  109, 'five_whys', '["1.缺少合格证和清单","2.包装员未执行出货前核对清单","3.包装工位无标准化核对流程","4.包装 SOP 中未定义文件装配步骤","5.文件装配依赖人工记忆，无防错机制"]', '包装工位缺少标准化文件装配流程，无防错机制依赖人工记忆', 10);

-- CAPA 009 纠正措施
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(14, 109, '在包装 SOP 中增加文件装配步骤和核对清单', 10, '2026-07-28', 'completed', '2026-07-27 16:00:00');

-- CAPA 009 预防措施
INSERT IGNORE INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(9,  109, '出货前增加 QA 最终抽检，随机抽查 10% 订单核对文件完整性', 3, '2026-08-05', 'pending', NULL),
(10, 109, '在出货包装区安装扫码系统，系统自动校验文件装配状态', 10, '2026-09-01', 'pending', NULL);

-- CAPA 010 根本原因分析
INSERT IGNORE INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(10, 110, 'five_whys', '["1.标签日期错误","2.标签打印时未核对生产报工日期","3.打印操作员自行输入日期","4.标签打印流程中无日期自动获取机制","5.标签打印机与 MES 系统未联网"]', '标签打印机与 MES 系统未联网，日期依赖手动输入，易出错', 4);

-- CAPA 010 纠正措施
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(15, 110, '标签日期改为从 MES 系统自动获取，取消手动输入', 4, '2026-07-28', 'completed', '2026-07-26 17:00:00');

-- CAPA 010 预防措施
INSERT IGNORE INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(11, 110, '标签打印后增加二次确认环节（班组长复核）', 4, '2026-07-30', 'completed', '2026-07-29 16:00:00');

-- CAPA 011 根本原因分析
INSERT IGNORE INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(11, 111, 'fishbone', '{"人":"供应商发货时拣货错误","机":"供应商仓储管理系统未防呆","料":"E500 与 E501 外观相似，仅丝印不同","法":"供应商发货前未执行 SKU 与 PO 的逐项核对"}', '供应商发货前未执行 SKU 核对，E500/E501 外观相似增加了混淆风险', 12);

-- CAPA 011 纠正措施
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(16, 111, '供应商更换的 E500 物料已到货，经 IQC 检验合格后入库', 12, '2026-08-02', 'completed', '2026-08-01 16:00:00'),
(17, 111, '要求供应商建立发货前 SKU 双人核对制度', 12, '2026-08-08', 'in_progress', NULL);

-- CAPA 011 预防措施
INSERT IGNORE INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(12, 111, '在 IQC 来料签收环节增加 SKU 与 PO 的逐项比对', 2, '2026-08-10', 'pending', NULL);

-- CAPA 012 根本原因分析
INSERT IGNORE INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(12, 112, 'five_whys', '["1.连接器表面油污","2.操作员未佩戴手套操作","3.手套管理制度执行不严","4.包装工位未强制要求佩戴手套","5.作业指导书中未明确防护要求"]', '作业指导书中未明确要求佩戴手套，且缺少日常监督检查', 9);

-- CAPA 012 纠正措施
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(18, 112, '修订包装工位作业指导书，明确佩戴无粉丁腈手套要求', 9, '2026-07-22', 'completed', '2026-07-21 16:00:00');

-- CAPA 012 预防措施
INSERT IGNORE INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(13, 112, '包装区配置手套自动 dispensing 装置，减少取用障碍', 9, '2026-08-05', 'pending', NULL),
(14, 112, '将手套佩戴纳入每日 IPQC 巡检必查项', 4, '2026-07-25', 'completed', '2026-07-24 16:00:00');

-- CAPA 013 根本原因分析
INSERT IGNORE INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(13, 113, 'fishbone', '{"人":"3 名操作员扭矩操作手法不一致","机":"2 把电动螺丝刀力矩传感器精度下降","料":"螺丝批次间摩擦系数有差异","法":"扭矩作业指导书未规定统一的旋入速度和角度"}', '多因素叠加：人员操作手法不一致 + 工具精度下降 + 物料摩擦系数波动 + 作业标准不明确', 7);

-- CAPA 013 纠正措施
INSERT IGNORE INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(19, 113, '更换 2 把力矩传感器失准的电动螺丝刀', 7, '2026-08-08', 'pending', NULL),
(20, 113, '对 4 名操作员进行扭矩作业标准化培训（统一旋入速度 30rpm，分两阶段旋入）', 7, '2026-08-10', 'pending', NULL);

-- CAPA 013 预防措施
INSERT IGNORE INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(15, 113, '修订扭矩作业指导书，明确旋入速度、阶段和判定标准', 7, '2026-08-15', 'pending', NULL),
(16, 113, '电动螺丝刀纳入月度校准计划，力矩偏差 >5% 立即送修', 11, '2026-08-20', 'pending', NULL);

-- ============================================================
-- Part 6: 补充效果验证数据
-- ============================================================

-- CAPA 001 效果验证（之前只有 D5-D8 中的部分）
INSERT IGNORE INTO capa_verifications (id, capa_id, verifier_id, verification_date, conclusion, evidence) VALUES
(3,  101, 7, '2026-08-05 00:00:00', 'effective', '炉温校准后连续 5 批次 BGA 焊点 X-Ray 检测合格率 100%（报告 #XRAY-2026-032）');

-- CAPA 003 效果验证（之前缺失）
INSERT IGNORE INTO capa_verifications (id, capa_id, verifier_id, verification_date, conclusion, evidence) VALUES
(4,  103, 2, '2026-08-15 00:00:00', 'effective', '后续 3 批 IC 芯片来料密封性检查全部合格，无氧化现象（IQC 报告 #IQS-2026-102/103/104）');

-- CAPA 005 效果验证（已有，补充后续跟踪）
INSERT IGNORE INTO capa_verifications (id, capa_id, verifier_id, verification_date, conclusion, evidence) VALUES
(5,  105, 5, '2026-08-05 00:00:00', 'effective', '改善后 2 个月内无客诉划伤报告，ISTA 3A 测试持续通过（报告 #TR-2026-058）');

-- CAPA 006 效果验证（进行中）
INSERT IGNORE INTO capa_verifications (id, capa_id, verifier_id, verification_date, conclusion, evidence) VALUES
(6,  106, 8, '2026-08-12 00:00:00', 'not_effective', 'B 型号胶材 HTS 测试仍不合格（信号衰减 4.2% > 3%），需继续评估替代料方案');

-- CAPA 007 效果验证（待执行）
-- （纠正措施仍在进行中，尚未验证）

-- CAPA 009 效果验证（进行中）
INSERT IGNORE INTO capa_verifications (id, capa_id, verifier_id, verification_date, conclusion, evidence) VALUES
(7,  109, 3, '2026-07-30 00:00:00', 'effective', '核查近 1 个月出货记录，文件装配完整率 100%（抽查 50 个订单）');

-- ============================================================
-- Part 7: 补充报废/返工记录
-- ============================================================

INSERT IGNORE INTO scrap_rework_records (id, type, defect_id, batch_id, quantity, reason, rework_steps, rework_inspection_required, rework_inspection_result, authorized_by, authorized_at) VALUES
(8,  'scrap',    11, 310, 15, '传感器温度漂移超标，无法通过返工修复，按报废处理', NULL, 0, NULL, 3, '2026-07-19 10:00:00'),
(9,  'scrap',    15, 313,  1, '电解电容极性贴反，需拆除重贴，评估后决定报废该 PCB 板', NULL, 0, NULL, 4, '2026-07-15 09:00:00'),
(10, 'scrap',    22, 319, 100, '物料型号错误（E501 vs E500），供应商责任，整批退货报废', NULL, 0, NULL, 12, '2026-07-08 11:00:00'),
(11, 'rework',   13, 311,  5, 'Feeder 供料器报警导致 5 pcs 半成品停滞，需重新检查焊点质量', '["1.检查 SMT 焊点外观","2.AOI 复测","3.功能测试","4.合格品放行"]', 1, 'pass', 6, '2026-07-17 09:00:00'),
(12, 'rework',   14, 312, 50, '包装受潮的 50 pcs 电阻需烘干处理后再做电性能检测', '["1.60°C 烘干 4 小时","2.取出后外观检查","3.电性能测试","4.合格品重新包装"]', 1, 'pass', 2, '2026-07-16 08:00:00'),
(13, 'rework',   16, 314,  6, '连接器表面油污用异丙醇擦拭清洁', '["1.异丙醇擦拭","2.无尘布擦干","3.外观检查","4.尺寸复检"]', 1, 'pass', 3, '2026-07-14 14:00:00'),
(14, 'rework',   18, 316, 10, 'SMA 连接器外径超差，评估后判定可返工（研磨至标准尺寸）', '["1.精密研磨外径至 4.00mm","2.外径千分尺复检","3.配合测试","4.外观检查"]', 1, 'pending', 2, '2026-07-12 10:00:00'),
(15, 'rework',   24, 321,  4, '扭矩不合格品需重新紧固至标准值', '["1.使用校准扭矩扳手","2.按标准扭矩重新紧固","3.标记确认","4.数据记录"]', 1, 'pending', 4, '2026-07-06 15:00:00');

-- ============================================================
-- Part 8: 验证查询
-- ============================================================
-- SELECT d.defect_no, d.severity, d.status, d.source_type, COUNT(c.id) AS capa_count
--   FROM defects d LEFT JOIN capa c ON c.defect_id = d.id
--  GROUP BY d.id ORDER BY d.id;
--
-- SELECT c.capa_no, c.current_phase, c.status,
--        (SELECT COUNT(*) FROM capa_temporary_measures WHERE capa_id = c.id) AS tmp_cnt,
--        (SELECT COUNT(*) FROM capa_root_causes WHERE capa_id = c.id) AS rc_cnt,
--        (SELECT COUNT(*) FROM capa_corrective_actions WHERE capa_id = c.id) AS ca_cnt,
--        (SELECT COUNT(*) FROM capa_preventive_actions WHERE capa_id = c.id) AS pa_cnt,
--        (SELECT COUNT(*) FROM capa_verifications WHERE capa_id = c.id) AS ver_cnt
--   FROM capa c ORDER BY c.id;
--
-- SELECT type, COUNT(*) AS cnt FROM scrap_rework_records GROUP BY type;
--
-- SELECT status, COUNT(*) AS cnt FROM defects GROUP BY status;
-- SELECT severity, COUNT(*) AS cnt FROM defects GROUP BY severity;
