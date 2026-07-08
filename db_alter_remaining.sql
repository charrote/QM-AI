-- Upgrade remaining base table PKs from INT to BIGINT
ALTER TABLE `Customers` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `DefectCodes` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Equipment` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `InspectionStandards` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Permissions` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Processes` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Products` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Roles` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Routings` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Suppliers` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Tools` MODIFY `Id` BIGINT AUTO_INCREMENT;
ALTER TABLE `Users` MODIFY `Id` BIGINT AUTO_INCREMENT;

-- Upgrade FK columns referencing base tables (now bigint)
ALTER TABLE `Boms` MODIFY `ProductId` BIGINT NULL, MODIFY `RoutingId` BIGINT NULL;
ALTER TABLE `Routings` MODIFY `ProcessId` BIGINT NULL, MODIFY `EquipmentId` BIGINT NULL;

-- inspection_plans FKs (camelCase columns)
ALTER TABLE `inspection_plans` 
  MODIFY `productId` BIGINT NULL, 
  MODIFY `materialId` BIGINT NULL, 
  MODIFY `supplierId` BIGINT NULL, 
  MODIFY `customerId` BIGINT NULL, 
  MODIFY `processId` BIGINT NULL, 
  MODIFY `equipmentId` BIGINT NULL,
  MODIFY `createdBy` BIGINT NULL, 
  MODIFY `organizationId` BIGINT NULL;

-- inspection_plan_items FK to InspectionStandard (now bigint)
ALTER TABLE `inspection_plan_items` MODIFY `inspectionStandardId` BIGINT NULL;

-- inspection_items FK to InspectionStandard (now bigint)  
ALTER TABLE `inspection_items` MODIFY `standardId` BIGINT NULL;

-- iqc_receipts FKs (camelCase columns)
ALTER TABLE `iqc_receipts` 
  MODIFY `productId` BIGINT NULL, 
  MODIFY `supplierId` BIGINT NULL, 
  MODIFY `receivedBy` BIGINT NULL;

-- iqc_inspections inspector FK (camelCase column)
ALTER TABLE `iqc_inspections` MODIFY `inspectorId` BIGINT NULL;

-- supplier_scores FKs (camelCase columns)
ALTER TABLE `supplier_scores` 
  MODIFY `supplierId` BIGINT NULL, 
  MODIFY `scoredById` BIGINT NULL;

-- org_id columns: upgrade from INT to BIGINT to match Organizations PK type
ALTER TABLE `Products` MODIFY `org_id` BIGINT NULL COMMENT '所属组织ID';
ALTER TABLE `Processes` MODIFY `org_id` BIGINT NULL COMMENT '所属组织ID';

SELECT 'SCHEMA_UPGRADE_COMPLETE' AS status;