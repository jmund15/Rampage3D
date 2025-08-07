// In "Jmo/Core/Input/IIntentSource.cs"
using Godot;
using System.Collections.Generic;

namespace Jmo.Core.Input
{
    /// <summary>
    /// Defines a contract for a component that provides player or AI intent.
    /// Its single responsibility is to translate raw input (from a player's controller or an AI's brain)
    /// into a standardized collection of abstract InputActions and their corresponding values.
    /// </summary>
    public interface IIntentSource
    {
        /// <summary>
        /// Populates the provided dictionary with the active intents for the current frame.
        /// </summary>
        /// <param name="intentCollection">
        /// A dictionary to be filled. The key is the InputAction resource, and the value is the
        /// action's state (e.g., a bool for a button press, a Vector2 for a stick, a float for a trigger).
        /// </param>
        void QueryIntent(Dictionary<InputAction, object> intentCollection);
    }
}