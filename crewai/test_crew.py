"""
CrewAI 验证 — 最小可用测试
"""
from crewai import Agent, Task, Crew, Process, LLM

llm = LLM(
    model="openai/UANTEKQ3",
    base_url="http://nat.ywapi.com:9234/v1",
    api_key="ux-6CNP4MMKATVQSG1IP0EVJ3O32R65SQA4",
    temperature=0.7,
)

情报员 = Agent(
    role="情报分析师",
    goal="基于已有信息，输出结构化情报摘要",
    backstory="你是一名资深的市场情报分析师，擅长提炼关键信息。",
    llm=llm,
    verbose=True,
)

分析师 = Agent(
    role="需求分析师",
    goal="基于情报摘要，分析痛点并给出建议",
    backstory="你是一名技术型需求分析师。",
    llm=llm,
    verbose=True,
)

任务1 = Task(
    description="我是一家做 MES 系统的公司，想了解汽车零部件行业"
               "的数字化需求。请输出一份简要的情报摘要，包含："
               "1. 行业趋势\n2. 典型痛点\n3. 常见需求",
    expected_output="一份结构化的情报摘要，包含行业趋势、典型痛点、常见需求三部分。",
    agent=情报员,
)

任务2 = Task(
    description="基于情报摘要，分析我们的 MES 系统"
               "（功能涵盖生产排程、质量管理、设备管理、OEE）"
               "在汽车零部件行业的切入机会和差异化建议。",
    expected_output="一份需求分析报告，包含切入机会和差异化建议。",
    agent=分析师,
    depends_on=[任务1],
)

crew = Crew(
    agents=[情报员, 分析师],
    tasks=[任务1, 任务2],
    process=Process.sequential,
    verbose=True,
)

print("=" * 60)
print("🚀 CrewAI 验证启动")
print("   情报员 → 分析师")
print("=" * 60)

result = crew.kickoff()

print("\n" + "=" * 60)
print("✅ 完成")
print("最终产出:")
print(result.raw if hasattr(result, 'raw') else result)
print("=" * 60)