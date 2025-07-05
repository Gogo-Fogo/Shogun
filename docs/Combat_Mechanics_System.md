# Combat Mechanics System Documentation

## Overview
Shogun: Flowers Fall in Blood features a multi-layered tactical combat system that combines elemental interactions, martial arts styles, stealth mechanics, counter-attacks, and skill-based mini-games. This creates a unique "Sekiro of mobile tactics" experience with 6 layers of strategic decision-making.

## 1. Elemental System (Genshin Impact Inspired)

### Element Types
- **Fire**: Aggressive, high damage, chance to inflict Burn
- **Water**: Adaptable, cleansing abilities, resistance to Fire
- **Earth**: Stability, defense boosts, reduces physical damage
- **Wind**: Speed, evasion boosts, increased movement
- **Lightning**: Fast attacks, chain damage, stun effects
- **Ice**: Control, freeze effects, slows enemies
- **Shadow**: Corruption, drains HP, causes debuffs

### Elemental Reactions
```
Fire + Water = Vaporize (1.5x damage)
Fire + Ice = Melt (2x damage)
Lightning + Water = Electro-charged (DoT damage)
Wind + Any = Swirl (AoE spread effect)
Ice + Water = Freeze (immobilize target)
Shadow + Any = Corrupt (reduces target stats)
```

### Elemental Advantage Loop
Earth > Water > Fire > Wind > Lightning > Ice > Shadow > Earth

### Combat Impact
- **Element advantage**: +25% damage
- **Element disadvantage**: -25% damage
- **Neutral matchup**: Standard damage
- **Elemental reaction**: Additional effects based on combination

## 2. Martial Arts System (Turn-Based Tactical)

### Weapon Types and Characteristics

#### Unarmed (Monk)
- **Speed**: High
- **Damage**: Low
- **Range**: Short
- **Special**: Stun effects, can't be disarmed
- **Stealth**: 70% (first turn), 50% (second turn)
- **Counter**: "Counter-Strike" - 30% chance, 150% damage

#### Sword (Samurai)
- **Speed**: Medium
- **Damage**: Medium
- **Range**: Short
- **Special**: Can parry, good vs armored
- **Stealth**: 40% (first turn), 20% (second turn)
- **Counter**: "Parry" - 50% chance, basic attack

#### Spear (Polearm)
- **Speed**: Medium
- **Damage**: Medium-High
- **Range**: Long
- **Special**: Can attack over allies, good vs cavalry
- **Stealth**: 30% (first turn), 10% (second turn)
- **Counter**: "Riposte" - 40% chance, extends range

#### Bow (Archer)
- **Speed**: Medium
- **Damage**: Medium
- **Range**: Very Long
- **Special**: Can't be countered at range
- **Stealth**: 60% (first turn), 40% (second turn)
- **Counter**: "Quick Shot" - 25% chance, ranged counter

#### Staff (Onmyoji)
- **Speed**: Low
- **Damage**: Medium (magical)
- **Range**: Medium
- **Special**: Magic focus, elemental boost, healing
- **Stealth**: 30% (first turn), 10% (second turn)
- **Counter**: "Magic Shield" - 35% chance, magical counter

#### Dual Daggers (Ninja)
- **Speed**: Very High
- **Damage**: Low-Medium
- **Range**: Short
- **Special**: Stealth, can attack twice
- **Stealth**: 90% (first turn), 70% (second turn)
- **Counter**: "Shadow Dodge" - 60% dodge, 15% counter

#### Heavy Weapons (Demon)
- **Speed**: Low
- **Damage**: Very High
- **Range**: Short
- **Special**: Breaks armor, slow but powerful
- **Stealth**: 10% (first turn), 0% (second turn)
- **Counter**: "Brute Force" - 20% chance, 200% damage

### Martial Arts Matchups
```
Sword vs Heavy: Sword +25% damage (precision vs armor)
Spear vs Bow: Spear +25% damage (reach advantage)
Unarmed vs Dagger: Unarmed +25% damage (technique vs speed)
Staff vs Sword: Staff +25% damage (magic vs physical)
Heavy vs Spear: Heavy +25% damage (power vs reach)
Bow vs Staff: Bow +25% damage (physical vs magical)
```

## 3. Stealth and Positioning System

### Bush Mechanics
Characters can hide in bushes to gain tactical advantages:

#### Stealth Effectiveness (Diminishing Returns)
```
First Turn in Bush:
- Ninja: 90% stealth
- Monk: 70% stealth
- Archer: 60% stealth
- Samurai: 40% stealth
- Onmyoji: 30% stealth
- Demon: 10% stealth

Second Turn in Same Bush:
- Ninja: 70% stealth (-20%)
- Monk: 50% stealth (-20%)
- Archer: 40% stealth (-20%)
- Samurai: 20% stealth (-20%)
- Onmyoji: 10% stealth (-20%)
- Demon: 0% stealth (-10%)

Third+ Turn: Additional -10% each turn
```

#### Stealth Benefits
- **Hidden Attack**: +50% damage if undetected
- **First Strike**: Hidden characters act first in turn order
- **Escape**: Can attempt to flee without being pursued
- **Ambush**: Can set up team attacks from stealth

#### Detection Mechanics
- **Enemy Movement**: Enemies passing by bushes have detection chance
- **Sound**: Moving or attacking reduces stealth effectiveness
- **Line of Sight**: Breaking cover immediately reveals character
- **Team Coordination**: Multiple hidden characters can coordinate attacks

## 4. Counter-Attack System

### Counter Mechanics
When attacked, characters have a chance to counter based on their martial arts style:

#### Counter Types
- **Parry**: Block attack and counter with basic attack
- **Dodge**: Avoid damage entirely
- **Counter-Strike**: Take damage but deal enhanced counter damage
- **Riposte**: Counter with extended range
- **Magic Shield**: Magical counter with elemental effects
- **Shadow Dodge**: High dodge chance with low counter chance
- **Brute Force**: Low chance but massive counter damage

#### Counter Triggers
- **Basic Attacks**: Standard counter chance
- **Special Attacks**: Reduced counter chance (-20%)
- **Ultimate Attacks**: Minimal counter chance (-50%)
- **Elemental Attacks**: Counter chance modified by elemental advantage

## 5. Mini-Game Combat System

### Offensive Mini-Games

#### Sword: "Precision Slash"
- **Mechanic**: Tap in rhythm for damage multiplier
- **Perfect**: 200% damage
- **Good**: 150% damage
- **Poor**: 100% damage
- **Miss**: 50% damage

#### Spear: "Thrust Timing"
- **Mechanic**: Hold and release at right moment
- **Perfect**: 180% damage
- **Good**: 140% damage
- **Poor**: 100% damage
- **Miss**: 60% damage

#### Bow: "Aim and Shoot"
- **Mechanic**: Drag to aim, release to fire
- **Perfect**: 200% damage, ignores cover
- **Good**: 150% damage
- **Poor**: 100% damage
- **Miss**: 0% damage

#### Staff: "Spell Casting"
- **Mechanic**: Trace magical symbols
- **Perfect**: 180% damage, elemental boost
- **Good**: 140% damage
- **Poor**: 100% damage
- **Miss**: 70% damage

#### Unarmed: "Combo Chain"
- **Mechanic**: Rapid taps in sequence
- **Perfect**: 160% damage, stun chance
- **Good**: 130% damage
- **Poor**: 100% damage
- **Miss**: 80% damage

#### Daggers: "Stealth Strike"
- **Mechanic**: Silent tap timing
- **Perfect**: 250% damage (if hidden), 150% (if visible)
- **Good**: 180% damage (if hidden), 120% (if visible)
- **Poor**: 100% damage
- **Miss**: 50% damage

#### Heavy: "Power Charge"
- **Mechanic**: Hold to charge, release for massive damage
- **Perfect**: 300% damage
- **Good**: 200% damage
- **Poor**: 150% damage
- **Miss**: 100% damage

### Defensive Mini-Games

#### Sword: "Parry Window"
- **Mechanic**: Tap at exact moment to block
- **Perfect**: 0% damage + counter-attack
- **Good**: 50% damage reduction
- **Poor**: 25% damage reduction
- **Miss**: Full damage

#### Monk: "Dodge Roll"
- **Mechanic**: Swipe to dodge in direction
- **Perfect**: 0% damage
- **Good**: 75% damage reduction
- **Poor**: 50% damage reduction
- **Miss**: Full damage

#### Ninja: "Shadow Step"
- **Mechanic**: Quick double-tap to vanish
- **Perfect**: 0% damage + reposition
- **Good**: 80% damage reduction
- **Poor**: 40% damage reduction
- **Miss**: Full damage

#### Spear: "Defensive Stance"
- **Mechanic**: Hold to reduce damage
- **Perfect**: 70% damage reduction
- **Good**: 50% damage reduction
- **Poor**: 30% damage reduction
- **Miss**: Full damage

#### Staff: "Magic Shield"
- **Mechanic**: Draw protective runes
- **Perfect**: 80% damage reduction + elemental resistance
- **Good**: 60% damage reduction
- **Poor**: 40% damage reduction
- **Miss**: Full damage

#### Heavy: "Armor Block"
- **Mechanic**: Simple tap but reduces damage less
- **Perfect**: 60% damage reduction
- **Good**: 40% damage reduction
- **Poor**: 20% damage reduction
- **Miss**: Full damage

## 6. System Integration Examples

### Scenario 1: Ninja Ambush
1. **Positioning**: Ninja hides in bush (90% stealth)
2. **Detection**: Enemy walks by, fails to detect
3. **Attack**: Ninja uses "Stealth Strike" mini-game
4. **Execution**: Perfect execution = 250% damage + stun
5. **Result**: Poor execution = 100% damage, enemy gets turn

### Scenario 2: Samurai vs Demon
1. **Attack**: Demon uses ultimate attack
2. **Defense**: Samurai gets "Parry Window" mini-game
3. **Execution**: Perfect parry = 0% damage + counter-attack
4. **Result**: Good parry = 50% damage reduction

### Scenario 3: Elemental + Martial Arts Combo
1. **Setup**: Fire Samurai vs Ice Demon
2. **Attack**: Samurai uses "Precision Slash" mini-game
3. **Execution**: Perfect execution + elemental advantage = 300% damage
4. **Result**: Poor execution + elemental disadvantage = 75% damage

## 7. Strategic Depth Layers

The system creates 6 layers of decision-making:

1. **Positioning**: Bush, cover, range, line of sight
2. **Elemental Matchups**: Fire vs Ice, reactions, team composition
3. **Martial Arts Matchups**: Sword vs Spear, weapon advantages
4. **Stealth Mechanics**: Hide vs engage, diminishing returns
5. **Counter-Attack Timing**: When to defend vs attack
6. **Mini-Game Skill**: Execution quality affects all outcomes

## 8. Balance Considerations

### Risk vs Reward
- **High-risk strategies** (stealth, perfect timing) offer high rewards
- **Safe strategies** (direct combat) offer consistent but lower rewards
- **Elemental reactions** require team coordination but offer massive benefits

### Skill Expression
- **Mini-games** create skill ceiling beyond pure strategy
- **Timing windows** reward practice and mastery
- **Multiple execution levels** accommodate different skill levels

### Accessibility
- **Simple inputs** (tap, swipe, hold) work on mobile
- **Visual feedback** clearly shows execution quality
- **Fallback options** ensure poor execution still has some effect

This system creates the "punishing, rewarding, beautiful, and obsessively replayable" experience that defines the game's vision. 