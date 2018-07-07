using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedMonitor
{
  public class DistributedLockingException : Exception
  {
    public DistributedLockingException(string message) : base(message)
    {
    }
  }
}
