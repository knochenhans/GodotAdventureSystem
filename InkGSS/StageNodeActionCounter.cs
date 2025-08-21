using Godot.Collections;

public class StageNodeActionCounter
{
	Dictionary<string, Dictionary<string, int>> stageNodeActionCounter = [];

	public void IncrementActionCounter(string stageNodeID, string actionID)
	{
		if (!stageNodeActionCounter.ContainsKey(stageNodeID))
			stageNodeActionCounter[stageNodeID] = [];

		if (!stageNodeActionCounter[stageNodeID].ContainsKey(actionID))
			stageNodeActionCounter[stageNodeID][actionID] = 0;

		stageNodeActionCounter[stageNodeID][actionID]++;
	}

	public int GetActionCounter(string stageNodeID, string actionID)
	{
		if (!stageNodeActionCounter.TryGetValue(stageNodeID, out Dictionary<string, int> actionDict))
			return 0;

		if (!actionDict.TryGetValue(actionID, out int count))
			return 0;

		return count;
	}

	public Dictionary<string, int> GetActionCounters(string stageNodeID)
	{
		if (!stageNodeActionCounter.TryGetValue(stageNodeID, out Dictionary<string, int> value))
			return [];

		return value;
	}

	public void SetActionCounters(string stageNodeID, Dictionary<string, int> actionCounters)
	{
		if (!stageNodeActionCounter.ContainsKey(stageNodeID))
			stageNodeActionCounter[stageNodeID] = [];

		foreach (var actionCounter in actionCounters)
			stageNodeActionCounter[stageNodeID][actionCounter.Key] = actionCounter.Value;
	}
}