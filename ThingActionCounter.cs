using Godot.Collections;

public class ThingActionCounter
{
	Dictionary<string, Dictionary<string, int>> thing_action_counter = new();

	public void IncrementActionCounter(string thingID, string actionID)
	{
		if (!thing_action_counter.ContainsKey(thingID))
			thing_action_counter[thingID] = new();

		if (!thing_action_counter[thingID].ContainsKey(actionID))
			thing_action_counter[thingID][actionID] = 0;

		thing_action_counter[thingID][actionID]++;
	}

	public int GetActionCounter(string thingID, string actionID)
	{
		if (!thing_action_counter.ContainsKey(thingID))
			return 0;

		if (!thing_action_counter[thingID].ContainsKey(actionID))
			return 0;

		return thing_action_counter[thingID][actionID];
	}

	public Dictionary<string, int> GetActionCounters(string thingID)
	{
		if (!thing_action_counter.ContainsKey(thingID))
			return new();

		return thing_action_counter[thingID];
	}

	public void SetActionCounters(string thingID, Dictionary<string, int> actionCounters)
	{
		if (!thing_action_counter.ContainsKey(thingID))
			thing_action_counter[thingID] = new();

		foreach (var actionCounter in actionCounters)
			thing_action_counter[thingID][actionCounter.Key] = actionCounter.Value;
	}
}