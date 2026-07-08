#!/usr/bin/env python3
"""QM-AI S7 E2E Test - all 27 endpoints"""
import json,urllib.request,urllib.error,uuid,datetime,sys

BU="http://localhost:5611/api/v1"

def ap(m,p,b=None):
    u="%s%s"%(BU,p)
    h={"Accept":"application/json"}
    if hasattr(ap,"tk"):h["Authorization"]="Bearer %s"%ap.tk
    if b:h["Content-Type"]="application/json"
    d=json.dumps(b).encode() if isinstance(b,dict) else None
    r=urllib.request.Request(u,data=d,headers=h,method=m)
    try:
        rs=urllib.request.urlopen(r,timeout=10)
        ra=rs.read().decode("utf-8",errors="replace")[:800]
        return rs.getcode(),json.loads(ra) if ra.strip() else {}
    except urllib.error.HTTPError as e:
        ra=e.read().decode("utf-8",errors="replace")[:800] if e.headers else "{}"
        try:return e.code,json.loads(ra)
        except:return e.code,{"err":ra[:200]}

P=F=0;ER=[]

def ck(l,c,x=(200,201)):
    global P,F
    if c in x:P+=1;print("  OK [%d] %s"%(c,l))
    else:F+=1;ER.append(l);print("  FAIL [%d] %s (want %s)"%(c,l,str(x)))

print("="*65)
print("QM-AI SPRINT 7 E2E INTEGRATION TEST")
print("="*65)

print("\n[Phase 0] Authentication")
co,da=ap("POST","/auth/login",{"username":"admin","password":"admin123"})
if co!=200 or not da.get("token"):print("ABORT:[%d]"%co);sys.exit(1)
ap.tk=da["token"];print("  OK Login (%d chars)"%len(da["token"]))

uu=uuid.uuid4().hex[:8]
ns=datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")

def md(n,st="reported"):
    return{"productName":n,"productId":None,"batchId":None,"equipmentId":None,"responsibleUserId":"1","severityLevel":"minor","status":st,"description":"E2E-"+n+"-"+uu,"reportedAt":ns,"sourceType":"iqc"}

# === DEFECTS (6 ep) ===
print("\n[Phase 1] DEFECTS (6 endpoints)")
ck("GET /defects",*ap("GET","/defects"))
ck("GET /defects/count",*ap("GET","/defects/count"))
co,dd=ap("POST","/defects",md("D-"+uu))
ck("POST /defects (create)",co)
di=dd.get("id") if isinstance(dd,dict) else None

if di:
    ck("/defects/%d"%di,*ap("GET","/defects/"+str(di)))
    co,_=ap("PUT","/defects/"+str(di),md("DU-"+uu,"closed"))
    ck("PUT /defects/%d"%di,co)
    cv,vd=ap("GET","/defects/"+str(di))
    if vd.get("status")=="closed":P+=1;print("  OK [%d] VERIFY update"%cv)
    else:F+=1;print("  FAIL [%d] VERIFY (got %s)"%(cv,vd.get("status")))
    dk,_=ap("DELETE","/defects/"+str(di))
    ck("DELETE /defects/%d"%di,dk,x=(204,))
else:print("  SKIP CRUD - no ID")

# === CAPA (13 ep) ===
print("\n[Phase 2] CAPA (13 endpoints)")
ck("GET /defects/capa",*ap("GET","/defects/capa"))

# Create defect for CAPA linkage (since we deleted the defects one above)
_,ldd=ap("POST","/defects",md("LK-"+uu))
lid=ldd.get("id") if isinstance(ldd,dict) else None

co,cd=ap("POST","/defects/capa",{
    "triggerDefectId":lid,"triggerSource":"manual","title":"CAPA-"+uu,
    "description":"E2E CAPA "+uu,"createdBy":"1","createdAt":ns})
ck("POST /defects/capa (create)",co)

ci=cd.get("id") if isinstance(cd,dict) else None

if ci:
    ck("/capa/%d"%ci,*ap("GET","/defects/capa/"+str(ci)))

    # Phase transitions (try phases 2 and 3)
    cp,_=ap("PUT","/defects/capa/"+str(ci)+"/phase",{"phase":2})
    ck("PUT capa phase->2",cp)

    cp3,_=ap("PUT","/defects/capa/"+str(ci)+"/phase",{"phase":3})
    ck("PUT capa phase->3",cp3)

    # Root causes (works at phase>=2)
    crc,_=ap("POST","/defects/capa/"+str(ci)+"/root-causes",{
        "causeDescription":"RC-"+uu,"analysisMethod":"5whys"})
    ck("POST root-causes capa/%d"%ci,crc)

    cg_rc,_=ap("GET","/defects/capa/"+str(ci)+"/root-causes")
    ck("GET root-causes capa/%d"%ci,cg_rc)

    # Corrective action (works at phase>=3)
    cac,_=ap("POST","/defects/capa/"+str(ci)+"/corrective-actions",{
        "actionDescription":"CA-"+uu,"assignedTo":"1",
        "dueDate":ns,"estimatedCost":0})
    ck("POST corrective-actions capa/%d"%ci,cac)

    # Preventive action (works at phase>=3)
    cpa,_=ap("POST","/defects/capa/"+str(ci)+"/preventive-actions",{
        "actionDescription":"PA-"+uu,"assignedTo":"1",
        "dueDate":ns,"estimatedCost":0})
    ck("POST preventive-actions capa/%d"%ci,cpa)

    # Verification (works at phase>=4)
    cp4,_=ap("PUT","/defects/capa/"+str(ci)+"/phase",{"phase":4})
    ck("PUT capa phase->4",cp4)

    cvf,_=ap("POST","/defects/capa/"+str(ci)+"/verifications",{
        "verificationDescription":"VF-"+uu,"verifiedBy":"1",
        "verifiedAt":ns,"effectivenessRating":"effective"})
    ck("POST verifications capa/%d"%ci,cvf)

    # Cleanup - delete capa last
    cdela,_=ap("DELETE","/defects/capa/"+str(ci))
    ck("DELETE /defects/capa/%d"%ci,cdela,x=(204,))

else:print("  SKIP CAPA chain - no ID")

# === SCRAP/REWORK (3 ep) ===
print("\n[Phase 3] Scrap/Rework (3 endpoints)")

# Need a defect for scrap/rework linkage
_,srd=ap("POST","/defects",md("SR-"+uu))
sid=srd.get("id") if isinstance(srd,dict) else None

ck("GET /defects/scrap-rework",*ap("GET","/defects/scrap-rework"))

if sid:
    csr,_=ap("POST","/defects/scrap-rework",{
        "defectId":sid,"recordType":"scrap",
        "quantity":5,"reason":"Material defect SR-"+uu,
        "handledBy":"1","processedAt":ns})
    ck("POST /defects/scrap-rework (create)",csr)

    sr_id=None;srr={}
    if csr in(200,201):sr_id=srr.get and None,None # placeholder

    # Try rework record too
    crw,_=ap("POST","/defects/scrap-rework",{
        "defectId":sid,"recordType":"rework",
        "quantity":3,"reason":"Cosmetic rework RW-"+uu,
        "handledBy":"1","processedAt":ns})
    ck("POST /defects/scrap-rework (rework)",crw)

else:print("  SKIP scrap-rework - no defect ID")

# === TRACE (5 ep) ===
print("\n[Phase 4] Trace (5 endpoints)")

ck("GET /trace/by-sn/E2E-SN-%s"%uu,*ap("GET","/trace/by-sn/E2E-SN-"+uu))
ck("GET /trace/by-batch/BATCH-%s"%uu,*ap("GET","/trace/by-batch/BATCH-"+uu))

# Equipment trace needs a real equipment ID from DB - try 1 first
cte,_=ap("GET","/trace/by-equipment/1")
ck("GET /trace/by-equipment/1",cte)

ck("GET /trace/ng-diffusion/BATCH-%s"%uu,*ap("GET","/trace/ng-diffusion/BATCH-"+uu))
ck("GET /trace/recall-simulation/BATCH-%s"%uu,*ap("GET","/trace/recall-simulation/BATCH-"+uu))

# === SUMMARY ===
print("\n"+("="*65))
print("RESULTS: %d PASSED, %d FAILED out of %d total"%(P,P+F,P+F))
if ER:print("\nFailed endpoints:")
for e in ER:print("  - %s"%e)
print("="*65)
