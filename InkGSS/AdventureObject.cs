using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using static Logger;

public partial class AdventureObject : Object
{
    public Dictionary<string, string> DefaultVerbReactions { get; set; } = [];
}