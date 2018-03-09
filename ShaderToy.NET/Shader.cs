namespace ShaderToy.NET
{
    public class Shader
    {
	    public string Name { get; set; }
	    public string ResourceName { get; set; }
	    public bool SoundEnabled { get; set; }
		public string Source { get; set; }

		public Shader(string name, string res)
        {
            Name = name;
            ResourceName = res;
        }
    } 
}
