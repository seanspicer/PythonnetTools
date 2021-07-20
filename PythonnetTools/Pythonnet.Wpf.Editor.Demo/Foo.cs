

namespace Pythonnet.Wpf.Editor.Demo
{
    public interface IBar
    {
        public string GoofyName { get; }
    }

    internal class Bar : IBar
    {
        public string GoofyName { get; }

        internal Bar(string name)
        {
            GoofyName = "I am a goofy " + name;
        }
    }
    
    public class Foo
    {
        public int Val { get; set; }
        public string Name { get; set; }
        
        public IBar Bar { get; set; }

        public Foo(int val, string name)
        {
            Val = val;
            Name = name;
            Bar = new Bar(name);
        }
    }
}