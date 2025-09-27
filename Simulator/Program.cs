using System;
using System.Linq;
using System.Collections.Generic;

// 시뮬레이션에 사용될 데이터 구조체 (클래스)들
public class PlayerStats
{
    public float baseAttack;
    public float criticalChance;
    public float criticalDamageBonus;
    public float defenseIgnoreRate;
    public float attackSpeed;
}

public class MonsterStats
{
    public float maxHealth;
    public float currentHealth;
    public float defenseRate;
}

public class SkillStats
{
    public float damageRatio;
    public int hitCount;
    public float delay;
    public float cooldown;
}

// DamageCalculator 클래스는 별도 파일에 있다고 가정합니다.

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- 몬스터 전투 시뮬레이터 (사용자 입력) ---");
        Console.WriteLine();
        
        bool showDetails = ReadYesNo("전투 상세 내역을 보시겠습니까? (Y/N): ");
        int simulationCount = ReadInt("시뮬레이션을 몇 번 반복하시겠습니까? (예: 1000): ");
        
        // 새로 추가된 부분: 분당 공격 불가 시간 입력
        float downtimePerMinute = ReadFloat("보스 패턴으로 인한 분당 공격 불가 시간(초)을 입력하세요: ");
        Console.WriteLine();

        // 1. 플레이어 스탯 입력
        var player = new PlayerStats
        {
            baseAttack = ReadFloat("플레이어의 기본 공격력을 입력하세요: "),
            criticalChance = ReadFloat("플레이어의 치명타 확률을 입력하세요 (예: 0.3): "),
            criticalDamageBonus = ReadFloat("플레이어의 치명타 추가 데미지를 입력하세요 (예: 0.5): "),
            defenseIgnoreRate = ReadFloat("플레이어의 방어력 무시율을 입력하세요 (예: 0.2): "),
            attackSpeed = ReadFloat("플레이어의 공격 속도를 입력하세요 (1.0 = 100%, 1.2 = 120%): ")
        };
        
        // 2. 몬스터 스탯 입력
        var monster = new MonsterStats
        {
            maxHealth = ReadFloat("몬스터의 최대 체력을 입력하세요: "),
            currentHealth = 0,
            defenseRate = ReadFloat("몬스터의 방어율을 입력하세요 (예: 50): ")
        };
        monster.currentHealth = monster.maxHealth;

        // 3. 스킬 스탯 입력
        var normalAttack = new SkillStats
        {
            damageRatio = ReadFloat("평타의 데미지 비율을 입력하세요 (예: 1.0): "),
            hitCount = ReadInt("평타의 타수를 입력하세요 (예: 1): "),
            delay = ReadFloat("평타의 기본 딜레이를 입력하세요 (예: 0.3): "),
            cooldown = 0.0f
        };
        
        Console.WriteLine();
        var skillAttack = new SkillStats
        {
            damageRatio = ReadFloat("스킬의 데미지 비율을 입력하세요 (예: 2.5): "),
            hitCount = ReadInt("스킬의 타수를 입력하세요 (예: 3): "),
            delay = ReadFloat("스킬의 기본 딜레이를 입력하세요 (예: 0.5): "),
            cooldown = ReadFloat("스킬의 쿨타임을 입력하세요 (예: 3.0): ")
        };

        // 새로 추가된 부분: 입력 값 요약
        Console.WriteLine("\n\n---------------- 입력 데이터 요약 ----------------");
        Console.WriteLine("플레이어 스탯");
        Console.WriteLine($"공격력: {player.baseAttack:F2} | 치명타율: {player.criticalChance * 100}% | 치명타 추가 데미지: {player.criticalDamageBonus * 100}% | 방어력 무시율: {player.defenseIgnoreRate * 100}% | 공격속도: {player.attackSpeed:F2}");
        Console.WriteLine("\n몬스터 스탯");
        Console.WriteLine($"체력: {monster.maxHealth:F2} | 방어율: {monster.defenseRate}%");
        Console.WriteLine($"보스 패턴으로 인한 분당 공격 불가 시간: {downtimePerMinute:F2}초");
        Console.WriteLine("\n스킬 스탯");
        Console.WriteLine($"평타 - 데미지 비율: {normalAttack.damageRatio:F2} | 타수: {normalAttack.hitCount} | 딜레이: {normalAttack.delay:F2}초");
        Console.WriteLine($"스킬 - 데미지 비율: {skillAttack.damageRatio:F2} | 타수: {skillAttack.hitCount} | 딜레이: {skillAttack.delay:F2}초 | 쿨타임: {skillAttack.cooldown:F2}초");
        Console.WriteLine("-------------------------------------------------");

        // 4. 평타 시뮬레이션 실행 및 결과 출력
        var normalAttackResults = RunMultipleSimulations(simulationCount, player, monster, normalAttack, showDetails);
        PrintSimulationResults(normalAttackResults, monster.maxHealth, "평타", downtimePerMinute);

        // 5. 스킬 공격 시뮬레이션 실행 및 결과 출력
        var skillAttackResults = RunMultipleSimulations(simulationCount, player, monster, skillAttack, showDetails);
        PrintSimulationResults(skillAttackResults, monster.maxHealth, "스킬", downtimePerMinute);

        // 6. 스킬 순환 시뮬레이션 실행 및 결과 출력
        var rotationResults = RunMultipleRotationSimulations(simulationCount, player, monster, normalAttack, skillAttack, showDetails);
        PrintSimulationResults(rotationResults, monster.maxHealth, "스킬 순환", downtimePerMinute);
    }

    /// <summary>
    /// 단일 공격 방식 시뮬레이션을 여러 번 반복하여 TTK 리스트를 반환합니다.
    /// </summary>
    static List<float> RunMultipleSimulations(int count, PlayerStats player, MonsterStats monster, SkillStats skill, bool showDetails)
    {
        var ttks = new List<float>();
        for (int i = 0; i < count; i++)
        {
            ttks.Add(SimulateCombat(player, monster, skill, "시뮬레이션", showDetails));
        }
        return ttks;
    }

    /// <summary>
    /// 스킬 순환 시뮬레이션을 여러 번 반복하여 TTK 리스트를 반환합니다.
    /// </summary>
    static List<float> RunMultipleRotationSimulations(int count, PlayerStats player, MonsterStats monster, SkillStats normal, SkillStats skill, bool showDetails)
    {
        var ttks = new List<float>();
        for (int i = 0; i < count; i++)
        {
            ttks.Add(SimulateRotation(player, monster, normal, skill, showDetails));
        }
        return ttks;
    }

    /// <summary>
    /// 단일 몬스터와의 전투를 시뮬레이션하고 TTK를 반환합니다.
    /// </summary>
    static float SimulateCombat(PlayerStats player, MonsterStats monster, SkillStats skill, string attackType, bool showDetails)
    {
        monster.currentHealth = monster.maxHealth;
        float totalTime = 0f;
        int attackCount = 0;
        float finalDamage = 0f;
        
        while (monster.currentHealth > 0)
        {
            float damage = DamageCalculator.CalculateOutgoingDamage(
                player.baseAttack * skill.damageRatio, 
                player.criticalChance,
                player.criticalDamageBonus,
                monster.defenseRate,
                player.defenseIgnoreRate
            );

            if (damage < 0.001f && finalDamage > monster.maxHealth * 0.99f) break;

            monster.currentHealth -= damage;
            finalDamage += damage;
            attackCount++;
            
            if (showDetails)
            {
                Console.WriteLine($"[{attackType}] 공격 {attackCount}회: {damage:F2} 데미지 (남은 체력: {(int)monster.currentHealth})");
            }

            totalTime += skill.delay / player.attackSpeed;
        }
        return totalTime;
    }

    /// <summary>
    /// 스킬 순환 전투를 시뮬레이션하고 TTK를 반환합니다.
    /// </summary>
    static float SimulateRotation(PlayerStats player, MonsterStats monster, SkillStats normal, SkillStats skill, bool showDetails)
    {
        monster.currentHealth = monster.maxHealth;
        float totalTime = 0f;
        int normalAttackCount = 0;
        int skillCasts = 0;
        float nextSkillAvailableTime = 0f;
        float finalDamage = 0f;

        while(monster.currentHealth > 0)
        {
            if (totalTime >= nextSkillAvailableTime)
            {
                float totalSkillDamage = 0;
                for (int i = 0; i < skill.hitCount; i++)
                {
                    float damage = DamageCalculator.CalculateOutgoingDamage(
                        player.baseAttack * skill.damageRatio,
                        player.criticalChance,
                        player.criticalDamageBonus,
                        monster.defenseRate,
                        player.defenseIgnoreRate
                    );
                    monster.currentHealth -= damage;
                    totalSkillDamage += damage;
                    if (monster.currentHealth <= 0) break;
                }
                finalDamage += totalSkillDamage;
                if (finalDamage < 0.001f && finalDamage > monster.maxHealth * 0.99f) break;

                if (showDetails)
                {
                    Console.WriteLine($"[스킬] 공격 {skillCasts + 1}회: {totalSkillDamage:F2} 데미지 (남은 체력: {(int)monster.currentHealth})");
                }
                
                totalTime += skill.delay / player.attackSpeed;
                skillCasts++;
                nextSkillAvailableTime = totalTime + skill.cooldown;
            }
            else
            {
                float damage = DamageCalculator.CalculateOutgoingDamage(
                    player.baseAttack * normal.damageRatio,
                    player.criticalChance,
                    player.criticalDamageBonus,
                    monster.defenseRate,
                    player.defenseIgnoreRate
                );
                
                monster.currentHealth -= damage;
                finalDamage += damage;
                normalAttackCount++;
                if (finalDamage < 0.001f && finalDamage > monster.maxHealth * 0.99f) break;

                if (showDetails)
                {
                    Console.WriteLine($"[평타] 공격 {normalAttackCount}회: {damage:F2} 데미지 (남은 체력: {(int)monster.currentHealth})");
                }
                totalTime += normal.delay / player.attackSpeed;
            }
        }
        return totalTime;
    }
    
    /// <summary>
    /// 시뮬레이션 결과(TTK 리스트)를 분석하여 출력합니다.
    /// </summary>
    static void PrintSimulationResults(List<float> ttks, float monsterHealth, string attackType, float downtimePerMinute)
    {
        float averageTtk = ttks.Average();
        float averageDps = monsterHealth / averageTtk;
        float minTtk = ttks.Min();
        float maxTtk = ttks.Max();

        Console.WriteLine($"\n## [{attackType}] 시뮬레이션 결과 ##");
        Console.WriteLine($"총 시뮬레이션 횟수: {ttks.Count}회");
        Console.WriteLine($"평균 몬스터 처치 시간 (TTK): {averageTtk:F2}초");
        Console.WriteLine($"최소 처치 시간: {minTtk:F2}초");
        Console.WriteLine($"최대 처치 시간: {maxTtk:F2}초");
        Console.WriteLine($"평균 DPS: {averageDps:F2}");
        Console.WriteLine("----------------------------------");
        PrintTtkGraph(ttks);
        
        // 새로 추가된 부분: 다운타임 반영 결과
        float totalDowntime = (averageTtk / 60.0f) * downtimePerMinute;
        float finalTtkWithDowntime = averageTtk + totalDowntime;
        float finalDpsWithDowntime = monsterHealth / finalTtkWithDowntime;

        Console.WriteLine($"\n** 보스 패턴을 고려한 최종 결과 (분당 {downtimePerMinute:F2}초 다운타임 적용) **");
        Console.WriteLine($"최종 평균 처치 시간: {finalTtkWithDowntime:F2}초");
        Console.WriteLine($"최종 평균 DPS: {finalDpsWithDowntime:F2}");
        Console.WriteLine("----------------------------------");
    }

    /// <summary>
    /// TTK 분포를 텍스트 그래프로 시각화합니다.
    /// </summary>
    static void PrintTtkGraph(List<float> ttks)
    {
        Console.WriteLine("TTK 분포 그래프:");
        const int barLength = 50;
        int bucketCount = 10;
        
        float minTtk = ttks.Min();
        float maxTtk = ttks.Max();
        float range = maxTtk - minTtk;
        float bucketSize = range / bucketCount;

        if (range <= 0)
        {
            Console.WriteLine("모든 TTK 값이 동일합니다.");
            return;
        }

        int[] buckets = new int[bucketCount];
        foreach (float ttk in ttks)
        {
            int bucketIndex = (int)((ttk - minTtk) / bucketSize);
            if (bucketIndex >= bucketCount) bucketIndex = bucketCount - 1;
            buckets[bucketIndex]++;
        }

        int maxBucketCount = buckets.Max();

        for (int i = 0; i < bucketCount; i++)
        {
            float bucketMin = minTtk + i * bucketSize;
            float bucketMax = bucketMin + bucketSize;
            float percentage = (float)buckets[i] / ttks.Count;
            int bar = (int)(percentage * barLength);
            
            Console.Write($"{bucketMin:F2}s - {bucketMax:F2}s | ");
            Console.Write(new string('#', bar));
            Console.WriteLine($" ({percentage:P1})");
        }
    }
    
    static float ReadFloat(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (float.TryParse(input, out float result))
            {
                return result;
            }
            Console.WriteLine("잘못된 입력입니다. 숫자를 입력해주세요.");
        }
    }
    
    static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine();
            if (int.TryParse(input, out int result))
            {
                return result;
            }
            Console.WriteLine("잘못된 입력입니다. 정수를 입력해주세요.");
        }
    }
    
    static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()?.ToUpper();
            if (input == "Y") return true;
            if (input == "N") return false;
            Console.WriteLine("잘못된 입력입니다. Y 또는 N을 입력해주세요.");
        }
    }
}