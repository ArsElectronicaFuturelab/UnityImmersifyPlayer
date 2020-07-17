public class EventIdentifierVideo : DeepSpace.JsonProtocol.EventIdentifierBase
{
	private const uint _offset = 100; // Values in this class should be higher then the ones in its base.

	public const uint VIDEO_SYNC = _offset + 1; // Synchronize video time from wall to floor.
	public const uint VIDEO_START_PAUSE = _offset + 2; // Start or Pause the currently loaded video.
	public const uint VIDEO_LOAD = _offset + 3; // Load the specified video.

	public new static string GetIdentifierName(uint identifier)
	{
		string name = "UNKNOWN";

		switch (identifier)
		{
			case VIDEO_SYNC:
				name = "VIDEO_SYNC";
				break;
			case VIDEO_START_PAUSE:
				name = "VIDEO_START_PAUSE";
				break;
			case VIDEO_LOAD:
				name = "VIDEO_LOAD";
				break;
			default:
				name = DeepSpace.JsonProtocol.EventIdentifierBase.GetIdentifierName(identifier);
				break;
		}

		return name;
	}
}
