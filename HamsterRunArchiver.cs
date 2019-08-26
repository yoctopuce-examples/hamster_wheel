using System;
using System.Collections.Generic;
using System.Text;

namespace Yoctopuce_Hamster_Wheel
{
    interface HamsterRunArchiver
    {
        void Init();
        void Add(HamsterRun newRun);
    }
}
