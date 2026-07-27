-- ============================================
-- QM-AI 设备管理 DEMO 数据插入脚本
-- 数据库: qmai
-- 执行方式: mysql -u root -p qmai < insert_equipment_demo.sql
-- ============================================

-- Step 1: 新增 6 台设备
INSERT INTO equipment (equipment_code, equipment_name, model, production_line, workshop, status, equipment_type, has_mqtt_connection, mqtt_topic_prefix, is_active, created_at, updated_at) VALUES
('HT-002', '回火炉', 'RX3-90-15', '热处理线', '热处理车间', 'maintenance', 'PLC', true, 'factory/heat/ht-002', true, NOW(), NOW()),
('GR-002', '平面磨床', 'M7520H', 'B线', '机加车间', 'running', 'PLC', true, 'factory/line-b/gr-002', true, NOW(), NOW()),
('ROBOT-WLD-01', '焊接机器人 #1', 'IRB 2600-16/1.5', '装配1线', '装配车间', 'running', '机器人', true, 'factory/assy-1/robot-wld-01', true, NOW(), NOW()),
('ROBOT-INS-01', '装配机器人 #1', 'IRB 4600-60/2.05', '装配1线', '装配车间', 'fault', '机器人', true, 'factory/assy-1/robot-ins-01', true, NOW(), NOW()),
('INJ-001', '注塑机 #1', 'MASTERY 180', 'C线', '注塑车间', 'running', 'CNC', true, 'factory/line-c/inj-001', true, NOW(), NOW()),
('PKG-001', '自动包装机', 'PackPro-3000', '包装线', '包装车间', 'idle', 'PLC', true, 'factory/pack/pkg-001', true, NOW(), NOW());

-- Step 2: 获取新增设备ID（执行后记下设备ID）
-- SELECT id, equipment_code FROM equipment WHERE equipment_code IN ('HT-002','GR-002','ROBOT-WLD-01','ROBOT-INS-01','INJ-001','PKG-001');

-- Step 3: 请将上面查到的设备ID替换下面的变量
-- 示例: 假设 HT-002=13, GR-002=14, ROBOT-WLD-01=15, ROBOT-INS-01=16, INJ-001=17, PKG-001=18
SET @HT002 = 13;
SET @GR002 = 14;
SET @ROBOT_WLD = 15;
SET @ROBOT_INS = 16;
SET @INJ001 = 17;
SET @PKG001 = 18;

-- Step 4: 获取参数组ID
-- SELECT id, code FROM param_groups;
SET @THERMO = 1;
SET @MECH = 2;
SET @DIM = 3;

-- Step 5: 插入设备-参数映射
INSERT INTO equipment_param_mappings (equipment_id, mqtt_topic, system_param_code, param_group_id, data_type, unit, created_at, updated_at) VALUES
-- HT-002 回火炉
(@HT002, 'factory/heat/ht-002/chamber_temp', 'temp_heat_treat', @THERMO, 'numeric', '℃', NOW(), NOW()),
(@HT002, 'factory/heat/ht-002/cooling_rate', 'temp_finishing', @THERMO, 'numeric', '℃/min', NOW(), NOW()),
(@HT002, 'factory/heat/ht-002/status', 'surface_defect', NULL, 'status', '-', NOW(), NOW()),
-- GR-002 平面磨床
(@GR002, 'factory/line-b/gr-002/spindle_speed', 'spindle_torque', @MECH, 'numeric', 'rpm', NOW(), NOW()),
(@GR002, 'factory/line-b/gr-002/grinding_temp', 'temp_finishing', @THERMO, 'numeric', '℃', NOW(), NOW()),
(@GR002, 'factory/line-b/gr-002/feed_rate', 'od_tolerance', @DIM, 'numeric', 'mm/pass', NOW(), NOW()),
(@GR002, 'factory/line-b/gr-002/surface_roughness', 'surface_roughness', NULL, 'numeric', 'μm', NOW(), NOW()),
(@GR002, 'factory/line-b/gr-002/status', 'surface_defect', NULL, 'status', '-', NOW(), NOW()),
-- ROBOT-WLD-01 焊接机器人
(@ROBOT_WLD, 'factory/assy-1/robot-wld-01/welding_current', 'cutting_pressure', @MECH, 'numeric', 'A', NOW(), NOW()),
(@ROBOT_WLD, 'factory/assy-1/robot-wld-01/welding_temp', 'temp_heat_treat', @THERMO, 'numeric', '℃', NOW(), NOW()),
(@ROBOT_WLD, 'factory/assy-1/robot-wld-01/welding_speed', 'surface_roughness', NULL, 'numeric', 'cm/min', NOW(), NOW()),
(@ROBOT_WLD, 'factory/assy-1/robot-wld-01/gas_flow', 'rust_prevention', NULL, 'numeric', 'L/min', NOW(), NOW()),
(@ROBOT_WLD, 'factory/assy-1/robot-wld-01/status', 'surface_defect', NULL, 'status', '-', NOW(), NOW()),
-- ROBOT-INS-01 装配机器人
(@ROBOT_INS, 'factory/assy-1/robot-ins-01/force_torque', 'spindle_torque', @MECH, 'numeric', 'N·m', NOW(), NOW()),
(@ROBOT_INS, 'factory/assy-1/robot-ins-01/position_x', 'od_tolerance', @DIM, 'numeric', 'mm', NOW(), NOW()),
(@ROBOT_INS, 'factory/assy-1/robot-ins-01/position_y', 'id_tolerance', @DIM, 'numeric', 'mm', NOW(), NOW()),
(@ROBOT_INS, 'factory/assy-1/robot-ins-01/status', 'surface_defect', NULL, 'status', '-', NOW(), NOW()),
-- INJ-001 注塑机
(@INJ001, 'factory/line-c/inj-001/barrel_temp', 'temp_heat_treat', @THERMO, 'numeric', '℃', NOW(), NOW()),
(@INJ001, 'factory/line-c/inj-001/injection_pressure', 'cutting_pressure', @MECH, 'numeric', 'MPa', NOW(), NOW()),
(@INJ001, 'factory/line-c/inj-001/injection_speed', 'surface_roughness', NULL, 'numeric', 'mm/s', NOW(), NOW()),
(@INJ001, 'factory/line-c/inj-001/mold_temp', 'temp_finishing', @THERMO, 'numeric', '℃', NOW(), NOW()),
(@INJ001, 'factory/line-c/inj-001/status', 'surface_defect', NULL, 'status', '-', NOW(), NOW()),
-- PKG-001 自动包装机
(@PKG001, 'factory/pack/pkg-001/seal_temp', 'temp_finishing', @THERMO, 'numeric', '℃', NOW(), NOW()),
(@PKG001, 'factory/pack/pkg-001/seal_pressure', 'cutting_pressure', @MECH, 'numeric', 'kPa', NOW(), NOW()),
(@PKG001, 'factory/pack/pkg-001/conveyor_speed', 'surface_roughness', NULL, 'numeric', 'm/min', NOW(), NOW()),
(@PKG001, 'factory/pack/pkg-001/status', 'surface_defect', NULL, 'status', '-', NOW(), NOW());

-- Step 6: 插入设备状态历史
INSERT INTO equipment_status_history (equipment_id, signal, signal_data, recorded_at) VALUES
-- HT-002 回火炉
(@HT002, 'running', NULL, '2026-07-25 06:00:00'),
(@HT002, 'fault', '{"code":"E102","desc":"加热管故障","error_code":102}', '2026-07-26 08:00:00'),
(@HT002, 'idle', '{"reason":"更换加热管","technician":"李工"}', '2026-07-26 08:10:00'),
-- GR-002 平面磨床
(@GR002, 'running', NULL, '2026-07-26 06:00:00'),
-- ROBOT-WLD-01 焊接机器人
(@ROBOT_WLD, 'running', NULL, '2026-07-26 06:00:00'),
(@ROBOT_WLD, 'idle', '{"reason":"焊丝更换"}', '2026-07-26 10:00:00'),
(@ROBOT_WLD, 'running', NULL, '2026-07-26 10:20:00'),
-- ROBOT-INS-01 装配机器人
(@ROBOT_INS, 'running', NULL, '2026-07-25 06:00:00'),
(@ROBOT_INS, 'fault', '{"code":"E500","desc":"关节3编码器异常","error_code":500}', '2026-07-26 09:30:00'),
(@ROBOT_INS, 'idle', '{"reason":"等待备件","priority":"high"}', '2026-07-26 09:35:00'),
-- INJ-001 注塑机
(@INJ001, 'running', NULL, '2026-07-26 06:00:00'),
(@INJ001, 'idle', '{"reason":"模具更换"}', '2026-07-26 12:00:00'),
(@INJ001, 'running', NULL, '2026-07-26 12:50:00'),
-- PKG-001 自动包装机
(@PKG001, 'running', NULL, '2026-07-26 06:00:00');

-- Step 7: 插入设备-质量关联分析
INSERT INTO equipment_quality_correlation (equipment_id, analysis_date, correlation_data, created_at) VALUES
-- HT-002 回火炉
(@HT002, '2026-07-25', '{"analysisType":"parameter_quality_correlation","parameters":[{"name":"炉温","correlation":0.95,"impact":"high","finding":"炉温每偏离10度，硬度HRC变化约1.5"},{"name":"保温时间","correlation":0.70,"impact":"medium","finding":"保温时间少于45分钟时硬度偏低"}],"summary":"炉温控制精度需保持在正负5度以内，保温时间不少于50分钟"}', NOW()),
-- GR-002 平面磨床
(@GR002, '2026-07-25', '{"analysisType":"parameter_quality_correlation","parameters":[{"name":"磨削温度","correlation":0.78,"impact":"high","finding":"磨削温度超过60度时表面产生微裂纹"},{"name":"进给率","correlation":0.62,"impact":"medium","finding":"进给率超过0.15mm/pass时粗糙度Ra超标"}],"summary":"建议磨削温度控制在60度以下，进给率保持0.08-0.12mm/pass"}', NOW()),
-- ROBOT-WLD-01 焊接机器人
(@ROBOT_WLD, '2026-07-25', '{"analysisType":"parameter_quality_correlation","parameters":[{"name":"焊接电流","correlation":0.87,"impact":"high","finding":"电流超过180A时出现烧穿缺陷"},{"name":"焊接速度","correlation":0.72,"impact":"medium","finding":"速度低于40cm/min时焊缝余高超标"}],"summary":"焊接电流控制在150-175A，速度保持40-60cm/min"}', NOW()),
-- ROBOT-INS-01 装配机器人
(@ROBOT_INS, '2026-07-25', '{"analysisType":"parameter_quality_correlation","parameters":[{"name":"力矩","correlation":0.85,"impact":"high","finding":"装配力矩超过15N*m时导致零件变形"},{"name":"位置X偏差","correlation":0.70,"impact":"medium","finding":"X轴偏差超过0.2mm时装配合格率下降"}],"summary":"力矩控制在10-14N*m，X轴定位精度需保持在正负0.1mm内"}', NOW()),
-- INJ-001 注塑机
(@INJ001, '2026-07-25', '{"analysisType":"parameter_quality_correlation","parameters":[{"name":"料筒温度","correlation":0.75,"impact":"high","finding":"温度超过230度出现飞边"},{"name":"注射压力","correlation":0.80,"impact":"high","finding":"压力超过90MPa时飞边风险显著增加"}],"summary":"料筒温度控制在210-225度，注射压力低于85MPa"}', NOW()),
-- PKG-001 自动包装机
(@PKG001, '2026-07-25', '{"analysisType":"parameter_quality_correlation","parameters":[{"name":"密封温度","correlation":0.82,"impact":"high","finding":"温度超过180度导致包装材料变形"},{"name":"密封压力","correlation":0.65,"impact":"medium","finding":"压力超过200kPa时包装袋破裂"}],"summary":"密封温度控制在160-175度，压力保持150-180kPa"}', NOW());

-- 验证
SELECT '设备总数: ' AS info, COUNT(*) AS cnt FROM equipment
UNION ALL
SELECT '参数映射数: ', COUNT(*) FROM equipment_param_mappings
UNION ALL
SELECT '状态历史数: ', COUNT(*) FROM equipment_status_history
UNION ALL
SELECT '质量关联数: ', COUNT(*) FROM equipment_quality_correlation;
