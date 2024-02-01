using System;
using Godot.Collections;
using Godot;

public static class MessageDataManager
{
    public static Dictionary<string, Dictionary<string, Array<string>>> ThingMessages { get; set; } = new();

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

                foreach (var thing in dictionary) // Thing
                {
                    var possibleVerbsDictionary = thing.Value.AsGodotDictionary(); // Get list of possible verbs for this thing

                    var messagesDict = new Dictionary<string, Array<string>>(); // Preparing the dictionary for this thing

                    foreach (var verb in possibleVerbsDictionary) // Verb
                    {
                        var messageLines = verb.Value.AsGodotArray<string>(); // Get messages for this verb

                        if (messageLines.Count == 0)
                            GD.Print($"No messages found for verb {verb.Key} in thing {thing.Key}");

                        messagesDict.Add(verb.Key.AsString(), messageLines);
                    }

                    ThingMessages.Add(thing.Key.AsString(), messagesDict);
                    GD.Print($"Thing {thing.Key} added to ThingMessages");
                }

                file.Close();
            }
            else
            {
                GD.Print("Error parsing character JSON: ", error);
                file.Close();
            }
        }
        else
            GD.Print("File not found: ", messageFilePath);
    }

    public static Array<string> GetMessages(string thingID, string verbID)
    {
        if (ThingMessages.ContainsKey(thingID))
        {
            return ThingMessages[thingID][verbID];
            // var messages = ThingMessages[thingID];

            // if (messages.ContainsKey(verbID))
            //     return ThingMessages[verbID];
            // else
            //     GD.Print($"Message {verbID} not found for object {thingID}");
        }
        else
            GD.Print($"Object {thingID} not found");

        return new Array<string>();
    }
}