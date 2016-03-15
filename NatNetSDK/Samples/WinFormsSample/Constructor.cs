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

        public Constructor(NatNetClientML natNetClientML)
        {
            this.natNetClientML = natNetClientML;
        }

        public void Construct(FrameOfMocapData frame)
        {
            /*if (frame.nRigidBodies < 1)
            {
                Console.WriteLine("no rigid body");
                return;
            }*/
            RigidBodyData rb = frame.RigidBodies[0];

            float[] quat = new float[4] { rb.qx, rb.qy, rb.qz, rb.qw };
            float[] eulers = natNetClientML.QuatToEuler(quat, (int)NATEulerOrder.NAT_XYZr);

            DateTime t = DateTime.Now;

            if (frame.nOtherMarkers > 0)
            {
                StreamWriter sw = new StreamWriter(new FileStream("a.txt", FileMode.OpenOrCreate));
                sw.WriteLine(t.ToString("HH:mm:ss fffffff"));
                sw.WriteLine("xxx");

                List<Marker> markers = new List<Marker>();
                for (int i = 0; i < frame.nOtherMarkers; ++i)
                {
                    Marker marker = frame.OtherMarkers[i];
                    markers.Add(marker);
                    sw.WriteLine(i + "\t" + marker.x + " " + marker.y + " " + marker.z);
                }
                sw.WriteLine("\n\n");

                sw.Close();
            }
        }
    }
}
