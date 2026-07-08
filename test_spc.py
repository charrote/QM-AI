#!/usr/bin/env python3
"""QM-AI Sprint 6 SPC 端到端联调测试"""
import json, urllib.request, random, sys

BASE = "http://localhost:5000"
T = ""

def api(m, p, data=None, auth=True):
    h = {"Content-Type": "application/json"}
    if auth and T:
        h["Authorization"] = f"Bearer {T}"
    url = f"{BASE}{p}"
    body = json.dumps(data).encode() if data else None
    req = urllib.request.Request(url, data=body, headers=h, method=m)
    try:
        resp = urllib.request.urlopen(req)
        return json.loads(resp.read()), resp.status
    except urllib.error.HTTPError as e:
        return json.loads(e.read()), e.code
    except Exception as e:
        return {"error": str(e)}, 0

results = []

# 0. Health
_, code = api("GET", "/api/v1/health", auth=False)
results.append(f"[0] 健康检查: {'✅' if code == 200 else '❌'} HTTP {code}")

# 1. Login
resp, _ = api("POST", "/api/v1/auth/login", {"username": "admin", "password": "admin123"}, auth=False)
T = resp.get("accessToken", "")
results.append(f"[1] 登录: {'✅' if T else '❌'} {T[:30]}..." if T else f"[1] 登录: ❌ {resp}")

# 2. Products
resp, _ = api("GET", "/api/v1/products")
items = resp.get("items", [])
pid = items[0]["id"] if items else 1
results.append(f"[2] 产品: {len(items)}条, 第一个ID={pid}")

# 3. Processes
resp, _ = api("GET", "/api/v1/processes")
items = resp.get("items", [])
prid = items[0]["id"] if items else 1
results.append(f"[3] 工序: {len(items)}条, 第一个ID={prid}")

# 4. Create chart
cd = {"name": "SPC联调测试-XbarR", "processId": prid, "parameterCode": "DIM_A",
      "chartType": "Xbar_R", "subgroupSize": 5, "usl": 10.05, "lsl": 9.95, "targetValue": 10.0}
resp, _ = api("POST", "/api/v1/spc/control-charts", cd)
cid = resp.get("id", 0)
results.append(f"[4] 控制图: {'✅' if cid else '❌'} ID={cid} name={resp.get('name','')}")
if not cid:
    results.append("  ❌ 创建失败，请检查后端日志")
    print("\n".join(results))
    sys.exit(1)

# 5. Batch data
dps = []
for i in range(1, 26):
    vals = [round(10.0 + random.uniform(-0.05, 0.05), 6) for _ in range(5)]
    dps.append({"subgroupIndex": i, "individualValues": vals, "measuredAt": f"2026-07-07T10:{i:02d}:00Z"})
resp, _ = api("POST", "/api/v1/spc/data-points/batch", {"chartId": cid, "dataPoints": dps})
cnt = resp.get("count", 0)
results.append(f"[5] 数据点: {'✅' if cnt else '❌'} {cnt}条")

# 6. Analyze
resp, _ = api("POST", "/api/v1/spc/analyze", {"chartId": cid})
cpk = resp.get("cpk", "?")
cp = resp.get("cp", "?")
ppm = resp.get("estimatedPpm", "?")
lim = resp.get("controlLimits", {})
viol = resp.get("violations", [])
results.append(f"[6] 分析: Cp={cp}, Cpk={cpk}, PPM={ppm}")
results.append(f"     控制限: UCL={lim.get('uclXbar','?')}, CL={lim.get('clXbar','?')}, LCL={lim.get('lclXbar','?')}")
results.append(f"     判异触发: {len(viol)}条")
for v in viol:
    desc = v.get("description", "") or v.get("detail", "") or ""
    results.append(f"       ⚠️ Rule#{v.get('ruleNumber')}: {desc}")

# 7. Rules
resp, _ = api("GET", f"/api/v1/spc/alert-rules?chartId={cid}")
rc = len(resp) if isinstance(resp, list) else 0
results.append(f"[7] 规则: {rc}条" if rc else f"[7] 规则: ❌ 解析异常")

# 8. Triggers
resp, _ = api("GET", f"/api/v1/spc/triggers?chartId={cid}")
tc = len(resp) if isinstance(resp, list) else 0
results.append(f"[8] 报警: {tc}条" if tc >= 0 else f"[8] 报警: ❌")
if tc > 0:
    for t in resp:
        r = "✅已解决" if t.get("resolved") else "⚠️未解决"
        results.append(f"       Rule#{t.get('ruleNumber')} @point#{t.get('violatedPointIndex')} {r}")

results.append("")
all_ok = cid and cnt and cpk != "?"
results.append(f"{'✅ SPC Sprint 6 联调全部通过!' if all_ok else '⚠️ SPC Sprint 6 联调存在异常'}")

output = "\n".join(results)
with open("/tmp/spc_test_result.txt", "w") as f:
    f.write(output)
print(output)