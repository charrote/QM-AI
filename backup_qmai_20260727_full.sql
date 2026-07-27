-- MySQL dump 10.13  Distrib 8.0.46, for Linux (aarch64)
--
-- Host: localhost    Database: qmai
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `__EFMigrationsHistory`
--

LOCK TABLES `__EFMigrationsHistory` WRITE;
/*!40000 ALTER TABLE `__EFMigrationsHistory` DISABLE KEYS */;
INSERT INTO `__EFMigrationsHistory` VALUES ('20260724035832_InitialCreate','9.0.6'),('20260727040000_AddPrePostWaitTimeToRoutingStep','9.0.6');
/*!40000 ALTER TABLE `__EFMigrationsHistory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `audits`
--

DROP TABLE IF EXISTS `audits`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `audits` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `audit_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `audit_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `title` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `StartDate` date NOT NULL,
  `EndDate` date NOT NULL,
  `AuditorId` bigint NOT NULL,
  `AuditorIdsJson` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `TotalFindings` int NOT NULL,
  `Conformities` int NOT NULL,
  `NonConformities` int NOT NULL,
  `Opportunities` int NOT NULL,
  `Scope` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `status` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `created_at` datetime(6) NOT NULL,
  `updated_at` datetime(6) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `audits`
--

LOCK TABLES `audits` WRITE;
/*!40000 ALTER TABLE `audits` DISABLE KEYS */;
/*!40000 ALTER TABLE `audits` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `boms`
--

DROP TABLE IF EXISTS `boms`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `boms` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `product_id` bigint NOT NULL,
  `material_code` varchar(50) DEFAULT NULL,
  `material_name` varchar(200) DEFAULT NULL,
  `quantity` decimal(10,2) DEFAULT NULL,
  `unit` varchar(20) DEFAULT NULL,
  `level` int DEFAULT NULL,
  `remark` varchar(500) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `boms_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `boms`
--

LOCK TABLES `boms` WRITE;
/*!40000 ALTER TABLE `boms` DISABLE KEYS */;
INSERT INTO `boms` VALUES (1,2,'MAT-001','45# 圆钢 Φ50',1.20,'kg',1,'宝钢供料，碳含量0.42-0.50%','2026-07-27 06:30:19','2026-07-27 06:30:19'),(2,2,'MAT-002','轴承 6205',2.00,'pcs',1,'SKF供料，深沟球轴承','2026-07-27 06:30:19','2026-07-27 06:30:19'),(3,2,'MAT-003','润滑油',0.05,'L',1,'中石化长城L-AN46','2026-07-27 06:30:19','2026-07-27 06:30:19'),(4,2,'MAT-004','防锈油',0.02,'L',1,'包装前防锈处理','2026-07-27 06:30:19','2026-07-27 06:30:19'),(5,2,'MAT-005','包装盒',1.00,'pcs',1,'瓦楞纸盒 200×80×80mm','2026-07-27 06:30:19','2026-07-27 06:30:19'),(6,2,'MAT-006','产品标签',1.00,'pcs',1,'含批次号、生产日期','2026-07-27 06:30:19','2026-07-27 06:30:19'),(7,3,'MAT-101','ADC12 铝合金锭',2.50,'kg',1,'压铸件主体材料','2026-07-27 06:30:19','2026-07-27 06:30:19'),(8,3,'MAT-102','脱模剂',0.03,'L',1,'水性脱模剂','2026-07-27 06:30:19','2026-07-27 06:30:19'),(9,3,'MAT-103','密封圈 O型圈 25×3',2.00,'pcs',1,'NBR橡胶，耐油','2026-07-27 06:30:19','2026-07-27 06:30:19'),(10,3,'MAT-104','螺栓 M6×20',4.00,'pcs',1,'8.8级镀锌螺栓','2026-07-27 06:30:19','2026-07-27 06:30:19'),(11,3,'MAT-105','垫片 Φ6',4.00,'pcs',1,'弹簧垫片','2026-07-27 06:30:19','2026-07-27 06:30:19'),(12,3,'MAT-106','防尘罩',1.00,'pcs',1,'硅胶材质','2026-07-27 06:30:19','2026-07-27 06:30:19'),(13,3,'MAT-107','泡沫内衬',1.00,'pcs',1,'EPE珍珠棉定制','2026-07-27 06:30:19','2026-07-27 06:30:19'),(14,4,'MAT-201','FR-4 玻纤基板 150×100×1.6',1.00,'pcs',1,'4层板，阻焊绿油','2026-07-27 06:30:19','2026-07-27 06:30:19'),(15,4,'MAT-202','MCU STM32F103C8T6',1.00,'pcs',1,'主控芯片','2026-07-27 06:30:19','2026-07-27 06:30:19'),(16,4,'MAT-203','电容 100μF/16V',6.00,'pcs',1,'电解电容','2026-07-27 06:30:19','2026-07-27 06:30:19'),(17,4,'MAT-204','电容 0.1μF/50V',12.00,'pcs',1,'陶瓷电容','2026-07-27 06:30:19','2026-07-27 06:30:19'),(18,4,'MAT-205','电阻 10KΩ 1/4W',8.00,'pcs',1,'贴片电阻','2026-07-27 06:30:19','2026-07-27 06:30:19'),(19,4,'MAT-206','USB Type-C 接口',1.00,'pcs',1,'沉板焊接','2026-07-27 06:30:19','2026-07-27 06:30:19'),(20,4,'MAT-207','排针 2×10 Pin',2.00,'pcs',1,'2.54mm间距','2026-07-27 06:30:19','2026-07-27 06:30:19'),(21,4,'MAT-208','晶振 8MHz',1.00,'pcs',1,'无源晶振','2026-07-27 06:30:19','2026-07-27 06:30:19'),(22,4,'MAT-209','防静电袋',1.00,'pcs',1,'屏蔽包装袋','2026-07-27 06:30:19','2026-07-27 06:30:19'),(23,5,'MAT-301','NBR 橡胶原料',0.15,'kg',1,'丁腈橡胶，硬度70A','2026-07-27 06:30:19','2026-07-27 06:30:19'),(24,5,'MAT-302','润滑粉',0.00,'kg',1,'模具脱模用','2026-07-27 06:30:19','2026-07-27 06:30:19'),(25,5,'MAT-303','PE 自封袋',1.00,'pcs',1,'包装用','2026-07-27 06:30:19','2026-07-27 06:30:19'),(26,5,'MAT-304','干燥剂',1.00,'pcs',1,'硅胶干燥剂 5g','2026-07-27 06:30:19','2026-07-27 06:30:19'),(27,6,'MAT-401','PVC 护套线 2×0.75mm²',1.50,'m',1,'线束主线材','2026-07-27 06:30:19','2026-07-27 06:30:19'),(28,6,'MAT-402','端子 HT-05B',4.00,'pcs',1,'公母对插端子','2026-07-27 06:30:19','2026-07-27 06:30:19'),(29,6,'MAT-403','热缩管 Φ6 红',0.10,'m',1,'两端绝缘保护','2026-07-27 06:30:19','2026-07-27 06:30:19'),(30,6,'MAT-404','扎带 100mm 黑色',2.00,'pcs',1,'尼龙扎带固定','2026-07-27 06:30:19','2026-07-27 06:30:19'),(31,6,'MAT-405','缠绕管 Φ8 黑色',0.30,'m',1,'线束保护套管','2026-07-27 06:30:19','2026-07-27 06:30:19'),(32,6,'MAT-406','PE 包装袋',1.00,'pcs',1,'含产品标签','2026-07-27 06:30:19','2026-07-27 06:30:19');
/*!40000 ALTER TABLE `boms` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `closure_rules`
--

DROP TABLE IF EXISTS `closure_rules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `closure_rules` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `code` varchar(50) NOT NULL,
  `condition_json` text,
  `logic` varchar(10) DEFAULT NULL,
  `description` text,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_by` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `closure_rules`
--

LOCK TABLES `closure_rules` WRITE;
/*!40000 ALTER TABLE `closure_rules` DISABLE KEYS */;
INSERT INTO `closure_rules` VALUES (1,'全部合格关单','RULE_ALL_PASS','[]','AND','所有检验项目全部 pass 时自动关单',1,1,'2026-07-24 05:15:36','2026-07-24 05:15:36'),(2,'最大缺陷数关单','RULE_MAX_DEFECT','[]','AND','缺陷数量≤0时自动关单',1,1,'2026-07-24 05:15:36','2026-07-24 05:15:36'),(3,'CPK达标关单','RULE_CPK_PASS','[]','AND','CPK≥1.33时自动关单',1,1,'2026-07-24 05:15:36','2026-07-24 05:15:36'),(4,'温度稳定关单','RULE_TEMP_STABLE','[]','AND','温度范围≤5℃且均值在175-185℃之间时关单',1,1,'2026-07-24 05:15:36','2026-07-24 05:15:36'),(5,'尺寸合格关单','RULE_DIM_CONFORM','[]','AND','尺寸合格率≥99.5%时自动关单',1,1,'2026-07-24 05:15:36','2026-07-24 05:15:36'),(6,'默认自动关单','RULE_AUTO_CLOSE','[]','AND','结果为 pass 时自动关单',1,1,'2026-07-24 05:15:36','2026-07-24 05:15:36');
/*!40000 ALTER TABLE `closure_rules` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `customers`
--

DROP TABLE IF EXISTS `customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customers` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `customer_code` varchar(50) NOT NULL,
  `customer_name` varchar(200) NOT NULL,
  `address` varchar(500) DEFAULT NULL,
  `contact_person` varchar(100) DEFAULT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `email` varchar(200) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_customers_customer_code` (`customer_code`)
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customers`
--

LOCK TABLES `customers` WRITE;
/*!40000 ALTER TABLE `customers` DISABLE KEYS */;
INSERT INTO `customers` VALUES (1,'CUST-E001','深圳华芯微电子科技有限公司','深圳市宝安区西乡街道固戍社区航城大道168号华芯科技园A栋','张伟明','13800138001','zhangweiming@huaxinmicro.com',1,'2026-01-15 09:00:00','2026-07-20 14:30:00'),(2,'CUST-E002','东莞精密电子科技有限公司','东莞市松山湖高新区科技大道28号精密电子大厦','李明华','13900139002','liminghua@jingjing-tech.com',1,'2026-02-10 10:00:00','2026-07-20 14:30:00'),(3,'CUST-E003','广州智联通讯设备有限公司','广州市天河区科学城科学大道100号智联大厦','王建国','13700137003','wangjianguo@zhiliancomm.com',1,'2026-03-05 11:00:00','2026-07-20 14:30:00'),(4,'CUST-E004','佛山新能源科技有限公司','佛山市顺德区北滘镇碧桂园大道88号创新产业园B区','陈思远','13600136004','chensiyuan@foshan-newenergy.com',1,'2026-04-18 08:30:00','2026-07-20 14:30:00'),(5,'CUST-E005','珠海博创半导体有限公司','珠海市香洲区南屏科技工业区屏东六路博创大厦','刘浩然','13500135005','liuhaoran@zhuhai-creation.com',1,'2026-05-22 09:15:00','2026-07-20 14:30:00'),(6,'CUST-E006','惠州亿纬锂能股份有限公司','惠州市惠城区东江高新区亿纬大道1号','周晓东','13400134006','zhouxiaodong@easypower.com',1,'2026-06-08 10:00:00','2026-07-20 14:30:00'),(7,'CUST-A001','广州汽车零部件制造有限公司','广州市花都区汽车城凤凰北路88号汽车零部件产业园','赵志刚','13300133007','zhaozhigang@gz-parts.com',1,'2026-01-20 09:30:00','2026-07-20 14:30:00'),(8,'CUST-A002','中山汽车传动系统有限公司','中山市火炬开发区科技东路16号传动系统科技园','孙磊','13200132008','sunlei@zs-drive.com',1,'2026-03-15 08:00:00','2026-07-20 14:30:00'),(9,'CUST-A003','佛山精密冲压件有限公司','佛山市南海区丹灶镇桂丹西路99号精密制造基地','吴建华','13100131009','wujianhua@fs-stamping.com',1,'2026-04-02 10:30:00','2026-07-20 14:30:00'),(10,'CUST-A004','东莞汽车电子控制系统有限公司','东莞市虎门镇港口大道南段汽车电子科技园C栋','郑伟强','13000130010','zhengweiqiang@dg-autoelec.com',1,'2026-05-10 11:00:00','2026-07-20 14:30:00'),(11,'CUST-H001','佛山美的集团配件有限公司','佛山市顺德区北滘镇美的工业城','黄丽娟','15800158011','huanglj@midea-accessories.com',1,'2026-02-28 09:00:00','2026-07-20 14:30:00'),(12,'CUST-H002','珠海格力电器股份有限公司配件部','珠海市香洲区前山金鸡西路格力工业园','林国强','15900159012','linguoguo@gree-parts.com',1,'2026-03-20 10:00:00','2026-07-20 14:30:00'),(13,'CUST-H003','中山华帝燃具股份有限公司品质部','中山市黄圃镇华帝工业城','杨慧芳','15600156013','yanghuifang@vatti.com',1,'2026-06-15 08:30:00','2026-07-20 14:30:00'),(14,'CUST-M001','深圳迈瑞医疗国际股份有限公司','深圳市南山区高新南一道10号迈瑞大厦','许文博','15500155014','xuwenbo@mindray.com',1,'2026-07-01 09:00:00','2026-07-20 14:30:00');
/*!40000 ALTER TABLE `customers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `defect_codes`
--

DROP TABLE IF EXISTS `defect_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `defect_codes` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `defect_code` varchar(50) NOT NULL,
  `defect_name` varchar(200) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  `defect_category` varchar(50) DEFAULT NULL,
  `defect_severity` varchar(10) DEFAULT NULL,
  `is_reworkable` tinyint(1) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_defect_codes_defect_code` (`defect_code`)
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `defect_codes`
--

LOCK TABLES `defect_codes` WRITE;
/*!40000 ALTER TABLE `defect_codes` DISABLE KEYS */;
INSERT INTO `defect_codes` VALUES (1,'D001','尺寸超差','零部件关键尺寸超出图纸公差范围','尺寸','MA',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(2,'D006','螺纹不合格','螺纹牙型、中径或旋合长度不符合标准要求','尺寸','MA',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(3,'D009','平面度超差','工件平面度超出允许偏差，影响装配贴合','尺寸','MI',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(4,'D010','同心度超差','回转体各截面同心度超出公差，导致旋转不平衡','尺寸','MA',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(5,'D011','孔径偏小','通孔或盲孔直径低于下限，影响配合件装配','尺寸','MA',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(6,'D002','表面划伤','工件表面有线性划痕，深度未超过允许值','外观','MI',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(7,'D003','裂纹','工件表面或内部存在裂纹，存在断裂风险','外观','CR',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(8,'D007','氧化生锈','金属表面氧化变色或出现锈斑','外观','MI',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(9,'D012','毛刺','工件边缘或孔口存在多余金属突起','外观','MI',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(10,'D013','磕碰变形','工件在搬运或存储过程中遭受磕碰导致局部变形','外观','MA',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(11,'D014','色差','涂覆或电镀层颜色与标准样板存在可见差异','外观','MI',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(12,'D015','流挂','涂装后漆膜局部过厚，形成流挂痕迹','外观','MI',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(13,'D004','硬度不足','热处理后硬度值低于图纸或标准要求','功能','MA',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(14,'D008','装配不良','零部件装配后出现松动、卡滞或间隙不当','功能','MA',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(15,'D016','密封泄漏','密封面存在泄漏，无法通过气密性或液密性测试','功能','CR',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(16,'D017','电气短路','电路中存在异常导电路径，可能导致设备损坏','功能','CR',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(17,'D018','绝缘不良','绝缘电阻或耐压测试不达标','功能','CR',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(18,'D005','材料夹杂','金属基体中含有非金属夹杂物，影响力学性能','材料','CR',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(19,'D019','材质不符','来料化学成分或金相组织与牌号要求不一致','材料','CR',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(20,'D020','砂眼气孔','铸件表面或内部存在砂眼或气孔缺陷','材料','MA',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(21,'D021','包装破损','外包装箱体破损、变形或防潮层损坏','其他','MI',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(22,'D022','标识错误','标签信息（批次号、日期、品名）与实物不符','其他','MA',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(23,'D023','数量短缺','交付数量少于送货单或订单数量','其他','MA',0,1,'2026-07-27 06:30:19','2026-07-27 06:30:19');
/*!40000 ALTER TABLE `defect_codes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dynamic_params`
--

DROP TABLE IF EXISTS `dynamic_params`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dynamic_params` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `group_id` bigint NOT NULL,
  `name` varchar(200) NOT NULL,
  `code` varchar(50) NOT NULL,
  `data_type` varchar(20) NOT NULL,
  `unit` varchar(20) DEFAULT NULL,
  `target_value` decimal(10,4) DEFAULT NULL,
  `usl` decimal(10,4) DEFAULT NULL,
  `lsl` decimal(10,4) DEFAULT NULL,
  `precision` decimal(10,4) DEFAULT NULL,
  `ai_strategy` text,
  `sort_order` int DEFAULT '0',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_by` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`),
  KEY `group_id` (`group_id`),
  CONSTRAINT `dynamic_params_ibfk_1` FOREIGN KEY (`group_id`) REFERENCES `param_groups` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dynamic_params`
--

LOCK TABLES `dynamic_params` WRITE;
/*!40000 ALTER TABLE `dynamic_params` DISABLE KEYS */;
INSERT INTO `dynamic_params` VALUES (1,1,'模具温度','TEMP_MOLD','numeric','℃',180.0000,190.0000,170.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(2,1,'熔体温度','TEMP_MELT','numeric','℃',260.0000,275.0000,245.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":3}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(3,1,'冷却温度','TEMP_COOL','numeric','℃',45.0000,55.0000,35.0000,1.0000,'{\"rule\":\"range\",\"threshold\":10}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(4,1,'注射压力','INJ_PRESS','numeric','MPa',85.0000,95.0000,75.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(5,1,'注射速度','INJ_SPEED','numeric','mm/s',60.0000,70.0000,50.0000,0.1000,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(6,1,'保压压力','HOLD_PRESS','numeric','MPa',45.0000,52.0000,38.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(7,2,'拉伸强度','TENSILE_STR','numeric','MPa',45.0000,50.0000,40.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(8,2,'延伸率','ELONGATION','numeric','%',15.0000,20.0000,10.0000,0.1000,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(9,2,'硬度','HARDNESS','numeric','Shore D',75.0000,80.0000,70.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(10,2,'冲击强度','IMPACT_STR','numeric','kJ/m²',25.0000,30.0000,20.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(11,3,'外径','OD_DIM','numeric','mm',50.0000,50.0500,49.9500,0.0100,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(12,3,'内径','ID_DIM','numeric','mm',30.0000,30.0300,29.9700,0.0100,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(13,3,'长度','LENGTH_DIM','numeric','mm',100.0000,100.1000,99.9000,0.0100,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(14,3,'壁厚','WALL_THICK','numeric','mm',3.0000,3.1000,2.9000,0.0100,'{\"rule\":\"range\",\"threshold\":0.2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(15,3,' circumference','CIRCUMF','numeric','mm',157.0000,157.5000,156.5000,0.1000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(16,3,'圆度','ROUNDNESS','numeric','mm',0.0200,0.0500,0.0000,0.0010,'{\"rule\":\"mean_shift\",\"threshold\":3}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(17,4,'色差ΔE','COLOR_DIFF','numeric','ΔE',1.5000,3.0000,0.0000,0.1000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(18,4,'划痕长度','SCRATCH_LEN','numeric','mm',0.0000,0.5000,0.0000,0.1000,'{\"rule\":\"attribute\",\"threshold\":0.3}',0,1,1,'2026-07-24 05:15:35','2026-07-24 05:15:35'),(19,4,'翘曲度','WARP_DEG','numeric','mm/m',0.5000,1.0000,0.0000,0.0100,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:15:36','2026-07-24 05:15:36'),(20,4,'表面缺陷','SURF_DEFECT','categorical','',NULL,NULL,NULL,1.0000,'{\"rule\":\"attribute\",\"threshold\":0.5}',0,1,1,'2026-07-24 05:15:44','2026-07-24 05:15:44'),(21,1,'保压时间','HOLD_TIME','numeric','s',20.0000,25.0000,15.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(22,1,'成型周期','CYCLE_TIME','numeric','s',45.0000,55.0000,35.0000,0.1000,'{\"rule\":\"trend\",\"threshold\":3}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(23,1,'料筒前段温度','BARREL_TEMP1','numeric','℃',220.0000,230.0000,210.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(24,1,'料筒中段温度','BARREL_TEMP2','numeric','℃',240.0000,250.0000,230.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(25,2,'弯曲模量','FLEX_MOD','numeric','GPa',2.1000,2.5000,1.8000,0.0100,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(26,2,'弯曲强度','FLEX_STR','numeric','MPa',70.0000,80.0000,60.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(27,2,'扭矩','TORQUE','numeric','N·m',12.0000,15.0000,9.0000,0.1000,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(28,3,'平面度','FLATNESS','numeric','mm',0.1000,0.2000,0.0000,0.0100,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(29,3,'位置度','POSITION','numeric','mm',0.0500,0.1000,0.0000,0.0010,'{\"rule\":\"mean_shift\",\"threshold\":2}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(30,4,'光泽度','G loss','numeric','GU',85.0000,90.0000,80.0000,1.0000,'{\"rule\":\"mean_shift\",\"threshold\":2.5}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36'),(31,4,'毛刺高度','BURR_H','numeric','mm',0.0000,0.1000,0.0000,0.0100,'{\"rule\":\"mean_shift\",\"threshold\":3}',0,1,1,'2026-07-24 05:17:36','2026-07-24 05:17:36');
/*!40000 ALTER TABLE `dynamic_params` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `equipment`
--

DROP TABLE IF EXISTS `equipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `equipment` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `equipment_code` varchar(50) NOT NULL,
  `equipment_name` varchar(200) NOT NULL,
  `model` varchar(100) DEFAULT NULL,
  `production_line` varchar(100) DEFAULT NULL,
  `workshop` varchar(100) DEFAULT NULL,
  `status` varchar(20) DEFAULT 'idle',
  `equipment_type` varchar(50) DEFAULT NULL,
  `has_mqtt_connection` tinyint(1) DEFAULT '0',
  `mqtt_topic_prefix` varchar(500) DEFAULT NULL,
  `org_id` bigint DEFAULT NULL,
  `workshop_id` bigint DEFAULT NULL,
  `line_id` bigint DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_equipment_equipment_code` (`equipment_code`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `equipment`
--

LOCK TABLES `equipment` WRITE;
/*!40000 ALTER TABLE `equipment` DISABLE KEYS */;
INSERT INTO `equipment` VALUES (1,'CNC-001','数控车床 #1','CK6140','A线','机加车间','running','CNC',1,'factory/line-a/cnc-001',2,4,8,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(2,'CNC-002','数控车床 #2','CK6150','A线','机加车间','running','CNC',1,'factory/line-a/cnc-002',2,4,8,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(3,'CNC-003','数控铣床 #1','XK714','B线','机加车间','running','CNC',1,'factory/line-b/cnc-003',2,4,9,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(4,'CNC-004','数控车床 #3','CK6180','C线','机加车间','running','CNC',1,'factory/line-c/cnc-004',2,4,10,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(5,'VMC-001','立式加工中心','VMC850','B线','机加车间','idle','CNC',1,'factory/line-b/vmc-001',2,4,9,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(6,'HT-001','热处理炉','RX3-45-12','热处理线','热处理车间','running','PLC',1,'factory/heat/ht-001',2,5,11,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(7,'HT-002','回火炉','RX3-90-15','热处理线','热处理车间','maintenance','PLC',1,'factory/heat/ht-002',2,5,11,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(8,'GR-001','磨床 #1','M7130','A线','机加车间','idle','PLC',0,NULL,2,4,8,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(9,'GR-002','平面磨床','M7520H','B线','机加车间','running','PLC',1,'factory/line-b/gr-002',2,4,9,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(10,'CMM-001','三坐标测量机','ZEISS CONTURA','质量检测线','质量中心','running','检测设备',0,NULL,2,7,14,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(11,'ROBOT-WLD-01','焊接机器人 #1','IRB 2600-16/1.5','装配1线','装配车间','running','机器人',1,'factory/assy-1/robot-wld-01',2,6,12,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(12,'ROBOT-INS-01','装配机器人 #1','IRB 4600-60/2.05','装配1线','装配车间','fault','机器人',1,'factory/assy-1/robot-ins-01',2,6,12,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(13,'ROBOT-PCK-01','包装机器人','IRB 14000-12/1.6','装配2线','装配车间','running','机器人',1,'factory/assy-2/robot-pck-01',2,6,13,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(14,'INJ-001','注塑机 #1','MASTERY 180','C线','注塑车间','running','CNC',1,'factory/line-c/inj-001',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(15,'PKG-001','自动包装机','PackPro-3000','包装线','包装车间','idle','PLC',1,'factory/pack/pkg-001',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(16,'PRESS-001','冲压床 #1','SMD-60/1.5','冲压线','冲压车间','running','PLC',1,'factory/stamp/press-001',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(17,'PRESS-002','冲压床 #2','SMD-100/2.5','冲压线','冲压车间','running','PLC',1,'factory/stamp/press-002',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(18,'PRESS-003','精密冲床','FAX-200-3','冲压线','冲压车间','maintenance','PLC',1,'factory/stamp/press-003',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(19,'PLATE-001','电镀线','HD-6000','表面处理线','表面处理车间','running','PLC',1,'factory/surface/plate-001',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(20,'PAINT-001','自动喷枪','Gema 3920','涂装线','表面处理车间','running','机器人',1,'factory/paint/paint-001',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(21,'SMT-001','SMT贴片机','YCM SA10N','SMT线','电子车间','running','CNC',1,'factory/smt/smt-001',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(22,'REFLOW-001','回流焊炉','HR-408N','SMT线','电子车间','running','PLC',1,'factory/smt/reflow-001',2,NULL,NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19');
/*!40000 ALTER TABLE `equipment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inspection_items`
--

DROP TABLE IF EXISTS `inspection_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inspection_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `item_code` varchar(50) NOT NULL,
  `item_name` varchar(200) NOT NULL,
  `description` text,
  `data_type` varchar(20) DEFAULT NULL,
  `unit` varchar(20) DEFAULT NULL,
  `usl` decimal(10,4) DEFAULT NULL,
  `lsl` decimal(10,4) DEFAULT NULL,
  `target_value` decimal(10,4) DEFAULT NULL,
  `chart_type` varchar(20) DEFAULT NULL,
  `subgroup_size` int DEFAULT NULL,
  `inspection_method` varchar(200) DEFAULT NULL,
  `sample_size` int DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  `created_by` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `data_collection_param_code` varchar(50) DEFAULT NULL,
  `ucl` decimal(15,6) DEFAULT NULL,
  `lcl` decimal(15,6) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `item_code` (`item_code`),
  KEY `idx_item_code` (`item_code`),
  KEY `idx_is_active` (`is_active`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inspection_items`
--

LOCK TABLES `inspection_items` WRITE;
/*!40000 ALTER TABLE `inspection_items` DISABLE KEYS */;
INSERT INTO `inspection_items` VALUES (1,'II-001','外径','轴类零件外径尺寸检验','numeric','mm',50.0500,49.9500,50.0000,'Xbar_R',5,'千分尺',5,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(2,'II-002','内径','孔类零件内径尺寸检验','numeric','mm',25.0300,24.9700,25.0000,'Xbar_R',5,'内径千分尺',5,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(3,'II-003','长度','零件长度尺寸检验','numeric','mm',100.1000,99.9000,100.0000,'Xbar_R',5,'游标卡尺',5,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(4,'II-004','表面粗糙度 Ra','加工表面粗糙度检验','numeric','μm',1.6000,0.0000,0.8000,'I_MR',1,'粗糙度仪',3,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(5,'II-005','硬度 HRC','热处理后硬度检验','numeric','HRC',58.0000,52.0000,55.0000,'I_MR',1,'硬度计',2,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(6,'II-006','外观检查','产品外观质量检查（划伤、锈蚀、毛刺等）','visual','-',NULL,NULL,NULL,NULL,NULL,'目视检查',NULL,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(7,'II-007','螺纹精度','螺纹尺寸及精度检验','attribute','-',NULL,NULL,NULL,NULL,NULL,'螺纹规',NULL,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(8,'II-008','直线度','轴类零件直线度检验','numeric','mm',0.0500,0.0000,0.0200,'I_MR',1,'百分表',1,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(9,'II-009','圆度','圆形截面圆度检验','numeric','mm',0.0300,0.0000,0.0150,'I_MR',1,'圆度仪',1,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(10,'II-010','化学成分 C%','钢材碳含量检验','numeric','%',0.4500,0.4200,0.4300,'I_MR',1,'光谱分析仪',1,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(11,'II-011','密封性测试','产品密封性能检验','attribute','-',NULL,NULL,NULL,NULL,NULL,'气密测试仪',NULL,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL),(12,'II-012','包装完整性','出厂包装完整性检查','visual','-',NULL,NULL,NULL,NULL,NULL,'目视检查',NULL,1,1,'2026-07-27 06:34:41','2026-07-27 06:34:41',NULL,NULL,NULL);
/*!40000 ALTER TABLE `inspection_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inspection_plan_items`
--

DROP TABLE IF EXISTS `inspection_plan_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inspection_plan_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `plan_id` bigint NOT NULL,
  `inspection_item_id` bigint NOT NULL,
  `sort_order` int DEFAULT '0',
  `target_value` decimal(15,6) DEFAULT NULL,
  `ucl` decimal(15,6) DEFAULT NULL,
  `lcl` decimal(15,6) DEFAULT NULL,
  `sample_size` int DEFAULT NULL,
  `usl` decimal(10,4) DEFAULT NULL,
  `lsl` decimal(10,4) DEFAULT NULL,
  `is_required` tinyint(1) DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `plan_id` (`plan_id`),
  KEY `inspection_item_id` (`inspection_item_id`),
  CONSTRAINT `inspection_plan_items_ibfk_1` FOREIGN KEY (`plan_id`) REFERENCES `inspection_plans` (`id`) ON DELETE CASCADE,
  CONSTRAINT `inspection_plan_items_ibfk_2` FOREIGN KEY (`inspection_item_id`) REFERENCES `inspection_items` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inspection_plan_items`
--

LOCK TABLES `inspection_plan_items` WRITE;
/*!40000 ALTER TABLE `inspection_plan_items` DISABLE KEYS */;
INSERT INTO `inspection_plan_items` VALUES (1,4,1,1,50.000000,NULL,NULL,5,50.0500,49.9500,1),(2,4,2,2,30.000000,NULL,NULL,5,30.0300,29.9700,1),(3,4,5,3,58.000000,NULL,NULL,3,NULL,NULL,1),(4,4,4,4,1.200000,NULL,NULL,3,1.6000,0.8000,1),(5,5,1,1,120.000000,NULL,NULL,5,120.1000,119.9000,1),(6,5,3,2,80.000000,NULL,NULL,5,80.0500,79.9500,1),(7,5,6,3,NULL,NULL,NULL,1,NULL,NULL,1),(8,5,12,4,NULL,NULL,NULL,1,NULL,NULL,0),(9,6,1,1,2.000000,NULL,NULL,10,2.0500,1.9500,1),(10,6,2,2,1.500000,NULL,NULL,10,1.5500,1.4500,1),(11,6,3,3,100.000000,NULL,NULL,10,100.0500,99.9500,1),(12,6,9,4,0.000000,NULL,NULL,5,0.0500,-0.0500,1),(13,7,1,1,50.000000,50.060000,49.940000,10,50.0300,49.9700,1),(14,7,2,2,30.000000,30.040000,29.960000,10,30.0200,29.9800,1),(15,7,8,3,0.000000,0.040000,-0.040000,5,0.0200,-0.0200,1),(16,7,9,4,0.000000,0.060000,-0.060000,5,0.0300,-0.0300,1),(17,7,4,5,0.600000,NULL,NULL,3,0.8000,0.4000,1),(18,7,5,6,58.000000,NULL,NULL,3,NULL,NULL,1),(19,8,6,1,NULL,NULL,NULL,1,NULL,NULL,1),(20,8,11,2,NULL,NULL,NULL,5,NULL,NULL,1),(21,8,12,3,NULL,NULL,NULL,1,NULL,NULL,1),(22,9,10,1,99.500000,NULL,NULL,3,NULL,NULL,1),(23,9,1,2,1.000000,NULL,NULL,5,1.0500,0.9500,1),(24,9,6,3,NULL,NULL,NULL,1,NULL,NULL,1),(25,9,12,4,NULL,NULL,NULL,1,NULL,NULL,0);
/*!40000 ALTER TABLE `inspection_plan_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inspection_plans`
--

DROP TABLE IF EXISTS `inspection_plans`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inspection_plans` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `plan_code` varchar(50) NOT NULL,
  `plan_name` varchar(200) NOT NULL,
  `inspection_type` varchar(10) DEFAULT NULL,
  `description` text,
  `product_id` bigint DEFAULT NULL,
  `material_id` bigint DEFAULT NULL,
  `supplier_id` bigint DEFAULT NULL,
  `customer_id` bigint DEFAULT NULL,
  `process_id` bigint DEFAULT NULL,
  `equipment_id` bigint DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  `created_by` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `plan_code` (`plan_code`),
  KEY `product_id` (`product_id`),
  KEY `supplier_id` (`supplier_id`),
  KEY `customer_id` (`customer_id`),
  KEY `process_id` (`process_id`),
  KEY `equipment_id` (`equipment_id`),
  CONSTRAINT `inspection_plans_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_2` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_3` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_4` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_5` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inspection_plans`
--

LOCK TABLES `inspection_plans` WRITE;
/*!40000 ALTER TABLE `inspection_plans` DISABLE KEYS */;
INSERT INTO `inspection_plans` VALUES (4,'PLAN-IQC-001','精密转轴A100来料检验计划','IQC','精密转轴A100原材料来料全检，涵盖外径、内径、硬度、表面粗糙度',2,NULL,1,NULL,NULL,NULL,1,1,'2026-07-27 07:20:09','2026-07-27 07:20:09'),(5,'PLAN-IQC-002','壳体B200来料检验计划','IQC','铝合金壳体来料外观及尺寸检验',3,NULL,2,NULL,NULL,NULL,1,1,'2026-07-27 07:20:09','2026-07-27 07:20:09'),(6,'PLAN-IPQC-001','PCB主板C300过程巡检计划','IPQC','PCB主板焊接及组装过程关键尺寸巡检',4,NULL,NULL,NULL,NULL,NULL,1,1,'2026-07-27 07:20:09','2026-07-27 07:20:09'),(7,'PLAN-FQC-001','精密转轴A100成品终检计划','FQC','精密转轴成品出厂前全数终检',2,NULL,1,NULL,NULL,NULL,1,1,'2026-07-27 07:20:09','2026-07-27 07:20:09'),(8,'PLAN-OQC-001','密封圈D400出货检验计划','OQC','密封圈成品出厂前外观及密封性抽检',5,NULL,7,NULL,NULL,NULL,1,1,'2026-07-27 07:20:09','2026-07-27 07:20:09'),(9,'PLAN-IQC-003','连接线束E500来料检验计划','IQC','连接线束原材料导电性能及外观检验',6,NULL,8,NULL,NULL,NULL,1,1,'2026-07-27 07:20:09','2026-07-27 07:20:09');
/*!40000 ALTER TABLE `inspection_plans` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inspection_standards`
--

DROP TABLE IF EXISTS `inspection_standards`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inspection_standards` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `standard_code` varchar(50) NOT NULL,
  `standard_name` varchar(200) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  `inspection_type` varchar(10) DEFAULT NULL,
  `product_id` bigint DEFAULT NULL,
  `process_id` bigint DEFAULT NULL,
  `item_name` varchar(200) DEFAULT NULL,
  `usl` decimal(10,4) DEFAULT NULL,
  `lsl` decimal(10,4) DEFAULT NULL,
  `target` decimal(10,4) DEFAULT NULL,
  `unit` varchar(20) DEFAULT NULL,
  `inspection_method` varchar(200) DEFAULT NULL,
  `sampling_frequency` varchar(200) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_inspection_standards_standard_code` (`standard_code`),
  KEY `product_id` (`product_id`),
  KEY `process_id` (`process_id`),
  CONSTRAINT `inspection_standards_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_standards_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=38 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inspection_standards`
--

LOCK TABLES `inspection_standards` WRITE;
/*!40000 ALTER TABLE `inspection_standards` DISABLE KEYS */;
INSERT INTO `inspection_standards` VALUES (1,'STD-001','外径直径检验','精密转轴精车后外径尺寸检验，公差 ±0.05mm','IPQC',2,8,'外径',50.0500,49.9500,50.0000,'mm','千分尺','每批次5件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(2,'STD-004','长度尺寸检验','精密转轴精车后长度尺寸检验，公差 ±0.1mm','IPQC',2,8,'长度',100.1000,99.9000,100.0000,'mm','游标卡尺','每批次5件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(3,'STD-005','轴承座直径检验','精密转轴轴承安装位外径检验，配合公差 h6','IPQC',2,11,'轴承座外径',24.9800,24.9500,24.9600,'mm','千分尺','每批次3件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(4,'STD-006','螺纹精度检验','转轴末端外螺纹 M10×1.25 精度检验','IPQC',2,9,'螺纹 M10×1.25',NULL,NULL,NULL,'-','螺纹规（通止规）','每班次2件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(5,'STD-010','直线度检验','精密转轴全轴直线度检验，允许偏差 ≤0.05mm','FQC',2,13,'直线度',0.0500,0.0000,0.0200,'mm','百分表 + V型铁','每批次3件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(6,'STD-011','圆度检验','精密转轴圆形截面圆度检验，允许偏差 ≤0.03mm','FQC',2,13,'圆度',0.0300,0.0000,0.0150,'mm','圆度仪','每批次2件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(7,'STD-012','粗糙度 Ra 检验','精密转轴加工面表面粗糙度 Ra 值检验','FQC',2,13,'表面粗糙度 Ra',1.6000,0.0000,0.8000,'μm','粗糙度仪','每批次3件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(8,'STD-013','外观综合检验','精密转轴出厂前外观检查：划伤、锈蚀、毛刺、磕碰','FQC',2,13,'外观',NULL,NULL,NULL,'-','目视检查 + 标准样板','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(9,'STD-025','包装完整性检验','精密转轴出厂包装完整性及标识核对','OQC',2,15,'包装完整性',NULL,NULL,NULL,'-','目视检查','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(10,'STD-026','标签标识核对','产品标签信息（批次号、品名、数量）与送货单一致性核对','OQC',2,15,'标签标识',NULL,NULL,NULL,'-','目视核对','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(11,'STD-007','外观质量检验','压铸件表面无砂眼、气孔、裂纹、冷隔等缺陷','IQC',3,6,'外观',NULL,NULL,NULL,'-','目视检查 + 标准缺陷样板','每批次抽检10%',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(12,'STD-008','外形尺寸检验','壳体关键外形尺寸：长×宽×高，公差 ±0.2mm','IQC',3,6,'外形尺寸 长×宽×高',200.2000,199.8000,200.0000,'mm','游标卡尺','每批次5件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(13,'STD-009','重量检验','壳体单件重量检验，标准重量 2.5kg ±5%','IQC',3,6,'重量',2.6250,2.3750,2.5000,'kg','电子秤','每批次3件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(14,'STD-014','机加工孔径检验','壳体螺栓孔 Φ8H7 孔径检验','IPQC',3,9,'孔径 Φ8H7',8.0150,8.0000,8.0080,'mm','内径千分尺','每班次3件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(15,'STD-015','壁厚检验','壳体关键部位壁厚检验，标准 3.0mm ±0.1mm','IPQC',3,13,'壁厚',3.1000,2.9000,3.0000,'mm','超声波测厚仪','每批次3件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(16,'STD-016','螺纹孔检验','壳体 M6 螺纹孔通止规检验','IPQC',3,9,'螺纹 M6',NULL,NULL,NULL,'-','螺纹规（通止规）','每班次2件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(17,'STD-017','表面处理质量检验','壳体阳极氧化/喷粉涂层质量检验：膜厚、附着力、颜色','FQC',3,13,'表面处理',15.0000,8.0000,12.0000,'μm','涂层测厚仪 + 百格法','每批次3件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(18,'STD-018','壳体外观终检','壳体成品外观终检：飞边、变形、色泽一致性','FQC',3,13,'外观',NULL,NULL,NULL,'-','目视检查 + 标准光源箱','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(19,'STD-027','壳体包装防护检验','壳体出厂包装防护：泡沫内衬到位、防撞措施','OQC',3,15,'包装防护',NULL,NULL,NULL,'-','目视检查','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(20,'STD-019','PCB 外观检验','PCB 基板外观检查：铜箔氧化、划痕、分层、字符清晰度','IQC',4,6,'外观',NULL,NULL,NULL,'-','目视检查 + 10倍放大镜','每批次抽检20%',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(21,'STD-020','板厚检验','PCB 基板厚度检验，标准 1.6mm ±0.15mm','IQC',4,6,'板厚',1.7500,1.4500,1.6000,'mm','千分尺','每批次3片',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(22,'STD-021','阻值检验','贴片电阻 10KΩ 阻值检验，偏差 ±1%','IQC',4,6,'电阻值 10KΩ',10.1000,9.9000,10.0000,'KΩ','LCR 电桥','每批次5pcs',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(23,'STD-022','焊接质量检验','SMT 贴片焊接质量：虚焊、连锡、偏移、立碑','IPQC',4,12,'焊接质量',NULL,NULL,NULL,'-','AOI 自动光学检测 + 目视复检','每班次抽检10%',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(24,'STD-023','电气功能测试','PCB 主板通电功能测试：供电、通信、IO 输出','FQC',4,13,'电气功能',NULL,NULL,NULL,'-','专用测试治具 + 固件烧录','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(25,'STD-024','绝缘电阻测试','PCB 主板绝缘电阻测试，标准 ≥100MΩ @500VDC','FQC',4,13,'绝缘电阻',NULL,100.0000,NULL,'MΩ','绝缘电阻测试仪','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(26,'STD-028','PCB 防静电包装检验','PCB 主板防静电包装检查：防静电袋密封、干燥剂、标签','OQC',4,15,'防静电包装',NULL,NULL,NULL,'-','目视检查','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(27,'STD-029','密封圈外观检验','橡胶密封圈外观：气泡、杂质、飞边、变形','IQC',5,6,'外观',NULL,NULL,NULL,'-','目视检查','每批次抽检10%',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(28,'STD-030','外径与线径检验','密封圈外径和截面直径检验，配合公差','IQC',5,6,'外径/线径',25.1500,24.8500,25.0000,'mm','游标卡尺','每批次5件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(29,'STD-031','硬度检验','橡胶密封圈邵氏硬度检验，标准 70A ±5','FQC',5,13,'硬度 Shore A',75.0000,65.0000,70.0000,'Shore A','邵氏硬度计','每批次3件',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(30,'STD-032','密封性测试','密封圈气密性测试：加压 0.3MPa 保持 30s 无泄漏','FQC',5,13,'密封性',NULL,0.3000,0.3000,'MPa','气密测试仪','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(31,'STD-033','包装与标识检验','密封圈 PE 自封袋包装、干燥剂、标签信息核对','OQC',5,15,'包装标识',NULL,NULL,NULL,'-','目视检查','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(32,'STD-034','线材外观检验','PVC 护套线外观：绝缘层破损、色差、直径偏差','IQC',6,6,'线材外观',NULL,NULL,NULL,'-','目视检查 + 卡尺','每批次抽检10%',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(33,'STD-035','端子压接质量检验','端子 HT-05B 压接质量：拉拔力、外观、位移量','IQC',6,6,'端子压接力',NULL,80.0000,120.0000,'N','拉拔力测试仪','每批次5pcs',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(34,'STD-036','导通测试','线束各线路导通性测试，标准电阻 ≤0.1Ω','FQC',6,13,'导通电阻',0.1000,0.0000,0.0500,'Ω','万用表/导通测试仪','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(35,'STD-037','绝缘电阻测试','线束线间及线对地绝缘电阻测试，标准 ≥100MΩ','FQC',6,13,'绝缘电阻',NULL,100.0000,NULL,'MΩ','兆欧表 500VDC','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(36,'STD-038','外观与尺寸检验','线束成品外观：热缩管收缩到位、扎带固定、总长度','FQC',6,13,'外观/总长度',1505.0000,1495.0000,1500.0000,'mm','卷尺 + 目视','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(37,'STD-039','包装完整性检验','线束 PE 包装袋密封性、产品标签、数量核对','OQC',6,15,'包装完整性',NULL,NULL,NULL,'-','目视检查','全检',1,'2026-07-27 06:30:19','2026-07-27 06:30:19');
/*!40000 ALTER TABLE `inspection_standards` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `iqc_anomalies`
--

DROP TABLE IF EXISTS `iqc_anomalies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `iqc_anomalies` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `anomaly_no` varchar(50) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `receipt_id` bigint DEFAULT NULL,
  `inspection_id` bigint DEFAULT NULL,
  `anomaly_type` varchar(20) DEFAULT NULL,
  `severity` varchar(10) DEFAULT NULL,
  `description` text,
  `status` varchar(20) DEFAULT 'open',
  `handler` varchar(100) DEFAULT NULL,
  `resolved_at` datetime DEFAULT NULL,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `anomaly_no` (`anomaly_no`),
  KEY `receipt_id` (`receipt_id`),
  CONSTRAINT `iqc_anomalies_ibfk_1` FOREIGN KEY (`receipt_id`) REFERENCES `iqc_receipts` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `iqc_anomalies`
--

LOCK TABLES `iqc_anomalies` WRITE;
/*!40000 ALTER TABLE `iqc_anomalies` DISABLE KEYS */;
/*!40000 ALTER TABLE `iqc_anomalies` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `iqc_inspection_items`
--

DROP TABLE IF EXISTS `iqc_inspection_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `iqc_inspection_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `inspection_id` bigint NOT NULL,
  `inspection_item_id` bigint DEFAULT NULL,
  `item_name` varchar(200) DEFAULT NULL,
  `measured_value` decimal(10,4) DEFAULT NULL,
  `usl` decimal(10,4) DEFAULT NULL,
  `lsl` decimal(10,4) DEFAULT NULL,
  `result` varchar(10) DEFAULT NULL,
  `defect_code_id` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `inspection_id` (`inspection_id`),
  KEY `defect_code_id` (`defect_code_id`),
  CONSTRAINT `iqc_inspection_items_ibfk_1` FOREIGN KEY (`inspection_id`) REFERENCES `iqc_inspections` (`id`) ON DELETE CASCADE,
  CONSTRAINT `iqc_inspection_items_ibfk_2` FOREIGN KEY (`defect_code_id`) REFERENCES `defect_codes` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `iqc_inspection_items`
--

LOCK TABLES `iqc_inspection_items` WRITE;
/*!40000 ALTER TABLE `iqc_inspection_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `iqc_inspection_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `iqc_inspections`
--

DROP TABLE IF EXISTS `iqc_inspections`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `iqc_inspections` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `inspection_no` varchar(50) NOT NULL,
  `receipt_id` bigint NOT NULL,
  `standard_id` bigint DEFAULT NULL,
  `sample_size` int DEFAULT NULL,
  `ac` int DEFAULT NULL,
  `re` int DEFAULT NULL,
  `defect_qty` int DEFAULT NULL,
  `sampling_level` varchar(10) DEFAULT NULL,
  `aql_value` decimal(5,2) DEFAULT NULL,
  `result` varchar(10) DEFAULT NULL,
  `inspector` varchar(100) DEFAULT NULL,
  `inspected_at` datetime DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `inspection_no` (`inspection_no`),
  KEY `receipt_id` (`receipt_id`),
  KEY `idx_standard_id` (`standard_id`),
  CONSTRAINT `iqc_inspections_ibfk_1` FOREIGN KEY (`receipt_id`) REFERENCES `iqc_receipts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `iqc_inspections`
--

LOCK TABLES `iqc_inspections` WRITE;
/*!40000 ALTER TABLE `iqc_inspections` DISABLE KEYS */;
/*!40000 ALTER TABLE `iqc_inspections` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `iqc_receipts`
--

DROP TABLE IF EXISTS `iqc_receipts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `iqc_receipts` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `receipt_no` varchar(50) NOT NULL,
  `supplier_id` bigint DEFAULT NULL,
  `product_id` bigint DEFAULT NULL,
  `batch_no` varchar(100) DEFAULT NULL,
  `quantity` decimal(10,2) DEFAULT NULL,
  `unit` varchar(20) DEFAULT NULL,
  `receipt_date` datetime DEFAULT NULL,
  `inspector` varchar(100) DEFAULT NULL,
  `status` varchar(20) DEFAULT 'pending',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `receipt_no` (`receipt_no`),
  KEY `supplier_id` (`supplier_id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `iqc_receipts_ibfk_1` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`) ON DELETE SET NULL,
  CONSTRAINT `iqc_receipts_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `iqc_receipts`
--

LOCK TABLES `iqc_receipts` WRITE;
/*!40000 ALTER TABLE `iqc_receipts` DISABLE KEYS */;
/*!40000 ALTER TABLE `iqc_receipts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `organizations`
--

DROP TABLE IF EXISTS `organizations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `organizations` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `code` varchar(50) NOT NULL,
  `name` varchar(200) NOT NULL,
  `level` varchar(20) NOT NULL,
  `parent_id` bigint DEFAULT NULL,
  `sort_order` int DEFAULT '0',
  `is_active` tinyint(1) DEFAULT '1',
  `location` varchar(500) DEFAULT NULL,
  `contact` json DEFAULT NULL,
  `description` text,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`),
  KEY `parent_id` (`parent_id`),
  CONSTRAINT `organizations_ibfk_1` FOREIGN KEY (`parent_id`) REFERENCES `organizations` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=15 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `organizations`
--

LOCK TABLES `organizations` WRITE;
/*!40000 ALTER TABLE `organizations` DISABLE KEYS */;
INSERT INTO `organizations` VALUES (1,'HQ','集团总部','group',NULL,1,1,NULL,NULL,NULL,'2026-07-24 04:16:24','2026-07-24 04:16:24',NULL),(2,'FACTORY_1','第一工厂','company',1,1,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(3,'FACTORY_2','第二工厂','company',1,2,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(4,'WS_MACHINING','机加车间','workshop',2,1,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(5,'WS_HEAT_TREAT','热处理车间','workshop',2,2,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(6,'WS_ASSEMBLY','装配车间','workshop',3,1,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(7,'WS_QUALITY','质量中心','workshop',3,2,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(8,'LINE_A','A线','line',4,1,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(9,'LINE_B','B线','line',4,2,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(10,'LINE_C','C线','line',5,1,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(11,'LINE_HEAT','热处理线','line',5,2,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(12,'LINE_ASSY_1','装配1线','line',6,1,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(13,'LINE_ASSY_2','装配2线','line',6,2,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL),(14,'LINE_QC','质量检测线','line',7,1,1,NULL,NULL,NULL,'2026-07-24 04:16:25','2026-07-24 04:16:25',NULL);
/*!40000 ALTER TABLE `organizations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `param_groups`
--

DROP TABLE IF EXISTS `param_groups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `param_groups` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `code` varchar(50) NOT NULL,
  `description` text,
  `sort_order` int DEFAULT '0',
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_by` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `param_groups`
--

LOCK TABLES `param_groups` WRITE;
/*!40000 ALTER TABLE `param_groups` DISABLE KEYS */;
INSERT INTO `param_groups` VALUES (1,'热力学参数组','thermo_params','温度、热量相关工艺参数',1,1,1,'2026-07-24 04:20:23','2026-07-24 04:20:23'),(2,'力学参数组','mech_params','压力、力值、扭矩等参数',2,1,1,'2026-07-24 04:20:23','2026-07-24 04:20:23'),(3,'尺寸参数组','dim_params','长度、直径、公差等尺寸参数',3,1,1,'2026-07-24 04:20:23','2026-07-24 04:20:23'),(4,'外观参数组','visual_params','外观、表面质量相关参数',4,1,1,'2026-07-24 04:20:23','2026-07-24 04:20:23');
/*!40000 ALTER TABLE `param_groups` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `param_realtime_values`
--

DROP TABLE IF EXISTS `param_realtime_values`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `param_realtime_values` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `param_code` varchar(50) NOT NULL,
  `equipment_id` bigint DEFAULT NULL,
  `value` decimal(10,4) DEFAULT NULL,
  `quality_result` varchar(10) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `timestamp` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_param_timestamp` (`param_code`,`timestamp`),
  KEY `idx_equip_timestamp` (`equipment_id`,`timestamp`)
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `param_realtime_values`
--

LOCK TABLES `param_realtime_values` WRITE;
/*!40000 ALTER TABLE `param_realtime_values` DISABLE KEYS */;
INSERT INTO `param_realtime_values` VALUES (1,'TEMP_MOLD',1,182.3000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:00:00'),(2,'TEMP_MOLD',1,179.8000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:05:00'),(3,'TEMP_MOLD',1,181.5000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:10:00'),(4,'TEMP_MELT',1,258.7000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:00:00'),(5,'TEMP_MELT',1,262.1000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:05:00'),(6,'TEMP_COOL',1,46.2000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:00:00'),(7,'TEMP_COOL',1,44.8000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:05:00'),(8,'INJ_PRESS',1,84.5000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:00:00'),(9,'INJ_PRESS',1,86.2000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:05:00'),(10,'INJ_PRESS',1,91.3000,'WARN',1,'2026-07-24 05:47:54','2026-07-24 08:10:00'),(11,'HOLD_PRESS',1,44.8000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:00:00'),(12,'HOLD_PRESS',1,43.2000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:05:00'),(13,'BARREL_TEMP1',1,221.5000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:00:00'),(14,'BARREL_TEMP1',1,218.9000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:05:00'),(15,'BARREL_TEMP2',1,241.3000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:00:00'),(16,'BARREL_TEMP2',1,239.7000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 08:05:00'),(17,'OD_DIM',2,50.0200,'PASS',1,'2026-07-24 05:47:54','2026-07-24 09:00:00'),(18,'OD_DIM',2,49.9800,'PASS',1,'2026-07-24 05:47:54','2026-07-24 09:05:00'),(19,'OD_DIM',2,50.0600,'FAIL',1,'2026-07-24 05:47:54','2026-07-24 09:10:00'),(20,'ID_DIM',2,30.0100,'PASS',1,'2026-07-24 05:47:54','2026-07-24 09:00:00'),(21,'ID_DIM',2,29.9900,'PASS',1,'2026-07-24 05:47:54','2026-07-24 09:05:00'),(22,'WALL_THICK',2,3.0500,'PASS',1,'2026-07-24 05:47:54','2026-07-24 09:00:00'),(23,'WALL_THICK',2,2.9500,'PASS',1,'2026-07-24 05:47:54','2026-07-24 09:05:00'),(24,'ROUNDNESS',2,0.0300,'PASS',1,'2026-07-24 05:47:54','2026-07-24 09:00:00'),(25,'TENSILE_STR',3,46.2000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 10:00:00'),(26,'TENSILE_STR',3,44.8000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 10:05:00'),(27,'HARDNESS',3,76.1000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 10:00:00'),(28,'HARDNESS',3,73.9000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 10:05:00'),(29,'COLOR_DIFF',4,1.8000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 11:00:00'),(30,'COLOR_DIFF',4,2.5000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 11:05:00'),(31,'COLOR_DIFF',4,3.8000,'FAIL',1,'2026-07-24 05:47:54','2026-07-24 11:10:00'),(32,'SCRATCH_LEN',4,0.3000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 11:00:00'),(33,'SCRATCH_LEN',4,0.1000,'PASS',1,'2026-07-24 05:47:54','2026-07-24 11:05:00'),(34,'BURR_H',4,0.0500,'PASS',1,'2026-07-24 05:47:54','2026-07-24 11:00:00'),(35,'BURR_H',4,0.1200,'FAIL',1,'2026-07-24 05:47:54','2026-07-24 11:05:00');
/*!40000 ALTER TABLE `param_realtime_values` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `permissions`
--

DROP TABLE IF EXISTS `permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `permissions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `code` varchar(200) NOT NULL,
  `module` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `permissions`
--

LOCK TABLES `permissions` WRITE;
/*!40000 ALTER TABLE `permissions` DISABLE KEYS */;
INSERT INTO `permissions` VALUES (1,'全部权限','*:*','System'),(2,'用户管理','system:user','M15'),(3,'角色管理','system:role','M15'),(4,'基础数据管理','basic:data','M02'),(5,'IQC 检验','iqc:inspect','M03'),(6,'IPQC 检验','ipqc:inspect','M04'),(7,'FQC 检验','fqc:inspect','M05'),(8,'SPC 查看','spc:view','M06'),(9,'不良管理','defect:manage','M07'),(10,'质量追溯','trace:view','M08'),(11,'AI 分析','ai:analyze','M10'),(12,'报表查看','report:view','M14');
/*!40000 ALTER TABLE `permissions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `processes`
--

DROP TABLE IF EXISTS `processes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `processes` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `process_code` varchar(100) NOT NULL,
  `process_name` varchar(200) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  `process_type` varchar(50) DEFAULT NULL,
  `department` varchar(100) DEFAULT NULL,
  `org_id` bigint DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_processes_process_code` (`process_code`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `processes`
--

LOCK TABLES `processes` WRITE;
/*!40000 ALTER TABLE `processes` DISABLE KEYS */;
INSERT INTO `processes` VALUES (1,'WX','焊接','焊接工序测试',NULL,NULL,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18'),(2,'ZZ','组装','组装工序测试',NULL,NULL,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18'),(3,'CS','测试','测试工序测试',NULL,NULL,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18'),(4,'BZ','包装','包装工序测试',NULL,NULL,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18'),(5,'JY','检验','检验工序测试',NULL,NULL,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18'),(6,'PRC-01','来料检验',NULL,'检验','品质部',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(7,'PRC-02','粗车',NULL,'加工','机加车间',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(8,'PRC-03','精车',NULL,'加工','机加车间',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(9,'PRC-04','钻孔',NULL,'加工','机加车间',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(10,'PRC-05','热处理',NULL,'加工','热处理车间',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(11,'PRC-06','研磨',NULL,'加工','机加车间',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(12,'PRC-07','过程检验',NULL,'检验','品质部',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(13,'PRC-08','成品检验',NULL,'检验','品质部',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(14,'PRC-09','清洗包装',NULL,'包装','包装车间',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(15,'PRC-10','出货检验',NULL,'检验','品质部',NULL,1,'2026-07-27 06:30:19','2026-07-27 06:30:19');
/*!40000 ALTER TABLE `processes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `product_code` varchar(100) NOT NULL,
  `product_name` varchar(200) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  `product_type` varchar(100) DEFAULT NULL,
  `unit` varchar(20) DEFAULT NULL,
  `default_inspection_level` varchar(10) DEFAULT NULL,
  `default_aql` decimal(5,2) DEFAULT NULL,
  `specification` longtext,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `org_id` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_products_product_code` (`product_code`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES (1,'TEST-001','测试产品 A','用于测试工艺路线功能',NULL,NULL,NULL,NULL,NULL,1,NULL,'2026-07-27 06:30:18','2026-07-27 06:30:18'),(2,'P001','精密转轴 A100',NULL,'机加工件','pcs','II',1.00,NULL,1,NULL,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(3,'P002','壳体 B200',NULL,'压铸件','pcs','II',0.65,NULL,1,NULL,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(4,'P003','PCB 主板 C300',NULL,'电子件','pcs','S-3',0.25,NULL,1,NULL,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(5,'P004','密封圈 D400',NULL,'橡胶件','pcs','I',2.50,NULL,1,NULL,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(6,'P005','连接线束 E500',NULL,'标准件','套','II',1.50,NULL,1,NULL,'2026-07-27 06:30:19','2026-07-27 06:30:19');
/*!40000 ALTER TABLE `products` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `description` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'Administrator','系统管理员，拥有全部权限'),(2,'Operator','质检操作员'),(3,'Inspector','质量检验员'),(4,'Engineer','质量工程师');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `routing_headers`
--

DROP TABLE IF EXISTS `routing_headers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `routing_headers` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `product_id` bigint NOT NULL COMMENT 'æ‰€å±žäº§å“ID',
  `route_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'å·¥è‰ºè·¯çº¿ç¼–å·',
  `route_name` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'å·¥è‰ºè·¯çº¿åç§°',
  `route_type` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'STD' COMMENT 'è·¯çº¿ç±»åž‹',
  `description` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'æè¿°',
  `is_default` tinyint(1) DEFAULT '0' COMMENT 'æ˜¯å¦é»˜è®¤',
  `is_active` tinyint(1) DEFAULT '1' COMMENT 'æ˜¯å¦å¯ç”¨',
  `sort_order` int DEFAULT '0' COMMENT 'æŽ’åº',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_route_code_product` (`route_code`,`product_id`),
  KEY `idx_routing_headers_product` (`product_id`),
  KEY `idx_routing_headers_type` (`route_type`),
  CONSTRAINT `routing_headers_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `routing_headers`
--

LOCK TABLES `routing_headers` WRITE;
/*!40000 ALTER TABLE `routing_headers` DISABLE KEYS */;
INSERT INTO `routing_headers` VALUES (1,1,'STD-001','标准工艺路线','STD',NULL,0,1,0,'2026-07-27 06:30:18','2026-07-27 06:30:18'),(2,2,'RT-P001','精密转轴A100工艺路线','STD','精密转轴A100工艺路线',0,1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(7,1,'A100-TEST','精密测试A100','ALT',NULL,0,1,1,'2026-07-27 06:46:21','2026-07-27 06:46:21'),(8,2,'A100-TEST001','精密测试001','ALT',NULL,0,1,2,'2026-07-27 06:52:42','2026-07-27 06:52:42');
/*!40000 ALTER TABLE `routing_headers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `routing_steps`
--

DROP TABLE IF EXISTS `routing_steps`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `routing_steps` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `routing_header_id` bigint NOT NULL COMMENT '所属路线头ID',
  `step_order` int NOT NULL COMMENT '工序顺序',
  `process_id` bigint DEFAULT NULL COMMENT '关联工序ID',
  `standard_time_minutes` decimal(10,2) DEFAULT '0.00' COMMENT '标准工时（分钟）',
  `description` varchar(500) DEFAULT NULL COMMENT '步骤备注',
  `is_active` tinyint(1) DEFAULT '1' COMMENT '是否启用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `pre_wait_time_minutes` double DEFAULT '0' COMMENT '前置等待时间（分钟）',
  `post_wait_time_minutes` double DEFAULT '0' COMMENT '后置等待时间（分钟）',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_step_order_header` (`routing_header_id`,`step_order`),
  KEY `idx_routing_steps_header` (`routing_header_id`),
  KEY `idx_routing_steps_process` (`process_id`),
  CONSTRAINT `routing_steps_ibfk_1` FOREIGN KEY (`routing_header_id`) REFERENCES `routing_headers` (`id`) ON DELETE CASCADE,
  CONSTRAINT `routing_steps_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `routing_steps`
--

LOCK TABLES `routing_steps` WRITE;
/*!40000 ALTER TABLE `routing_steps` DISABLE KEYS */;
INSERT INTO `routing_steps` VALUES (1,1,1,1,10.00,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18',0,0),(2,1,2,2,15.00,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18',0,0),(3,1,3,3,20.00,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18',0,0),(4,1,4,4,25.00,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18',0,0),(5,1,5,5,30.00,NULL,1,'2026-07-27 06:30:18','2026-07-27 06:30:18',0,0),(6,7,1,1,10.00,NULL,1,'2026-07-27 06:46:21','2026-07-27 06:46:21',NULL,NULL),(7,7,2,2,15.00,NULL,1,'2026-07-27 06:46:21','2026-07-27 06:46:21',NULL,NULL),(8,7,3,3,20.00,NULL,1,'2026-07-27 06:46:21','2026-07-27 06:46:21',NULL,NULL),(9,7,4,4,25.00,NULL,1,'2026-07-27 06:46:21','2026-07-27 06:46:21',NULL,NULL),(10,7,5,5,30.00,NULL,1,'2026-07-27 06:46:21','2026-07-27 06:46:21',NULL,NULL),(11,8,1,1,10.00,NULL,1,'2026-07-27 06:52:42','2026-07-27 06:53:01',NULL,NULL),(12,8,3,2,15.00,NULL,1,'2026-07-27 06:52:42','2026-07-27 06:53:01',NULL,NULL),(13,8,2,3,20.00,'',1,'2026-07-27 06:52:42','2026-07-27 06:54:12',1,3),(14,8,4,4,25.00,NULL,1,'2026-07-27 06:52:42','2026-07-27 06:53:01',NULL,NULL),(15,8,5,5,30.00,NULL,1,'2026-07-27 06:52:42','2026-07-27 06:53:01',NULL,NULL);
/*!40000 ALTER TABLE `routing_steps` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `routings`
--

DROP TABLE IF EXISTS `routings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `routings` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `product_id` bigint NOT NULL,
  `routing_code` varchar(50) NOT NULL,
  `routing_name` varchar(200) DEFAULT NULL,
  `description` text,
  `step_order` int DEFAULT NULL,
  `process_id` bigint DEFAULT NULL,
  `standard_time_minutes` decimal(10,2) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`),
  KEY `process_id` (`process_id`),
  CONSTRAINT `routings_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE,
  CONSTRAINT `routings_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `routings`
--

LOCK TABLES `routings` WRITE;
/*!40000 ALTER TABLE `routings` DISABLE KEYS */;
INSERT INTO `routings` VALUES (1,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',1,6,5.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(2,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',2,7,15.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(3,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',3,8,20.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(4,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',4,9,10.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(5,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',5,10,30.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(6,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',6,11,25.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(7,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',7,12,5.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(8,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',8,13,5.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(9,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',9,14,10.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(10,2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',10,15,5.00,1,'2026-07-27 06:30:19','2026-07-27 06:30:19');
/*!40000 ALTER TABLE `routings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `spc_alert_rules`
--

DROP TABLE IF EXISTS `spc_alert_rules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `spc_alert_rules` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ChartId` bigint NOT NULL,
  `RuleNumber` int NOT NULL,
  `RuleName` varchar(200) NOT NULL,
  `RuleDescription` text,
  `Enabled` tinyint(1) NOT NULL DEFAULT '1',
  `TriggerThreshold` int NOT NULL,
  `SigmaThreshold` decimal(5,2) NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_spc_alert_rules_chart` (`ChartId`),
  CONSTRAINT `spc_alert_rules_ibfk_1` FOREIGN KEY (`ChartId`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `spc_alert_rules`
--

LOCK TABLES `spc_alert_rules` WRITE;
/*!40000 ALTER TABLE `spc_alert_rules` DISABLE KEYS */;
/*!40000 ALTER TABLE `spc_alert_rules` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `spc_alert_triggers`
--

DROP TABLE IF EXISTS `spc_alert_triggers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `spc_alert_triggers` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ChartId` bigint NOT NULL,
  `RuleId` bigint NOT NULL,
  `RuleNumber` int NOT NULL,
  `TriggeredAt` datetime NOT NULL,
  `ViolatedPointIndex` int NOT NULL,
  `Detail` json DEFAULT NULL,
  `Resolved` tinyint(1) NOT NULL DEFAULT '0',
  `ResolvedAt` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_spc_alert_triggers_chart` (`ChartId`),
  KEY `idx_spc_alert_triggers_rule` (`RuleId`),
  CONSTRAINT `spc_alert_triggers_ibfk_1` FOREIGN KEY (`ChartId`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE,
  CONSTRAINT `spc_alert_triggers_ibfk_2` FOREIGN KEY (`RuleId`) REFERENCES `spc_alert_rules` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `spc_alert_triggers`
--

LOCK TABLES `spc_alert_triggers` WRITE;
/*!40000 ALTER TABLE `spc_alert_triggers` DISABLE KEYS */;
/*!40000 ALTER TABLE `spc_alert_triggers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `spc_analysis_results`
--

DROP TABLE IF EXISTS `spc_analysis_results`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `spc_analysis_results` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ChartId` bigint NOT NULL,
  `AnalysisType` varchar(20) NOT NULL,
  `Cp` decimal(10,4) DEFAULT NULL,
  `Cpk` decimal(10,4) DEFAULT NULL,
  `Pp` decimal(10,4) DEFAULT NULL,
  `Ppk` decimal(10,4) DEFAULT NULL,
  `SigmaWithin` decimal(15,6) DEFAULT NULL,
  `SigmaOverall` decimal(15,6) DEFAULT NULL,
  `EstimatedPpm` decimal(15,2) DEFAULT NULL,
  `DataPointsUsed` int DEFAULT NULL,
  `AnalysisPeriodStart` datetime DEFAULT NULL,
  `AnalysisPeriodEnd` datetime DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_spc_analysis_chart` (`ChartId`),
  CONSTRAINT `spc_analysis_results_ibfk_1` FOREIGN KEY (`ChartId`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `spc_analysis_results`
--

LOCK TABLES `spc_analysis_results` WRITE;
/*!40000 ALTER TABLE `spc_analysis_results` DISABLE KEYS */;
/*!40000 ALTER TABLE `spc_analysis_results` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `spc_anova_results`
--

DROP TABLE IF EXISTS `spc_anova_results`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `spc_anova_results` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ChartId` bigint NOT NULL,
  `source` varchar(20) NOT NULL,
  `SumOfSquares` decimal(20,4) NOT NULL,
  `DegreesFreedom` int NOT NULL,
  `MeanSquare` decimal(20,4) NOT NULL,
  `FRatio` decimal(10,4) NOT NULL,
  `PValue` decimal(10,6) NOT NULL,
  `Significant` tinyint(1) NOT NULL,
  `analysis_date` datetime NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_spc_anova_chart` (`ChartId`),
  CONSTRAINT `spc_anova_results_ibfk_1` FOREIGN KEY (`ChartId`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `spc_anova_results`
--

LOCK TABLES `spc_anova_results` WRITE;
/*!40000 ALTER TABLE `spc_anova_results` DISABLE KEYS */;
/*!40000 ALTER TABLE `spc_anova_results` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `spc_control_charts`
--

DROP TABLE IF EXISTS `spc_control_charts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `spc_control_charts` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(200) NOT NULL,
  `ProcessId` bigint NOT NULL,
  `parameter_code` varchar(50) NOT NULL,
  `chart_type` varchar(10) NOT NULL,
  `subgroup_size` int NOT NULL DEFAULT '5',
  `usl` decimal(15,6) DEFAULT NULL,
  `lsl` decimal(15,6) DEFAULT NULL,
  `TargetValue` decimal(15,6) DEFAULT NULL,
  `Cl` decimal(15,6) DEFAULT NULL,
  `Ucl` decimal(15,6) DEFAULT NULL,
  `Lcl` decimal(15,6) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_spc_charts_name` (`name`),
  KEY `idx_spc_charts_parameter` (`parameter_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `spc_control_charts`
--

LOCK TABLES `spc_control_charts` WRITE;
/*!40000 ALTER TABLE `spc_control_charts` DISABLE KEYS */;
/*!40000 ALTER TABLE `spc_control_charts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `spc_data_points`
--

DROP TABLE IF EXISTS `spc_data_points`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `spc_data_points` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ChartId` bigint NOT NULL,
  `subgroup_index` int NOT NULL,
  `individual_values` json NOT NULL,
  `subgroup_mean` decimal(15,6) DEFAULT NULL,
  `subgroup_range` decimal(15,6) DEFAULT NULL,
  `MeasuredAt` datetime NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_spc_data_chart` (`ChartId`),
  KEY `idx_spc_data_measured` (`MeasuredAt`),
  CONSTRAINT `spc_data_points_ibfk_1` FOREIGN KEY (`ChartId`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `spc_data_points`
--

LOCK TABLES `spc_data_points` WRITE;
/*!40000 ALTER TABLE `spc_data_points` DISABLE KEYS */;
/*!40000 ALTER TABLE `spc_data_points` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `spc_data_sources`
--

DROP TABLE IF EXISTS `spc_data_sources`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `spc_data_sources` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `ChartId` bigint NOT NULL,
  `source_type` varchar(10) NOT NULL,
  `InspectionItemId` bigint DEFAULT NULL,
  `product_id` bigint DEFAULT NULL,
  `ProcessId` bigint DEFAULT NULL,
  `SupplierId` bigint DEFAULT NULL,
  `CustomerId` bigint DEFAULT NULL,
  `equipment_id` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `ChartId` (`ChartId`),
  KEY `InspectionItemId` (`InspectionItemId`),
  CONSTRAINT `spc_data_sources_ibfk_1` FOREIGN KEY (`ChartId`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE,
  CONSTRAINT `spc_data_sources_ibfk_2` FOREIGN KEY (`InspectionItemId`) REFERENCES `inspection_items` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `spc_data_sources`
--

LOCK TABLES `spc_data_sources` WRITE;
/*!40000 ALTER TABLE `spc_data_sources` DISABLE KEYS */;
/*!40000 ALTER TABLE `spc_data_sources` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `supplier_scores`
--

DROP TABLE IF EXISTS `supplier_scores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `supplier_scores` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `supplier_id` bigint NOT NULL,
  `score` int DEFAULT NULL,
  `assessment_date` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `supplier_id` (`supplier_id`),
  CONSTRAINT `supplier_scores_ibfk_1` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=21 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `supplier_scores`
--

LOCK TABLES `supplier_scores` WRITE;
/*!40000 ALTER TABLE `supplier_scores` DISABLE KEYS */;
INSERT INTO `supplier_scores` VALUES (1,1,93,'2026-06-30 10:00:00'),(2,2,88,'2026-06-30 10:00:00'),(3,3,95,'2026-06-30 10:00:00'),(4,4,89,'2026-06-30 10:00:00'),(5,5,87,'2026-06-30 10:00:00'),(6,6,82,'2026-06-30 10:00:00'),(7,7,96,'2026-06-30 10:00:00'),(8,8,92,'2026-06-30 10:00:00'),(9,9,84,'2026-06-30 10:00:00'),(10,10,69,'2026-06-30 10:00:00'),(11,11,98,'2026-06-30 10:00:00'),(12,12,79,'2026-06-30 10:00:00'),(13,13,87,'2026-06-30 10:00:00'),(14,14,55,'2026-06-30 10:00:00'),(15,15,90,'2026-06-30 10:00:00'),(16,16,84,'2026-06-30 10:00:00'),(17,17,80,'2026-06-30 10:00:00'),(18,18,85,'2026-06-30 10:00:00'),(19,19,92,'2026-06-30 10:00:00'),(20,20,86,'2026-06-30 10:00:00');
/*!40000 ALTER TABLE `supplier_scores` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `suppliers`
--

DROP TABLE IF EXISTS `suppliers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `suppliers` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `supplier_code` varchar(50) NOT NULL,
  `supplier_name` varchar(200) NOT NULL,
  `address` varchar(500) DEFAULT NULL,
  `contact_person` varchar(100) DEFAULT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `email` varchar(200) DEFAULT NULL,
  `rating` varchar(10) DEFAULT NULL,
  `supply_category` varchar(100) DEFAULT NULL,
  `score` int DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `status` varchar(20) NOT NULL DEFAULT 'active',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_suppliers_supplier_code` (`supplier_code`)
) ENGINE=InnoDB AUTO_INCREMENT=41 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `suppliers`
--

LOCK TABLES `suppliers` WRITE;
/*!40000 ALTER TABLE `suppliers` DISABLE KEYS */;
INSERT INTO `suppliers` VALUES (1,'SUP-T01','山特维克（Sandvik）','江苏省苏州市工业园区星湖街328号','张磊','0512-6288-1100','zhang.lei@sandvik.com','4.50','刀具',90,1,'active','2026-07-27 06:57:53'),(2,'SUP-T02','肯纳金属（Kennametal）','江苏省苏州市虎丘区滨河路568号','王芳','0512-6288-2200','wang.fang@kennametal.com','4.20','刀具',84,1,'active','2026-07-27 06:57:53'),(3,'SUP-T03','OSG 精密刀具','上海市浦东新区金桥路18号','李明','0512-6288-3300','li.ming@osg.com.cn','4.60','刀具',92,1,'active','2026-07-27 06:57:53'),(4,'SUP-T04','三菱综合材料','江苏省南京市江宁区双龙大道1688号','赵静','0512-6288-4400','zhao.jing@mia.com.cn','4.30','刀具',86,1,'active','2026-07-27 06:57:53'),(5,'SUP-T05','圣戈班磨具（Saint-Gobain）','浙江省嘉兴市秀洲区洪兴路256号','孙伟','0512-6288-5500','sun.wei@saint-gobain.com','4.10','刀具',82,1,'active','2026-07-27 06:57:53'),(6,'SUP-T06','金意磨料磨具','江苏省常州市新北区龙城大道77号','周磊','0512-6288-6600','zhou.lei@jinyi-abrasive.com','3.90','刀具',78,1,'active','2026-07-27 06:57:53'),(7,'SUP-R01','宝钢股份特钢事业部','上海市宝山区富锦路885号','钱国栋','021-5660-8800','qian.gd@baosteel.com','4.70','原材料',94,1,'active','2026-07-27 06:57:53'),(8,'SUP-R02','西南铝业（集团）有限责任公司','重庆市大渡口区茄子溪街道','吴昊','023-6506-1100','wu.hao@swal.com.cn','4.40','原材料',88,1,'active','2026-07-27 06:57:53'),(9,'SUP-R03','中铝山东新材料有限公司','山东省淄博市临淄区齐园路16号','郑涛','0533-6789-1000','zheng.tao@chalco.com','4.00','原材料',80,1,'active','2026-07-27 06:57:53'),(10,'SUP-R04','抚顺特殊钢（集团）','辽宁省抚顺市望花区抚北一路9号','冯强','024-4466-7700','feng.qiang@fusugroup.com','2.80','原材料',56,1,'inactive','2026-07-27 06:57:53'),(11,'SUP-P01','江苏恒立液压股份有限公司','江苏省常州市武进区武南路99号','马超','0513-8380-8800','ma.chao@hengli.com','4.80','零部件',96,1,'active','2026-07-27 06:57:53'),(12,'SUP-P02','宁波海天塑机集团有限公司','浙江省宁波市北仑区大碶街道','何敏','0574-8811-6600','he.min@海天.com','3.60','零部件',72,1,'active','2026-07-27 06:57:53'),(13,'SUP-P03','上海电气自动化集团','上海市静安区万荣路1268号','韩雪','021-6248-3300','han.xue@shenergy.com','4.10','零部件',82,1,'active','2026-07-27 06:57:53'),(14,'SUP-P04','广州数控设备有限公司','广东省广州市天河区东圃镇黄村北路43号','曹阳','020-3666-5500','cao.yang@gsk.com.cn','2.20','零部件',44,1,'blacklisted','2026-07-27 06:57:53'),(15,'SUP-PK01','上海紫江企业集团股份有限公司','上海市松江区沪亭北路1118号','徐峰','021-5745-1100','xu.feng@zijiang.com','4.30','包材',86,1,'active','2026-07-27 06:57:53'),(16,'SUP-PK02','山东盈泰包装科技有限公司','山东省济南市历城区工业北路21号','田欣','0531-8866-2200','tian.xin@yingtai.com','3.70','包材',74,1,'active','2026-07-27 06:57:53'),(17,'SUP-PK03','温州东鹏包装有限公司','浙江省温州市瓯海区郭溪镇东瓯工业区','赖伟','0577-6288-3300','lai.wei@dpbzw.com','3.50','包材',70,1,'active','2026-07-27 06:57:53'),(18,'SUP-E01','沈阳机床（集团）有限责任公司','辽宁省沈阳市皇姑区崇山路102号','谢鹏','024-2412-5500','xie.peng@smtc.com.cn','3.80','设备',76,1,'active','2026-07-27 06:57:53'),(19,'SUP-E02','深圳市汇川技术股份有限公司','广东省深圳市南山区桃源街道留仙洞3333号','袁莉','0755-8622-8800','yuan.li@invt.com.cn','4.50','设备',90,1,'active','2026-07-27 06:57:53'),(20,'SUP-E03','苏州固锝电子科技股份有限公司','江苏省苏州市吴中区越溪街道吴中大道5168号','彭军','0512-6522-9900','peng.jun@sgd-semi.com','4.00','设备',80,1,'active','2026-07-27 06:57:53');
/*!40000 ALTER TABLE `suppliers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sys_dict_items`
--

DROP TABLE IF EXISTS `sys_dict_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sys_dict_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `type_code` varchar(50) NOT NULL,
  `item_label` varchar(200) NOT NULL,
  `item_value` varchar(100) NOT NULL,
  `sort_order` int DEFAULT '0',
  `color` varchar(20) DEFAULT NULL,
  `is_default` tinyint(1) DEFAULT '0',
  `status` tinyint(1) DEFAULT '1',
  `remark` text,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_dict_items_type` (`type_code`),
  KEY `idx_dict_items_sort` (`type_code`,`sort_order`)
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sys_dict_items`
--

LOCK TABLES `sys_dict_items` WRITE;
/*!40000 ALTER TABLE `sys_dict_items` DISABLE KEYS */;
INSERT INTO `sys_dict_items` VALUES (1,'equipment_type','CNC加工中心','CNC',1,NULL,0,1,NULL,'2026-07-24 04:16:25'),(2,'equipment_type','PLC设备','PLC',2,NULL,0,1,NULL,'2026-07-24 04:16:25'),(3,'equipment_type','检测设备','检测设备',3,NULL,0,1,NULL,'2026-07-24 04:16:25'),(4,'equipment_type','机器人','机器人',4,NULL,0,1,NULL,'2026-07-24 04:16:25'),(5,'equipment_type','其他','其他',99,NULL,0,1,NULL,'2026-07-24 04:16:25'),(6,'process_type','加工','加工',1,NULL,0,1,NULL,'2026-07-24 04:16:25'),(7,'process_type','检验','检验',2,NULL,0,1,NULL,'2026-07-24 04:16:25'),(8,'process_type','装配','装配',3,NULL,0,1,NULL,'2026-07-24 04:16:25'),(9,'process_type','包装','包装',4,NULL,0,1,NULL,'2026-07-24 04:16:25'),(10,'process_type','热处理','热处理',5,NULL,0,1,NULL,'2026-07-24 04:16:25'),(11,'process_type','表面处理','表面处理',6,NULL,0,1,NULL,'2026-07-24 04:16:25'),(12,'defect_category','外观','外观',1,NULL,0,1,NULL,'2026-07-24 04:16:25'),(13,'defect_category','尺寸','尺寸',2,NULL,0,1,NULL,'2026-07-24 04:16:25'),(14,'defect_category','功能','功能',3,NULL,0,1,NULL,'2026-07-24 04:16:25'),(15,'defect_category','材料','材料',4,NULL,0,1,NULL,'2026-07-24 04:16:25'),(16,'defect_category','性能','性能',5,NULL,0,1,NULL,'2026-07-24 04:16:25'),(17,'defect_category','其他','其他',99,NULL,0,1,NULL,'2026-07-24 04:16:25'),(18,'severity','CR - 严重','CR',1,'#F56C6C',0,1,NULL,'2026-07-24 04:16:25'),(19,'severity','MA - 主要','MA',2,'#E6A23C',0,1,NULL,'2026-07-24 04:16:25'),(20,'severity','MI - 次要','MI',3,'#909399',0,1,NULL,'2026-07-24 04:16:25'),(21,'inspection_type','IQC来料检验','IQC',1,NULL,0,1,NULL,'2026-07-24 04:16:25'),(22,'inspection_type','IPQC过程检验','IPQC',2,NULL,0,1,NULL,'2026-07-24 04:16:25'),(23,'inspection_type','FQC成品检验','FQC',3,NULL,0,1,NULL,'2026-07-24 04:16:25'),(24,'inspection_type','OQC出货检验','OQC',4,NULL,0,1,NULL,'2026-07-24 04:16:25'),(25,'product_category','成品','成品',1,NULL,0,1,NULL,'2026-07-24 04:16:25'),(26,'product_category','半成品','半成品',2,NULL,0,1,NULL,'2026-07-24 04:16:25'),(27,'product_category','原材料','原材料',3,NULL,0,1,NULL,'2026-07-24 04:16:25'),(28,'product_category','辅料','辅料',4,NULL,0,1,NULL,'2026-07-24 04:16:25'),(29,'tool_type','车刀','车刀',1,NULL,0,1,NULL,'2026-07-24 04:16:25'),(30,'tool_type','铣刀','铣刀',2,NULL,0,1,NULL,'2026-07-24 04:16:25'),(31,'tool_type','钻头','钻头',3,NULL,0,1,NULL,'2026-07-24 04:16:25'),(32,'tool_type','磨具','磨具',4,NULL,0,1,NULL,'2026-07-24 04:16:25'),(33,'tool_type','丝锥','丝锥',5,NULL,0,1,NULL,'2026-07-24 04:16:25'),(34,'tool_type','其他','其他',99,NULL,0,1,NULL,'2026-07-24 04:16:25'),(35,'supply_category','原材料','原材料',1,NULL,0,1,NULL,'2026-07-24 04:16:25'),(36,'supply_category','零部件','零部件',2,NULL,0,1,NULL,'2026-07-24 04:16:25'),(37,'supply_category','包材','包材',3,NULL,0,1,NULL,'2026-07-24 04:16:25'),(38,'supply_category','设备','设备',4,NULL,0,1,NULL,'2026-07-24 04:16:25'),(39,'supply_category','服务','服务',5,NULL,0,1,NULL,'2026-07-24 04:16:25'),(40,'material_unit','个','个',1,NULL,0,1,NULL,'2026-07-24 04:16:25'),(41,'material_unit','件','件',2,NULL,0,1,NULL,'2026-07-24 04:16:25'),(42,'material_unit','套','套',3,NULL,0,1,NULL,'2026-07-24 04:16:25'),(43,'material_unit','kg','kg',4,NULL,0,1,NULL,'2026-07-24 04:16:25'),(44,'material_unit','g','g',5,NULL,0,1,NULL,'2026-07-24 04:16:25'),(45,'material_unit','m','m',6,NULL,0,1,NULL,'2026-07-24 04:16:25'),(46,'material_unit','L','L',7,NULL,0,1,NULL,'2026-07-24 04:16:25'),(47,'material_unit','pcs','pcs',8,NULL,0,1,NULL,'2026-07-24 04:16:25');
/*!40000 ALTER TABLE `sys_dict_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `sys_dict_types`
--

DROP TABLE IF EXISTS `sys_dict_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `sys_dict_types` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `type_code` varchar(50) NOT NULL,
  `type_name` varchar(200) NOT NULL,
  `is_system` tinyint(1) DEFAULT '0',
  `status` tinyint(1) DEFAULT '1',
  `remark` text,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `type_code` (`type_code`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sys_dict_types`
--

LOCK TABLES `sys_dict_types` WRITE;
/*!40000 ALTER TABLE `sys_dict_types` DISABLE KEYS */;
INSERT INTO `sys_dict_types` VALUES (1,'equipment_type','设备类型',1,1,NULL,'2026-07-24 04:16:25'),(2,'process_type','工序类型',1,1,NULL,'2026-07-24 04:16:25'),(3,'defect_category','不良分类',1,1,NULL,'2026-07-24 04:16:25'),(4,'severity','严重等级',1,1,NULL,'2026-07-24 04:16:25'),(5,'inspection_type','检验类型',1,1,NULL,'2026-07-24 04:16:25'),(6,'product_category','产品类别',1,1,NULL,'2026-07-24 04:16:25'),(7,'tool_type','刀具类型',1,1,NULL,'2026-07-24 04:16:25'),(8,'supply_category','供应类别',1,1,NULL,'2026-07-24 04:16:25'),(9,'material_unit','物料单位',1,1,NULL,'2026-07-24 04:16:25');
/*!40000 ALTER TABLE `sys_dict_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tools`
--

DROP TABLE IF EXISTS `tools`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tools` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `tool_code` varchar(50) NOT NULL,
  `tool_name` varchar(200) NOT NULL,
  `model` varchar(100) DEFAULT NULL,
  `tool_type` varchar(50) DEFAULT NULL,
  `design_life` decimal(10,2) DEFAULT NULL,
  `life_unit` varchar(20) DEFAULT NULL,
  `life_current` decimal(10,2) DEFAULT NULL,
  `supplier` varchar(200) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `IX_tools_tool_code` (`tool_code`)
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tools`
--

LOCK TABLES `tools` WRITE;
/*!40000 ALTER TABLE `tools` DISABLE KEYS */;
INSERT INTO `tools` VALUES (1,'T-001','外圆车刀 90°','WNMG080408','车刀',500.00,'cycles',120.00,'山特维克',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(2,'T-006','内圆车刀 35°','VNGN160404','车刀',450.00,'cycles',85.00,'山特维克',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(3,'T-007','切断车刀 3mm','GGHN2525M12','车刀',800.00,'cycles',320.00,'肯纳',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(4,'T-008','精车刀 75°','DNGN160404','车刀',600.00,'cycles',550.00,'OSG',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(5,'T-009','滚花刀直纹 Φ30','RHR-30A','车刀',2000.00,'cycles',200.00,'山特维克',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(6,'T-010','圆弧车刀 R2','CNMG120408','车刀',400.00,'cycles',380.00,'肯纳',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(7,'T-003','面铣刀 Φ50','F4042.BS.050','铣刀',400.00,'cycles',200.00,'肯纳',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(8,'T-011','球头铣刀 Φ10','C4-10R','铣刀',200.00,'cycles',50.00,'三菱',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(9,'T-012','立铣刀 Φ6 4刃','S46R-6.0-AL','铣刀',300.00,'cycles',150.00,'OSG',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(10,'T-013','键槽铣刀 Φ8','H8-64.408','铣刀',250.00,'cycles',30.00,'OSG',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(11,'T-014','圆鼻铣刀 R3','R3-4032F10R','铣刀',350.00,'cycles',100.00,'三菱',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(12,'T-015','倒角铣刀 60°','CR60-200','铣刀',500.00,'cycles',420.00,'肯纳',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(13,'T-002','钻头 Φ8','D924-8.0','钻头',300.00,'cycles',45.00,'OSG',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(14,'T-004','铰刀 Φ10H7','HR500-10.0','钻头',350.00,'cycles',80.00,'OSG',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(15,'T-016','麻花钻 Φ12','D924-12.0','钻头',280.00,'cycles',160.00,'OSG',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(16,'T-017','中心钻 A2 Φ2','AC2.0','钻头',1000.00,'cycles',650.00,'三菱',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(17,'T-018','深孔钻 Φ16 喷液','BTA-16','钻头',200.00,'cycles',90.00,'山特维克',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(18,'T-019','螺旋槽丝锥 M8×1.25','HSP-M8×1.25','丝锥',800.00,'cycles',200.00,'OSG',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(19,'T-020','板牙 SW12','SW12','丝锥',1500.00,'cycles',500.00,'山特维克',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(20,'T-005','砂轮 400×40','SA-40040','磨具',200.00,'hours',60.00,'圣戈班',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(21,'T-021','CBN 砂轮 Φ200×20','CBN-200×20','磨具',100.00,'hours',30.00,'圣戈班',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(22,'T-022','树脂砂轮 Φ100×6×32','RES-100×6×32','磨具',80.00,'hours',75.00,'金意',1,'2026-07-27 06:55:34','2026-07-27 06:55:34'),(23,'T-023','油石 R5 100×25×25','STL-R5-100','磨具',50.00,'hours',10.00,'金意',1,'2026-07-27 06:55:34','2026-07-27 06:55:34');
/*!40000 ALTER TABLE `tools` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `username` varchar(100) NOT NULL,
  `password_hash` varchar(500) NOT NULL,
  `display_name` varchar(200) DEFAULT NULL,
  `avatar` varchar(500) DEFAULT NULL,
  `email` varchar(200) DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  `role_id` bigint DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`),
  KEY `role_id` (`role_id`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'admin','f3HD2bToxZABMh2HvLnNO2UK+5ra2h8g2h56obhrnZPnMnYDkGxevSgr1lD3hhHA','系统管理员','','admin@qm-ai.com',1,1,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(2,'operator','mY3EufvrNQhQfK1YV023QZ+MthzyKMDAv/iZekYa0rzfioYJHJIZOmoZhegZ4cRx','质检操作员',NULL,'operator@qm-ai.com',1,2,'2026-07-27 06:30:19','2026-07-27 06:30:19'),(3,'inspector','V4Lg2VRlFKE9sHGRp/63AalcP5Y5e7HIaEtvV6KLPtkPgaq/+T3Ebb4fbT4Onuov','质量检验员',NULL,'inspector@qm-ai.com',1,3,'2026-07-27 06:30:19','2026-07-27 06:30:19');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'qmai'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-07-27  8:13:28
