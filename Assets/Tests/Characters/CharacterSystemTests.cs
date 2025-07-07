using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Shogun.Features.Characters;

namespace Shogun.Tests.Characters
{
    /// <summary>
    /// Tests for the character system functionality.
    /// </summary>
    public class CharacterSystemTests
    {
        private CharacterDefinition testDefinition;
        
        [SetUp]
        public void SetUp()
        {
            // Create a test character definition
            testDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            
            // Set up test data using reflection since properties are read-only
            var nameField = typeof(CharacterDefinition).GetField("characterName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField.SetValue(testDefinition, "Test Samurai");
            
            var healthField = typeof(CharacterDefinition).GetField("baseHealth", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            healthField.SetValue(testDefinition, 100f);
            
            var attackField = typeof(CharacterDefinition).GetField("baseAttack", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            attackField.SetValue(testDefinition, 50f);
            
            var defenseField = typeof(CharacterDefinition).GetField("baseDefense", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            defenseField.SetValue(testDefinition, 30f);
            
            var speedField = typeof(CharacterDefinition).GetField("baseSpeed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            speedField.SetValue(testDefinition, 10f);
            
            // Set elemental and martial arts types
            var elementalField = typeof(CharacterDefinition).GetField("elementalType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            elementalField.SetValue(testDefinition, ElementalType.Fire);
            
            var martialArtsField = typeof(CharacterDefinition).GetField("martialArtsType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            martialArtsField.SetValue(testDefinition, MartialArtsType.Sword);
        }
        
        [Test]
        public void CharacterDefinition_Creation_IsValid()
        {
            // Assert
            Assert.IsNotNull(testDefinition);
            Assert.AreEqual("Test Samurai", testDefinition.CharacterName);
            Assert.AreEqual(100f, testDefinition.BaseHealth);
            Assert.AreEqual(50f, testDefinition.BaseAttack);
            Assert.AreEqual(30f, testDefinition.BaseDefense);
            Assert.AreEqual(10f, testDefinition.BaseSpeed);
            Assert.AreEqual(ElementalType.Fire, testDefinition.ElementalType);
            Assert.AreEqual(MartialArtsType.Sword, testDefinition.MartialArtsType);
        }
        
        [Test]
        public void CharacterInstance_Creation_IsValid()
        {
            // Act
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(testDefinition, instance.Definition);
            Assert.IsNotNull(instance.Stats);
            Assert.AreEqual(100f, instance.CurrentHealth);
            Assert.AreEqual(100f, instance.MaxHealth);
            Assert.IsTrue(instance.IsAlive);
            Assert.IsFalse(instance.IsHidden);
            Assert.IsFalse(instance.IsInBush);
        }
        
        [Test]
        public void CharacterStats_Initialization_IsCorrect()
        {
            // Act
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Assert
            Assert.AreEqual(100f, instance.Stats.Health);
            Assert.AreEqual(50f, instance.Stats.Attack);
            Assert.AreEqual(30f, instance.Stats.Defense);
            Assert.AreEqual(10f, instance.Stats.Speed);
            Assert.AreEqual(1, instance.Stats.Level);
        }
        
        [Test]
        public void CharacterInstance_TakeDamage_ReducesHealth()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            float initialHealth = instance.CurrentHealth;
            
            // Act
            instance.TakeDamage(25f);
            
            // Assert
            Assert.AreEqual(initialHealth - 25f, instance.CurrentHealth);
            Assert.IsTrue(instance.IsAlive);
        }
        
        [Test]
        public void CharacterInstance_TakeDamage_HandlesDeath()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            bool deathEventRaised = false;
            instance.OnDeath += () => deathEventRaised = true;
            
            // Act
            instance.TakeDamage(150f); // More than max health
            
            // Assert
            Assert.AreEqual(0f, instance.CurrentHealth);
            Assert.IsFalse(instance.IsAlive);
            Assert.IsTrue(deathEventRaised);
        }
        
        [Test]
        public void CharacterInstance_Heal_RestoresHealth()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            instance.TakeDamage(50f);
            float healthAfterDamage = instance.CurrentHealth;
            
            // Act
            instance.Heal(25f);
            
            // Assert
            Assert.AreEqual(healthAfterDamage + 25f, instance.CurrentHealth);
            Assert.IsTrue(instance.IsAlive);
        }
        
        [Test]
        public void CharacterInstance_Heal_DoesNotExceedMaxHealth()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            instance.TakeDamage(25f);
            
            // Act
            instance.Heal(50f); // More than needed
            
            // Assert
            Assert.AreEqual(100f, instance.CurrentHealth);
            Assert.IsTrue(instance.IsAlive);
        }
        
        [Test]
        public void CharacterStats_LevelUp_IncreasesStats()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            float initialHealth = instance.Stats.Health;
            float initialAttack = instance.Stats.Attack;
            
            // Act
            instance.Stats.AddExperience(100f); // Enough to level up
            
            // Assert
            Assert.AreEqual(2, instance.Stats.Level);
            Assert.Greater(instance.Stats.Health, initialHealth);
            Assert.Greater(instance.Stats.Attack, initialAttack);
        }
        
        [Test]
        public void CharacterStats_ElementalEffectiveness_CalculatesCorrectly()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act & Assert
            // Fire > Wind (advantage)
            float fireVsWind = instance.Stats.GetElementalEffectiveness(ElementalType.Fire, ElementalType.Wind);
            Assert.AreEqual(1.25f, fireVsWind);
            
            // Fire < Water (disadvantage)
            float fireVsWater = instance.Stats.GetElementalEffectiveness(ElementalType.Fire, ElementalType.Water);
            Assert.AreEqual(0.75f, fireVsWater);
            
            // Same type (neutral)
            float fireVsFire = instance.Stats.GetElementalEffectiveness(ElementalType.Fire, ElementalType.Fire);
            Assert.AreEqual(1.0f, fireVsFire);
        }
        
        [Test]
        public void CharacterStats_MartialArtsEffectiveness_CalculatesCorrectly()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act & Assert
            // Sword > Heavy (advantage)
            float swordVsHeavy = instance.Stats.GetMartialArtsEffectiveness(MartialArtsType.Sword, MartialArtsType.HeavyWeapons);
            Assert.AreEqual(1.25f, swordVsHeavy);
            
            // Sword < Staff (disadvantage)
            float swordVsStaff = instance.Stats.GetMartialArtsEffectiveness(MartialArtsType.Sword, MartialArtsType.Staff);
            Assert.AreEqual(0.75f, swordVsStaff);
            
            // Same type (neutral)
            float swordVsSword = instance.Stats.GetMartialArtsEffectiveness(MartialArtsType.Sword, MartialArtsType.Sword);
            Assert.AreEqual(1.0f, swordVsSword);
        }
        
        [Test]
        public void CharacterStats_ElementalReactions_WorkCorrectly()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act & Assert
            // Fire + Water = Vaporize
            float vaporize = instance.Stats.GetElementalReactionMultiplier(ElementalType.Fire, ElementalType.Water);
            Assert.AreEqual(1.5f, vaporize);
            
            // Fire + Ice = Melt
            float melt = instance.Stats.GetElementalReactionMultiplier(ElementalType.Fire, ElementalType.Ice);
            Assert.AreEqual(2.0f, melt);
            
            // Lightning + Water = Electro-charged
            float electroCharged = instance.Stats.GetElementalReactionMultiplier(ElementalType.Lightning, ElementalType.Water);
            Assert.AreEqual(1.3f, electroCharged);
            
            // No reaction
            float noReaction = instance.Stats.GetElementalReactionMultiplier(ElementalType.Fire, ElementalType.Earth);
            Assert.AreEqual(1.0f, noReaction);
        }
        
        [Test]
        public void CharacterInstance_AttackRange_IsCorrect()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act & Assert
            float shortRange = instance.Stats.GetAttackRangeRadius(AttackRange.Short);
            float midRange = instance.Stats.GetAttackRangeRadius(AttackRange.Mid);
            float longRange = instance.Stats.GetAttackRangeRadius(AttackRange.Long);
            
            Assert.AreEqual(1.5f, shortRange);
            Assert.AreEqual(3f, midRange);
            Assert.AreEqual(5f, longRange);
        }
        
        [Test]
        public void CharacterInstance_CanAttackTarget_WorksCorrectly()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            instance.MoveTo(new Vector2Int(0, 0));
            
            // Act & Assert
            // Target within range
            bool canAttackNear = instance.CanAttackTarget(new Vector2Int(2, 0));
            Assert.IsTrue(canAttackNear);
            
            // Target out of range
            bool canAttackFar = instance.CanAttackTarget(new Vector2Int(10, 0));
            Assert.IsFalse(canAttackFar);
        }
        
        [Test]
        public void CharacterInstance_TurnManagement_WorksCorrectly()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act
            instance.MoveTo(new Vector2Int(1, 1));
            instance.PerformBasicAttack();
            
            // Assert
            Assert.IsTrue(instance.HasMovedThisTurn);
            Assert.IsTrue(instance.HasAttackedThisTurn);
            Assert.IsFalse(instance.CanMove);
            Assert.IsFalse(instance.CanAttack);
            
            // Act - Start new turn
            instance.StartNewTurn();
            
            // Assert
            Assert.IsFalse(instance.HasMovedThisTurn);
            Assert.IsFalse(instance.HasAttackedThisTurn);
            Assert.IsTrue(instance.CanMove);
            Assert.IsTrue(instance.CanAttack);
        }
        
        [Test]
        public void CharacterInstance_StealthMechanics_WorkCorrectly()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act - Enter bush
            instance.EnterBush(new Vector2Int(1, 1));
            
            // Assert
            Assert.IsTrue(instance.IsInBush);
            Assert.AreEqual(1, instance.TurnsInBush);
            // Note: Stealth success is random, so we can't assert on IsHidden
            
            // Act - Second turn in bush
            instance.StartNewTurn();
            instance.EnterBush(new Vector2Int(1, 1)); // Stay in same bush
            
            // Assert
            Assert.AreEqual(2, instance.TurnsInBush);
            
            // Act - Leave bush
            instance.LeaveBush();
            
            // Assert
            Assert.IsFalse(instance.IsInBush);
            Assert.AreEqual(0, instance.TurnsInBush);
            Assert.IsFalse(instance.IsHidden);
        }
        
        [Test]
        public void CharacterStats_StealthEffectiveness_DiminishesOverTime()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act & Assert
            float firstTurn = instance.Stats.GetStealthEffectiveness(MartialArtsType.Sword, 1);
            float secondTurn = instance.Stats.GetStealthEffectiveness(MartialArtsType.Sword, 2);
            float thirdTurn = instance.Stats.GetStealthEffectiveness(MartialArtsType.Sword, 3);
            
            Assert.AreEqual(40f, firstTurn); // Sword base stealth
            Assert.AreEqual(20f, secondTurn); // -20%
            Assert.AreEqual(10f, thirdTurn); // Additional -10%
        }
        
        [Test]
        public void CharacterStats_CounterChances_AreCorrect()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act & Assert
            float swordCounter = instance.Stats.GetCounterChance(MartialArtsType.Sword);
            float ninjaCounter = instance.Stats.GetCounterChance(MartialArtsType.DualDaggers);
            float heavyCounter = instance.Stats.GetCounterChance(MartialArtsType.HeavyWeapons);
            
            Assert.AreEqual(50f, swordCounter);
            Assert.AreEqual(15f, ninjaCounter); // Low counter, high dodge
            Assert.AreEqual(20f, heavyCounter);
        }
        
        [Test]
        public void CharacterStats_CounterDamageMultipliers_AreCorrect()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            
            // Act & Assert
            float swordMultiplier = instance.Stats.GetCounterDamageMultiplier(MartialArtsType.Sword);
            float unarmedMultiplier = instance.Stats.GetCounterDamageMultiplier(MartialArtsType.Unarmed);
            float heavyMultiplier = instance.Stats.GetCounterDamageMultiplier(MartialArtsType.HeavyWeapons);
            
            Assert.AreEqual(100f, swordMultiplier); // Normal damage
            Assert.AreEqual(150f, unarmedMultiplier); // High counter damage
            Assert.AreEqual(200f, heavyMultiplier); // Very high counter damage
        }
        
        [Test]
        public void CharacterInstance_CounterAttack_WorksCorrectly()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            bool counterEventRaised = false;
            float counterDamage = 0f;
            instance.OnCounterAttack += (damage) => { counterEventRaised = true; counterDamage = damage; };
            
            // Act
            instance.TakeDamage(50f);
            bool counterSuccess = instance.AttemptCounterAttack();
            
            // Assert
            // Note: Counter success is random, so we can't assert on the result
            // But we can verify the event system works
            if (counterSuccess)
            {
                Assert.IsTrue(counterEventRaised);
                Assert.Greater(counterDamage, 0f);
            }
        }
        
        [Test]
        public void CharacterInstance_StealthBreaksOnAction()
        {
            // Arrange
            CharacterInstance instance = new CharacterInstance(testDefinition);
            instance.EnterBush(new Vector2Int(1, 1));
            
            // Simulate successful stealth (we can't control the random, so we'll set it manually)
            var hiddenField = typeof(CharacterInstance).GetField("isHidden", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hiddenField.SetValue(instance, true);
            
            // Act - Move (should break stealth)
            instance.MoveTo(new Vector2Int(2, 2));
            
            // Assert
            Assert.IsFalse(instance.IsHidden);
        }
        
        [Test]
        public void CharacterFactory_CreateCharacter_WorksCorrectly()
        {
            // Act
            CharacterInstance instance = CharacterFactory.CreateCharacter(testDefinition);
            
            // Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(testDefinition, instance.Definition);
        }
        
        [Test]
        public void CharacterFactory_CreateCharacterWithLevel_WorksCorrectly()
        {
            // Act
            CharacterInstance instance = CharacterFactory.CreateCharacter(testDefinition);
            instance.Stats.AddExperience(CharacterFactory.CalculateExperienceForLevel(5));
            
            // Assert
            Assert.IsNotNull(instance);
            Assert.AreEqual(5, instance.Stats.Level);
        }
        
        [Test]
        public void CharacterInstance_DamageCalculation_IncludesAllModifiers()
        {
            // Arrange
            CharacterInstance attacker = new CharacterInstance(testDefinition); // Fire Sword
            CharacterDefinition defenderDef = ScriptableObject.CreateInstance<CharacterDefinition>();
            
            // Set up defender as Water Heavy (disadvantage for attacker)
            var nameField = typeof(CharacterDefinition).GetField("characterName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField.SetValue(defenderDef, "Test Defender");
            
            var elementalField = typeof(CharacterDefinition).GetField("elementalType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            elementalField.SetValue(defenderDef, ElementalType.Water);
            
            var martialArtsField = typeof(CharacterDefinition).GetField("martialArtsType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            martialArtsField.SetValue(defenderDef, MartialArtsType.HeavyWeapons);
            
            CharacterInstance defender = new CharacterInstance(defenderDef);
            
            // Act
            float damage = attacker.CalculateDamageAgainst(defender);
            
            // Assert
            // Should be reduced due to Fire vs Water disadvantage (0.75x)
            // But increased due to Sword vs Heavy advantage (1.25x)
            // Net effect: 0.75 * 1.25 = 0.9375x
            float expectedDamage = attacker.Stats.Attack * 0.75f * 1.25f;
            Assert.AreEqual(expectedDamage, damage, 0.1f);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testDefinition != null)
            {
                Object.DestroyImmediate(testDefinition);
            }
        }
    }
} 