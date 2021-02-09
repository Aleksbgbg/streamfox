namespace Streamfox.Server.Controllers
{
    using System;

    public class Clock : IClock
    {
        public DateTime CurrentTime()
        {
            return DateTime.Now;
        }
    }
}