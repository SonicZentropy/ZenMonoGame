namespace LDtkMonogameExample;

using System;
static class Program

{
	private static NezTiledTestGame _game;

	internal static void RunGame()
	{
		_game = new NezTiledTestGame();
		_game.Run();

		_game.Dispose();

	}

	/// <summary>
	/// The main entry point for the application.
	/// </summary>

	[STAThread]
	static void Main(string[] args)
	{
		RunGame();
	}

}