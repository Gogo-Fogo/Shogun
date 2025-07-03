# Automated Tests

This folder contains unit and integration tests for the Shogun project.

## How to Use
- Open **Window > General > Test Runner** in Unity.
- Add test scripts in this folder or in subfolders by feature (e.g., Combat, Characters).
- Use NUnit ([Test] attributes) for writing tests.

## Folder Organization
- Place general/core tests in this folder.
- For feature-specific tests, create subfolders (e.g., Combat/, Characters/).

## Example
- `Combat/CombatTests.cs` for combat system tests.
- `Characters/CharacterTests.cs` for character logic tests.

---
For more info, see Unity's [Test Framework documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/index.html). 