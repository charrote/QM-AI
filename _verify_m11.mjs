#!/usr/bin/env node
/**
 * Ad-hoc verification for M11 Equipment Link frontend files.
 * Validates cross-file import/export consistency WITHOUT compiling.
 */
import { readFileSync } from 'fs';

const ROOT = '/Users/Yoo/SVN/00.GITHUB/QM-AI/frontend/src';
const TYPES   = readFileSync(`${ROOT}/types/equipmentLink.ts`, 'utf8');
const API     = readFileSync(`${ROOT}/api/equipmentLink.ts`, 'utf8');
const VIEW    = readFileSync(`${ROOT}/views/EquipmentLink.vue`, 'utf8');
const BASIC   = readFileSync(`${ROOT}/types/basicData.ts`, 'utf8');
const REQ     = readFileSync(`${ROOT}/api/request.ts`, 'utf8');

let fail = 0;
function ok(cond, msg) { if (!cond) { console.log(`FAIL: ${msg}`); fail++; } else { console.log(`PASS: ${msg}`); } }

// --- 1. Types: all required exports ---
['EquipmentParamMapping','EquipmentStatusHistory','EquipmentQualityCorrelation',
 'CreateParamMapping','CreateStatusRecord'].forEach(n => {
  ok(TYPES.includes(`export interface ${n}`), `types exports ${n}`);
});
ok(TYPES.includes('export const DATA_TYPE_OPTIONS'), 'DATA_TYPE_OPTIONS constant');
ok(TYPES.includes('export const SIGNAL_OPTIONS'),    'SIGNAL_OPTIONS constant');

// --- 2. Types: camelCase ---
ok(TYPES.includes('equipmentId'),  'equipmentId (camelCase)');
ok(TYPES.includes('mqttTopic'),    'mqttTopic (camelCase)');
ok(TYPES.includes('systemParamCode'),'systemParamCode (camelCase)');

// --- 3. API: imports resolve ---
ok(API.includes("from './request'"),                'imports request from ./request');
ok(API.includes("from '@/types/basicData'"),        'imports PagedRequest/Equipment from basicData');
ok(API.includes("from '@/types/equipmentLink'"),    'imports types from equipmentLink');
ok(BASIC.includes('interface PagedRequest'),        'basicData has PagedRequest');
ok(BASIC.includes('interface PagedResult'),         'basicData has PagedResult');
ok(BASIC.includes('interface Equipment'),           'basicData has Equipment');
ok(REQ.includes('export default request'),          'request.ts default-exports request');

// --- 4. API: method signatures ---
['mappings(','mappingById(','createMapping(','removeMapping(','statuses(','correlations(','linkedEquipments('].forEach(m => {
  ok(API.includes(m), `method ${m}`);
});
ok(/\.then\(r\s*=>\s*r\.data\)/.test(API),        '.then(r => r.data) pattern');
ok(!API.includes('(r: any)'),                       'no (r:any) anti-pattern');
ok(API.includes(': Promise<PagedResult<EquipmentParamMapping>>'), 'mappings -> PagedResult<>');
ok(API.includes(': Promise<EquipmentParamMapping>'),             'single-entity returns');
ok(API.includes(': void'),                               'removeMapping -> void');
ok(API.includes(': Promise<EquipmentStatusHistory[]>'),  'statuses -> array');
ok(API.includes(': Promise<EquipmentQualityCorrelation[]>'),'correlations -> array');
ok(API.includes(': Promise<Equipment[]>'),               'linkedEquipments -> Equipment[]');

// --- 5. View: structure ---
const sc = VIEW.match(/<script setup lang="ts">(.*?)<\/script>/s)?.[0]??'';
const tm = VIEW.match(/<template>([\s\S]*?)<\/template>/s)?.[0]??'';
const st = VIEW.match(/<style[^>]*>([\s\S]*)<\/style>/s)?.[0]??'';

ok(sc.includes("defineOptions({ name: 'EquipmentLink' })"),   'name declared');
ok(sc.includes('@element-plus/icons-vue'),                    'icons import');
ok(tm.includes('<el-table '),                                  'table present');
ok(tm.includes('+新建映射'),                                    '+新建映射 button');
ok(tm.includes('<el-dialog '),                                 'dialog present');

// Column labels per spec: device name, MQTT topic, sys param, data type, unit
['MQTT Topic','系统参数','数据类型','单位'].forEach(l => { ok(tm.includes(l), `column "${l}"`); });

// Form bindings cover CreateParamMapping fields (paramGroupId is optional, omitted OK)
['equipmentId','mqttTopic','systemParamCode','dataType','.unit'].forEach(f => { ok(VIEW.includes(f), `binds ${f}`); });

// Dark theme colors in styles
['#1a2332','#0d1a2a','#1e2d3d'].forEach(c => { ok(st.includes(c), `color ${c}`); });
ok(st.includes(':deep('),                                     ':deep() for EP overrides');

// Line budget <= 80
const lc = VIEW.split('\n').length;
console.log(lc<=80 ? `PASS: view ${lc} lines (<=80)` : `WARN: view ${lc} lines (>80)`);

console.log(`\n=== RESULT: ${fail?'FAILED ('+fail+')':'ALL PASS'} ===`);
process.exit(fail||0);
