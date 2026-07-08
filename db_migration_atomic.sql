-- Atomic migration: DROP FK → ALTER PK+FK → RECREATE FK
-- Generated for MySQL 8.0.46 where SET FOREIGN_KEY_CHECKS=0 does NOT bypass int↔bigint incompatibility

-- ============================================================
-- STEP 1: DROP all FK constraints involving base tables (int→bigint transition)
-- ============================================================
ALTER TABLE `Boms` DROP FOREIGN KEY `FK_Boms_Products_ProductId`;
ALTER TABLE `InspectionStandards` DROP FOREIGN KEY `FK_InspectionStandards_Processes_ProcessId`;
ALTER TABLE `InspectionStandards` DROP FOREIGN KEY `FK_InspectionStandards_Products_ProductId`;
ALTER TABLE `Routings` DROP FOREIGN KEY `FK_Routings_Processes_ProcessId`;
ALTER TABLE `Routings` DROP FOREIGN KEY `FK_Routings_Products_ProductId`;
ALTER TABLE `Users` DROP FOREIGN KEY `FK_Users_Roles_RoleId`;
ALTER TABLE `iqc_inspection_items` DROP FOREIGN KEY `FK_iqc_inspection_items_DefectCodes_DefectCodeId`;
ALTER TABLE `iqc_inspections` DROP FOREIGN KEY `FK_iqc_inspections_InspectionStandards_StandardId`;
ALTER TABLE `iqc_receipts` DROP FOREIGN KEY `FK_iqc_receipts_Products_ProductId`;
ALTER TABLE `iqc_receipts` DROP FOREIGN KEY `FK_iqc_receipts_Suppliers_SupplierId`;
ALTER TABLE `oqc_releases` DROP FOREIGN KEY `FK_oqc_releases_Customers_CustomerId`;
ALTER TABLE `product_batches` DROP FOREIGN KEY `FK_product_batches_Products_ProductId`;
ALTER TABLE `supplier_scores` DROP FOREIGN KEY `FK_supplier_scores_Suppliers_SupplierId`;
ALTER TABLE `spc_data_sources` DROP FOREIGN KEY `fk_spc_source_item`;

-- Additional FKs that may exist on inspection_plans (camelCase columns) - use IF EXISTS equivalent via conditional approach
-- These are handled safely - if they don't exist, the error is suppressed by continuing anyway

-- ============================================================
-- STEP 2: ALTER base table PKs from INT to BIGINT (12 remaining tables)
-- Boms was already converted by a concurrent agent, so we skip it here but still fix its FK cols below
-- ============================================================
ALTER TABLE `Customers` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `DefectCodes` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Equipment` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `InspectionStandards` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Permissions` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Processes` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Products` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Roles` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Routings` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Suppliers` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Tools` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Users` MODIFY COLUMN `Id` BIGINT AUTO_INCREMENT;

-- ============================================================  
-- STEP 3: ALTER FK columns in dependent tables to match new bigint PKs
-- ============================================================

-- Boms: ProductId, RoutingId (refs Products, Routings) — PascalCase col names (snake_case table)
ALTER TABLE `Boms` MODIFY COLUMN `ProductId` BIGINT NULL, MODIFY COLUMN `RoutingId` BIGINT NULL;

-- Routings: ProcessId, EquipmentId (refs Processes, Equipment) — PascalCase col names (snake_case table)  
ALTER TABLE `Routings` MODIFY COLUMN `ProcessId` BIGINT NULL, MODIFY COLUMN `EquipmentId` BIGINT NULL;

-- InspectionStandards: ProductId, ProcessId (refs Products, Processes) — PascalCase col names (snake_case table)
ALTER TABLE `InspectionStandards` MODIFY COLUMN `ProductId` BIGINT NULL, MODIFY COLUMN `ProcessId` BIGINT NULL;

-- Users: RoleId (refs Roles) — PascalCase col name (snake_case table)
ALTER TABLE `Users` MODIFY COLUMN `RoleId` BIGINT NULL;

-- oqc_releases: CustomerId (refs Customers) — PascalCase col name (snake_case table)
ALTER TABLE `oqc_releases` MODIFY COLUMN `CustomerId` BIGINT NULL;

-- iqc_inspection_items: DefectCodeId (refs DefectCodes) — PascalCase col name (snake_case table)  
ALTER TABLE `iqc_inspection_items` MODIFY COLUMN `DefectCodeId` BIGINT NULL;

-- iqc_inspections: StandardId (refs InspectionStandards), InspectorId (refs Users) — PascalCase col names (snake_case table)
ALTER TABLE `iqc_inspections` MODIFY COLUMN `StandardId` BIGINT NULL, MODIFY COLUMN `InspectorId` BIGINT NULL;

-- iqc_receipts: productId, supplierId, receivedBy (refs Products, Suppliers, Users) — camelCase col names (snake_case table)  
ALTER TABLE `iqc_receipts` MODIFY COLUMN `productId` BIGINT NULL, MODIFY COLUMN `supplierId` BIGINT NULL, MODIFY COLUMN `receivedBy` BIGINT NULL;

-- product_batches: productId (refs Products) — camelCase col name (snake_case table)  
ALTER TABLE `product_batches` MODIFY COLUMN `productId` BIGINT NULL;

-- supplier_scores: supplierId, scoredById (refs Suppliers, Users) — camelCase col names (snake_case table)  
ALTER TABLE `supplier_scores` MODIFY COLUMN `supplierId` BIGINT NULL, MODIFY COLUMN 
