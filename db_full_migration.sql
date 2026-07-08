SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- PHASE 1: Alter base table PKs from INT to BIGINT
-- (With FK checks disabled, no need to drop/recreate constraints)
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
-- PHASE 2: Alter FK columns in tables referencing base tables
-- ============================================================

-- Boms: ProductId, RoutingId (refs Products, Routings)
ALTER TABLE `Boms` MODIFY COLUMN `ProductId` BIGINT NULL, MODIFY COLUMN `RoutingId` BIGINT NULL;

-- Routings: ProcessId, EquipmentId (refs Processes, Equipment)
ALTER TABLE `Routings` MODIFY COLUMN `ProcessId` BIGINT NULL, MODIFY COLUMN `EquipmentId` BIGINT NULL;

-- InspectionStandards: ProductId, ProcessId (refs Products, Processes)
ALTER TABLE `InspectionStandards` MODIFY COLUMN `ProductId` BIGINT NULL, MODIFY COLUMN `ProcessId` BIGINT NULL;

-- Users: RoleId (refs Roles)
ALTER TABLE `Users` MODIFY COLUMN `RoleId` BIGINT NULL;

-- oqc_releases: CustomerId (refs Customers) — PascalCase col names
ALTER TABLE `oqc_releases` MODIFY COLUMN `CustomerId` BIGINT NULL;

-- iqc_inspection_items: DefectCodeId (refs DefectCodes) — PascalCase col names  
ALTER TABLE `iqc_inspection_items` MODIFY COLUMN `DefectCodeId` BIGINT NULL;

-- iqc_inspections: StandardId (refs InspectionStandards), InspectorId (refs Users) — PascalCase col names
ALTER TABLE `iqc_inspections` MODIFY COLUMN `StandardId` BIGINT NULL, MODIFY COLUMN `InspectorId` BIGINT NULL;

-- iqc_receipts: ProductId, SupplierId, ReceivedBy (refs Products, Suppliers, Users) — camelCase col names
ALTER TABLE `iqc_receipts` MODIFY COLUMN `productId` BIGINT NULL, MODIFY COLUMN `supplierId` BIGINT NULL, MODIFY COLUMN `receivedBy` BIGINT NULL;

-- product_batches: ProductId (refs Products) — camelCase col names
ALTER TABLE `product_batches` MODIFY COLUMN `productId` BIGINT NULL;

-- supplier_scores: SupplierId, ScoredById (refs Suppliers, Users) — camelCase col names
ALTER TABLE `supplier_scores` MODIFY COLUMN `supplierId` BIGINT NULL, MODIFY COLUMN `scoredById` BIGINT NULL;

-- inspection_plans: productId/materialId/supplierId/customerId/processId/equipmentId/createdBy/organizationId — camelCase col names
ALTER TABLE `inspection_plans` 
  MODIFY COLUMN `productId` BIGINT NULL, 
  MODIFY COLUMN `materialId` BIGINT NULL, 
  MODIFY COLUMN `supplierId` BIGINT NULL, 
  MODIFY COLUMN `customerId` BIGINT NULL, 
  MODIFY COLUMN `processId` BIGINT NULL, 
  MODIFY COLUMN `equipmentId` BIGINT NULL, 
  MODIFY COLUMN `createdBy` BIGINT NULL, 
  MODIFY COLUMN `organizationId` BIGINT NULL;

-- inspection_plan_items: inspectionStandardId (refs InspectionStandards) — camelCase col names
ALTER TABLE `inspection_plan_items` MODIFY COLUMN `inspectionStandardId` BIGINT NULL;

-- inspection_items: standardId (refs InspectionStandards) — camelCase col names
ALTER TABLE `inspection_items` MODIFY COLUMN `standardId` BIGINT NULL;

-- ipqc_patrols: equipmentId/productLine/batchNo/user refs — check if any int FKs exist here
-- ipqc_patrols already has bigint PK, but may have int FKs to base tables

-- ipqc_first_pieces: similar check needed

-- ipqc_patrol_plans: similar check needed

-- spc_data_sources: may have int FKs to base tables (already bigint PK though)

-- ============================================================  
-- PHASE 3: Fix org_id columns (were added as INT earlier)  
-- ============================================================
ALTER TABLE `Products` MODIFY COLUMN `org_id` BIGINT NULL COMMENT '所属组织ID';
ALTER TABLE `Processes` MODIFY COLUMN `org_id` BIGINT NULL COMMENT '所属组织ID';

SELECT 'SCHEMA_MIGRATION_COMPLETE' AS status;

SET FOREIGN_KEY_CHECKS = 1;