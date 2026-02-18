using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EndOfAnEra_AntiCrash
{
    internal class Renderers
    {
        internal static uint GetTrianglesCountImpl(Mesh M)
        {
            uint result = 0;
            for (int i = M.subMeshCount - 1; i >= 0; i--)
            {
                result += M.GetTrianglesCountImpl(i);
            }
            return result;
        }
    }
}
