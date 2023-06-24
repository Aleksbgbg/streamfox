namespace Streamfox.Server.Controllers
{
    using System;

    public interface IClock
    {
        DateTime CurrentTime();
    }
}