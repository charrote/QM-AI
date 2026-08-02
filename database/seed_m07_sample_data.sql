-- ============================================================================
-- M07 – 不良与异常管理 样例数据
-- 运行方式: docker exec -i qm-ai-mysql mysql -u root -p'qmai_root_2024' qmai < database/seed_m07_sample_data.sql
-- ============================================================================

-- ─── Step 0: 创建 CAPA 子表（如不存在）──────────────────

CREATE TABLE IF NOT EXISTS capa_temporary_measures (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    capa_id BIGINT NOT NULL,
    description TEXT NOT NULL,
    executed_by BIGINT,
    executed_at DATETIME,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS capa_root_causes (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    capa_id BIGINT NOT NULL,
    analysis_method VARCHAR(20) NOT NULL DEFAULT 'five_whys',
    content JSON NOT NULL COMMENT '5Why问答或鱼骨图数据',
    root_cause_summary TEXT NOT NULL,
    created_by BIGINT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS capa_corrective_actions (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    capa_id BIGINT NOT NULL,
    action_description TEXT NOT NULL,
    responsible_person BIGINT NOT NULL,
    due_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    completed_at DATETIME,
    remarks TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS capa_preventive_actions (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    capa_id BIGINT NOT NULL,
    action_description TEXT NOT NULL,
    responsible_person BIGINT NOT NULL,
    due_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    completed_at DATETIME,
    remarks TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS capa_verifications (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    capa_id BIGINT NOT NULL,
    verifier_id BIGINT NOT NULL,
    verification_date DATETIME NOT NULL,
    conclusion VARCHAR(20) NOT NULL COMMENT 'effective/not_effective/requires_revision',
    evidence TEXT,
    image_urls JSON,
    remarks TEXT,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (capa_id) REFERENCES capa(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ─── Step 1: 补充样例用户（仅插入缺失的，ID 4-13）───

INSERT IGNORE INTO users (id, username, password_hash, display_name, email, role_id) VALUES
(4,  'fangwang',  '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '王芳',   'fangwang@qmai.com',   2),
(5,  'leizhao',   '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '赵磊',   'leizhao@qmai.com',    2),
(6,  'jingchen',  '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '陈静',   'jingchen@qmai.com',   2),
(7,  'gongliu',   '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '刘工',   'liugong@qmai.com',    2),
(8,  'gongchen',  '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '陈工',   'chengong@qmai.com',   2),
(9,  'jinghuang', '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '黄工',   'huangong@qmai.com',   2),
(10, 'jingwu',    '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '吴工',   'wugong@qmai.com',     2),
(11, 'gongzhou',  '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '周工',   'zhougong@qmai.com',   2),
(12, 'sunmanager','$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '孙经理', 'sunmanager@qmai.com', 3),
(13, 'qimozhang', '$2b$12$LJ3m4ys3Lg0VHWnFvE6Q8eH3Jk5Xm2Zv7Yw9AbCdEfGhIjKlMnOpQ', '张质量', 'zhangzhi@qmai.com',   2);

-- ─── Step 2: 补充样例产品（仅插入缺失的）─────────────

INSERT IGNORE INTO products (id, product_code, product_name, description, product_type) VALUES
(201, 'PCB-A100',  '主控 PCB 板 A100',  'STM32 主控板，用于智能控制器', 'PCB'),
(203, 'BOX-B200',  '铝合金外壳 B200',  'IP67 防护铝合金外壳', '结构件'),
(205, 'ASS-C300',  '装配组件 C300',    '电机驱动装配组件', '组件'),
(207, 'PKG-D400',  '智能传感器 D400',  '温湿度传感器成品', '成品'),
(209, 'IC-E500',   '电源管理 IC E500', 'LDO 电源管理芯片', '元器件'),
(210, 'HSG-F600',  '防水连接器 F600',  'IP67 防水航空连接器', '连接器'),
(212, 'LED-G700',  '指示 LED G700',    'LED 指示灯组件', '电子元器件');

-- ─── Step 3: 插入缺陷记录 ────────────────────────────

INSERT INTO defects (id, defect_no, severity, source_type, source_ref_id, product_id, batch_id, equipment_id, quantity, description, image_urls, discovered_by, discovered_at, status) VALUES
(1,  'DEF-2026-001', 'major',   'IQC',            101, 201, 301, 0, 3,  'PCB 板焊点虚焊，3 颗 BGA 芯片引脚未充分润湿，导致信号传输不稳定', NULL, 2, '2026-07-28 08:30:00', 'investigating'),
(2,  'DEF-2026-002', 'critical','FQC',            202, 203, 302, 0, 12, '成品外壳色差超标，批次 302 共抽检 200 pcs，12 pcs 色差 ΔE>2.5，超出允收标准', NULL, 3, '2026-07-27 14:15:00', 'open'),
(3,  'DEF-2026-003', 'minor',   'IPQC-PATROL',    303, 205, 303, 10, 1, '装配线第 3 工位螺丝扭矩偏低（实测 1.8N·m，标准要求 2.0±0.2N·m）', NULL, 4, '2026-07-26 10:00:00', 'resolved'),
(4,  'DEF-2026-004', 'major',   'CUSTOMER',        0, 207,   0, 0, 5, '客户退货 5 pcs，功能测试 PASS 但外观划伤，划痕深度约 0.3mm，怀疑包装防护不足', NULL, 5, '2026-07-25 09:45:00', 'open'),
(5,  'DEF-2026-005', 'minor',   'PRODUCTION',      0, 201, 304, 12, 2, 'SMT 贴片偏移，2 pcs LED 灯珠位置偏差 0.15mm（允差 0.2mm），在临界范围内', NULL, 6, '2026-07-24 16:20:00', 'closed'),
(6,  'DEF-2026-006', 'critical','IQC',            401, 209, 305, 0, 50, 'IC 芯片引脚氧化严重，到货抽检 50 pcs 全部发现引脚发暗，影响焊接可靠性', NULL, 2, '2026-07-23 11:30:00', 'investigating'),
(7,  'DEF-2026-007', 'major',   'FQC',            501, 210, 306, 0, 8, '成品防水测试不合格，8 pcs 在 IP67 防水测试中 30 秒内出现渗水', NULL, 3, '2026-07-22 13:00:00', 'investigating'),
(8,  'DEF-2026-008', 'minor',   'IPQC-FIRSTPIECE',601, 212, 307, 0, 1, '首件检测连接器针脚弯曲 1 根，已手动校正，未影响批量生产', NULL, 4, '2026-07-21 07:50:00', 'closed');

-- ─── Step 4: 插入 CAPA 记录 ──────────────────────────

INSERT INTO capa (id, capa_no, defect_id, severity, title, description, current_phase, status, created_by, assigned_to, due_date, created_at) VALUES
(101, 'CAPA-2026-001', 1,  'major',   'PCB BGA 虚焊问题纠正与预防', 'BGA 芯片虚焊导致信号传输不稳定，需追溯根本原因并建立长效预防机制', 3, 'in_progress', 2, 7, '2026-08-15', '2026-07-28 09:00:00'),
(102, 'CAPA-2026-002', 2,  'critical','成品外壳色差超标根因分析与改善', '批次 302 色差超标，ΔE>2.5，需追溯喷涂工艺参数', 2, 'in_progress', 3, 8, '2026-08-10', '2026-07-27 15:00:00'),
(103, 'CAPA-2026-003', 6,  'critical','IC 芯片引脚氧化供应商质量投诉', '供应商来料 IC 芯片引脚氧化，50 pcs 全部不合格', 4, 'in_progress', 2, 12, '2026-08-20', '2026-07-23 12:00:00'),
(104, 'CAPA-2026-004', 7,  'major',   'IP67 防水测试不合格改善', '8 pcs 成品在 IP67 防水测试中渗水，需追溯密封工艺', 1, 'open', 3, 9, '2026-08-25', '2026-07-22 14:00:00'),
(105, 'CAPA-2026-005', 4,  'major',   '客户退货外观划伤包装防护改善', '客户收到 5 pcs 划伤品，需改善包装防护方案', 5, 'in_progress', 5, 10, '2026-08-05', '2026-07-25 10:00:00');

-- ─── Step 5: 插入 CAPA 子表数据 ────────────────────────

-- 5.1 临时措施
INSERT INTO capa_temporary_measures (id, capa_id, description, executed_by, executed_at) VALUES
(1, 101, '对已生产批次 301 全检 BGA 焊点，X-Ray 检测', 2, '2026-07-28 10:00:00'),
(2, 102, '隔离批次 302 全部库存，禁止出货', 3, '2026-07-27 15:30:00'),
(3, 103, '退货处理，启用备选供应商紧急供货', 12, '2026-07-23 14:00:00'),
(4, 104, '批次 306 全部返工重新进行防水测试', 9, '2026-07-22 15:00:00'),
(5, 105, '在现有包装内增加 EPE 泡沫缓冲层', 10, '2026-07-25 11:00:00');

-- 5.2 根本原因分析
INSERT INTO capa_root_causes (id, capa_id, analysis_method, content, root_cause_summary, created_by) VALUES
(1, 101, 'five_whys', '["1.焊点虚焊","2.锡膏温度偏低","3.回流焊炉温曲线偏移","4.热电偶校准过期","5.校准周期管理缺失"]', '回流焊炉温校准周期管理缺失，导致实际炉温低于设定值', 7),
(2, 102, 'fishbone', '{"人":"操作员未按时校准色差仪","机":"喷枪压力波动","料":"油漆批次更换","法":"喷涂厚度标准不统一"}', '油漆批次更换后未重新校准喷涂参数，导致色差', 8),
(3, 103, 'five_whys', '["1.引脚氧化","2.包装密封不良","3.供应商包装工艺变更未通知","4.来料检验标准未包含密封性检查"]', '供应商变更包装工艺未执行 PCN 通知，来料检验标准存在漏洞', 2),
(4, 105, 'five_whys', '["1.外观划伤","2.运输中产品互相碰撞","3.内包装隔层不足","4.包装设计未考虑堆叠场景","5.包装验证试验不充分"]', '包装设计方案未覆盖实际运输堆叠场景，内包装隔层不足以防止碰撞', 10);

-- 5.3 纠正措施
INSERT INTO capa_corrective_actions (id, capa_id, action_description, responsible_person, due_date, status, completed_at) VALUES
(1, 101, '立即重新校准回流焊炉温曲线，更换热电偶', 11, '2026-08-02', 'completed', '2026-08-01 16:00:00'),
(2, 101, '建立炉温校准 SOP，每月校准一次', 2, '2026-08-10', 'in_progress', NULL),
(3, 103, '向供应商发出 8D 报告，要求限期整改', 12, '2026-08-05', 'in_progress', NULL),
(4, 103, '更新 IQC 检验标准，增加密封性检查项目', 2, '2026-08-08', 'completed', '2026-08-06 17:00:00'),
(5, 105, ' redesign 内包装，增加独立分隔格', 10, '2026-08-01', 'completed', '2026-07-31 16:00:00');

-- 5.4 预防措施
INSERT INTO capa_preventive_actions (id, capa_id, action_description, responsible_person, due_date, status) VALUES
(1, 101, 'SMT 产线导入炉温自动监控系统，异常自动报警', 11, '2026-08-30', 'pending'),
(2, 103, '将密封性检查纳入供应商年度审核 checklist', 2, '2026-09-15', 'pending'),
(3, 105, '所有新产品包装设计必须通过 ISTA 3A 运输振动试验', 5, '2026-08-04', 'completed');

-- 5.5 效果验证
INSERT INTO capa_verifications (id, capa_id, verifier_id, verification_date, conclusion, evidence) VALUES
(1, 105, 5, '2026-07-30 00:00:00', 'effective', '新包装设计通过 ISTA 3A 测试，外观无划伤（测试报告 #TR-2026-045）');

-- ─── Step 6: 插入报废/返工记录 ─────────────────────────

INSERT INTO scrap_rework_records (id, type, defect_id, batch_id, quantity, reason, rework_steps, rework_inspection_required, rework_inspection_result, authorized_by, authorized_at) VALUES
(1, 'scrap',    2, 302, 12, '成品外壳色差超标（ΔE>2.5），无法返工修复，按报废处理', NULL, 0, NULL, 3, '2026-07-28 10:00:00'),
(2, 'rework',   1, 301,  3, 'BGA 芯片虚焊，需重新回流焊接', '["1.拆除虚焊BGA芯片","2.清理焊盘残留锡膏","3.重新植球","4.回流焊接","5.X-Ray复检"]', 1, 'pass', 2, '2026-07-29 09:00:00'),
(3, 'scrap',    6, 305, 50, 'IC 芯片引脚氧化严重，无法通过清洗修复，供应商责任，整批退货', NULL, 0, NULL, 12, '2026-07-24 14:00:00'),
(4, 'rework',   7, 306,  8, 'IP67 防水测试不合格，需重新打胶密封', '["1.拆解外壳","2.清除旧密封胶","3.重新涂敷密封胶","4.组装","5.重新进行IP67防水测试"]', 1, NULL, 9, '2026-07-23 10:00:00'),
(5, 'rework',   5, 304,  2, 'SMT LED 贴片偏移，需返正', '["1.加热融化锡膏","2.使用显微镜定位偏移LED","3.校正位置至允差范围内","4.重新回流焊接"]', 1, 'pass', 6, '2026-07-25 08:00:00'),
(6, 'scrap',    4,   0,  5, '客户退货 5 pcs 外观划伤，无法修复，按客诉报废处理', NULL, 0, NULL, 5, '2026-07-26 11:00:00'),
(7, 'rework',   3, 303,  1, '螺丝扭矩偏低，需重新紧固至标准值', '["1.使用校准扭矩扳手","2.按标准扭矩2.0±0.2N·m重新紧固","3.标记确认"]', 1, 'pass', 4, '2026-07-26 11:00:00');

-- ============================================================================
-- 验证查询
-- ============================================================================
-- SELECT d.defect_no, d.severity, d.status, COUNT(c.id) AS capa_count
--   FROM defects d LEFT JOIN capa c ON c.defect_id = d.id
--  GROUP BY d.id ORDER BY d.id;
--
-- SELECT c.capa_no, c.current_phase, c.status,
--        (SELECT COUNT(*) FROM capa_corrective_actions WHERE capa_id = c.id) AS corrective_cnt
--   FROM capa c ORDER BY c.id;
--
-- SELECT type, COUNT(*) AS cnt FROM scrap_rework_records GROUP BY type;
