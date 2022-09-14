using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolCryptographyC
{
    internal class FileWork
    {
        public Socket socket;
        
        public FileWork(Socket socket)
        {
            this.socket = socket;
        }
        public bool TransferFile()
        {
            try
            {
                //wait ask get file
                Segment? segment;
                do
                {
                    segment = Segment.ParseSegment(socket);
                } while ((segment == null) || (segment.Type != TypeSegment.ASK_GET_FILE) || (segment.Payload == null));
                FileInfo fileInfo = new FileInfo(Encoding.UTF8.GetString(segment.Payload));

                byte[]? bufferFile = null;
                byte[]? buffer = null;

                //check file and send system message
                if (!fileInfo.Exists)
                {
                    bufferFile = Segment.PackSegment(TypeSegment.FILE, (byte)0, Encoding.UTF8.GetBytes("error"));
                }
                long numAllBlock = (long)Math.Ceiling((double)fileInfo.Length / (double)Segment.lengthBlockFile);
                if ((fileInfo.Length == 0) || (numAllBlock >= 256))
                {
                    bufferFile = Segment.PackSegment(TypeSegment.FILE, (byte)0, Encoding.UTF8.GetBytes("error"));
                }
                if (bufferFile == null)
                {
                    //send first file part (system message or number of block + fileInfo)
                    buffer = Encoding.UTF8.GetBytes(fileInfo.Name);
                    bufferFile = new byte[buffer.Length + 1];
                    bufferFile[0] = (byte)numAllBlock;
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        bufferFile[i + 1] = buffer[i];
                    }
                    buffer = Segment.PackSegment(TypeSegment.FILE, (byte)0, bufferFile);
                    if (buffer != null)
                    {
                        socket.Send(buffer);
                    }

                    //send file
                    using (FileStream fstream = File.Open(fileInfo.Name, FileMode.Open))
                    {
                        //load file part
                        int numReadByte;
                        for (int i = 0; i < numAllBlock; i++)
                        {
                            buffer = new byte[Segment.lengthBlockFile];
                            fstream.Seek(i * Segment.lengthBlockFile, SeekOrigin.Begin);
                            numReadByte = fstream.Read(buffer);
                            bufferFile = new byte[numReadByte];
                            for (int j = 0; j < numReadByte; j++)
                            {
                                bufferFile[j] = buffer[j];
                            }

                            //send part file
                            buffer = Segment.PackSegment(TypeSegment.FILE, (byte)i, bufferFile);
                            if (buffer != null)
                            {
                                socket.Send(buffer);
                            }
                        }
                    }
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        public bool GetFile(string homePath, FileInfo fileInfo)
        {
            try
            {
                //ask get file
                byte[]? bufferFile = Segment.PackSegment(TypeSegment.ASK_GET_FILE, (byte)0, Encoding.UTF8.GetBytes(fileInfo.Name));
                if (bufferFile != null)
                {
                    socket.Send(bufferFile);
                }
                else
                {
                    return false;
                }

                //get first part file (system message or number of block + fileInfo)
                Segment? segment;
                segment = Segment.ParseSegment(socket);
                if (segment == null)
                {
                    return false;
                }
                if ((segment.Type != TypeSegment.FILE) || (segment.Payload == null))
                {
                    return false;
                }
                if (Encoding.UTF8.GetString(segment.Payload) == "error")
                {
                    return false;
                }

                byte numAllBlock = segment.Payload[0];
                byte[] buffer = new byte[segment.Payload.Length - 1];
                for (int i = 1; i < segment.Payload.Length; i++)
                {
                    buffer[i - 1] = segment.Payload[i];
                }
                fileInfo = new FileInfo(homePath + Encoding.UTF8.GetString(buffer));

                //get file
                using (FileStream fstream = new FileStream(homePath + fileInfo, FileMode.OpenOrCreate))
                {
                    for (int i = 0; i < numAllBlock; i++)
                    {
                        segment = Segment.ParseSegment(socket);
                        if (segment == null)
                        {
                            return false;
                        }
                        if ((segment.Type != TypeSegment.FILE) || (segment.Payload == null))
                        {
                            return false;
                        }
                        fstream.Seek(i * Segment.lengthBlockFile, SeekOrigin.Begin);
                        fstream.Write(segment.Payload);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
