using System.Collections.Generic;
using VideoCataloger;
using System.Runtime;
class Script
{
    // this is a sample script to show how to pipe action.
    // make an action that runs this script and set it as a pipe action on the action page
    // then from the cmd line run "fvc.exe Hello world"
    // hello world should be printed in the console 
  static public async System.Threading.Tasks.Task Run ( IScripting scripting, string arguments ) 
  { 
    scripting.GetConsole().Clear();
    scripting.GetConsole().WriteLine("Revieved:"  + arguments );
    scripting.GetActionPipe().WriteLine(arguments);
  }
}
