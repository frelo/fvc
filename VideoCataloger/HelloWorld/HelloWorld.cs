#region samples_helloworld

using System.Runtime;
using VideoCataloger;

/// <summary>
///  This sample prints a simple message in the console.
/// </summary>
public class Script
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public async System.Threading.Tasks.Task Run(VideoCataloger.IScripting scripting, string arg)
    {
        scripting.GetConsole().WriteLine("Hello world");
    }
}

#endregion


