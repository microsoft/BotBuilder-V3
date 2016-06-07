function preferButtons(session, choiceCnt, rePrompt) {
    switch (getChannelId(session)) {
        case 'facebook':
            return (choiceCnt <= 3);
        case 'telegram':
        case 'kik':
        default:
            return false;
    }
}
exports.preferButtons = preferButtons;
function getChannelId(session) {
    return session.message.address.channelId.toLowerCase();
}
exports.getChannelId = getChannelId;
