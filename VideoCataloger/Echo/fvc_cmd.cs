using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

//
// OBS: This is not an action or a fast video cataloger script, it will not run from inside fast video cataloger.
// This is the source to the fvc.exe cmd line tool. It shows how to send data over a pipe to a pipe action
// the fvc app is included in compiled form in the root of the install folder.
//

namespace FVC
{

    internal class Program
    {
        private const string ActionPipeName = "29270040-21A7-4DCB-8163-3BAE923D829F";

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("No argument provided");
                    Console.WriteLine("Syntax: FVC [argument to action]");
                    return;
                }

                System.IO.Pipes.NamedPipeClientStream m_ActionPipeClient = null;
                using (m_ActionPipeClient = new NamedPipeClientStream(ActionPipeName))
                {
                    int TimeoutMs = 1000;
                    m_ActionPipeClient.Connect(TimeoutMs);

                    string response = string.Join(" ", args);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    m_ActionPipeClient.Write(responseBytes, 0, responseBytes.Length);
                    m_ActionPipeClient.Flush();

                    const int read_buffer_size = 1024;
                    byte[] readbuffer = new byte[read_buffer_size];
                    int bytes_read = 0;
                    do
                    {
                        bytes_read = m_ActionPipeClient.Read(readbuffer, 0, readbuffer.Length);
                        if (bytes_read>0)
                        {
                            string from_stream = Encoding.UTF8.GetString(readbuffer, 0, bytes_read);
                            Console.Write(from_stream);
                        }
                    } while (bytes_read>0);
                }
            }
            catch (System.TimeoutException timeout)
            {
                Console.WriteLine("Failed to connect to the Fast video cataloger action pipe.");
                Console.WriteLine("Make sure Fast video cataloger is running and that the action pipe is enabled in preferences.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
