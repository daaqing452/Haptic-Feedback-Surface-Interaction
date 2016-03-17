using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NatNetML;
using System.IO;

namespace WinFormTestApp
{
    class Constructor
    {
        NatNetClientML natNetClientML;
        FrameOfMocapData lastFrame;
        StreamWriter fileWriter = new StreamWriter(new FileStream("a.txt", FileMode.Append));

        public Constructor(NatNetClientML natNetClientML)
        {
            this.natNetClientML = natNetClientML;
        }

        ~Constructor()
        {
            fileWriter.Close();
        }

        public void Construct(FrameOfMocapData frame)
        {
            fileWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss fffffff"));

            if (frame.nRigidBodies < 1)
            {
                fileWriter.WriteLine("no rigid body");
                //return;
            }
            else
            {
                RigidBodyData rb = frame.RigidBodies[0];
                //float[] quat = new float[4] { rb.qx, rb.qy, rb.qz, rb.qw };
                //float[] eulers = natNetClientML.QuatToEuler(quat, (int)NATEulerOrder.NAT_XYZr);
                fileWriter.WriteLine("rb: " + rb.x + " " + rb.y + " " + rb.z);
            }
            
            List<Marker> markers = new List<Marker>();
            for (int i = 0; i < frame.nOtherMarkers; ++i)
            {
                Marker marker = frame.OtherMarkers[i];
                markers.Add(marker);
                fileWriter.WriteLine(i + "\t" + marker.x + " " + marker.y + " " + marker.z);
            }
            fileWriter.WriteLine("\n\n");
        }
    }
}
