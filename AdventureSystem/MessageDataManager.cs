using Godot.Collections;
using Godot;

public static class MessageDataManager
{
    // public static Dictionary<string, Dictionary<string, Array<string>>> ThingMessages { get; set; } = new();

    // // Contains states for things after they have been interacted with 
    // public static Dictionary<string, Dictionary<string, string>> ThingStates { get; set; } = new();

    public static Dictionary Dictionary { get; set; }

    public static void LoadMessages(string messageFilePath)
    {
        if (FileAccess.FileExists(messageFilePath))
        {
            var file = FileAccess.Open(messageFilePath, FileAccess.ModeFlags.Read);
            string jsonContent = file.GetAsText();
            var jsonParser = new Json();
            Error error = jsonParser.Parse(jsonContent);

            if (error == Error.Ok)
            {
                var dictionary = jsonParser.Data.AsGodotDictionary();

                Dictionary = dictionary;

                // foreach (var thing in dictionary) // Thing
                // {
                //     var possibleVerbsDictionary = thing.Value.AsGodotDictionary(); // Get list of possible verbs for this thing

                //     var messagesDict = new Dictionary<string, Array<string>>(); // Preparing the dictionary for this thing

                //     foreach (var verb in possibleVerbsDictionary) // Verb
                //     {
                //         if (verb.Key.AsString() == "$state")
                //         {

                //         }

                //         var messageLines = verb.Value.AsGodotArray<string>(); // Get messages for this verb

                //         if (messageLines.Count == 0)
                //             GD.Print($"No messages found for verb {verb.Key} in thing {thing.Key}");

                //         messagesDict.Add(verb.Key.AsString(), messageLines);
                //     }

                //     ThingMessages.Add(thing.Key.AsString(), messagesDict);
                //     GD.Print($"Thing {thing.Key} added to ThingMessages");
                // }

                file.Close();
            }
            else
            {
                GD.PushError("Error parsing character JSON: ", error);
                file.Close();
            }
        }
        else
            GD.PushError("File not found: ", messageFilePath);
    }

    public static string GetMessages(string thingID, string verbID)
    {
        // if (ThingMessages.ContainsKey(thingID))
        // {
        //     return ThingMessages[thingID][verbID];
        // }
        if (Dictionary.ContainsKey(thingID))
        {
            var thing = Dictionary[thingID].AsGodotDictionary();
            if (thing.ContainsKey(verbID))
            {
                return thing[verbID].AsString();
            }
            else
                GD.PushWarning($"Verb {verbID} not found for thing {thingID}");
        }
        else
            GD.PushWarning($"Thing {thingID} not found");

        return "";
    }
}