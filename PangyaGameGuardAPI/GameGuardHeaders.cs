namespace PangyaGameGuardAPI
{
	public struct GameGuardFirst
	{
		public string FileName { get; set; }
		public byte[] Signature { get; set; }//292 bytes
		public uint Sign1 { get; set; }
		public uint Filename_size { get; set; }
		public uint Signature_size { get; set; }
		public uint Sign2 { get; set; }
		public GameGuardFirst Reader(string name, byte[] sig, uint sign1, uint filename_size, uint sign_size, uint sign2)
		{
			FileName = name;
			Signature = sig;
			Sign1 = sign1;
			Filename_size = filename_size;
			Signature_size = sign_size;
			Sign2 = sign2;
			return this;
		}
	}
	public struct GameGuardTwo
	{
		public string FileName { get; set; }
		public byte[] Signature { get; set; }
		public uint Sign1 { get; set; }
		public uint Filename_size { get; set; }
		public uint Signature_size { get; set; }
		public uint Sign2 { get; set; }
		public GameGuardTwo Reader(string name, byte[] sig, uint sign1, uint filename_size, uint sign_size, uint sign2)
		{
			FileName = name;
			Signature = sig;
			Sign1 = sign1;
			Filename_size = filename_size;
			Signature_size = sign_size;
			Sign2 = sign2;
			return this;
		}
	}
	public struct GameGuardHeaders
	{
		public GameGuardFirst GameGuardFirst;
		public GameGuardTwo GameGuardTwo;

		public GameGuardFirst ReadFirst(string name, byte[] sig, uint sign1, uint filename_size, uint sign_size, uint sign2)
		{
			return GameGuardFirst.Reader(name, sig, sign2, filename_size, sign_size, sign2);
		}

		public GameGuardTwo ReadTwo(string name, byte[] sig, uint sign1, uint filename_size, uint sign_size, uint sign2)
		{
			return GameGuardTwo.Reader(name, sig, sign2, filename_size, sign_size, sign2);
		}
	}
}
