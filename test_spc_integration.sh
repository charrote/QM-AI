#!/bin/bash
# SPC Sprint 6 端到端联调测试脚本
# 默认用户: admin / admin123

BASE="http://localhost:5000"
TOKEN=""

echo "============================================"
echo "QM-AI SPC Sprint 6 端到端联调测试"
echo "============================================"

# 0. 健康检查
echo ""
echo "[0/8] 健康检查"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" $BASE/api/v1/health)
echo "Health: HTTP $HTTP_CODE"

# 1. 登录获取 Token
echo ""
echo "[1/8] 登录获取 JWT Token"
LOGIN_RESP=$(curl -s -X POST $BASE/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}')
TOKEN=$(echo $LOGIN_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('accessToken',''))")
if [ -z "$TOKEN" ]; then
  echo "  ❌ 登录失败: $LOGIN_RESP"
  exit 1
fi
echo "  ✅ Token 获取成功 (${TOKEN:0:30}...)"

# 2. 查看基础数据（产品/工序）
echo ""
echo "[2/8] 查看基础数据（产品列表）"
PROD_RESP=$(curl -s -H "Authorization: Bearer $TOKEN" $BASE/api/v1/products)
PROD_COUNT=$(echo $PROD_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(len(d.get('items',[])))")
echo "  产品数量: $PROD_COUNT"
if [ "$PROD_COUNT" -gt 0 ]; then
  FIRST_PROD=$(echo $PROD_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d['items'][0]['id'])")
  FIRST_PROD_ID=$FIRST_PROD
  echo "  ✅ 第一个产品ID: $FIRST_PROD_ID"
fi

# 3. 查看工序列表
echo ""
echo "[3/8] 查看工序列表"
PROC_RESP=$(curl -s -H "Authorization: Bearer $TOKEN" $BASE/api/v1/processes)
PROC_COUNT=$(echo $PROC_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(len(d.get('items',[])))")
echo "  工序数量: $PROC_COUNT"
if [ "$PROC_COUNT" -gt 0 ]; then
  FIRST_PROC=$(echo $PROC_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d['items'][0]['id'])")
  FIRST_PROC_ID=$FIRST_PROC
  echo "  ✅ 第一个工序ID: $FIRST_PROC_ID"
fi

# 4. 创建SPC控制图
echo ""
echo "[4/8] 创建 SPC 控制图 (Xbar-R, n=5)"
CHART_RESP=$(curl -s -X POST $BASE/api/v1/spc/control-charts \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"name\":\"SPC联调测试-精加工XbarR\",\"processId\":${FIRST_PROC_ID:-1},\"parameterCode\":\"DIM_A\",\"chartType\":\"Xbar_R\",\"subgroupSize\":5,\"usl\":10.05,\"lsl\":9.95,\"targetValue\":10.0}")
CHART_ID=$(echo $CHART_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('id',''))")
echo "  响应: $(echo $CHART_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('name',''))")"
echo "  控制图ID: $CHART_ID"

# 5. 批量导入数据点
echo ""
echo "[5/8] 批量导入数据点 (25个子组, 每组5个)"
# 生成测试数据
DATA="{"
DATA+="\"chartId\":${CHART_ID},"
DATA+="\"dataPoints\":["
for i in $(seq 1 25); do
  if [ $i -gt 1 ]; then DATA+=","; fi
  DATA+="{\"subgroupIndex\":$i,\"individualValues\":[$((1000+i)),$((1000+i+1)),$((1000+i-1)),$((1000+i+2)),$((1000+i-2))],"
  DATA+="\"measuredAt\":\"2026-07-07T10:0${i}:00Z\"}"
done
DATA+="]}"
DATA_RESP=$(curl -s -X POST $BASE/api/v1/spc/data-points/batch \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "$DATA")
DATA_COUNT=$(echo $DATA_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('count',''))")
echo "  导入数据点: $DATA_COUNT 条"

# 6. 运行SPC分析
echo ""
echo "[6/8] 运行 SPC 分析"
ANALYZE_RESP=$(curl -s -X POST $BASE/api/v1/spc/analyze \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"chartId\":${CHART_ID}}")
# 提取关键指标
CPK=$(echo $ANALYZE_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('cpk','N/A'))")
Cp=$(echo $ANALYZE_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('cp','N/A'))")
PPM=$(echo $ANALYZE_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('estimatedPpm','N/A'))")
VIOLATIONS=$(echo $ANALYZE_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(len(d.get('violations',[])))")
UCL_X=$(echo $ANALYZE_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('controlLimits',{}).get('uclXbar','N/A'))")
LCL_X=$(echo $ANALYZE_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('controlLimits',{}).get('lclXbar','N/A'))")
CL_X=$(echo $ANALYZE_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(d.get('controlLimits',{}).get('clXbar','N/A'))")
echo "  Cp:  $Cp"
echo "  Cpk: $CPK"
echo "  PPM: $PPM"
echo "  X̄ CL:  $CL_X"
echo "  X̄ UCL: $UCL_X"
echo "  X̄ LCL: $LCL_X"
echo "  判异触发: $VIOLATIONS 条"

# 7. 查看判异规则
echo ""
echo "[7/8] 查看判异规则配置"
RULES_RESP=$(curl -s -H "Authorization: Bearer $TOKEN" $BASE/api/v1/spc/alert-rules?chartId=${CHART_ID})
RULE_COUNT=$(echo $RULES_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(len(d))")
echo "  规则数量: $RULE_COUNT"
echo $RULES_RESP | python3 -c "
import sys,json
rules=json.loads(sys.stdin.read())
for r in rules:
  status='✅' if r.get('enabled') else '⏸️'
  print(f\"  {status} Rule #{r.get('ruleNumber')}: {r.get('ruleName')}\")
"

# 8. 查看报警触发记录
echo ""
echo "[8/8] 查看报警触发记录"
TRIGGERS_RESP=$(curl -s -H "Authorization: Bearer $TOKEN" $BASE/api/v1/spc/triggers?chartId=${CHART_ID})
TRIGGER_COUNT=$(echo $TRIGGERS_RESP | python3 -c "import sys,json; d=json.loads(sys.stdin.read()); print(len(d))")
echo "  触发记录数: $TRIGGER_COUNT"
echo $TRIGGERS_RESP | python3 -c "
import sys,json
triggers=json.loads(sys.stdin.read())
for t in triggers:
  resolved='✅已解决' if t.get('resolved') else '⚠️未解决'
  print(f\"  Rule#{t.get('ruleNumber')} @ point#{t.get('violatedPointIndex')} {resolved}\")
" if [ "$TRIGGER_COUNT" -gt 0 ]; then echo ""; fi

echo ""
echo "============================================"
echo "SPC Sprint 6 联调完成 ✅"
echo "============================================"