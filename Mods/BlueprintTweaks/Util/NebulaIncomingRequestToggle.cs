using System;
using NebulaAPI;

namespace BlueprintTweaks
{
    public class NebulaIncomingRequestToggle : IDisposable
    {
        IDisposable toggle;
        
        public NebulaIncomingRequestToggle()
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                toggle = NebulaModAPI.MultiplayerSession.Factories.IsIncomingRequest.On();
            }
        }

        public void Dispose()
        {
            if (NebulaModAPI.IsMultiplayerActive)
            {
                toggle?.Dispose();
            }
        }
    }
}