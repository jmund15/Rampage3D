# Project: 3D Rampage - AI & Systems Charter (Version 2.0)

## Core Vision
To create a 3D arcade-style monster "Rampage" game. The core loop involves players controlling unique monsters to destroy a city, fight off military resistance, and cause mayhem. The design should feel dynamic and chaotic, but the underlying systems should be robust, modular, and maintainable, with an emphasis on creating an immersive AI ecosystem.

## Key Gameplay & AI Pillars
1.  **Living City:** The city must feel alive *before* the player's attack. Civilians and vehicles should have believable "peace-time" routines governed by clear, high-level states.
2.  **Localized Chaos:** AI should react intelligently to danger primarily within their own perception radius. The chaos should spread organically as AIs react to each other's panic, creating a cascading effect rather than relying on a global "alert" state.
3.  **Dynamic AI Reactions & Hierarchical State Logic:** AI must not be one-dimensional. They should be able to assess their situation and switch high-level goals (via the State Machine), which in turn activates the appropriate detailed logic (via the Behavior Tree). Examples include switching from `Idling` to `Fleeing` or from `Patrolling` to `EngagingThreat`.
4.  **Escalating Threat:** The military response is a core gameplay element. It should arrive in waves, deploy forces intelligently, and use vehicles and tactics appropriate to the threat level.
5.  **System-Driven Emergence:** The ultimate goal is to create systems that allow for unscripted, interesting interactions. Examples: a civilian stealing an abandoned tank to escape, a police car swerving to avoid a fleeing pedestrian and crashing, a monster throwing a car that triggers a chain reaction of explosions.

## Architectural Goals
-   **Modularity & Single Responsibility:** Core systems (AI, Health, Vehicles, Steering) should be designed as reusable components that each do *one thing* well. This facilitates testing, expansion, and reuse.
-   **Data-Driven Design:** Extensively utilize Godot's `Resource` system for AI behaviors (`BTCondition`, `UtilityConsideration`), entity properties (`VelocityIDResource`, `DriverBehavior`), and configurations (`VehicleSeat`). This empowers design and iteration directly within the Godot editor.
-   **Performance:** Systems must be designed and profiled to handle a city populated with dozens of AI agents simultaneously, avoiding per-frame bottlenecks.
-   **DRY (Don't Repeat Yourself):** Avoid boilerplate and duplicated code by using base classes, helper functions, and well-designed abstractions where appropriate.

---

## Progress & Refinement Tracker

### Tier 1: Foundational Architecture Refactoring
*This tier focuses on high-impact changes that improve the core structure, making future development easier and more robust.*

-   [ ] **Refactor Core Navigation & Steering Subsystem (High Priority):**
    -   [ ] **Simplify `AINav3DComponent`:** Reduce its role to a pure pathfinding "GPS". It should only be responsible for holding a target, providing the next path point, and reporting if the target has been reached.
    -   [ ] **Create new `AISteeringComponent`:** This new component will be the "driver". Its responsibility is to take the ideal direction from the `AINav3DComponent` and blend it with the context map (danger, interest vectors) to produce a final `DesiredVelocity`.
    -   [ ] **Consolidate Sensors:** Create a dedicated `AISensors` node on the agent to hold `AIRayDetector3D` and `AIAreaDetector3D`. The `AISteeringComponent` will query this node to build its context map, removing sensory logic from the navigation component.

-   [ ] **Implement `UtilitySelector` CompositeTask:**
    -   **Goal:** Enable true utility-based decision-making.
    -   **Action:** Create a new composite node that evaluates the utility score of all its `UtilityAction` children (via their `Consideration` resource) and executes only the one with the highest score. This is crucial for making the AI feel less scripted and more intelligent.

### Tier 2: Code Quality & Best Practices
*This tier addresses specific issues in the current implementation to improve robustness and reduce code duplication.*

-   [ ] **Fix `FindAnyVehicle` Race Condition:**
    -   **Problem:** Two AIs can find and path to the same vehicle seat simultaneously.
    -   **Action:** Implement an atomic `bool TryReserveSeat(VehicleSeat seat, OccupantComponent3D occupant)` method in `VehicleOccupantsComponent`. The `FindAnyVehicle` action must call this method and only succeed if it returns `true`.

-   [ ] **Refactor `NavigationServer3D.MapChanged` boilerplate:**
    -   **Problem:** Multiple BT actions contain identical logic to wait for the navigation map to load.
    -   **Action:** Create an abstract base class, `NavReadyBehaviorAction`, that handles the `MapChanged` signal subscription and provides an abstract `OnNavMapReady()` method for children to implement. Refactor `SetRandomNavPoint`, `FindEnterableBuilding`, etc., to inherit from this new class.

-   [ ] **Correct `FindEnterableBuilding` Logic:**
    -   **Problem:** The current implementation iterates and sets paths, resulting in pathing to the *last* valid building, not the *closest*.
    -   **Action:** Refactor the logic to first iterate through all buildings to find the one with the minimum distance, *then* set a single navigation path to that closest building's entrance.

-   [ ] **Refactor `VehicleToNavPoint` Inheritance:**
    -   **Problem:** The class inherits from `SetRandomNavPoint` but its purpose is monitoring, not setting. This is a violation of the Liskov Substitution Principle.
    -   **Action:** Change its base class to `BehaviorAction` and rename it to something more descriptive, like `MonitorVehicleTravel` or `FailIfVehicleStuck`, to clarify its sole purpose.

### Tier 3: AI Behavior Implementation
*This tier focuses on building out the specific behaviors described in the vision using the newly refactored architecture.*

-   [ ] **Peace-Time Civilian AI Loop (`Peaceful` BTState):**
    -   [x] `FindAnyVehicle` action successfully finds the closest, most appropriate vehicle seat.
    -   [x] `TravelToNavPoint` action correctly paths the AI agent to the vehicle entrance.
    -   [x] `EmbarkInVehicle` action successfully parents the agent, hides it, and updates the vehicle's state.
    -   [ ] `FindVehiclePOI` or `DriveToRandomPosition` successfully sets a destination for the vehicle's `AINavComponent`.
    -   [ ] `MonitorVehicleTravel` action correctly tracks the vehicle's progress and fails if the vehicle becomes stuck for a set duration.
    -   [ ] `DisembarkFromVehicle` action successfully restores the agent to the world.
    -   [ ] `WaitIdle` action correctly pauses AI for a duration.

-   [ ] **Fleeing/Danger AI Loop (`Fleeing` BTState):**
    -   [ ] Create "Fleeing" `BTState` and associated Behavior Tree.
    -   [ ] Use a `UtilitySelector` to decide the best way to flee:
        -   `UtilityAction`: Find the nearest car.
        -   `UtilityAction`: Run away on foot to a designated "exit area".
        -   `UtilityAction`: Cower or hide behind scenery.
    -   [ ] Ensure AI driving a vehicle in flee mode will target a city "exit point" instead of a random POI.

-   [ ] **Military AI Loop (`Combat` BTState):**
    -   [ ] Implement spawning logic for military vehicles arriving at city entrances.
    -   [ ] AI can successfully disembark from military vehicles.
    -   [ ] AI can identify the player/monster as a primary threat.
    -   [ ] AI can use weapons to engage the player from a safe distance.
    -   [ ] Tank AI can fire projectiles.
    -   [ ] Helicopter AI can maintain a circling pattern while firing.

### Tier 4: Future Expansion & System Ideas
*Long-term goals and ideas for deepening the simulation once the core loops are stable.*

-   [ ] **Advanced Perception System:**
    -   **Idea:** Move beyond simple radius detection. Create a `PerceptionComponent` that simulates sight (view cones, line-of-sight checks) and sound (emitting and hearing "noise" events like explosions or screams). This would allow for stealth mechanics and more realistic AI reactions.

-   [ ] **Dynamic AI Attributes:**
    -   **Idea:** Create systems to modify AI `Resource`s like `DriverBehavior` at runtime. An AI that narrowly escapes a monster attack could have its `DriverAggression` temporarily plummet, causing it to drive erratically or slowly. An AI that sees other AIs successfully escape in cars might have its utility for "find car" increase.

-   [ ] **Squad-Based AI:**
    -   **Idea:** Introduce a "Squad" node that has its own Blackboard and Behavior Tree. This Squad BT would observe the state of its members (e.g., police officers) and issue high-level commands (e.g., "Flank Target," "Provide Suppressing Fire") by setting variables on its members' Blackboards.

-   [ ] **AI Knowledge Base & Memory:**
    -   **Idea:** Give AIs a simple memory system. A military unit could "remember" the player's last known position and investigate that area even if it loses line of sight. A civilian could "remember" that a certain street is blocked by rubble and avoid pathing through it in the future.

-   [ ] **Editor & Workflow Improvements:**
    -   **Goal:** Make level design and AI setup faster and less error-prone.
    -   **Action:** Investigate creating a custom **Godot Editor Plugin** to provide a dedicated UI for the `BuildingComponent`. This would replace the complex and brittle `_GetPropertyList` logic, making it easier to design and texture new buildings without editing code.