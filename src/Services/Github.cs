// ReSharper disable InconsistentNaming
namespace Loial.Github
{
    //REF: https://developer.github.com/v3/activity/events/types/#pushevent
    public class PushEvent
    {
        public string Ref { get; set; }
        public Repository Repository { get; set; }
    };

    public class Repository
    {
        public string Name { get; set; }
        public string Full_Name { get; set; }
    };
}
