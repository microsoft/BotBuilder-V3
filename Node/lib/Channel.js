function preferButtons(session, choiceCnt, rePrompt) {
    switch (getChannelId(session)) {
        case 'facebook':
        case 'skype':
            return (choiceCnt <= 3);
        case 'telegram':
        case 'kik':
        case 'emulator':
            return true;
        default:
            return false;
    }
}
exports.preferButtons = preferButtons;
function getChannelId(session) {
    return session.message.address.channelId.toLowerCase();
}
exports.getChannelId = getChannelId;
