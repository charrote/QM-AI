-- MySQL dump 10.13  Distrib 9.7.1, for macos26.4 (arm64)
--
-- Host: localhost    Database: qmai
-- ------------------------------------------------------
-- Server version	9.7.1

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
-- Current Database: `qmai`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `qmai` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `qmai`;

--
-- Table structure for table `ai_analysis_results`
--

DROP TABLE IF EXISTS `ai_analysis_results`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ai_analysis_results` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `analysis_type` enum('risk','detection','prediction','root_cause','optimization') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `source_module` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `source_ref_id` bigint DEFAULT NULL,
  `result` json DEFAULT NULL,
  `confidence` decimal(5,4) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ai_analysis_results`
--

LOCK TABLES `ai_analysis_results` WRITE;
/*!40000 ALTER TABLE `ai_analysis_results` DISABLE KEYS */;
/*!40000 ALTER TABLE `ai_analysis_results` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ai_models`
--

DROP TABLE IF EXISTS `ai_models`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ai_models` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `model_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `model_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `version` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `status` enum('training','deployed','archived','failed') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `metrics` json DEFAULT NULL,
  `file_path` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `trained_at` datetime DEFAULT NULL,
  `deployed_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ai_models`
--

LOCK TABLES `ai_models` WRITE;
/*!40000 ALTER TABLE `ai_models` DISABLE KEYS */;
/*!40000 ALTER TABLE `ai_models` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ai_warnings`
--

DROP TABLE IF EXISTS `ai_warnings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ai_warnings` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `warning_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `severity` enum('info','warning','critical') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `title` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `source_module` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `source_ref_id` bigint DEFAULT NULL,
  `is_read` tinyint(1) DEFAULT '0',
  `created_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ai_warnings`
--

LOCK TABLES `ai_warnings` WRITE;
/*!40000 ALTER TABLE `ai_warnings` DISABLE KEYS */;
/*!40000 ALTER TABLE `ai_warnings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `audit_findings`
--

DROP TABLE IF EXISTS `audit_findings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `audit_findings` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `audit_id` bigint NOT NULL COMMENT '关联审核',
  `finding_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'conformity/non_conformity/opportunity',
  `severity` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'major/minor',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `evidence` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '客观证据',
  `requirement_ref` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '引用标准条款',
  `status` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'open' COMMENT 'open/rectifying/verified/rejected/closed',
  `rectification_plan` json DEFAULT NULL COMMENT '整改措施（JSON）',
  `responsible_user_id` bigint DEFAULT NULL COMMENT '整改责任人ID',
  `rectification_due_date` date DEFAULT NULL COMMENT '整改截止日',
  `verified_by` bigint DEFAULT NULL COMMENT '验证人ID',
  `verified_at` datetime DEFAULT NULL COMMENT '验证时间',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_audit_findings_audit` (`audit_id`),
  KEY `idx_audit_findings_status` (`status`),
  CONSTRAINT `audit_findings_ibfk_1` FOREIGN KEY (`audit_id`) REFERENCES `audits` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `audit_findings`
--

LOCK TABLES `audit_findings` WRITE;
/*!40000 ALTER TABLE `audit_findings` DISABLE KEYS */;
/*!40000 ALTER TABLE `audit_findings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `audits`
--

DROP TABLE IF EXISTS `audits`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `audits` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `audit_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `audit_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'internal/process/product',
  `title` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '描述',
  `start_date` date NOT NULL COMMENT '开始日期',
  `end_date` date NOT NULL COMMENT '结束日期',
  `auditor_id` bigint NOT NULL COMMENT '审核人ID',
  `auditor_ids_json` json DEFAULT NULL COMMENT '审核人IDs（JSON数组）',
  `total_findings` int DEFAULT '0' COMMENT '总发现数',
  `conformities` int DEFAULT '0' COMMENT '符合项数',
  `non_conformities` int DEFAULT '0' COMMENT '不符合项数',
  `opportunities` int DEFAULT '0' COMMENT '改进机会数',
  `scope` json DEFAULT NULL COMMENT '审核范围（产线/工序/产品）',
  `status` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'planned' COMMENT 'planned/in_progress/completed/archived',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `audit_no` (`audit_no`),
  KEY `idx_audits_status` (`status`),
  KEY `idx_audits_type` (`audit_type`),
  KEY `idx_audits_date` (`start_date`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `material_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `material_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `quantity` decimal(10,3) DEFAULT NULL,
  `unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `level` int DEFAULT NULL,
  `remark` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '备注',
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `boms_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `boms`
--

LOCK TABLES `boms` WRITE;
/*!40000 ALTER TABLE `boms` DISABLE KEYS */;
INSERT INTO `boms` VALUES (1,1,'MAT-001','45# 圆钢 Φ50',1.200,'kg',1,'宝钢供料，碳含量0.42-0.50%',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(2,1,'MAT-002','轴承 6205',2.000,'pcs',1,'SKF供料，深沟球轴承',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(3,1,'MAT-003','润滑油',0.050,'L',1,'中石化长城L-AN46',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(4,1,'MAT-004','防锈油',0.020,'L',1,'包装前防锈处理',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(5,1,'MAT-005','包装盒',1.000,'pcs',1,'瓦楞纸盒 200×80×80mm',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(6,1,'MAT-006','产品标签',1.000,'pcs',1,'含批次号、生产日期',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(7,2,'MAT-101','ADC12 铝合金锭',2.500,'kg',1,'压铸件主体材料',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(8,2,'MAT-102','脱模剂',0.030,'L',1,'水性脱模剂',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(9,2,'MAT-103','密封圈 O型圈 25×3',2.000,'pcs',1,'NBR橡胶，耐油',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(10,2,'MAT-104','螺栓 M6×20',4.000,'pcs',1,'8.8级镀锌螺栓',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(11,2,'MAT-105','垫片 Φ6',4.000,'pcs',1,'弹簧垫片',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(12,2,'MAT-106','防尘罩',1.000,'pcs',1,'硅胶材质',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(13,2,'MAT-107','泡沫内衬',1.000,'pcs',1,'EPE珍珠棉定制',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(14,3,'MAT-201','FR-4 玻纤基板 150×100×1.6',1.000,'pcs',1,'4层板，阻焊绿油',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(15,3,'MAT-202','MCU STM32F103C8T6',1.000,'pcs',1,'主控芯片',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(16,3,'MAT-203','电容 100μF/16V',6.000,'pcs',1,'电解电容',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(17,3,'MAT-204','电容 0.1μF/50V',12.000,'pcs',1,'陶瓷电容',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(18,3,'MAT-205','电阻 10KΩ 1/4W',8.000,'pcs',1,'贴片电阻',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(19,3,'MAT-206','USB Type-C 接口',1.000,'pcs',1,'沉板焊接',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(20,3,'MAT-207','排针 2×10 Pin',2.000,'pcs',1,'2.54mm间距',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(21,3,'MAT-208','晶振 8MHz',1.000,'pcs',1,'无源晶振',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(22,3,'MAT-209','防静电袋',1.000,'pcs',1,'屏蔽包装袋',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(23,4,'MAT-301','NBR 橡胶原料',0.150,'kg',1,'丁腈橡胶，硬度70A',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(24,4,'MAT-302','润滑粉',0.001,'kg',1,'模具脱模用',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(25,4,'MAT-303','PE 自封袋',1.000,'pcs',1,'包装用',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(26,4,'MAT-304','干燥剂',1.000,'pcs',1,'硅胶干燥剂 5g',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(27,5,'MAT-401','PVC 护套线 2×0.75mm²',1.500,'m',1,'线束主线材',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(28,5,'MAT-402','端子 HT-05B',4.000,'pcs',1,'公母对插端子',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(29,5,'MAT-403','热缩管 Φ6 红',0.100,'m',1,'两端绝缘保护',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(30,5,'MAT-404','扎带 100mm 黑色',2.000,'pcs',1,'尼龙扎带固定',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(31,5,'MAT-405','缠绕管 Φ8 黑色',0.300,'m',1,'线束保护套管',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(32,5,'MAT-406','PE 包装袋',1.000,'pcs',1,'含产品标签',NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28');
/*!40000 ALTER TABLE `boms` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `capa`
--

DROP TABLE IF EXISTS `capa`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `capa` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `capa_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `defect_id` bigint DEFAULT NULL,
  `anomaly_id` bigint DEFAULT NULL,
  `complaint_id` bigint DEFAULT NULL,
  `severity` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'major',
  `title` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `current_phase` int NOT NULL DEFAULT '0' COMMENT '0=创建/1=临时措施/2=根因分析/3=纠正措施/4=预防措施/5=验证/6=关闭',
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'open',
  `created_by` bigint NOT NULL,
  `assigned_to` bigint DEFAULT NULL,
  `due_date` date DEFAULT NULL,
  `closed_at` datetime DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `capa_code` (`capa_code`),
  KEY `idx_status` (`status`),
  KEY `idx_phase` (`current_phase`),
  KEY `idx_capa_org` (`org_id`),
  KEY `defect_id` (`defect_id`),
  CONSTRAINT `capa_ibfk_1` FOREIGN KEY (`defect_id`) REFERENCES `defects` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `capa`
--

LOCK TABLES `capa` WRITE;
/*!40000 ALTER TABLE `capa` DISABLE KEYS */;
/*!40000 ALTER TABLE `capa` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `capa_corrective_actions`
--

DROP TABLE IF EXISTS `capa_corrective_actions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `capa_corrective_actions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `capa_id` bigint NOT NULL,
  `action_description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `responsible_person` bigint NOT NULL,
  `due_date` date NOT NULL,
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending',
  `completed_at` datetime DEFAULT NULL,
  `remarks` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `capa_id` (`capa_id`),
  CONSTRAINT `capa_corrective_actions_ibfk_1` FOREIGN KEY (`capa_id`) REFERENCES `capa` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `capa_corrective_actions`
--

LOCK TABLES `capa_corrective_actions` WRITE;
/*!40000 ALTER TABLE `capa_corrective_actions` DISABLE KEYS */;
/*!40000 ALTER TABLE `capa_corrective_actions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `capa_preventive_actions`
--

DROP TABLE IF EXISTS `capa_preventive_actions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `capa_preventive_actions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `capa_id` bigint NOT NULL,
  `action_description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `responsible_person` bigint NOT NULL,
  `due_date` date NOT NULL,
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending',
  `completed_at` datetime DEFAULT NULL,
  `remarks` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `capa_id` (`capa_id`),
  CONSTRAINT `capa_preventive_actions_ibfk_1` FOREIGN KEY (`capa_id`) REFERENCES `capa` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `capa_preventive_actions`
--

LOCK TABLES `capa_preventive_actions` WRITE;
/*!40000 ALTER TABLE `capa_preventive_actions` DISABLE KEYS */;
/*!40000 ALTER TABLE `capa_preventive_actions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `capa_root_causes`
--

DROP TABLE IF EXISTS `capa_root_causes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `capa_root_causes` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `capa_id` bigint NOT NULL,
  `analysis_method` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'five_whys',
  `content` json NOT NULL COMMENT '5Why问答或鱼骨图数据',
  `root_cause_summary` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_by` bigint NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `capa_id` (`capa_id`),
  CONSTRAINT `capa_root_causes_ibfk_1` FOREIGN KEY (`capa_id`) REFERENCES `capa` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `capa_root_causes`
--

LOCK TABLES `capa_root_causes` WRITE;
/*!40000 ALTER TABLE `capa_root_causes` DISABLE KEYS */;
/*!40000 ALTER TABLE `capa_root_causes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `capa_temporary_measures`
--

DROP TABLE IF EXISTS `capa_temporary_measures`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `capa_temporary_measures` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `capa_id` bigint NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `executed_by` bigint DEFAULT NULL,
  `executed_at` datetime DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `capa_id` (`capa_id`),
  CONSTRAINT `capa_temporary_measures_ibfk_1` FOREIGN KEY (`capa_id`) REFERENCES `capa` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `capa_temporary_measures`
--

LOCK TABLES `capa_temporary_measures` WRITE;
/*!40000 ALTER TABLE `capa_temporary_measures` DISABLE KEYS */;
/*!40000 ALTER TABLE `capa_temporary_measures` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `capa_verifications`
--

DROP TABLE IF EXISTS `capa_verifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `capa_verifications` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `capa_id` bigint NOT NULL,
  `verifier_id` bigint NOT NULL,
  `verification_date` datetime NOT NULL,
  `conclusion` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'effective/not_effective/requires_revision',
  `evidence` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `image_urls` json DEFAULT NULL,
  `remarks` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `capa_id` (`capa_id`),
  CONSTRAINT `capa_verifications_ibfk_1` FOREIGN KEY (`capa_id`) REFERENCES `capa` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `capa_verifications`
--

LOCK TABLES `capa_verifications` WRITE;
/*!40000 ALTER TABLE `capa_verifications` DISABLE KEYS */;
/*!40000 ALTER TABLE `capa_verifications` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `closure_rules`
--

DROP TABLE IF EXISTS `closure_rules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `closure_rules` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '规则名称',
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '规则编码',
  `condition_json` json NOT NULL COMMENT '条件表达式JSON',
  `logic` varchar(5) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'AND' COMMENT '逻辑运算符 AND/OR',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '描述',
  `is_active` tinyint(1) DEFAULT '1' COMMENT '是否启用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL DEFAULT '1' COMMENT '创建人ID',
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `closure_rules`
--

LOCK TABLES `closure_rules` WRITE;
/*!40000 ALTER TABLE `closure_rules` DISABLE KEYS */;
/*!40000 ALTER TABLE `closure_rules` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `complaint_events`
--

DROP TABLE IF EXISTS `complaint_events`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `complaint_events` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `complaint_id` bigint NOT NULL COMMENT '关联客诉',
  `event_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '事件类型',
  `event_data` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '事件数据（JSON）',
  `created_by` bigint NOT NULL COMMENT '操作用户',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP COMMENT '发生时间',
  PRIMARY KEY (`id`),
  KEY `idx_events_complaint` (`complaint_id`),
  KEY `idx_events_type` (`event_type`),
  KEY `idx_events_created` (`created_at`),
  CONSTRAINT `complaint_events_ibfk_1` FOREIGN KEY (`complaint_id`) REFERENCES `complaints` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `complaint_events`
--

LOCK TABLES `complaint_events` WRITE;
/*!40000 ALTER TABLE `complaint_events` DISABLE KEYS */;
/*!40000 ALTER TABLE `complaint_events` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `complaints`
--

DROP TABLE IF EXISTS `complaints`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `complaints` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `complaint_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `customer_id` bigint DEFAULT NULL,
  `product_id` bigint DEFAULT NULL,
  `batch_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `complaint_date` date DEFAULT NULL,
  `subject` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '投诉主题',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `severity` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'critical/major/minor',
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'new' COMMENT 'new/acknowledged/in_progress/closed',
  `five_w2h_json` json DEFAULT NULL COMMENT '5W2H分析',
  `assigned_to` bigint DEFAULT NULL COMMENT '指派人',
  `due_date` date DEFAULT NULL,
  `acknowledged_at` datetime DEFAULT NULL,
  `closed_at` datetime DEFAULT NULL,
  `created_by` bigint DEFAULT NULL COMMENT '创建人',
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `complaint_code` (`complaint_code`),
  KEY `product_id` (`product_id`),
  KEY `idx_complaints_org` (`org_id`),
  KEY `idx_complaints_customer` (`customer_id`),
  KEY `idx_complaints_status` (`status`),
  CONSTRAINT `complaints_ibfk_1` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`),
  CONSTRAINT `complaints_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `complaints`
--

LOCK TABLES `complaints` WRITE;
/*!40000 ALTER TABLE `complaints` DISABLE KEYS */;
/*!40000 ALTER TABLE `complaints` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `customers`
--

DROP TABLE IF EXISTS `customers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `customers` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `customer_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `customer_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `contact_person` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `address` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `customer_code` (`customer_code`),
  KEY `idx_customers_org` (`org_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `customers`
--

LOCK TABLES `customers` WRITE;
/*!40000 ALTER TABLE `customers` DISABLE KEYS */;
/*!40000 ALTER TABLE `customers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `d8_reports`
--

DROP TABLE IF EXISTS `d8_reports`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `d8_reports` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `complaint_id` bigint NOT NULL,
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'd0' COMMENT 'd0/d1/d2/d3/d4/d5/d6/d7/d8',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `complaint_id` (`complaint_id`),
  CONSTRAINT `d8_reports_ibfk_1` FOREIGN KEY (`complaint_id`) REFERENCES `complaints` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `d8_reports`
--

LOCK TABLES `d8_reports` WRITE;
/*!40000 ALTER TABLE `d8_reports` DISABLE KEYS */;
/*!40000 ALTER TABLE `d8_reports` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `defect_codes`
--

DROP TABLE IF EXISTS `defect_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `defect_codes` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `defect_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `defect_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `defect_category` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `defect_severity` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'CR/MA/MI',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `is_reworkable` tinyint(1) DEFAULT '0' COMMENT '是否可返工',
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `defect_code` (`defect_code`),
  KEY `idx_defect_codes_org` (`org_id`),
  KEY `idx_defect_codes_category` (`defect_category`),
  KEY `idx_defect_codes_severity` (`defect_severity`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `defect_codes`
--

LOCK TABLES `defect_codes` WRITE;
/*!40000 ALTER TABLE `defect_codes` DISABLE KEYS */;
/*!40000 ALTER TABLE `defect_codes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `defects`
--

DROP TABLE IF EXISTS `defects`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `defects` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `defect_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '缺陷编码',
  `severity` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'major' COMMENT 'critical/major/minor',
  `source_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'iqc/ipqc/fqc/oqc/customer',
  `source_id` bigint DEFAULT NULL COMMENT '来源ID(检验单/客诉等)',
  `product_id` bigint DEFAULT NULL,
  `batch_id` bigint DEFAULT NULL,
  `equipment_id` bigint DEFAULT NULL,
  `quantity` decimal(15,2) NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `image_urls` json DEFAULT NULL,
  `discovered_by` bigint DEFAULT NULL,
  `discovered_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'open' COMMENT 'open/investigating/resolved/closed',
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `defect_code` (`defect_code`),
  KEY `idx_defect_code` (`defect_code`),
  KEY `idx_source_type` (`source_type`),
  KEY `idx_status` (`status`),
  KEY `idx_defects_org` (`org_id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `defects_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `defects`
--

LOCK TABLES `defects` WRITE;
/*!40000 ALTER TABLE `defects` DISABLE KEYS */;
/*!40000 ALTER TABLE `defects` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `document_versions`
--

DROP TABLE IF EXISTS `document_versions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `document_versions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `document_id` bigint NOT NULL,
  `version` int NOT NULL,
  `minio_key` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `change_description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '变更说明',
  `created_by` bigint NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_doc_versions_document` (`document_id`),
  CONSTRAINT `document_versions_ibfk_1` FOREIGN KEY (`document_id`) REFERENCES `documents` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `document_versions`
--

LOCK TABLES `document_versions` WRITE;
/*!40000 ALTER TABLE `document_versions` DISABLE KEYS */;
/*!40000 ALTER TABLE `document_versions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `documents`
--

DROP TABLE IF EXISTS `documents`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `documents` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `title` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `doc_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'sop/work_instruction/inspection_standard/8d_report/audit_report/other',
  `minio_key` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'MinIO对象键',
  `file_size_bytes` bigint DEFAULT NULL COMMENT '文件大小（字节）',
  `file_hash` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'SHA-256哈希',
  `version` int NOT NULL DEFAULT '1',
  `status` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'draft' COMMENT 'draft/reviewing/approved/archived',
  `approved_by` bigint DEFAULT NULL COMMENT '审批人ID',
  `rejection_reason` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '驳回理由',
  `approved_at` datetime DEFAULT NULL COMMENT '审批时间',
  `expires_at` date DEFAULT NULL COMMENT '有效期',
  `created_by` bigint NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_documents_status` (`status`),
  KEY `idx_documents_type` (`doc_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `documents`
--

LOCK TABLES `documents` WRITE;
/*!40000 ALTER TABLE `documents` DISABLE KEYS */;
/*!40000 ALTER TABLE `documents` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dynamic_params`
--

DROP TABLE IF EXISTS `dynamic_params`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dynamic_params` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `group_id` bigint NOT NULL COMMENT '所属参数组ID',
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '参数名',
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '参数编码(唯一)',
  `data_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'numeric' COMMENT '数据类型: numeric/categorical/boolean',
  `unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '单位',
  `target_value` decimal(15,6) DEFAULT NULL COMMENT '目标值',
  `usl` decimal(15,6) DEFAULT NULL COMMENT '上规格限',
  `lsl` decimal(15,6) DEFAULT NULL COMMENT '下规格限',
  `precision` decimal(10,2) DEFAULT '1.00' COMMENT '精度/小数位数',
  `ai_strategy` json DEFAULT NULL COMMENT 'AI策略预置配置',
  `sort_order` int DEFAULT '0' COMMENT '排序号',
  `is_active` tinyint(1) DEFAULT '1' COMMENT '是否启用',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL DEFAULT '1' COMMENT '创建人ID',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_code` (`code`),
  KEY `group_id` (`group_id`),
  CONSTRAINT `dynamic_params_ibfk_1` FOREIGN KEY (`group_id`) REFERENCES `param_groups` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dynamic_params`
--

LOCK TABLES `dynamic_params` WRITE;
/*!40000 ALTER TABLE `dynamic_params` DISABLE KEYS */;
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
  `equipment_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `equipment_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `equipment_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `model` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `manufacturer` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `installation_date` date DEFAULT NULL,
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'idle' COMMENT 'running/idle/fault/maintenance',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `equipment_code` (`equipment_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `equipment`
--

LOCK TABLES `equipment` WRITE;
/*!40000 ALTER TABLE `equipment` DISABLE KEYS */;
/*!40000 ALTER TABLE `equipment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `equipment_param_mappings`
--

DROP TABLE IF EXISTS `equipment_param_mappings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `equipment_param_mappings` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `equipment_id` bigint NOT NULL,
  `mqtt_topic` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `system_param_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `param_group_id` bigint DEFAULT NULL COMMENT '关联参数组ID',
  `data_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'numeric' COMMENT 'numeric/count/status',
  `unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_equip_param_mapping_equipment` (`equipment_id`),
  KEY `idx_equip_param_mapping_param` (`param_group_id`),
  CONSTRAINT `equipment_param_mappings_ibfk_1` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`),
  CONSTRAINT `equipment_param_mappings_ibfk_2` FOREIGN KEY (`param_group_id`) REFERENCES `param_groups` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `equipment_param_mappings`
--

LOCK TABLES `equipment_param_mappings` WRITE;
/*!40000 ALTER TABLE `equipment_param_mappings` DISABLE KEYS */;
/*!40000 ALTER TABLE `equipment_param_mappings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `equipment_quality_correlation`
--

DROP TABLE IF EXISTS `equipment_quality_correlation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `equipment_quality_correlation` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `equipment_id` bigint DEFAULT NULL,
  `analysis_date` date DEFAULT NULL,
  `correlation_data` json DEFAULT NULL,
  `conclusion` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`id`),
  KEY `equipment_id` (`equipment_id`),
  CONSTRAINT `equipment_quality_correlation_ibfk_1` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `equipment_quality_correlation`
--

LOCK TABLES `equipment_quality_correlation` WRITE;
/*!40000 ALTER TABLE `equipment_quality_correlation` DISABLE KEYS */;
/*!40000 ALTER TABLE `equipment_quality_correlation` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `equipment_status_history`
--

DROP TABLE IF EXISTS `equipment_status_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `equipment_status_history` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `equipment_id` bigint DEFAULT NULL,
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `started_at` datetime DEFAULT NULL,
  `ended_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `equipment_id` (`equipment_id`),
  CONSTRAINT `equipment_status_history_ibfk_1` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `equipment_status_history`
--

LOCK TABLES `equipment_status_history` WRITE;
/*!40000 ALTER TABLE `equipment_status_history` DISABLE KEYS */;
/*!40000 ALTER TABLE `equipment_status_history` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `fqc_inspection_items`
--

DROP TABLE IF EXISTS `fqc_inspection_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fqc_inspection_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `inspection_id` bigint DEFAULT NULL,
  `param_id` bigint DEFAULT NULL,
  `inspection_item_id` bigint DEFAULT NULL COMMENT '关联检验项目主数据',
  `item_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '检验项目名称',
  `item_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '检验项目编码',
  `data_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'numeric' COMMENT '数据类型',
  `measured_value` decimal(12,4) DEFAULT NULL,
  `usl` decimal(12,4) DEFAULT NULL COMMENT '规格上限',
  `lsl` decimal(12,4) DEFAULT NULL COMMENT '规格下限',
  `result` enum('pending','pass','fail') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending',
  `image_urls` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '图片URLs (JSON)',
  PRIMARY KEY (`id`),
  KEY `inspection_id` (`inspection_id`),
  KEY `param_id` (`param_id`),
  KEY `idx_fqc_items_inspection_item` (`inspection_item_id`),
  CONSTRAINT `fqc_inspection_items_ibfk_1` FOREIGN KEY (`inspection_id`) REFERENCES `fqc_inspections` (`id`),
  CONSTRAINT `fqc_inspection_items_ibfk_2` FOREIGN KEY (`param_id`) REFERENCES `dynamic_params` (`id`),
  CONSTRAINT `fqc_inspection_items_ibfk_3` FOREIGN KEY (`inspection_item_id`) REFERENCES `inspection_items` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `fqc_inspection_items`
--

LOCK TABLES `fqc_inspection_items` WRITE;
/*!40000 ALTER TABLE `fqc_inspection_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `fqc_inspection_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `fqc_inspections`
--

DROP TABLE IF EXISTS `fqc_inspections`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `fqc_inspections` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `inspection_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `batch_id` bigint DEFAULT NULL COMMENT '关联批次',
  `work_order_id` bigint DEFAULT NULL COMMENT '关联工单',
  `quantity` decimal(18,4) DEFAULT NULL COMMENT '批次数量',
  `sample_size` int DEFAULT NULL,
  `total_checked` int DEFAULT '0' COMMENT '已检数量',
  `total_pass` int DEFAULT '0' COMMENT '合格数量',
  `total_fail` int DEFAULT '0' COMMENT '不合格数量',
  `ac` int DEFAULT '0' COMMENT '合格判定数Ac',
  `re` int DEFAULT '0' COMMENT '不合格判定数Re',
  `inspection_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'full/sampling',
  `conclusion` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT 'qualified/unqualified/pending',
  `inspector_id` bigint DEFAULT NULL COMMENT '检验员ID',
  `aql_level` decimal(5,2) DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `checked_at` datetime DEFAULT NULL COMMENT '检验时间',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `inspection_no` (`inspection_no`),
  KEY `idx_fqc_org` (`org_id`),
  KEY `idx_fqc_conclusion` (`conclusion`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `fqc_inspections`
--

LOCK TABLES `fqc_inspections` WRITE;
/*!40000 ALTER TABLE `fqc_inspections` DISABLE KEYS */;
/*!40000 ALTER TABLE `fqc_inspections` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `inspection_items`
--

DROP TABLE IF EXISTS `inspection_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inspection_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `item_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '检验项目编码',
  `item_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '检验项目名称',
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '描述',
  `data_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'numeric' COMMENT '数据类型: numeric/visual/attribute',
  `unit` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '单位',
  `usl` decimal(15,6) DEFAULT NULL COMMENT '规格上限 USL',
  `lsl` decimal(15,6) DEFAULT NULL COMMENT '规格下限 LSL',
  `target_value` decimal(15,6) DEFAULT NULL COMMENT '目标值',
  `ucl` decimal(15,6) DEFAULT NULL COMMENT '管理上限 UCL',
  `lcl` decimal(15,6) DEFAULT NULL COMMENT '管理下限 LCL',
  `data_collection_param_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '数采参数编码(关联dynamic_params.code)',
  `chart_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '控制图类型: none/Xbar_R/Xbar_S/I_MR/P/U/C',
  `subgroup_size` int DEFAULT NULL COMMENT '默认子组大小(SPC用)',
  `inspection_method` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '检验方法/工具',
  `sample_size` int DEFAULT NULL COMMENT '默认抽样数量',
  `is_active` tinyint(1) DEFAULT '1',
  `created_by` bigint NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `item_code` (`item_code`),
  KEY `idx_inspection_items_active` (`is_active`),
  KEY `idx_inspection_items_param_code` (`data_collection_param_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inspection_items`
--

LOCK TABLES `inspection_items` WRITE;
/*!40000 ALTER TABLE `inspection_items` DISABLE KEYS */;
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
  `plan_id` bigint NOT NULL COMMENT '关联计划',
  `inspection_item_id` bigint NOT NULL COMMENT '关联检验项目',
  `sort_order` int DEFAULT '0' COMMENT '排序号',
  `usl` decimal(15,6) DEFAULT NULL COMMENT '规格上限(覆盖)',
  `lsl` decimal(15,6) DEFAULT NULL COMMENT '规格下限(覆盖)',
  `target_value` decimal(15,6) DEFAULT NULL COMMENT '目标值(覆盖)',
  `ucl` decimal(15,6) DEFAULT NULL COMMENT '管理上限(覆盖)',
  `lcl` decimal(15,6) DEFAULT NULL COMMENT '管理下限(覆盖)',
  `sample_size` int DEFAULT NULL COMMENT '抽样数量(覆盖)',
  `is_required` tinyint(1) DEFAULT '1' COMMENT '是否必检',
  PRIMARY KEY (`id`),
  KEY `idx_plan_items_plan` (`plan_id`),
  KEY `idx_plan_items_item` (`inspection_item_id`),
  CONSTRAINT `inspection_plan_items_ibfk_1` FOREIGN KEY (`plan_id`) REFERENCES `inspection_plans` (`id`) ON DELETE CASCADE,
  CONSTRAINT `inspection_plan_items_ibfk_2` FOREIGN KEY (`inspection_item_id`) REFERENCES `inspection_items` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inspection_plan_items`
--

LOCK TABLES `inspection_plan_items` WRITE;
/*!40000 ALTER TABLE `inspection_plan_items` DISABLE KEYS */;
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
  `plan_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '计划编码',
  `plan_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '计划名称',
  `inspection_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '检验类型: IQC/IPQC/FQC/OQC',
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '描述',
  `product_id` bigint DEFAULT NULL COMMENT '产品',
  `material_id` bigint DEFAULT NULL COMMENT '材料(关联products表)',
  `supplier_id` bigint DEFAULT NULL COMMENT '供应商',
  `customer_id` bigint DEFAULT NULL COMMENT '客户',
  `process_id` bigint DEFAULT NULL COMMENT '工艺/工序',
  `equipment_id` bigint DEFAULT NULL COMMENT '设备',
  `is_active` tinyint(1) DEFAULT '1',
  `created_by` bigint NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `plan_code` (`plan_code`),
  KEY `material_id` (`material_id`),
  KEY `customer_id` (`customer_id`),
  KEY `process_id` (`process_id`),
  KEY `equipment_id` (`equipment_id`),
  KEY `idx_plans_type` (`inspection_type`),
  KEY `idx_plans_product` (`product_id`),
  KEY `idx_plans_supplier` (`supplier_id`),
  CONSTRAINT `inspection_plans_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_2` FOREIGN KEY (`material_id`) REFERENCES `products` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_3` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_4` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_5` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`) ON DELETE SET NULL,
  CONSTRAINT `inspection_plans_ibfk_6` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inspection_plans`
--

LOCK TABLES `inspection_plans` WRITE;
/*!40000 ALTER TABLE `inspection_plans` DISABLE KEYS */;
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
  `standard_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `standard_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '描述',
  `item_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '检验项目名称',
  `product_id` bigint DEFAULT NULL,
  `process_id` bigint DEFAULT NULL,
  `inspection_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'IQC/IPQC/FQC/OQC',
  `usl` decimal(10,4) DEFAULT NULL COMMENT '规格上限 USL',
  `lsl` decimal(10,4) DEFAULT NULL COMMENT '规格下限 LSL',
  `target` decimal(10,4) DEFAULT NULL COMMENT '目标值',
  `unit` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '单位',
  `sampling_method` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `sampling_frequency` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '抽样频率',
  `aql` decimal(5,2) DEFAULT NULL,
  `inspection_level` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `items` json DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `standard_code` (`standard_code`),
  KEY `product_id` (`product_id`),
  KEY `process_id` (`process_id`),
  KEY `idx_standards_org` (`org_id`),
  CONSTRAINT `inspection_standards_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`),
  CONSTRAINT `inspection_standards_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inspection_standards`
--

LOCK TABLES `inspection_standards` WRITE;
/*!40000 ALTER TABLE `inspection_standards` DISABLE KEYS */;
/*!40000 ALTER TABLE `inspection_standards` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ipqc_ai_risk_scores`
--

DROP TABLE IF EXISTS `ipqc_ai_risk_scores`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ipqc_ai_risk_scores` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `equipment_id` bigint NOT NULL COMMENT '关联设备',
  `process_id` bigint NOT NULL COMMENT '关联工序',
  `work_order_id` bigint DEFAULT NULL COMMENT '关联工单',
  `risk_score` int DEFAULT '0' COMMENT '风险评分 0-100',
  `risk_level` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'normal' COMMENT '风险等级：normal/warning/critical',
  `factors_json` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '风险因素分解 (JSON)',
  `trend_direction` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '趋势方向：stable/rising/falling',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_risk_equipment` (`equipment_id`),
  KEY `idx_risk_process` (`process_id`),
  KEY `idx_risk_created` (`created_at`),
  CONSTRAINT `ipqc_ai_risk_scores_ibfk_1` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`),
  CONSTRAINT `ipqc_ai_risk_scores_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ipqc_ai_risk_scores`
--

LOCK TABLES `ipqc_ai_risk_scores` WRITE;
/*!40000 ALTER TABLE `ipqc_ai_risk_scores` DISABLE KEYS */;
/*!40000 ALTER TABLE `ipqc_ai_risk_scores` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ipqc_closure_status`
--

DROP TABLE IF EXISTS `ipqc_closure_status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ipqc_closure_status` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `work_order` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `status` enum('open','closed') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'open',
  `closed_at` datetime DEFAULT NULL,
  `rule_id` bigint DEFAULT NULL,
  `spc_result` json DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `rule_id` (`rule_id`),
  CONSTRAINT `ipqc_closure_status_ibfk_1` FOREIGN KEY (`rule_id`) REFERENCES `closure_rules` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ipqc_closure_status`
--

LOCK TABLES `ipqc_closure_status` WRITE;
/*!40000 ALTER TABLE `ipqc_closure_status` DISABLE KEYS */;
/*!40000 ALTER TABLE `ipqc_closure_status` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ipqc_first_piece_items`
--

DROP TABLE IF EXISTS `ipqc_first_piece_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ipqc_first_piece_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `first_piece_id` bigint NOT NULL,
  `inspection_item_id` bigint DEFAULT NULL COMMENT '关联检验项目主数据',
  `item_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '检验项目名称',
  `item_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '检验项目编码',
  `data_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'numeric',
  `usl` decimal(12,4) DEFAULT NULL COMMENT '规格上限',
  `lsl` decimal(12,4) DEFAULT NULL COMMENT '规格下限',
  `actual_value` decimal(12,4) DEFAULT NULL COMMENT '实测值',
  `result` enum('pass','fail','pending') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending',
  `image_urls` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '图片URLs (JSON)',
  `remarks` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '备注',
  PRIMARY KEY (`id`),
  KEY `first_piece_id` (`first_piece_id`),
  KEY `idx_fp_items_inspection_item` (`inspection_item_id`),
  CONSTRAINT `ipqc_first_piece_items_ibfk_1` FOREIGN KEY (`first_piece_id`) REFERENCES `ipqc_first_pieces` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ipqc_first_piece_items_ibfk_2` FOREIGN KEY (`inspection_item_id`) REFERENCES `inspection_items` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ipqc_first_piece_items`
--

LOCK TABLES `ipqc_first_piece_items` WRITE;
/*!40000 ALTER TABLE `ipqc_first_piece_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `ipqc_first_piece_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ipqc_first_pieces`
--

DROP TABLE IF EXISTS `ipqc_first_pieces`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ipqc_first_pieces` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `fp_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `product_id` bigint DEFAULT NULL,
  `process_id` bigint DEFAULT NULL,
  `equipment_id` bigint DEFAULT NULL,
  `work_order` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `batch_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `inspector` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `conclusion` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT '结论：pass/fail/pending',
  `reason` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '不合格原因',
  `shift` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '班次',
  `checked_at` datetime DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `fp_no` (`fp_no`),
  KEY `product_id` (`product_id`),
  KEY `process_id` (`process_id`),
  KEY `equipment_id` (`equipment_id`),
  KEY `idx_ipqc_fp_org` (`org_id`),
  KEY `idx_ipqc_fp_result` (`conclusion`),
  CONSTRAINT `ipqc_first_pieces_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`),
  CONSTRAINT `ipqc_first_pieces_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`),
  CONSTRAINT `ipqc_first_pieces_ibfk_3` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ipqc_first_pieces`
--

LOCK TABLES `ipqc_first_pieces` WRITE;
/*!40000 ALTER TABLE `ipqc_first_pieces` DISABLE KEYS */;
/*!40000 ALTER TABLE `ipqc_first_pieces` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ipqc_patrol_items`
--

DROP TABLE IF EXISTS `ipqc_patrol_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ipqc_patrol_items` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `patrol_id` bigint NOT NULL COMMENT '关联巡检记录',
  `inspection_item_id` bigint DEFAULT NULL COMMENT '关联检验项目主数据',
  `item_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '检验项目名称',
  `item_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '检验项目编码',
  `data_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'numeric' COMMENT '数据类型',
  `usl` decimal(12,4) DEFAULT NULL COMMENT '规格上限',
  `lsl` decimal(12,4) DEFAULT NULL COMMENT '规格下限',
  `actual_value` decimal(12,4) DEFAULT NULL COMMENT '实测值',
  `result` enum('pass','fail','pending') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT '结果',
  `image_urls` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '图片URLs (JSON)',
  PRIMARY KEY (`id`),
  KEY `patrol_id` (`patrol_id`),
  KEY `idx_patrol_items_inspection_item` (`inspection_item_id`),
  CONSTRAINT `ipqc_patrol_items_ibfk_1` FOREIGN KEY (`patrol_id`) REFERENCES `ipqc_patrols` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ipqc_patrol_items_ibfk_2` FOREIGN KEY (`inspection_item_id`) REFERENCES `inspection_items` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ipqc_patrol_items`
--

LOCK TABLES `ipqc_patrol_items` WRITE;
/*!40000 ALTER TABLE `ipqc_patrol_items` DISABLE KEYS */;
/*!40000 ALTER TABLE `ipqc_patrol_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ipqc_patrol_plan_equipment`
--

DROP TABLE IF EXISTS `ipqc_patrol_plan_equipment`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ipqc_patrol_plan_equipment` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `patrol_plan_id` bigint NOT NULL COMMENT '关联巡检计划',
  `equipment_id` bigint NOT NULL COMMENT '关联设备',
  `sort_order` int DEFAULT '0' COMMENT '排序',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_plan_equipment` (`patrol_plan_id`,`equipment_id`),
  KEY `equipment_id` (`equipment_id`),
  CONSTRAINT `ipqc_patrol_plan_equipment_ibfk_1` FOREIGN KEY (`patrol_plan_id`) REFERENCES `ipqc_patrol_plans` (`id`) ON DELETE CASCADE,
  CONSTRAINT `ipqc_patrol_plan_equipment_ibfk_2` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ipqc_patrol_plan_equipment`
--

LOCK TABLES `ipqc_patrol_plan_equipment` WRITE;
/*!40000 ALTER TABLE `ipqc_patrol_plan_equipment` DISABLE KEYS */;
/*!40000 ALTER TABLE `ipqc_patrol_plan_equipment` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ipqc_patrol_plans`
--

DROP TABLE IF EXISTS `ipqc_patrol_plans`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ipqc_patrol_plans` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `plan_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `equipment_id` bigint DEFAULT NULL,
  `process_id` bigint DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `interval_minutes` int DEFAULT NULL,
  `inspector` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'active' COMMENT 'active/paused/completed',
  PRIMARY KEY (`id`),
  UNIQUE KEY `plan_no` (`plan_no`),
  KEY `equipment_id` (`equipment_id`),
  KEY `process_id` (`process_id`),
  CONSTRAINT `ipqc_patrol_plans_ibfk_1` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`),
  CONSTRAINT `ipqc_patrol_plans_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ipqc_patrol_plans`
--

LOCK TABLES `ipqc_patrol_plans` WRITE;
/*!40000 ALTER TABLE `ipqc_patrol_plans` DISABLE KEYS */;
/*!40000 ALTER TABLE `ipqc_patrol_plans` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ipqc_patrols`
--

DROP TABLE IF EXISTS `ipqc_patrols`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ipqc_patrols` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `patrol_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `plan_id` bigint DEFAULT NULL,
  `inspector` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `patrol_time` datetime DEFAULT NULL,
  `equipment_id` bigint DEFAULT NULL,
  `process_id` bigint DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `conclusion` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT '结论：pass/fail/pending',
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT '状态',
  `remark` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`id`),
  UNIQUE KEY `patrol_no` (`patrol_no`),
  KEY `plan_id` (`plan_id`),
  KEY `equipment_id` (`equipment_id`),
  KEY `process_id` (`process_id`),
  CONSTRAINT `ipqc_patrols_ibfk_1` FOREIGN KEY (`plan_id`) REFERENCES `ipqc_patrol_plans` (`id`),
  CONSTRAINT `ipqc_patrols_ibfk_2` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`),
  CONSTRAINT `ipqc_patrols_ibfk_3` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ipqc_patrols`
--

LOCK TABLES `ipqc_patrols` WRITE;
/*!40000 ALTER TABLE `ipqc_patrols` DISABLE KEYS */;
/*!40000 ALTER TABLE `ipqc_patrols` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `iqc_anomalies`
--

DROP TABLE IF EXISTS `iqc_anomalies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `iqc_anomalies` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `anomaly_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `receipt_id` bigint DEFAULT NULL,
  `inspection_id` bigint DEFAULT NULL,
  `failed_item_ids` json DEFAULT NULL COMMENT '不合格检验项目 ID 列表',
  `defect_qty` int DEFAULT '0' COMMENT '不合格品数量',
  `anomaly_type` enum('quality','quantity','document','packaging','environment','other') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'quality' COMMENT '异常类型',
  `severity` enum('critical','major','minor') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `isolated_inventory` decimal(15,2) DEFAULT '0.00' COMMENT '隔离库存量',
  `disposition` enum('none','return','concession','rework','special_purchase') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'none' COMMENT '处置方式',
  `disposition_by` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '处置决定人',
  `disposition_date` datetime DEFAULT NULL COMMENT '处置决定时间',
  `handler_dept` enum('quality','purchasing','engineering','production','other') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '处理人部门',
  `status` enum('open','quarantined','investigating','mrb_reviewing','mrb_approved','disposed','processing','resolved','closed') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'open' COMMENT '状态',
  `handler` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `mrb_reviewed` tinyint(1) DEFAULT '0' COMMENT 'MRB 评审是否完成',
  `mrb_reviewer` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'MRB 评审人',
  `mrb_reviewed_at` datetime DEFAULT NULL COMMENT 'MRB 评审完成时间',
  `capa_id` bigint DEFAULT NULL COMMENT '关联 CAPA 单 ID',
  `first_response_at` datetime DEFAULT NULL COMMENT '首次响应时间',
  `supplier_notified` tinyint(1) DEFAULT '0' COMMENT '是否已通知供应商',
  `supplier_response_at` datetime DEFAULT NULL COMMENT '供应商回复时间',
  `resolved_at` datetime DEFAULT NULL,
  `created_by` bigint DEFAULT NULL COMMENT '创建人 ID',
  `updated_by` bigint DEFAULT NULL COMMENT '最后更新人 ID',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `anomaly_no` (`anomaly_no`),
  KEY `receipt_id` (`receipt_id`),
  KEY `inspection_id` (`inspection_id`),
  KEY `idx_anomalies_capa` (`capa_id`),
  KEY `idx_anomalies_mrb` (`mrb_reviewed`),
  KEY `idx_anomalies_disposition` (`disposition`),
  CONSTRAINT `iqc_anomalies_ibfk_1` FOREIGN KEY (`receipt_id`) REFERENCES `iqc_receipts` (`id`),
  CONSTRAINT `iqc_anomalies_ibfk_2` FOREIGN KEY (`inspection_id`) REFERENCES `iqc_inspections` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `inspection_id` bigint DEFAULT NULL,
  `param_id` bigint DEFAULT NULL COMMENT '关联 dynamic_params.id',
  `inspection_item_id` bigint DEFAULT NULL COMMENT '关联检验项目主数据',
  `item_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '检验项目名称（冗余）',
  `measured_value` decimal(12,4) DEFAULT NULL,
  `usl` decimal(12,4) DEFAULT NULL COMMENT '规格上限',
  `lsl` decimal(12,4) DEFAULT NULL COMMENT '规格下限',
  `result` enum('pass','fail') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `defect_code_id` bigint DEFAULT NULL,
  `remark` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  KEY `inspection_id` (`inspection_id`),
  KEY `param_id` (`param_id`),
  KEY `defect_code_id` (`defect_code_id`),
  KEY `idx_iqc_items_inspection_item` (`inspection_item_id`),
  CONSTRAINT `iqc_inspection_items_ibfk_1` FOREIGN KEY (`inspection_id`) REFERENCES `iqc_inspections` (`id`),
  CONSTRAINT `iqc_inspection_items_ibfk_2` FOREIGN KEY (`param_id`) REFERENCES `dynamic_params` (`id`),
  CONSTRAINT `iqc_inspection_items_ibfk_3` FOREIGN KEY (`inspection_item_id`) REFERENCES `inspection_items` (`id`) ON DELETE SET NULL,
  CONSTRAINT `iqc_inspection_items_ibfk_4` FOREIGN KEY (`defect_code_id`) REFERENCES `defect_codes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `inspection_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `receipt_id` bigint DEFAULT NULL,
  `standard_id` bigint DEFAULT NULL,
  `sample_size` int DEFAULT NULL,
  `ac` int DEFAULT NULL,
  `re` int DEFAULT NULL,
  `defect_qty` int DEFAULT '0',
  `sampling_level` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '抽样水平',
  `aql_value` decimal(5,2) DEFAULT NULL COMMENT 'AQL值',
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `result` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT 'pending/pass/fail/scrap',
  `inspector` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `inspected_at` datetime DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `inspection_no` (`inspection_no`),
  KEY `receipt_id` (`receipt_id`),
  KEY `standard_id` (`standard_id`),
  CONSTRAINT `iqc_inspections_ibfk_1` FOREIGN KEY (`receipt_id`) REFERENCES `iqc_receipts` (`id`),
  CONSTRAINT `iqc_inspections_ibfk_2` FOREIGN KEY (`standard_id`) REFERENCES `inspection_standards` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `receipt_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `supplier_id` bigint DEFAULT NULL,
  `product_id` bigint DEFAULT NULL,
  `batch_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `quantity` int DEFAULT NULL,
  `unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `receipt_date` datetime DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `inspector` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending' COMMENT 'pending/inspecting/completed/anomaly',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '创建时间',
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT '更新时间',
  PRIMARY KEY (`id`),
  UNIQUE KEY `receipt_no` (`receipt_no`),
  KEY `supplier_id` (`supplier_id`),
  KEY `product_id` (`product_id`),
  KEY `idx_iqc_receipts_org` (`org_id`),
  CONSTRAINT `iqc_receipts_ibfk_1` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`),
  CONSTRAINT `iqc_receipts_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `iqc_receipts`
--

LOCK TABLES `iqc_receipts` WRITE;
/*!40000 ALTER TABLE `iqc_receipts` DISABLE KEYS */;
/*!40000 ALTER TABLE `iqc_receipts` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `oqc_releases`
--

DROP TABLE IF EXISTS `oqc_releases`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `oqc_releases` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `batch_id` bigint DEFAULT NULL COMMENT '关联批次',
  `customer_id` bigint DEFAULT NULL COMMENT '关联客户',
  `release_number` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '' COMMENT '放行单号（唯一）',
  `release_date` datetime DEFAULT NULL COMMENT '放行日期',
  `quantity` decimal(18,4) NOT NULL DEFAULT '0.0000' COMMENT '放行数量',
  `authorized_by` int DEFAULT NULL COMMENT '授权人ID',
  `e_signature_url` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '电子签名URL（MinIO）',
  `signature_time` datetime DEFAULT NULL COMMENT '签名时间',
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'pending' COMMENT 'pending/signed/released/cancelled',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_oqc_releases_release_number` (`release_number`),
  KEY `idx_oqc_releases_batch` (`batch_id`),
  KEY `idx_oqc_releases_customer` (`customer_id`),
  CONSTRAINT `oqc_releases_ibfk_1` FOREIGN KEY (`batch_id`) REFERENCES `product_batches` (`id`),
  CONSTRAINT `oqc_releases_ibfk_2` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `oqc_releases`
--

LOCK TABLES `oqc_releases` WRITE;
/*!40000 ALTER TABLE `oqc_releases` DISABLE KEYS */;
/*!40000 ALTER TABLE `oqc_releases` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `organizations`
--

DROP TABLE IF EXISTS `organizations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `organizations` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '组织编码',
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '组织名称',
  `level` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '层级：group/company/workshop/line',
  `parent_id` bigint DEFAULT NULL COMMENT '父级组织 ID',
  `sort_order` int DEFAULT '0' COMMENT '排序号',
  `is_active` tinyint(1) DEFAULT '1' COMMENT '是否启用',
  `location` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '位置/地址',
  `contact` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '联系人信息 (JSON)',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '描述',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint DEFAULT NULL COMMENT '创建人',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_organizations_code` (`code`),
  KEY `idx_organizations_parent` (`parent_id`),
  KEY `idx_organizations_level` (`level`),
  CONSTRAINT `organizations_ibfk_1` FOREIGN KEY (`parent_id`) REFERENCES `organizations` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `organizations`
--

LOCK TABLES `organizations` WRITE;
/*!40000 ALTER TABLE `organizations` DISABLE KEYS */;
/*!40000 ALTER TABLE `organizations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `packaging_confirmations`
--

DROP TABLE IF EXISTS `packaging_confirmations`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `packaging_confirmations` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `batch_id` bigint NOT NULL COMMENT '关联批次',
  `packaging_method` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '包装方式',
  `qty_per_box` int DEFAULT NULL COMMENT '每箱数量',
  `total_boxes` int DEFAULT NULL COMMENT '总箱数',
  `label_printed` tinyint(1) DEFAULT '0' COMMENT '标签是否已打印',
  `confirmed_by` int NOT NULL COMMENT '确认人 ID',
  `confirmed_at` datetime DEFAULT CURRENT_TIMESTAMP COMMENT '确认时间',
  PRIMARY KEY (`id`),
  KEY `idx_packaging_batch` (`batch_id`),
  CONSTRAINT `packaging_confirmations_ibfk_1` FOREIGN KEY (`batch_id`) REFERENCES `product_batches` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `packaging_confirmations`
--

LOCK TABLES `packaging_confirmations` WRITE;
/*!40000 ALTER TABLE `packaging_confirmations` DISABLE KEYS */;
/*!40000 ALTER TABLE `packaging_confirmations` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `param_groups`
--

DROP TABLE IF EXISTS `param_groups`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `param_groups` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '组名',
  `code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '组编码',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '描述',
  `sort_order` int DEFAULT '0' COMMENT '排序号',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL DEFAULT '1' COMMENT '创建人ID',
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `param_groups`
--

LOCK TABLES `param_groups` WRITE;
/*!40000 ALTER TABLE `param_groups` DISABLE KEYS */;
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
  `param_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '参数编码',
  `equipment_id` bigint DEFAULT NULL COMMENT '设备ID',
  `value` decimal(15,6) DEFAULT NULL COMMENT '数值(数值型)',
  `value_raw` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '原始值(枚举型/布尔型)',
  `timestamp` datetime NOT NULL COMMENT '采集时间',
  `quality_result` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'UNKNOWN' COMMENT '质量结果 OK/NG/UNKNOWN',
  PRIMARY KEY (`id`),
  KEY `idx_param_time` (`param_code`,`timestamp`),
  KEY `idx_equip_time` (`equipment_id`,`timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `param_realtime_values`
--

LOCK TABLES `param_realtime_values` WRITE;
/*!40000 ALTER TABLE `param_realtime_values` DISABLE KEYS */;
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
  `name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `code` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `module` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `code` (`code`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `permissions`
--

LOCK TABLES `permissions` WRITE;
/*!40000 ALTER TABLE `permissions` DISABLE KEYS */;
INSERT INTO `permissions` VALUES (1,'全部权限','*:*','System',NULL),(2,'用户管理','system:user','M15',NULL),(3,'角色管理','system:role','M15',NULL),(4,'基础数据管理','basic:data','M02',NULL),(5,'IQC 检验','iqc:inspect','M03',NULL),(6,'IPQC 检验','ipqc:inspect','M04',NULL),(7,'FQC 检验','fqc:inspect','M05',NULL),(8,'SPC 查看','spc:view','M06',NULL),(9,'不良管理','defect:manage','M07',NULL),(10,'质量追溯','trace:view','M08',NULL),(11,'AI 分析','ai:analyze','M10',NULL),(12,'报表查看','report:view','M14',NULL);
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
  `process_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `process_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `process_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `department` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '所属部门/车间（显示冗余）',
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `process_code` (`process_code`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `processes`
--

LOCK TABLES `processes` WRITE;
/*!40000 ALTER TABLE `processes` DISABLE KEYS */;
INSERT INTO `processes` VALUES (1,'PRC-01','来料检验','检验',NULL,'品质部',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(2,'PRC-02','粗车','加工',NULL,'机加车间',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(3,'PRC-03','精车','加工',NULL,'机加车间',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(4,'PRC-04','钻孔','加工',NULL,'机加车间',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(5,'PRC-05','热处理','加工',NULL,'热处理车间',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(6,'PRC-06','研磨','加工',NULL,'机加车间',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(7,'PRC-07','过程检验','检验',NULL,'品质部',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(8,'PRC-08','成品检验','检验',NULL,'品质部',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(9,'PRC-09','清洗包装','包装',NULL,'包装车间',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(10,'PRC-10','出货检验','检验',NULL,'品质部',NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28');
/*!40000 ALTER TABLE `processes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `product_batches`
--

DROP TABLE IF EXISTS `product_batches`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `product_batches` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `batch_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '批次编号（LOT-YYYYMMDD-X）',
  `source` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'manual' COMMENT '来源：manual / ipqc-auto / work-order',
  `product_id` bigint NOT NULL COMMENT '关联产品',
  `work_order_id` bigint DEFAULT NULL COMMENT '关联工单',
  `quantity` decimal(18,4) NOT NULL DEFAULT '0.0000' COMMENT '数量',
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'in_progress' COMMENT 'in_progress/inspected/released/quarantined',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `batch_code` (`batch_code`),
  UNIQUE KEY `uk_product_batches_batch_code` (`batch_code`),
  KEY `idx_product_batches_status` (`status`),
  KEY `idx_product_batches_product` (`product_id`),
  CONSTRAINT `product_batches_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `product_batches`
--

LOCK TABLES `product_batches` WRITE;
/*!40000 ALTER TABLE `product_batches` DISABLE KEYS */;
/*!40000 ALTER TABLE `product_batches` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `product_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `product_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '产品描述',
  `product_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '产品类别',
  `specification` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `default_inspection_level` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'II',
  `default_aql` decimal(10,4) DEFAULT NULL COMMENT '默认AQL值',
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `product_code` (`product_code`),
  KEY `idx_products_org` (`org_id`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES (1,'P001','精密转轴 A100',NULL,'机加工件',NULL,'pcs','II',1.0000,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(2,'P002','壳体 B200',NULL,'压铸件',NULL,'pcs','II',0.6500,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(3,'P003','PCB 主板 C300',NULL,'电子件',NULL,'pcs','S-3',0.2500,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(4,'P004','密封圈 D400',NULL,'橡胶件',NULL,'pcs','I',2.5000,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(5,'P005','连接线束 E500',NULL,'标准件',NULL,'套','II',1.5000,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28');
/*!40000 ALTER TABLE `products` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `role_permissions`
--

DROP TABLE IF EXISTS `role_permissions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `role_permissions` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `role_id` bigint NOT NULL,
  `permission_id` bigint NOT NULL,
  PRIMARY KEY (`id`),
  KEY `role_id` (`role_id`),
  KEY `permission_id` (`permission_id`),
  CONSTRAINT `role_permissions_ibfk_1` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`),
  CONSTRAINT `role_permissions_ibfk_2` FOREIGN KEY (`permission_id`) REFERENCES `permissions` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `role_permissions`
--

LOCK TABLES `role_permissions` WRITE;
/*!40000 ALTER TABLE `role_permissions` DISABLE KEYS */;
/*!40000 ALTER TABLE `role_permissions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'Administrator','系统管理员，拥有全部权限','2026-07-30 06:10:28'),(2,'Operator','质检操作员','2026-07-30 06:10:28'),(3,'Inspector','质量检验员','2026-07-30 06:10:28'),(4,'Engineer','质量工程师','2026-07-30 06:10:28');
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
  `product_id` bigint NOT NULL COMMENT '关联产品',
  `route_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '工艺路线编码',
  `route_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '工艺路线名称',
  `route_type` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'STD' COMMENT '类型',
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '描述',
  `is_default` tinyint(1) DEFAULT '0' COMMENT '是否默认',
  `is_active` tinyint(1) DEFAULT '1' COMMENT '是否启用',
  `sort_order` int DEFAULT '0' COMMENT '排序',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_routing_code` (`product_id`,`route_code`),
  KEY `idx_routing_product` (`product_id`),
  CONSTRAINT `routing_headers_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `routing_headers`
--

LOCK TABLES `routing_headers` WRITE;
/*!40000 ALTER TABLE `routing_headers` DISABLE KEYS */;
INSERT INTO `routing_headers` VALUES (1,1,'RT-P001','精密转轴A100工艺路线','STD','Auto migrated from routings',0,1,0,'2026-07-30 06:10:28','2026-07-30 06:10:28');
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
  `routing_header_id` bigint DEFAULT NULL,
  `routing_id` bigint NOT NULL,
  `step_order` int NOT NULL,
  `process_id` bigint NOT NULL,
  `process_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `workcenter` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `standard_time` int DEFAULT NULL,
  `description` text COLLATE utf8mb4_unicode_ci,
  `standard_time_minutes` int DEFAULT NULL,
  `pre_wait_time_minutes` double DEFAULT NULL,
  `post_wait_time_minutes` double DEFAULT NULL,
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `routing_id` (`routing_id`),
  KEY `process_id` (`process_id`),
  KEY `idx_routing_steps_header` (`routing_header_id`),
  CONSTRAINT `routing_steps_ibfk_1` FOREIGN KEY (`routing_id`) REFERENCES `routings` (`id`),
  CONSTRAINT `routing_steps_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `routing_steps`
--

LOCK TABLES `routing_steps` WRITE;
/*!40000 ALTER TABLE `routing_steps` DISABLE KEYS */;
INSERT INTO `routing_steps` VALUES (1,1,1,1,1,'来料检验',NULL,5,'精密转轴A100工艺路线',5,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(2,1,2,2,2,'粗车',NULL,15,'精密转轴A100工艺路线',15,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(3,1,3,3,3,'精车',NULL,20,'精密转轴A100工艺路线',20,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(4,1,4,4,4,'钻孔',NULL,10,'精密转轴A100工艺路线',10,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(5,1,5,5,5,'热处理',NULL,30,'精密转轴A100工艺路线',30,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(6,1,6,6,6,'研磨',NULL,25,'精密转轴A100工艺路线',25,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(7,1,7,7,7,'过程检验',NULL,5,'精密转轴A100工艺路线',5,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(8,1,8,8,8,'成品检验',NULL,5,'精密转轴A100工艺路线',5,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(9,1,9,9,9,'清洗包装',NULL,10,'精密转轴A100工艺路线',10,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(10,1,10,10,10,'出货检验',NULL,5,'精密转轴A100工艺路线',5,NULL,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28');
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
  `routing_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '工艺路线编号（同一路线多步骤共享）',
  `routing_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '工艺路线名称',
  `description` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '工艺路线描述',
  `step_order` int NOT NULL DEFAULT '0' COMMENT '工序顺序',
  `process_id` bigint DEFAULT NULL COMMENT '关联工序ID',
  `standard_time_minutes` decimal(10,2) DEFAULT '0.00' COMMENT '标准工时（分钟）',
  `product_id` bigint NOT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`),
  KEY `process_id` (`process_id`),
  CONSTRAINT `routings_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`),
  CONSTRAINT `routings_ibfk_2` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `routings`
--

LOCK TABLES `routings` WRITE;
/*!40000 ALTER TABLE `routings` DISABLE KEYS */;
INSERT INTO `routings` VALUES (1,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',1,1,5.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(2,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',2,2,15.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(3,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',3,3,20.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(4,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',4,4,10.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(5,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',5,5,30.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(6,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',6,6,25.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(7,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',7,7,5.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(8,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',8,8,5.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(9,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',9,9,10.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(10,'RT-P001','精密转轴A100工艺路线','精密转轴A100工艺路线',10,10,5.00,1,NULL,1,'2026-07-30 06:10:28','2026-07-30 06:10:28');
/*!40000 ALTER TABLE `routings` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `scrap_rework_records`
--

DROP TABLE IF EXISTS `scrap_rework_records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `scrap_rework_records` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'scrap/rework',
  `defect_id` bigint DEFAULT NULL,
  `batch_id` bigint DEFAULT NULL,
  `quantity` decimal(15,2) NOT NULL,
  `reason` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `rework_steps` json DEFAULT NULL COMMENT '返工步骤(仅返工)',
  `rework_inspection_required` tinyint(1) DEFAULT '0',
  `rework_inspection_result` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'pending',
  `authorized_by` bigint NOT NULL,
  `authorized_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_type` (`type`),
  KEY `defect_id` (`defect_id`),
  CONSTRAINT `scrap_rework_records_ibfk_1` FOREIGN KEY (`defect_id`) REFERENCES `defects` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `scrap_rework_records`
--

LOCK TABLES `scrap_rework_records` WRITE;
/*!40000 ALTER TABLE `scrap_rework_records` DISABLE KEYS */;
/*!40000 ALTER TABLE `scrap_rework_records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `spc_alert_rules`
--

DROP TABLE IF EXISTS `spc_alert_rules`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `spc_alert_rules` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `chart_id` bigint NOT NULL,
  `rule_number` int NOT NULL COMMENT '1-8',
  `rule_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `rule_description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `enabled` tinyint(1) DEFAULT '1',
  `trigger_threshold` int DEFAULT '1' COMMENT '触发阈值(如连续N点)',
  `sigma_threshold` decimal(5,2) DEFAULT '2.00' COMMENT 'σ阈值',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `chart_id` (`chart_id`),
  CONSTRAINT `spc_alert_rules_ibfk_1` FOREIGN KEY (`chart_id`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `chart_id` bigint NOT NULL,
  `rule_id` bigint NOT NULL,
  `rule_number` int NOT NULL,
  `triggered_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `violated_point_index` int NOT NULL COMMENT '触发点子组索引',
  `detail` json DEFAULT NULL COMMENT '触发详情',
  `resolved` tinyint(1) DEFAULT '0',
  `resolved_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_spc_triggers_chart` (`chart_id`,`triggered_at`),
  KEY `rule_id` (`rule_id`),
  CONSTRAINT `spc_alert_triggers_ibfk_1` FOREIGN KEY (`chart_id`) REFERENCES `spc_control_charts` (`id`),
  CONSTRAINT `spc_alert_triggers_ibfk_2` FOREIGN KEY (`rule_id`) REFERENCES `spc_alert_rules` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `chart_id` bigint NOT NULL,
  `analysis_type` enum('cpk','ppk','capability') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `cp` decimal(10,4) DEFAULT NULL,
  `cpk` decimal(10,4) DEFAULT NULL,
  `pp` decimal(10,4) DEFAULT NULL,
  `ppk` decimal(10,4) DEFAULT NULL,
  `sigma_within` decimal(15,6) DEFAULT NULL,
  `sigma_overall` decimal(15,6) DEFAULT NULL,
  `estimated_ppm` decimal(15,2) DEFAULT NULL COMMENT 'Estimated DPMO',
  `data_points_used` int DEFAULT NULL COMMENT '参与分析的数据点数量',
  `analysis_period_start` date DEFAULT NULL,
  `analysis_period_end` date DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `chart_id` (`chart_id`),
  CONSTRAINT `spc_analysis_results_ibfk_1` FOREIGN KEY (`chart_id`) REFERENCES `spc_control_charts` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `chart_id` bigint NOT NULL,
  `source` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'operator/machine/material/method/environment',
  `sum_of_squares` decimal(20,4) DEFAULT NULL,
  `degrees_freedom` int DEFAULT NULL,
  `mean_square` decimal(20,4) DEFAULT NULL,
  `f_ratio` decimal(10,4) DEFAULT NULL,
  `p_value` decimal(10,6) DEFAULT NULL,
  `significant` tinyint(1) DEFAULT '0',
  `analysis_date` date NOT NULL,
  PRIMARY KEY (`id`),
  KEY `chart_id` (`chart_id`),
  CONSTRAINT `spc_anova_results_ibfk_1` FOREIGN KEY (`chart_id`) REFERENCES `spc_control_charts` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `process_id` bigint NOT NULL,
  `parameter_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `chart_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Xbar_R / Xbar_S / I_MR',
  `subgroup_size` int NOT NULL DEFAULT '5',
  `usl` decimal(15,6) DEFAULT NULL,
  `lsl` decimal(15,6) DEFAULT NULL,
  `target_value` decimal(15,6) DEFAULT NULL,
  `cl` decimal(15,6) DEFAULT NULL COMMENT 'Center Line',
  `ucl` decimal(15,6) DEFAULT NULL COMMENT 'Upper Control Limit',
  `lcl` decimal(15,6) DEFAULT NULL COMMENT 'Lower Control Limit',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `created_by` bigint NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_spc_charts_name` (`name`),
  KEY `idx_spc_charts_param` (`parameter_code`),
  KEY `process_id` (`process_id`),
  CONSTRAINT `spc_control_charts_ibfk_1` FOREIGN KEY (`process_id`) REFERENCES `processes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `chart_id` bigint NOT NULL,
  `subgroup_index` int NOT NULL COMMENT '子组编号',
  `individual_values` json NOT NULL COMMENT '子组内原始值数组',
  `subgroup_mean` decimal(15,6) DEFAULT NULL COMMENT 'X̄',
  `subgroup_range` decimal(15,6) DEFAULT NULL COMMENT 'R (或 S)',
  `measured_at` datetime NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_spc_dp_chart_subgroup` (`chart_id`,`subgroup_index`),
  CONSTRAINT `spc_data_points_ibfk_1` FOREIGN KEY (`chart_id`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `chart_id` bigint NOT NULL COMMENT '关联控制图',
  `source_type` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '数据源类型: IQC/IPQC/FQC',
  `inspection_item_id` bigint DEFAULT NULL COMMENT '关联检验项目（null表示全部）',
  `product_id` bigint DEFAULT NULL COMMENT '过滤：产品',
  `process_id` bigint DEFAULT NULL COMMENT '过滤：工序',
  `supplier_id` bigint DEFAULT NULL COMMENT '过滤：供应商',
  `customer_id` bigint DEFAULT NULL COMMENT '过滤：客户',
  `equipment_id` bigint DEFAULT NULL COMMENT '过滤：设备',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `inspection_item_id` (`inspection_item_id`),
  KEY `idx_spc_ds_chart` (`chart_id`),
  CONSTRAINT `spc_data_sources_ibfk_1` FOREIGN KEY (`chart_id`) REFERENCES `spc_control_charts` (`id`) ON DELETE CASCADE,
  CONSTRAINT `spc_data_sources_ibfk_2` FOREIGN KEY (`inspection_item_id`) REFERENCES `inspection_items` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
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
  `supplier_id` bigint DEFAULT NULL,
  `assessment_date` date DEFAULT NULL COMMENT '评分日期',
  `score` decimal(5,2) DEFAULT NULL,
  `dimension_scores` json DEFAULT NULL COMMENT '维度评分',
  `grade` varchar(1) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '评级：A/B/C/D',
  `evaluation` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '评估意见',
  PRIMARY KEY (`id`),
  KEY `idx_supplier_scores_supplier` (`supplier_id`),
  CONSTRAINT `supplier_scores_ibfk_1` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `supplier_scores`
--

LOCK TABLES `supplier_scores` WRITE;
/*!40000 ALTER TABLE `supplier_scores` DISABLE KEYS */;
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
  `supplier_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `supplier_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `contact_person` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `address` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci,
  `rating` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '供应商等级：A/B/C/D',
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'active' COMMENT 'active/inactive/blacklisted',
  `supply_category` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '供应产品类别',
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `supplier_code` (`supplier_code`),
  KEY `idx_suppliers_org` (`org_id`),
  KEY `idx_suppliers_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `suppliers`
--

LOCK TABLES `suppliers` WRITE;
/*!40000 ALTER TABLE `suppliers` DISABLE KEYS */;
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
  `type_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '字典类型编码',
  `item_label` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '显示标签',
  `item_value` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '选项值',
  `sort_order` int DEFAULT '0' COMMENT '排序号',
  `color` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '颜色标识',
  `is_default` tinyint(1) DEFAULT '0' COMMENT '是否默认',
  `status` tinyint(1) DEFAULT '1' COMMENT '状态',
  `remark` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '备注',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_dict_item` (`type_code`,`item_value`),
  KEY `idx_dict_type` (`type_code`),
  CONSTRAINT `sys_dict_items_ibfk_1` FOREIGN KEY (`type_code`) REFERENCES `sys_dict_types` (`type_code`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=58 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sys_dict_items`
--

LOCK TABLES `sys_dict_items` WRITE;
/*!40000 ALTER TABLE `sys_dict_items` DISABLE KEYS */;
INSERT INTO `sys_dict_items` VALUES (1,'inspection_type','来料检验','IQC',1,NULL,1,1,NULL,'2026-07-30 06:04:18'),(2,'inspection_type','过程检验','IPQC',2,NULL,0,1,NULL,'2026-07-30 06:04:18'),(3,'inspection_type','成品检验','FQC',3,NULL,0,1,NULL,'2026-07-30 06:04:18'),(4,'inspection_type','出货检验','OQC',4,NULL,0,1,NULL,'2026-07-30 06:04:18'),(5,'defect_severity','严重','MA',1,NULL,1,1,NULL,'2026-07-30 06:04:18'),(6,'defect_severity','轻微','MI',2,NULL,0,1,NULL,'2026-07-30 06:04:18'),(7,'defect_severity','致命','CR',3,NULL,0,1,NULL,'2026-07-30 06:04:18'),(8,'status','启用','active',1,NULL,1,1,NULL,'2026-07-30 06:04:18'),(9,'status','禁用','inactive',2,NULL,0,1,NULL,'2026-07-30 06:04:18'),(10,'priority','高','HIGH',1,NULL,0,1,NULL,'2026-07-30 06:04:18'),(11,'priority','中','MEDIUM',2,NULL,1,1,NULL,'2026-07-30 06:04:18'),(12,'priority','低','LOW',3,NULL,0,1,NULL,'2026-07-30 06:04:18'),(13,'equipment_status','运行中','RUNNING',1,NULL,1,1,NULL,'2026-07-30 06:04:18'),(14,'equipment_status','停机','STOPPED',2,NULL,0,1,NULL,'2026-07-30 06:04:18'),(15,'equipment_status','维护中','MAINTENANCE',3,NULL,0,1,NULL,'2026-07-30 06:04:18'),(16,'org_level','集团','group',1,NULL,1,1,NULL,'2026-07-30 06:04:18'),(17,'org_level','公司','company',2,NULL,0,1,NULL,'2026-07-30 06:04:18'),(18,'org_level','车间','workshop',3,NULL,0,1,NULL,'2026-07-30 06:04:18'),(19,'org_level','产线','line',4,NULL,0,1,NULL,'2026-07-30 06:04:18');
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
  `type_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '字典类型编码',
  `type_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '字典类型名称',
  `is_system` tinyint(1) DEFAULT '0' COMMENT '是否系统内置',
  `status` tinyint(1) DEFAULT '1' COMMENT '状态',
  `remark` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci COMMENT '备注',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_dict_type_code` (`type_code`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `sys_dict_types`
--

LOCK TABLES `sys_dict_types` WRITE;
/*!40000 ALTER TABLE `sys_dict_types` DISABLE KEYS */;
INSERT INTO `sys_dict_types` VALUES (1,'inspection_type','检验类型',1,1,'IQC/IPQC/FQC/OQC','2026-07-30 06:04:18'),(2,'defect_severity','缺陷严重度',1,1,'MA/MI/CR','2026-07-30 06:04:18'),(3,'status','状态',1,1,'通用状态','2026-07-30 06:04:18'),(4,'priority','优先级',1,1,'HIGH/MEDIUM/LOW','2026-07-30 06:04:18'),(5,'equipment_status','设备状态',1,1,'RUNNING/STOPPED/MAINTENANCE','2026-07-30 06:04:18'),(6,'org_level','组织层级',1,1,'group/company/workshop/line','2026-07-30 06:04:18');
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
  `tool_code` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `tool_name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `model` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '工具型号',
  `tool_type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `design_life` decimal(10,2) DEFAULT NULL COMMENT '设计寿命',
  `life_unit` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'cycles' COMMENT '寿命单位',
  `life_current` decimal(10,2) DEFAULT '0.00' COMMENT '当前已用寿命',
  `supplier` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '供应商',
  `status` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT 'active' COMMENT 'active/worn/broken/retired',
  `equipment_id` bigint DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `tool_code` (`tool_code`),
  KEY `idx_tools_org` (`org_id`),
  KEY `idx_tools_equipment` (`equipment_id`),
  KEY `idx_tools_status` (`status`),
  CONSTRAINT `tools_ibfk_1` FOREIGN KEY (`equipment_id`) REFERENCES `equipment` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tools`
--

LOCK TABLES `tools` WRITE;
/*!40000 ALTER TABLE `tools` DISABLE KEYS */;
/*!40000 ALTER TABLE `tools` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `trace_records`
--

DROP TABLE IF EXISTS `trace_records`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `trace_records` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `trace_no` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `product_id` bigint DEFAULT NULL,
  `batch_no` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `sn` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `trace_type` enum('sn','batch','equipment','tool') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `trace_data` json DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `product_id` (`product_id`),
  CONSTRAINT `trace_records_ibfk_1` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `trace_records`
--

LOCK TABLES `trace_records` WRITE;
/*!40000 ALTER TABLE `trace_records` DISABLE KEYS */;
/*!40000 ALTER TABLE `trace_records` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `username` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `password_hash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `display_name` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `avatar` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `phone` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `org_id` bigint DEFAULT NULL COMMENT '所属组织',
  `is_active` tinyint(1) DEFAULT '1',
  `role_id` bigint DEFAULT NULL,
  `last_login_at` datetime DEFAULT NULL,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`),
  KEY `idx_users_org` (`org_id`),
  KEY `idx_users_role_id` (`role_id`),
  KEY `idx_users_is_active` (`is_active`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'admin','YNAedq2DqdKTemWpHr525fP9TWz2Ha18NMFizpA97NOI7jFv4phW2fAIjP4AB39k','系统管理员','','admin@qm-ai.com',NULL,NULL,1,1,NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(2,'operator','e6Jpl7pRE55wfmkiQOymQzdbdzSzurvgzBOF0ENDrJj6PeChTJewEW752jzgaZWG','质检操作员',NULL,'operator@qm-ai.com',NULL,NULL,1,2,NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28'),(3,'inspector','64Tx/yBGH9v9O9MKTCwilMbVMkyiSOwBcVdbqyzONqP3WJGTc/5u8nJIqLRuhsvd','质量检验员',NULL,'inspector@qm-ai.com',NULL,NULL,1,3,NULL,'2026-07-30 06:10:28','2026-07-30 06:10:28');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-07-30 15:41:20
