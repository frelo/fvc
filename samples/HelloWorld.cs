#region samples_helloworld

using System.Runtime;

/// <summary>
///  This sample prints a simple message in the console.
/// </summary>
public class HelloWorld
{
    /// <summary>
    ///  Run sample. This is the entry function called by fvc.
    /// </summary>
    static public void Run(VideoCataloger.IScripting scripting, string arg)
    {
        scripting.GetConsole().WriteLine("Hello world");
    }
}

#endregion


