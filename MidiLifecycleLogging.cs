using Microsoft.Extensions.Logging;

namespace midi_router;

public static partial class MidiLifecycleLogging
{
    [LoggerMessage(1200, LogLevel.Information, "MIDI session created: {SessionName}, sessionId={SessionId}.")]
    public static partial void SessionCreated(this ILogger logger, string sessionName, Guid sessionId);

    [LoggerMessage(1201, LogLevel.Information, "MIDI session destroyed: {SessionName}, sessionId={SessionId}.")]
    public static partial void SessionDestroyed(this ILogger logger, string sessionName, Guid sessionId);

    [LoggerMessage(1202, LogLevel.Information, "MIDI virtual device created: {DeviceName}, endpointId={EndpointId}.")]
    public static partial void VirtualDeviceCreated(this ILogger logger, string deviceName, string endpointId);

    [LoggerMessage(1203, LogLevel.Information, "MIDI virtual device destroyed: {DeviceName}.")]
    public static partial void VirtualDeviceDestroyed(this ILogger logger, string deviceName);

    [LoggerMessage(1204, LogLevel.Debug, "MIDI routing endpoint connection opened: {EndpointId}, connectionId={ConnectionId}, virtual={IsVirtual}.")]
    public static partial void EndpointConnectionOpened(
        this ILogger logger, string endpointId, Guid connectionId, bool isVirtual);

    [LoggerMessage(1205, LogLevel.Debug, "MIDI routing endpoint connection closed: {ConnectionId}, virtual={IsVirtual}.")]
    public static partial void EndpointConnectionClosed(this ILogger logger, Guid connectionId, bool isVirtual);

    [LoggerMessage(1206, LogLevel.Debug, "MIDI routing synchronization started: selectedDevices={SelectedDevices}.")]
    public static partial void RoutingSynchronizationStarted(this ILogger logger, int selectedDevices);

    [LoggerMessage(1207, LogLevel.Information, "MIDI profile loaded: {ProfileId}, name={ProfileName}.")]
    public static partial void ProfileLoaded(this ILogger logger, string profileId, string profileName);

    [LoggerMessage(1208, LogLevel.Information, "MIDI profile created: {ProfileId}, name={ProfileName}.")]
    public static partial void ProfileCreated(this ILogger logger, string profileId, string profileName);

    [LoggerMessage(1209, LogLevel.Information, "MIDI profile selected: {ProfileId}.")]
    public static partial void ProfileSelected(this ILogger logger, string profileId);

    [LoggerMessage(1210, LogLevel.Information, "MIDI profile renamed: {ProfileId}, name={ProfileName}.")]
    public static partial void ProfileRenamed(this ILogger logger, string profileId, string profileName);

    [LoggerMessage(1211, LogLevel.Information, "MIDI profile deleted: {ProfileId}.")]
    public static partial void ProfileDeleted(this ILogger logger, string profileId);

    [LoggerMessage(1212, LogLevel.Debug, "MIDI profile state changed: {ProfileId}, selectedDevices={SelectedDevices}, channelAssignments={ChannelAssignments}.")]
    public static partial void ProfileStateChanged(
        this ILogger logger, string profileId, int selectedDevices, int channelAssignments);

    [LoggerMessage(1213, LogLevel.Debug, "MIDI application settings changed: {SettingName}.")]
    public static partial void SettingsChanged(this ILogger logger, string settingName);
}
