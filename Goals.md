# Project: 3D Rampage - AI & Systems Charter

## Core Vision
To create a 3D arcade-style monster "Rampage" game. The core loop involves players controlling unique monsters to destroy a city, fight off military resistance, and cause mayhem. The design should feel dynamic and chaotic, but the underlying systems should be robust, modular, and maintainable, with an emphasis on creating an immersive AI ecosystem.

## Key Gameplay & AI Pillars
1.  **Living City:** The city should feel alive *before* the player's attack. Civilians and vehicles should have believable "peace-time" routines.
2.  **Localized Chaos:** AI should react to danger within their immediate vicinity, not based on a global "alert" state. The chaos should spread organically from the source of destruction.
3.  **Dynamic AI Reactions:** AI should not be one-dimensional. They should be able to assess their situation and switch behaviors dynamically (e.g., flee on foot, find a car, hide, or fight back).
4.  **Escalating Threat:** The military response should be a core part of the gameplay loop, arriving in waves and deploying forces intelligently.
5.  **Emergent Possibilities:** The systems should allow for unscripted, interesting interactions, such as a civilian stealing an abandoned tank to escape.

## Architectural Goals
- **Modularity:** Core systems (AI, health, vehicles) should be designed as reusable components and interfaces to facilitate easy expansion and potential use in future projects.
- **Data-Driven Design:** Utilize Godot's `Resource` system heavily for AI behaviors (Conditions, Considerations) and entity properties (Velocity, DriverBehavior) to empower design through the editor.
- **Performance:** Keep systems performant enough to handle a city populated with dozens of AI agents simultaneously.
- **Maintainability:** Avoid overly complex, monolithic classes. Prioritize clear separation of responsibilities.

---

## Progress & Refinement Tracker

### High-Priority Architectural Refinements
- [ ] **Refactor `AINav3DComponent`:** Separate its responsibilities.
    - [ ] Keep `AINav3DComponent` for pathfinding only.
    - [ ] `AIRayDetector3D` / `AIAreaDetector3D` remain pure sensors.
    - [ ] Create a new `AISteeringComponent` to blend pathfollowing and context-based avoidance.
- [ ] **Implement `UtilitySelector`:** Create a new `CompositeTask` for the Behavior Tree that evaluates `UtilityAction` children and runs the highest-scoring one.
- [ ] **Simplify `BuildingComponent`:** Investigate creating a Godot Editor Plugin to handle the building generation UI, replacing the complex `_GetPropertyList` override. This will improve long-term maintainability.

### AI Behavior Implementation
- [ ] **Peace-Time AI Loop:**
    - [ ] Implement BT for walking to/from target points (e.g., parks).
    - [ ] Implement BT for finding and entering a building.
    - [ ] Implement BT for finding a vehicle and driving to a destination.
- [ ] **Fleeing AI Behavior:**
    - [ ] Create "Flee" `BTState` and associated Behavior Tree.
    - [ ] Implement `UtilityActions` for different flee options (find car, run on foot, cower).
    - [ ] AI successfully drives out of the city when fleeing in a car.
- [ ] **Military AI:**
    - [ ] AI can arrive in ground/air vehicles.
    - [ ] AI can disembark from vehicles.
    - [ ] AI can engage the player (monster) with attacks.
- [ ] **Dynamic Switching:**
    - [ ] Test scenario: AI car gets stuck or destroyed.
    - [ ] AI correctly transitions from "Driving" BT to "Fleeing on Foot" BT.

### Functional & Bug-Fixing
- [ ] **Test Vehicle Reverse Logic:** Thoroughly test the reverse behavior in `GroundVehicleComponent` to eliminate bugs and ensure it feels responsive.
- [ ] **Harden `LocatorComponent3D`:** Add more robust null-checking to the LINQ query to prevent crashes from unexpected object types.
- [ ] **Profile `AINav3DComponent`:** Once many AI are active, profile the `GetWeightedPathPosition` method. If it's a bottleneck, implement updates on a timer instead of every frame.