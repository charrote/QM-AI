-- ============================================================================
-- 检验计划管理 种子数据
-- 用途: 为检验计划管理模块插入丰富的测试数据
-- 包含: 设备主数据(25台) + 检验计划(12个) + 计划明细(60条)
-- 执行: docker exec -i qm-ai-mysql mysql -uroot -pqmai_root_2024 qmai < seed_inspection_plans.sql
-- ============================================================================

SET NAMES utf8mb4;
SET time_zone = '+08:00';
SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS equipment;
DROP TABLE IF EXISTS inspection_plan_items;
DROP TABLE IF EXISTS inspection_plans;

SET FOREIGN_KEY_CHECKS = 1;

CREATE TABLE equipment (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    equipment_code VARCHAR(50) UNIQUE NOT NULL COMMENT '设备编码',
    equipment_name VARCHAR(200) NOT NULL COMMENT '设备名称',
    model VARCHAR(100) COMMENT '型号',
    production_line VARCHAR(100) COMMENT '产线',
    workshop VARCHAR(100) COMMENT '车间',
    org_id BIGINT COMMENT '所属组织',
    workshop_id BIGINT COMMENT '车间ID',
    line_id BIGINT COMMENT '产线ID',
    status VARCHAR(20) DEFAULT 'idle' COMMENT '状态: idle/running/maintenance',
    equipment_type VARCHAR(50) COMMENT '设备类型',
    has_mqtt_connection TINYINT(1) DEFAULT 0 COMMENT '是否MQTT接入',
    mqtt_topic_prefix VARCHAR(500) COMMENT 'MQTT主题前缀',
    is_active TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE inspection_plans (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    plan_code VARCHAR(50) UNIQUE NOT NULL COMMENT '计划编码',
    plan_name VARCHAR(200) NOT NULL COMMENT '计划名称',
    inspection_type VARCHAR(10) NOT NULL COMMENT '检验类型: IQC/IPQC/FQC/OQC',
    description VARCHAR(500) COMMENT '描述',
    product_id BIGINT COMMENT '产品',
    material_id BIGINT COMMENT '材料',
    supplier_id BIGINT COMMENT '供应商',
    customer_id BIGINT COMMENT '客户',
    process_id BIGINT COMMENT '工艺/工序',
    equipment_id BIGINT COMMENT '设备',
    is_active TINYINT(1) DEFAULT TRUE,
    created_by BIGINT NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE SET NULL,
    FOREIGN KEY (material_id) REFERENCES products(id) ON DELETE SET NULL,
    FOREIGN KEY (supplier_id) REFERENCES suppliers(id) ON DELETE SET NULL,
    FOREIGN KEY (customer_id) REFERENCES customers(id) ON DELETE SET NULL,
    FOREIGN KEY (process_id) REFERENCES processes(id) ON DELETE SET NULL,
    FOREIGN KEY (equipment_id) REFERENCES equipment(id) ON DELETE SET NULL,
    INDEX idx_plans_type (inspection_type),
    INDEX idx_plans_product (product_id),
    INDEX idx_plans_supplier (supplier_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE inspection_plan_items (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    plan_id BIGINT NOT NULL COMMENT '关联计划',
    inspection_item_id BIGINT NOT NULL COMMENT '关联检验项目',
    sort_order INT DEFAULT 0 COMMENT '排序号',
    usl DECIMAL(15,6) COMMENT '规格上限(覆盖)',
    lsl DECIMAL(15,6) COMMENT '规格下限(覆盖)',
    target_value DECIMAL(15,6) COMMENT '目标值(覆盖)',
    ucl DECIMAL(15,6) COMMENT '管理上限(覆盖)',
    lcl DECIMAL(15,6) COMMENT '管理下限(覆盖)',
    sample_size INT COMMENT '抽样数量(覆盖)',
    is_required TINYINT(1) DEFAULT TRUE COMMENT '是否必检',
    FOREIGN KEY (plan_id) REFERENCES inspection_plans(id) ON DELETE CASCADE,
    FOREIGN KEY (inspection_item_id) REFERENCES inspection_items(id) ON DELETE CASCADE,
    INDEX idx_plan_items_plan (plan_id),
    INDEX idx_plan_items_item (inspection_item_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- 设备主数据 25台
INSERT INTO equipment (equipment_code, equipment_name, model, production_line, workshop, status, equipment_type, is_active) VALUES
('EQ-CNC-001','数控车床 CNC-618','CK618','产线A-机加','机加工车间','idle','机加工设备',1),
('EQ-CNC-002','数控车床 CNC-614','CK614','产线A-机加','机加工车间','idle','机加工设备',1),
('EQ-CNC-003','数控铣床 VM-850','VM-850','产线A-机加','机加工车间','idle','机加工设备',1),
('EQ-CNC-004','加工中心 MC-1000','MV-1000','产线A-机加','机加工车间','idle','机加工设备',1),
('EQ-CNC-005','数控磨床 MK-100','MK100A','产线A-研磨','研磨车间','idle','机加工设备',1),
('EQ-HT-001','箱式电阻炉','SX2-4-12','产线B-热处理','热处理车间','idle','热处理设备',1),
('EQ-HT-002','渗碳淬火炉','QX3-90-9','产线B-热处理','热处理车间','idle','热处理设备',1),
('EQ-HT-003','真空淬火炉','RZQ-80','产线B-热处理','热处理车间','idle','热处理设备',1),
('EQ-INS-001','三坐标测量仪','ZEISS LEGEX 7/7/6','检测中心','检测中心','idle','检测设备',1),
('EQ-INS-002','投影仪','OPTIC-300','检测中心','检测中心','idle','检测设备',1),
('EQ-INS-003','粗糙度仪','TR220','检测中心','检测中心','idle','检测设备',1),
('EQ-INS-004','硬度计','HV-1000','检测中心','检测中心','idle','检测设备',1),
('EQ-INS-005','圆度仪','CHY-200','检测中心','检测中心','idle','检测设备',1),
('EQ-INS-006','光谱分析仪','LECO GC-800','检测中心','检测中心','idle','检测设备',1),
('EQ-INS-007','气密测试仪','ATEQ F20','检测中心','检测中心','idle','检测设备',1),
('EQ-INS-008','外径千分尺','三丰 293-240','机加工车间','机加工车间','idle','量具',1),
('EQ-INS-009','内径千分尺','三丰 344-252','机加工车间','机加工车间','idle','量具',1),
('EQ-INS-010','游标卡尺','三丰 500-196','机加工车间','机加工车间','idle','量具',1),
('EQ-INS-011','百分表','三丰 2001SB','机加工车间','机加工车间','idle','量具',1),
('EQ-INJ-001','注塑机 INJ-200','MA-200','产线C-成型','成型车间','idle','成型设备',1),
('EQ-DIE-001','压铸机 DIE-360','YJ3G-360','产线C-成型','成型车间','idle','成型设备',1),
('EQ-ASM-001','自动组装线','ASM-LINE-01','产线D-组装','组装车间','idle','组装设备',1),
('EQ-ASM-002','螺丝锁付机','FUJI NX-F','产线D-组装','组装车间','idle','组装设备',1),
('EQ-CLN-001','超声波清洗机','KQ-500DE','产线E-后处理','后处理车间','idle','清洗设备',1),
('EQ-CLN-002','真空包装机','DZ-400','产线E-后处理','后处理车间','idle','包装设备',1);

-- 检验计划 12个
INSERT INTO inspection_plans (plan_code, plan_name, inspection_type, description, product_id, material_id, supplier_id, customer_id, process_id, equipment_id, is_active) VALUES
('IP-IQC-001','精密转轴A100来料检验计划','IQC','宝钢45#圆钢来料检验 (外径、内径、长度、化学成分)',2,NULL,7,NULL,6,NULL,1),
('IP-IQC-002','PCB主板C300来料检验计划','IQC','PCB基板来料检验 (外观、尺寸、阻抗)',4,NULL,20,NULL,6,NULL,1),
('IP-IQC-003','壳体B200铝锭来料检验计划','IQC','西南铝业铝锭来料检验 (化学成分、力学性能、尺寸)',3,NULL,8,NULL,6,NULL,1),
('IP-IQC-004','密封圈D400橡胶原料来料检验计划','IQC','橡胶原料来料检验 (硬度、拉伸强度、外观)',NULL,NULL,6,NULL,6,NULL,1),
('IP-IPQC-001','精车工序首件检验计划','IPQC','精密转轴精车工序首件检验 (外径、长度、粗糙度、直线度)',2,NULL,NULL,NULL,8,1,1),
('IP-IPQC-002','热处理工序首件检验计划','IPQC','精密转轴渗碳淬火首件检验 (硬度、渗碳层深度、金相组织)',2,NULL,NULL,NULL,10,6,1),
('IP-IPQC-003','精车工序巡检计划','IPQC','精车工序定时巡检 (尺寸、表面、设备参数)',2,NULL,NULL,NULL,8,1,1),
('IP-IPQC-004','壳体B200压铸巡检计划','IPQC','壳体压铸工序巡检 (外观、尺寸、重量、气密性)',3,NULL,NULL,NULL,11,21,1),
('IP-FQC-001','精密转轴A100成品检验计划','FQC','精密转轴出厂成品全尺寸检验 (外径、内径、长度、粗糙度、硬度、外观)',2,NULL,NULL,1,13,NULL,1),
('IP-FQC-002','壳体B200成品检验计划','FQC','壳体成品出厂检验 (外观、尺寸、气密性、包装)',3,NULL,NULL,2,13,NULL,1),
('IP-FQC-003','PCB主板C300成品检验计划','FQC','PCB主板出厂成品检验 (外观、阻抗、焊接质量、功能)',4,NULL,NULL,3,13,NULL,1),
('IP-OQC-001','通用出货检验计划','OQC','通用出货终检 (外观、包装、标识、数量确认)',NULL,NULL,NULL,1,15,NULL,1);

-- 计划明细 60条
-- IP-IQC-001: 精密转轴A100来料 (4项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (1,1,1,50.050000,49.950000,50.000000,NULL,NULL,10,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (1,2,2,25.030000,24.970000,25.000000,NULL,NULL,10,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (1,3,3,100.100000,99.900000,100.000000,NULL,NULL,10,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (1,10,4,0.450000,0.420000,0.430000,NULL,NULL,3,1);

-- IP-IQC-002: PCB主板C300来料 (3项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (2,3,1,200.100000,199.900000,200.000000,NULL,NULL,20,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (2,1,2,100.050000,99.950000,100.000000,NULL,NULL,20,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (2,6,3,NULL,NULL,NULL,NULL,NULL,NULL,1);

-- IP-IQC-003: 壳体B200铝锭来料 (5项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (3,1,1,100.100000,99.900000,100.000000,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (3,3,2,300.200000,299.800000,300.000000,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (3,5,3,65.000000,55.000000,60.000000,NULL,NULL,3,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (3,6,4,NULL,NULL,NULL,NULL,NULL,NULL,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (3,10,5,0.350000,0.250000,0.300000,NULL,NULL,3,1);

-- IP-IQC-004: 密封圈D400橡胶来料 (4项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (4,6,1,NULL,NULL,NULL,NULL,NULL,NULL,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (4,5,2,75.000000,65.000000,70.000000,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (4,11,3,NULL,NULL,NULL,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (4,12,4,NULL,NULL,NULL,NULL,NULL,NULL,0);

-- IP-IPQC-001: 精车工序首件 (5项, 含控制限)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (5,1,1,50.050000,49.950000,50.000000,50.030000,49.970000,3,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (5,2,2,25.030000,24.970000,25.000000,25.020000,24.980000,3,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (5,3,3,100.100000,99.900000,100.000000,100.050000,99.950000,3,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (5,4,4,1.600000,0.000000,0.800000,1.200000,0.400000,3,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (5,8,5,0.050000,0.000000,0.020000,0.035000,0.005000,1,1);

-- IP-IPQC-002: 热处理工序首件 (4项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (6,5,1,58.000000,52.000000,55.000000,57.000000,53.000000,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (6,9,2,0.030000,0.000000,0.015000,0.022000,0.005000,3,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (6,6,3,NULL,NULL,NULL,NULL,NULL,NULL,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (6,8,4,0.050000,0.000000,0.020000,0.035000,0.005000,3,1);

-- IP-IPQC-003: 精车工序巡检 (4项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (7,1,1,50.050000,49.950000,50.000000,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (7,3,2,100.100000,99.900000,100.000000,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (7,4,3,1.600000,0.000000,0.800000,NULL,NULL,3,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (7,6,4,NULL,NULL,NULL,NULL,NULL,NULL,0);

-- IP-IPQC-004: 壳体B200压铸巡检 (5项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (8,6,1,NULL,NULL,NULL,NULL,NULL,NULL,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (8,7,2,NULL,NULL,NULL,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (8,5,3,65.000000,55.000000,60.000000,NULL,NULL,3,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (8,11,4,NULL,NULL,NULL,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (8,12,5,NULL,NULL,NULL,NULL,NULL,NULL,0);

-- IP-FQC-001: 精密转轴A100成品 (7项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (9,1,1,50.050000,49.950000,50.000000,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (9,2,2,25.030000,24.970000,25.000000,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (9,3,3,100.100000,99.900000,100.000000,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (9,4,4,1.600000,0.000000,0.800000,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (9,5,5,58.000000,52.000000,55.000000,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (9,8,6,0.050000,0.000000,0.020000,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (9,6,7,NULL,NULL,NULL,NULL,NULL,13,1);

-- IP-FQC-002: 壳体B200成品 (5项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (10,3,1,150.150000,149.850000,150.000000,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (10,6,2,NULL,NULL,NULL,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (10,7,3,NULL,NULL,NULL,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (10,11,4,NULL,NULL,NULL,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (10,12,5,NULL,NULL,NULL,NULL,NULL,13,1);

-- IP-FQC-003: PCB主板C300成品 (5项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (11,6,1,NULL,NULL,NULL,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (11,3,2,200.100000,199.900000,200.000000,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (11,1,3,100.050000,99.950000,100.000000,NULL,NULL,5,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (11,12,4,NULL,NULL,NULL,NULL,NULL,13,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (11,11,5,NULL,NULL,NULL,NULL,NULL,NULL,0);

-- IP-OQC-001: 通用出货检验 (4项)
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (12,6,1,NULL,NULL,NULL,NULL,NULL,NULL,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (12,12,2,NULL,NULL,NULL,NULL,NULL,NULL,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (12,3,3,NULL,NULL,NULL,NULL,NULL,NULL,1);
INSERT INTO inspection_plan_items (plan_id, inspection_item_id, sort_order, usl, lsl, target_value, ucl, lcl, sample_size, is_required) VALUES (12,6,4,NULL,NULL,NULL,NULL,NULL,NULL,0);

-- 验证查询
SELECT '=== 检验计划汇总 ===' AS '';
SELECT plan_code, plan_name, inspection_type,
       CASE inspection_type
           WHEN 'IQC' THEN '来料检验'
           WHEN 'IPQC' THEN '过程检验'
           WHEN 'FQC' THEN '成品检验'
           WHEN 'OQC' THEN '出货检验'
       END AS type_cn, is_active
FROM inspection_plans ORDER BY inspection_type, id;

SELECT '=== 计划明细汇总 ===' AS '';
SELECT ip.plan_code, ipi.sort_order, ii.item_name,
       CASE WHEN ipi.is_required = 1 THEN '必检' ELSE '选检' END AS check_type,
       ipi.sample_size
FROM inspection_plan_items ipi
JOIN inspection_plans ip ON ipi.plan_id = ip.id
JOIN inspection_items ii ON ipi.inspection_item_id = ii.id
ORDER BY ip.id, ipi.sort_order;
